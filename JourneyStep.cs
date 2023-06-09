using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Journeys
{

    // A JourneyStep is a single step between two waypoints, eg A to B, following a specific lane (which dictates mode of transport)
    // The step from A to B is different from the step from A' to B' if A'B' starts or ends at a different offset or uses a different lane
    // Or to put it the other way around: a JourneyStep is unique iff both its start and end Waypoints are identical in all three components
    // of each. It may be noted that even this does not of itself guarantee *complete* uniqueness of the journey; for example one citizen might be
    // using Lane 4 on a bicycle, another using the same lane in a car, or one person may be on Line 3 and another on Line 10, both on the same public
    // transport lane, if the two lines share stops.

    // About public transport steps: Consider a step from A to B where both are public transport nodes.  The actual path of travel (hence: the mesh)
    // is along a path AA' to B'B, most often consisting of a chain of several land-based waypoints. The step from A to A' (stop to transport on road location) is
    // considered a public transport step (on A's line).  The path from A' to B' (found in the properties of the network node for A) is used to create
    // the mesh for JourneyStep that is indexed as AB. All in all where there is a
    // transport stop node at A, you can end up with 3 steps: AA' (getting on transport: looks like going from the side of the road into the middle of the
    // road); AB (with mesh based on A'B'); and possibly A'A (people getting off transport at A).

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

        private Mesh[] m_RouteMeshes;                   // calculated once only (as a RenderGroup.MeshData converted to Mesh) ready for use for the adjusted-width Meshes of each LineInfo - adjusted to be used directly for blended line
        private float m_routemeshHalfwidth;             // will get set to overall step SubHeat (the right halfwidth for blended lines if all cims on step are selected as showing)
        private float m_routemeshHalfheight;            // gets set as 5f and never changes
        private readonly bool m_endJourney;             // flag if the journey ends here at EndStep (if so add a final bobble)

        private static readonly object jstepLock = new object();

        public readonly Dictionary<ushort, int> m_CimLines;  // I make only token nods to full encapsulation. Public here so I can iterate over it directly instead of either having to pull a copy or write a JStep method
        private Dictionary<int, LineInfo> m_LineInfos;
        private LineInfo m_blendedLineInfo;              // separate LineInfo, outside the m_Lineinfo dictionary, for case of wanting blended mesh for whole step
        private List<int> m_heatOrderedLines;            // line numbers (m_LineInfo indices) in sequence of descending heat (so wide lines drawn before narrow). List will frequently only have one member

        public int RawHeat { get; private set; }        // could be implemented as just m_Cims.Count, but frequent reference is wanted so make as quick as possible
        public int SubHeat { get; private set; }        // this is the number of cims showing (ie marked as m_showCim=true in m_Cims) - heat of the subselection
        public bool NeedsMesh { get; private set; }     // true after construction - mesh is not calculated unless step is actually needed


        // not really a hash in the normal sense - just a unique string identifier. You cannot have two different steps that are the same on all these elements
        // this function is used by the step manager to determine if a step is a new step in the dictionary
        public string Hashname => StartStep.Segment + "+" + EndStep.Segment + "+" + StartStep.Offset + "+" + EndStep.Offset + "+" + StartStep.Lane + "+" + EndStep.Lane;

        // NB although public, this constructor should not be called directly, it is only for use by the manager after checking that we
        // do not have an instance of the same step already in the list
        public JourneyStep(Waypoint pointA, Waypoint pointB, ushort cim, int travelMode, Color lineColour, bool endJourney)
        {
            StartStep = pointA;
            EndStep = pointB;
            m_CimLines = new Dictionary<ushort, int> { { cim, travelMode } };
            m_LineInfos = new Dictionary<int, LineInfo>();
            m_blendedLineInfo = new LineInfo();
            m_heatOrderedLines = new List<int>();       // this is a list of m_LineInfos indices, in sequence of descending line subheat
            RawHeat = 1;
            SubHeat = 1;
            m_LineInfos.Add(travelMode, new LineInfo(lineColour));
            NeedsMesh = true;
            m_endJourney = endJourney;
            m_routemeshHalfwidth = 4f;  // just to get things started and keep VisualStudio happy. This gets reset before use in fact
            m_routemeshHalfheight = 5f; // this I have never changed from the PV original
        }

        public bool AugmentStepData(ushort cim, int travelMode, Color lineColour)
        {
            lock (jstepLock)
            {
                if (m_CimLines.ContainsKey(cim))
                {
                    // originally I considered this to be a serious error, but the realized there are scenarios in which a citizen
                    // does in fact validly use the same step twice, and it does not occur otherwise
                    return false;
                }
                m_CimLines.Add(cim, travelMode);
                ++RawHeat;
                ++SubHeat;
                if (m_LineInfos.TryGetValue(travelMode, out LineInfo lineinfo))
                {
                    ++lineinfo.LineHeat;
                    ++lineinfo.LineSubHeat;
                }
                else
                {
                    m_LineInfos.Add(travelMode, new LineInfo(lineColour));
                }
                return true;
            }
        }

        // The initial SubHeat test is so that very little time is wasted on steps that have already been fully hidden, so even though
        // this is called A LOT, the overhead is not excessive
        public void HideAllCims()
        {
            lock (jstepLock)
            {
                if (SubHeat > 0)
                {
                    SubHeat = 0;
                    foreach (LineInfo linfo in m_LineInfos.Values)
                        linfo.LineSubHeat = 0;
                }
            }
        }

        // ShowCitizen will fatal error if the citizen is not here. I took away the check for speed, this function gets called num_cims*num_showing_steps times per instance selection
        public void ShowCitizen(ushort cim)
        {
            lock (jstepLock)
            {
                SubHeat++;
                m_LineInfos[m_CimLines[cim]].LineSubHeat++;
            }
        }

        // to be absolutely sure if a cim is on PT on a step, you need to look them up individually, as follows - it is not enough
        // to say that a step is or is not a transport step, it may be both (eg for a car lane shared with a tram lane on narrow tram roads, or a bus+car lane)
        public bool IsTransportStep(ushort cim)
        {
            return m_CimLines[cim] > 31;
        }

        public void SetRouteMeshes(bool showAllCims = false)
        {
            lock (jstepLock)
            {
                if (showAllCims)
                {
                    SubHeat = RawHeat;
                    foreach (LineInfo lineInfo in m_LineInfos.Values)
                        lineInfo.LineSubHeat = lineInfo.LineHeat;
                }
                // directly setting subheat 0 is a dirty trick for hiding a step that is not actually otherwise cleared. It is not correct (spurious linesubheats and meshes will remain - inconsistently)
                // but it works quick and dirty because DrawTheMeshes checks and rejects steps with subheat 0
                if (SubHeat == 0)
                    return;
                bool showBlended = Singleton<JourneyVisualizer>.instance.ShowBlended;

                if (NeedsMesh)
                {
                    if (StartStep == null || EndStep == null)
                    {
                        Debug.Log("JV Error: SetRouteMesh called to create mesh with deficient start or end steps");
                        return;
                    }
                    List<Waypoint> route = new List<Waypoint>() { StartStep, EndStep };
                    // initialize the mesh per width of current SubHeat - this may then sometimes be preset for showing blended lines - but always use SetMesh/MeshAdjust because heat is very volatile
                    // NB we adjust the routemesh as the mesh for blended, to avoid yet another clone, so its halfwidth is volatile, but the big calculation only has to be done once, after that we just adjust it
                    m_routemeshHalfwidth = JVutils.Categorize(SubHeat);
                    m_RouteMeshes = JVutils.MeshFromData(JVutils.CreateMeshData(route, m_endJourney, m_routemeshHalfwidth, m_routemeshHalfheight));
                    NeedsMesh = false;
                }

                if (m_LineInfos.Count > 1)
                {
                    var sorted = m_LineInfos.OrderByDescending(x => x.Value.LineSubHeat);
                    m_heatOrderedLines = sorted.Select(id => id.Key).ToList();
                    if (showBlended)
                    {
                        int cat = JVutils.Categorize(SubHeat);
                        m_blendedLineInfo.m_lineColor = m_LineInfos[m_heatOrderedLines[0]].m_lineColor;      // m_lineColor is always set (in constructor) even if the rest is blank
                        m_blendedLineInfo.m_heatColor = JVutils.cutoffsColor[cat];
                        m_blendedLineInfo.m_halfWidth = JVutils.HalfWidthHeat(cat);
                        m_blendedLineInfo.LineSubHeat = SubHeat;
                        JVutils.MeshAdjust(m_RouteMeshes, m_routemeshHalfwidth, m_blendedLineInfo.m_halfWidth, m_endJourney);
                        m_routemeshHalfwidth = m_blendedLineInfo.m_halfWidth;
                        m_blendedLineInfo.m_meshes = m_RouteMeshes;
                    }
                    else
                    {
                        foreach (int line in m_heatOrderedLines)
                        {
                            LineInfo thisLineinfo = m_LineInfos[line];
                            int cat = JVutils.Categorize(thisLineinfo.LineSubHeat);
                            thisLineinfo.m_halfWidth = JVutils.HalfWidthHeat(cat);
                            thisLineinfo.m_heatColor = JVutils.cutoffsColor[cat];
                            thisLineinfo.SetMesh(m_RouteMeshes, m_routemeshHalfwidth, m_endJourney);
                        }
                    }
                }
                else
                {
                    m_heatOrderedLines = m_LineInfos.Keys.ToList();      // we know here it is a list of one item
                    LineInfo thisLineinfo = m_LineInfos[m_heatOrderedLines[0]];
                    int cat = JVutils.Categorize(thisLineinfo.LineSubHeat);
                    thisLineinfo.m_halfWidth = JVutils.HalfWidthHeat(cat);
                    thisLineinfo.m_heatColor = JVutils.cutoffsColor[cat];
                    // when there is only one line we can use routemesh for it instead of having to clone
                    JVutils.MeshAdjust(m_RouteMeshes, m_routemeshHalfwidth, thisLineinfo.m_halfWidth, m_endJourney);
                    m_routemeshHalfwidth = thisLineinfo.m_halfWidth;
                    thisLineinfo.m_meshes = m_RouteMeshes;
                    if (showBlended)
                        m_blendedLineInfo = m_LineInfos[m_heatOrderedLines[0]];
                }
            }
        }

        public void DumpStep(ushort stepID)
        {
            string ans = "\nDump of step " + stepID;
            ans = ans + "\nPointA: seg " + StartStep.Segment + " lane: " + StartStep.Lane + " offset: " + StartStep.Offset;
            ans = ans + "\nPointB: seg " + EndStep.Segment + " lane: " + EndStep.Lane + " offset: " + EndStep.Offset;
            List<ushort> cimlist = new List<ushort>();
            foreach (ushort cim in m_CimLines.Keys)
                cimlist.Add(cim);
            ans = ans + "\nNumber of cims: " + cimlist.Count + "\n";
            ans = ans + "RawHeat: " + RawHeat + "  SubHeat: " + SubHeat + "\n";
            ans += "List of cims(travelMode): ";
            foreach (ushort cim in cimlist)
            {
                ans = ans + " " + cim + "(" + m_CimLines[cim] + ")";
            }
            ans += "\nList of lines (LineHeat, LineSubHeat): ";
            foreach (int line in m_LineInfos.Keys)
            {
                ans = ans + " " + line + "(" + m_LineInfos[line].LineHeat + ", " + m_LineInfos[line].LineSubHeat + ")";
            }
            ans += "\nm_heatOrderedLines is: ";
            for (int idx = 0; idx < m_heatOrderedLines.Count; idx++)
            {
                ans += m_heatOrderedLines[idx] + ", ";
            }
            ans += "\n";
            // for debug - this is what happens in Draw
            List<LineInfo> lineInfos = new List<LineInfo>();
            if (Singleton<JourneyVisualizer>.instance.ShowBlended)
            {
                lineInfos.Add(m_blendedLineInfo);
            }
            else
            {
                foreach (int lineIndex in m_heatOrderedLines)
                {
                    lineInfos.Add(m_LineInfos[lineIndex]);
                    ans += "Draw line index " + lineIndex + " linesubheat: " + m_LineInfos[lineIndex].LineSubHeat + ", halfwidth " + m_LineInfos[lineIndex].m_halfWidth + ", mesh has length " + m_LineInfos[lineIndex].m_meshes.Length + "\n";
                }
            }
            ans += "routmeshHalfwidth is " + m_routemeshHalfwidth + " routeMeshes length is " + m_RouteMeshes.Length;
            ans += "\nNeedsMesh: " + NeedsMesh;
            ans += "\n";
            Debug.Log(ans);
        }


        // DrawTheMeshes is called via the Render entry point rather than through SimulationStep
        public void DrawTheMeshes(RenderManager.CameraInfo cameraInfo, Material material)
        {
            lock (jstepLock)
            {
                if (NeedsMesh || SubHeat == 0)
                    return;
                JourneyVisualizer theJV = Singleton<JourneyVisualizer>.instance;
                NetManager theNetManager = Singleton<NetManager>.instance;
                TransportManager theTransportManager = Singleton<TransportManager>.instance;
                material.SetFloat(theTransportManager.ID_StartOffset, -1000f);        // this is the "generic" usage (transportline and netadjust use this, although PV version does not
                bool heatmap = theJV.HeatMap;
                List<LineInfo> lineInfos = new List<LineInfo>();
                if (theJV.ShowBlended)
                {
                    lineInfos.Add(m_blendedLineInfo);
                }
                else
                {
                    foreach (int lineIndex in m_heatOrderedLines)
                        lineInfos.Add(m_LineInfos[lineIndex]);
                }
                foreach (LineInfo thisLineInfo in lineInfos)
                {
                    if (thisLineInfo.LineSubHeat == 0)
                        continue;
                    material.color = heatmap ? thisLineInfo.m_heatColor : thisLineInfo.m_lineColor;
                    int length = thisLineInfo.m_meshes.Length;
                    // in current version of program with "short steps" there is only ever one mesh in this array, in fact
                    for (int index = 0; index < length; ++index)
                    {
                        Mesh mesh = thisLineInfo.m_meshes[index];
                        if (mesh != null && cameraInfo.Intersect(mesh.bounds))
                        {
                            //if (thisSubJourney.m_requireSurfaceLine)
                            //    theTerrainManager.SetWaterMaterialProperties(mesh.bounds.center, material);
                            if (material.SetPass(0))
                            {
                                ++theNetManager.m_drawCallData.m_overlayCalls;
                                Graphics.DrawMeshNow(mesh, Matrix4x4.identity);
                            }
                        }
                    }
                }
            }
        }

        // the reason the following objects are classes instead of structs is so they can be referred to and directly modified in place within their dictionaries
        // (because objects are passed by reference while structs are passed by value)

        // ******************************* LineInfo subclass ****************************************/
        //
        // used in the m_Lineinfo dictionary, which refers to LineInfo members by line number (well - actually by travelMode)
        //
        private class LineInfo
        {
            public Color m_lineColor;
            public Color m_heatColor;
            public Mesh[] m_meshes;
            public float m_halfWidth;
            public int LineHeat { get; set; }
            public int LineSubHeat { get; set; }

            public LineInfo()
            {
            }

            public LineInfo(Color lineColor)
            {
                LineHeat = 1;
                LineSubHeat = 1;
                m_lineColor = lineColor;
                m_meshes = null;
            }

            // SetMesh uses the generic Mesh (routeMesh) to produce a Mesh of required halfwidth m_halfWidth (not passed as parameter - is object member)
            // the argument routemeshHalfWidth is the width of the generic mesh, used in the calculations
            public void SetMesh(Mesh[] data, float routemeshHalfwidth, bool endNode)
            {
                m_meshes = JVutils.MeshClone(data);
                JVutils.MeshAdjust(m_meshes, routemeshHalfwidth, m_halfWidth, endNode);
            }
        }
    }
}


