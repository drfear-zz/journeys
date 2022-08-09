using ColossalFramework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



namespace Journeys
{

    ////[TargetType(typeof(PathVisualizer))]


    // A Waypoint is a network-based location (ie based on segment and position within the segment, rather than being
    // a vector gridReference, with a JourneyStep being the journey from one Waypoint to another
    // It may be noted that a Waypoint is identical to a PathUnit.Position; strictly I would not need to declare this as
    // as a new class, but it seems to make sense to keep things self-contained within the Journeys framework

    public class Waypoint
    {
        public ushort Segment { get; }

        public byte Offset { get; }

        public byte Lane { get; }

        // for debugging
        public void Dprint()
        {
            Debug.Log("\nm_segment: " + Segment + ", m_offset: " + Offset + ", m_lane: " + Lane);
        }

        public Waypoint(PathUnit.Position pathPosition)
        {
            Segment = pathPosition.m_segment;
            Offset = pathPosition.m_offset;
            Lane = pathPosition.m_lane;
        }

    }

    // A JourneyStep is a single step between two waypoints, eg A to B, following a specific lane (which dictates mode of transport)
    // The step from A to B is different from the step from A' to B' if A'B' starts or ends at a different offset or uses a different lane
    // Or to put it the other way around: a JourneyStep is unique iff both its start and end Waypoints are identical in all three components
    // of each. It may be noted that even this does not of itself guarantee *complete* uniqueness of the journey; for example one citizen might be
    // using Lane 4 on a bicycle, another using the same lane in a car, or one person may be on Line 3 and another on Line 10, both on the same public
    // transport lane, if the two lines share stops.

    // About public transport steps: Consider a step from A to B where both are public transport nodes.  The actual path of travel (hence: the mesh)
    // is along a path AA' to B'B, most often consisting of a chain of several land-based waypoints. The step from A to A' (stop to transport on road location) is
    // considered a public transport step (on A's line).  The path from A' to B' (found in the properties of the network node for A) is used to create
    // the mesh for JourneyStep that is indexed as AB. (The land-based steps are discarded after creating the mesh.)  All in all where there is a
    // transport stop node at A, you can end up with 3 steps: AA' (getting on transport: looks like going from the side of the road into the middle of the
    // road); AB (with mesh based on A'B'); and possibly A'A (people getting off transport at A).  NOTA BENE The step is recorded as A' to B' and it might
    // be the same step as X'Y' for another line (with each line having its own network nodes)

    // A JourneyStep object ALSO contains further information which would, in the main game components, be called a JourneyStepInfo. But I
    // just include that info in the object itself.  This info is: a list of the citizens who take the journey step, the transport line (mode of travel)
    // and the rendering mesh.

    // A JStep also stores its rendering mesh. (It may be noted that because of this, line arrows are not so beautifully spaced as they are
    // in PathVisualizer or Public Transport views.  To get nice line arrows along the whole journey, you need to calculate the meshes as one 
    // single thing (so as to get currentLength and totalLength and lengthScale right for each part).  My JStep constructor will use length
    // info from a previous step if available, but it can only ever be completely right for a single previous starting point.
    // Note: this is a big departure from previous versions, where the meshes were stored with the journey.  But that can mean for a full
    // train step, drawing (and storing) the same mesh 240 times.

    // Public transport steps work like this.
    // Take first a step AB where A is on land B is ptrans.  From B's segment.m_path find the land path B1-B2-...B' (B' is not B; B' is a ptrans segment)
    // NOTE: you can tell immediately if waypoint X is a ptrans stop by looking at its segment's m_path: 0 if not ptrans.
    // we need to set JSteps for A-B1 (the tiny step of getting on the tram) and for B1-B' (not actually corresponding to any single game segment, but
    // these are land waypoints
    // For a journey ABC, so far A-B1 then B1-B', if C is ptrans and waypoint C1 is the same as waypoint B' (same in all 3 components! normally differs only
    // in lane) the the passenger has not got off the tram, this is a continuing journey. It will be on the same line (ie B and C are same line) because C is
    // a ptrans stop on the SAME line as B, if it was a different line, it would have a different segment number (IE: line segments are unique to line, aka
    // every line has its own set of nodes and segments, potentially overlapping those for another line.)
    // Next we come to C'D. If D is land, then we just do the microstep C'D to get off the tram.  If D is transport, we've stayed on the same tram.
    // Note that when we stay on, the last waypoint of the land expansion, ie B', is the same as the first waypoint of the next land expansion, ie C1.  Be careful
    // not to duplicate this when creating steps! (it would do no actual harm to leave it in as a zero-length step, but better to not do that).

    // All in all it turns out relatively simple:
    // land to land: easy case.  Don't forget though that a journey ABC has steps AB and BC (ie waypoint B appears in both steps).
    // land to transport: first step is land to tram ministep (A to B1), second is from B1 to B' where B1 is the first waypoint in the land journey and B'
    // is the last. (The mesh uses the whole journey, which need not be retained as JSteps.)
    // transport to transport: B' will be repeated as the first waypoint of C's land journey, ie B' = C1.  So you can skip that and just record C1 to C'
    // transport to land: this is always just a ministep eg C' to D, where D is the stop (on land).

    public class JourneyStep
    {

        public Waypoint StartStep { get; private set; }
        public Waypoint EndStep { get; private set; }
        private List<Waypoint> m_route;         // really not needed after debugging, this is only needed to redraw the meshdata from scratch (which is never needed)

        private RenderGroup.MeshData[] m_RouteMeshes;   // will form the basis for the Mesh[]es in each LineInfo
        private float m_routemeshHalfwidth;
        private float m_routemeshHalfheight;
        private readonly bool m_endJourney;

        private static readonly object jstepLock = new object();

        private Dictionary<ushort, CimStepInfo> m_Cims;
        private Dictionary<int, LineInfo> m_Lineinfo;

        public int RawHeat { get; private set; }        // could be implemented as just m_Cims.Count, but frequent reference is wanted so make as quick as possible
        public int SubHeat { get; private set; }       // this is the number of cims showing (ie marked as m_showCim=true in m_Cims); heat of the subselection

        public bool NeedsReheat { get; private set; }
        public bool NeedsMesh { get; private set; }
        public bool HasMesh => m_RouteMeshes != null;

        private class CimStepInfo
        {
            public int m_travelMode;
            public bool m_showCim;

            public CimStepInfo(int travelmode, bool show = true)
            {
                m_travelMode = travelmode;
                m_showCim = show;
            }
        }

        private class LineInfo
        {
            public Color m_lineColor;
            public Color m_heatColor;
            public Mesh[] m_meshes;
            public float m_halfWidth;

            public int LineHeat { get; set; }
            public int LineSubHeat { get; set; }

            public LineInfo(bool show, Color lineColor)
            {
                LineHeat = 1;
                if (show)
                    LineSubHeat = 1;
                else
                    LineSubHeat = 0;
                m_lineColor = lineColor;
                m_meshes = null;
            }

            // returns num cims on line, caller check if zero if so remove the object
            public int Reduce(bool showing)
            {
                if (LineHeat == 1)
                    NullifyMeshes();
                if (showing)
                    --LineSubHeat;
                return --LineHeat;
            }

            public void SetMesh(RenderGroup.MeshData[] data, float routemeshHalfwidth)
            {
                m_meshes = JVutils.MeshFromData(data);
                JVutils.MeshAdjust(m_meshes, routemeshHalfwidth, m_halfWidth);
            }

            public void HideOneCim() => --LineSubHeat;
            public void UnhideOneCim() => ++LineSubHeat;
            public void NullifyMeshes()
            {
                if (m_meshes == null)
                    return;
                for (int idx = 0; idx < m_meshes.Length; idx++)
                {
                    Mesh mesh = m_meshes[idx];
                    if (mesh != null) UnityEngine.Object.Destroy(mesh); 
                }
                m_meshes = null;
            }
        }

        //public void KillRouteMeshes()
        //{
        //    lock (jstepLock)
        //    {
        //        if (m_RouteMeshes == null)
        //            return;
        //        for (int idx = 0; idx < m_RouteMeshes.Length; idx++)
        //        {
        //            var mesh = m_RouteMeshes[idx];
        //            if (mesh != null) mesh. UnityEngine.Object.Destroy(mesh);  // this might not really be needed, GC should take care of it, but I follow PV usage
        //        }
        //        m_RouteMeshes = null;
        //    }
        //}

        public void KillMeshes()
        {
            lock (jstepLock)
            {
                if (m_Lineinfo != null)
                {
                    foreach (LineInfo linfo in m_Lineinfo.Values)
                    {
                        linfo.NullifyMeshes();
                    }
                }
                m_RouteMeshes = null;
            }
        }

        public string Hashname => StartStep.Segment + "+" + EndStep.Segment + "+" + StartStep.Offset + "+" + EndStep.Offset + "+" + StartStep.Lane + "+" + EndStep.Lane;

        // NB this constructor should not be called directly, it is only for use by the manager after checking that we
        // do not have an instance of the same step already in the list
        public JourneyStep(List<Waypoint> route, ushort citizenID, LineColorPair lineColorPair, bool endJourney, bool show = true)
        {
            //lock (jstepLock)    // this probably makes no difference in a constructor. If meshes are here, the constructor exits with the meshes still not assigned anyway
            //{
                //Debug.Log("In JourneyStep constructor with args\nroute: " + Printlist(route) + "\ncitizenID: " + citizenID +
                //    "\nLineColorPair travelmode: " + lineColorPair.m_travelmode + "\nendJourney: " + endJourney);
                StartStep = route[0];
                EndStep = route[route.Count - 1];
            m_route = new List<Waypoint>();
            m_route.AddRange(route);
                // m_route = route;
                m_Cims = new Dictionary<ushort, CimStepInfo>();
                m_Lineinfo = new Dictionary<int, LineInfo>();
                m_Cims.Add(citizenID, new CimStepInfo(lineColorPair.m_travelmode, show));
                RawHeat = 1;
                if (show)
                    SubHeat = 1;
                else
                    SubHeat = 0;
                m_Lineinfo.Add(lineColorPair.m_travelmode, new LineInfo(show, lineColorPair.m_lineColor));
                NeedsReheat = true;
                NeedsMesh = true;
                m_endJourney = endJourney;
                m_routemeshHalfwidth = 4f;
                m_routemeshHalfheight = 5f;
                //m_RouteMeshes = CreateMesh(route, endJourney, m_routemeshHalfwidth, m_routemeshHalfheight);
                //NeedsMesh = false;
            //}
//            SetRouteMesh(route, endJourney, lineColorPair.m_lineColor, 4f, 5f);
        }

        private class TravelHeats
        {
            public int m_travelMode;
            public int m_heat;
        }

        public void SetRouteMeshes(bool forceReheat = false)
        {
            lock (jstepLock)
            {
                if (NeedsMesh || NeedsReheat || forceReheat)
                {
                    var theJV = Singleton<JourneyVisualizer>.instance;
                    bool ignoreHidden = !theJV.GetHeatOnlyAsSelected;
                    int denominator = ignoreHidden ? theJV.m_journeysCount : theJV.m_selectedJourneysCount;
                    int discreteCats = theJV.GetDiscreteHeats;
                    if (NeedsMesh)
                    {
                        if (m_route == null)
                        {
                            Debug.Log("JV: SetRouteMesh called to create mesh with empty m_route");
                            return;
                        }
                        // with multiple lines, the first is drawn to width per overall RawHeat, which is also the case when there is only one (so lineheat = rawheat)
                        m_routemeshHalfwidth = JVutils.HalfWidthHeat(RawHeat, denominator, discreteCats);
                        m_RouteMeshes = JVutils.CreateMeshData(m_route, m_endJourney, m_routemeshHalfwidth, m_routemeshHalfheight);
                        //Debug.Log("JV before adjust, first vertex: " + JVutils.VDprint(m_RouteMeshes[0].m_vertices[0]));
                        //Debug.Log("JV: length of RouteMeshes: " + m_RouteMeshes.Length);
                        NeedsMesh = false;
                    }
                    int lineInfoCount = m_Lineinfo.Count;
                    if (lineInfoCount > 1)
                    {
                        int lineIdx = 0;
                        TravelHeats[] heats = new TravelHeats[lineInfoCount];
                        foreach (int line in m_Lineinfo.Keys)
                        {
                            heats[lineIdx++] = new TravelHeats { m_travelMode = line, m_heat = ignoreHidden ? m_Lineinfo[line].LineHeat : m_Lineinfo[line].LineSubHeat };
                        }
                        IOrderedEnumerable<TravelHeats> sorted = heats.OrderByDescending(x => x.m_heat);
                        int stepHeat = ignoreHidden ? RawHeat : SubHeat;
                        int usedHeat = 0;
                        foreach (TravelHeats heat in sorted)
                        {
                            int remainingHeat = stepHeat - usedHeat;
                            if (remainingHeat <= 0)
                                break;
                            LineInfo thisLineinfo = m_Lineinfo[heat.m_travelMode];
                            thisLineinfo.m_halfWidth = JVutils.HalfWidthHeat(remainingHeat, denominator, discreteCats);
                            thisLineinfo.m_heatColor = JVutils.ColourHeat(remainingHeat, denominator, discreteCats);
                            thisLineinfo.SetMesh(m_RouteMeshes, m_routemeshHalfwidth);
                            usedHeat = heat.m_heat;
                        }
                    }
                    else
                    {
                        int stepHeat = ignoreHidden ? RawHeat : SubHeat;
                        if (stepHeat > 0)
                        {
                            LineInfo thisLineinfo = m_Lineinfo[m_Lineinfo.First().Key];                 // it's not really .First, it's the only one
                            thisLineinfo.m_halfWidth = JVutils.HalfWidthHeat(stepHeat, denominator, discreteCats);
                            thisLineinfo.m_heatColor = JVutils.ColourHeat(stepHeat, denominator, discreteCats);
                            thisLineinfo.SetMesh(m_RouteMeshes, m_routemeshHalfwidth);
                        }
                    }
                    NeedsReheat = false;
                }
            }
        }

        public string Printlist(List<Waypoint> wplist)
        {
            string ans = "";
            foreach (Waypoint idx in wplist)
            {
                ans = ans + idx.Segment + ", ";
            }
            return ans;
        }

        public bool AugmentStepData(ushort citizenID, LineColorPair lineColorPair, bool show = true)
        {
            lock (jstepLock)
            {
                if (m_Cims.ContainsKey(citizenID))
                {
                    Debug.LogError("JV Error: Attempt to add a citizen (" + citizenID + ") to a JourneyStep they are already on");
                    return false;
                }
                m_Cims.Add(citizenID, new CimStepInfo(lineColorPair.m_travelmode, show));
                ++RawHeat;
                if (show)
                    ++SubHeat;
                NeedsReheat = true;
                if (m_Lineinfo.TryGetValue(lineColorPair.m_travelmode, out LineInfo lineinfo))
                {
                    ++lineinfo.LineHeat;
                    if (show)
                        ++lineinfo.LineSubHeat;
                }
                else
                {
                    m_Lineinfo.Add(lineColorPair.m_travelmode, new LineInfo(show, lineColorPair.m_lineColor));
                }
                return true;
            }
        }

        // returns modified number of Cims (in total), therefore caller check if this is 0 then this JourneyStep should be removed from main dictionary
        // checks for and returns counts unchanged if citizen is not here
        public int ReduceStepData(ushort citizenID)
        {
            lock (jstepLock)
            {
                if (m_Cims.TryGetValue(citizenID, out CimStepInfo stepinfo))
                {
                    if (stepinfo.m_showCim)
                        --SubHeat;
                    int tmode = stepinfo.m_travelMode;
                    NeedsReheat = true;
                    if (m_Lineinfo[tmode].Reduce(stepinfo.m_showCim) == 0)
                    {
                        m_Lineinfo.Remove(tmode);
                    }
                    m_Cims.Remove(citizenID);
                    --RawHeat;
                }
                else
                {
                    Debug.LogError("JV Error: attempt to remove citizen " + citizenID + " from a JourneyStep they are not on");
                }
                return RawHeat;
            }
        }

        public List<ushort> HitSegmentLane(ushort segmentID, byte lane)
        {
            lock (jstepLock)
            {
                HideAllCims();
                HashSet<ushort> outhash = new HashSet<ushort>();
                bool foundhit = false;
                foreach (Waypoint waypoint in m_route)
                {
                    if (waypoint.Segment == segmentID && waypoint.Lane == lane)
                    {
                        foundhit = true;
                        break;  
                    }
                }
                if (foundhit)
                {
                    return m_Cims.Keys.ToList();
                }
                else
                {
                    return null;
                }
            }
        }

        private void HideAllCims()
        {
            foreach (CimStepInfo ciminfo in m_Cims.Values)
            {
                ciminfo.m_showCim = false;
            }
            foreach (LineInfo linfo in m_Lineinfo.Values)
            {
                linfo.LineSubHeat = 0;
            }
            SubHeat = 0;
            NeedsReheat = true;
        }

        // Does nothing if the citizen is not here, or already hidden
        public void HideCitizen(ushort citizenID)
        {
            lock (jstepLock)
            {
                if (m_Cims.TryGetValue(citizenID, out CimStepInfo stepinfo))
                {
                    if (stepinfo.m_showCim == true)
                    {
                        stepinfo.m_showCim = false;
                        --SubHeat;
                        m_Lineinfo[stepinfo.m_travelMode].HideOneCim();
                    }
                }
            }
        }

        // Does nothing if the citizen is not here, or not hidden
        public void ShowCitizen(ushort citizenID)
        {
            lock (jstepLock)
            {
                if (m_Cims.TryGetValue(citizenID, out CimStepInfo stepinfo))
                {
                    if (stepinfo.m_showCim == false)
                    {
                        stepinfo.m_showCim = true;
                        ++SubHeat;
                        m_Lineinfo[stepinfo.m_travelMode].UnhideOneCim();
                    }
                }
            }
        }

        public void DrawTheMeshes(RenderManager.CameraInfo cameraInfo, Material material)
        {
            lock (jstepLock)
            {
                if (NeedsReheat || NeedsMesh || SubHeat == 0)
                    return;
                NetManager theNetManager = Singleton<NetManager>.instance;
                TransportManager theTransportManager = Singleton<TransportManager>.instance;
                material.SetFloat(theTransportManager.ID_StartOffset, -1000f);        // this is the "generic" usage (transportline and netadjust use this, although PV version does not
                bool heatmap = Singleton<JourneyVisualizer>.instance.GetHeatMap;
                foreach (LineInfo thisLineInfo in m_Lineinfo.Values)
                {
                    if (thisLineInfo.LineSubHeat == 0)
                        continue;
                    material.color = heatmap ? thisLineInfo.m_heatColor : thisLineInfo.m_lineColor;
                    int length = thisLineInfo.m_meshes.Length;
                    for (int index = 0; index < length; ++index)
                    {
                        Mesh mesh = thisLineInfo.m_meshes[index];
                        if (mesh != null && cameraInfo.Intersect(mesh.bounds))
                        {
                            //if (thisSubJourney.m_requireSurfaceLine)
                            //    theTerrainManager.SetWaterMaterialProperties(mesh.bounds.center, material);
                            if (material.SetPass(0))
                            {
                                //Debug.Log("about to call DrawMeshNow");
                                ++theNetManager.m_drawCallData.m_overlayCalls;
                                Graphics.DrawMeshNow(mesh, Matrix4x4.identity);
                            }
                        }
                    }
                }
            }
        }


    }
}

