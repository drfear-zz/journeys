using ColossalFramework;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.PlatformServices;
using System;
using System.Collections.Generic;
using System.Threading;
using ColossalFramework.UI;
using UnityEngine;
using Journeys.RedirectionFramework;
using Journeys.RedirectionFramework.Attributes;
using Journeys.RedirectionFramework.Extensions;
using System.Linq;
using System.Runtime.InteropServices;
using System.Collections;



namespace Journeys
{

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

        public Waypoint StartStep { get; }
        public Waypoint EndStep { get; }
        private List<Waypoint> m_route;
        public Mesh[] m_RouteMeshes;        // should be private, just public for debug

        private Dictionary<ushort, CimStepInfo> m_Cims;
        private Dictionary<int, LineInfo> m_Lineinfo;
        private int m_hiddenCimsCount;
        private bool m_endJourney;
        private bool m_needsReheat;     // true = the counts on the mesh(es) have changed since last drawn
        private bool m_needsRenew;      // true = travelmodes (eg Lines) have been added or removed since last drawn

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
            public int m_lineCimsCount;
            public int m_hiddenCimsCount;
            public Color m_lineColor;
            public Mesh[] m_meshes;

            public LineInfo(bool show, Color lineColor)
            {
                m_lineCimsCount = 1;
                if (show)
                    m_hiddenCimsCount = 0;
                else
                    m_hiddenCimsCount = 1;
                m_lineColor = lineColor;
                m_meshes = null;
            }

            public int Reduce(bool showing)
            {
                if (m_lineCimsCount == 1)
                    NullifyMeshes();
                if (!showing)
                    --m_hiddenCimsCount;
                return --m_lineCimsCount;
            }

            public void HideOneCim() => ++m_hiddenCimsCount;
            public void UnhideOneCim() => --m_hiddenCimsCount;
            public void NullifyMeshes()
            {
                if (m_meshes == null)
                    return;
                for (int idx = 0; idx < m_meshes.Length; idx++)
                {
                    Mesh mesh = m_meshes[idx];
                    if (mesh != null) UnityEngine.Object.Destroy(mesh);  // this might not be needed, also it might not work cos it might destroy meshes underneath also
                }
                m_meshes = null;
            }
        }

        public void KillRouteMeshes()
        {
            if (m_RouteMeshes == null)
                return;
            for (int idx = 0; idx < m_RouteMeshes.Length; idx++)
            {
                Mesh mesh = m_RouteMeshes[idx];
                if (mesh != null) UnityEngine.Object.Destroy(mesh);  // this might not be needed, also it might not work cos it might destroy meshes underneath also
            }
            m_RouteMeshes = null;
        }

        public void KillLineMeshes()
        {
            if (m_Lineinfo != null)
            {
                foreach (int idx in m_Lineinfo.Keys)
                {
                    m_Lineinfo[idx].NullifyMeshes();
                }
            }
        }

        public int RawHeat => m_Cims.Count;

        public int SubHeat => m_Cims.Count - m_hiddenCimsCount;
        public bool NeedsReheat => m_needsReheat;
        public bool NeedsRenew => m_needsRenew;
        public string Hashname => StartStep.Segment + "+" + EndStep.Segment + "+" + StartStep.Offset + "+" + EndStep.Offset + "+" + StartStep.Lane + "+" + EndStep.Lane;

        // NB this constructor should not be called directly, it is only for use by the manager after checking that we
        // do not have an instance of the same step already in the list
        public JourneyStep(List<Waypoint> route, ushort citizenID, LineColorPair lineColorPair, bool endJourney, bool show = true)
        {
            Debug.Log("In JourneyStep constructor with args\nroute: " + printlist(route) + "\ncitizenID: " + citizenID +
                "\nLineColorPair travelmode: " + lineColorPair.m_travelmode + "\nendJourney: " + endJourney);
            StartStep = route[0];
            EndStep = route[route.Count - 1];
            m_route = route;
            m_Cims = new Dictionary<ushort, CimStepInfo>();
            m_Lineinfo = new Dictionary<int, LineInfo>();
            m_Cims.Add(citizenID, new CimStepInfo(lineColorPair.m_travelmode, show));
            if (show)
                m_hiddenCimsCount = 0;
            else
                m_hiddenCimsCount = 1;
            m_endJourney = endJourney;
            CreateMesh();
            m_Lineinfo.Add(lineColorPair.m_travelmode, new LineInfo(show, lineColorPair.m_lineColor));
            m_needsReheat = false;
            m_needsRenew = false;
        }
        public string printlist(List<Waypoint> wplist)
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
            if (m_Cims.ContainsKey(citizenID))
            {
                Debug.LogError("JV Error: Attempt to add a citizen to a JourneyStep they are already on");
                return false;
            }
            m_Cims.Add(citizenID, new CimStepInfo(lineColorPair.m_travelmode, show));
            if (!show)
                ++m_hiddenCimsCount;
            m_needsReheat = true;
            if (m_Lineinfo.TryGetValue(lineColorPair.m_travelmode, out LineInfo lineinfo))
            {
                ++lineinfo.m_lineCimsCount;
                if (!show)
                    ++lineinfo.m_hiddenCimsCount;
            }
            else
            {
                m_Lineinfo.Add(lineColorPair.m_travelmode, new LineInfo(show, lineColorPair.m_lineColor));
                m_needsRenew = true;
            }
            return true;
        }

        // returns modified number of Cims (in total), therefore caller check if this is 0 then this JourneyStep should be removed from main dictionary
        // checks for and returns counts unchanged if citizen is not here
        public int ReduceStepData(ushort citizenID)
        {
            if (m_Cims.TryGetValue(citizenID, out CimStepInfo stepinfo))
            {
                if (!stepinfo.m_showCim)
                    --m_hiddenCimsCount;
                int tmode = stepinfo.m_travelMode;
                m_needsReheat = true;
                if (m_Lineinfo[tmode].Reduce(stepinfo.m_showCim) == 0)
                {
                    m_Lineinfo.Remove(tmode);
                    m_needsRenew = true;
                }
                m_Cims.Remove(citizenID);
            }
            else
            {
                Debug.LogError("JV Error: attempt to remove citizen " + citizenID + " from a JourneyStep they are not on");
            }
            return m_Cims.Count;
        }

        // Does nothing if the citizen is not here, or already hidden
        public void HideCitizen(ushort citizenID)
        {
            if (m_Cims.TryGetValue(citizenID, out CimStepInfo stepinfo))
            {
                if (stepinfo.m_showCim == true)
                {
                    stepinfo.m_showCim = false;
                    ++m_hiddenCimsCount;
                    m_Lineinfo[stepinfo.m_travelMode].HideOneCim();
                }
            }
        }

        // Does nothing if the citizen is not here, or not hidden
        public void ShowCitizen(ushort citizenID)
        {
            if (m_Cims.TryGetValue(citizenID, out CimStepInfo stepinfo))
            {
                if (stepinfo.m_showCim == false)
                {
                    stepinfo.m_showCim = true;
                    --m_hiddenCimsCount;
                    m_Lineinfo[stepinfo.m_travelMode].UnhideOneCim();
                }
            }

        }
        private void CreateMesh()
        {
            TerrainManager theTerrainManager = Singleton<TerrainManager>.instance;

            // I do not really know what requireSurfaceLine is for, I have never seen it set and it always seems to work if set to false
            // I have left the mechanism in here intact in case I ever find it should not always be false for my use cases
            bool requireSurfaceLine = false;
            // Note the flow is: use a TempUpdateMeshData to do the calculations, then pass that into a RenderGroup.MeshData, then (in former UpdateMesh step)
            // pass that into a Mesh[].  I do not really follow why the RenderGroup.MeshData intermediate step is needed.  For now I keep it
            TransportLine.TempUpdateMeshData[] data = !requireSurfaceLine ? new TransportLine.TempUpdateMeshData[1] : new TransportLine.TempUpdateMeshData[81];
            int curveCount = 0;
            float totalLength = 0.0f;
            Vector3 endGridRef = Vector3.zero;
            CalculateRouteSegmentCount(m_route, ref data, ref curveCount, ref totalLength, ref endGridRef); //path1 was uint pathID same as journey.m_pathUnit once set up
            if (curveCount != 0)
            {
                if (requireSurfaceLine)
                    ++data[theTerrainManager.GetPatchIndex(endGridRef)].m_pathSegmentCount;
                else
                    ++data[0].m_pathSegmentCount;  // I have no idea why, but anyway: add 1 to the segment count if there were (any number of) curves
            }

            // and now we create the skeleton mesh (values are unpopulated members of TempUpdateMeshData).
            int length = 0;
            for (int index = 0; index < data.Length; ++index)
            {
                int pathSegmentCount = data[index].m_pathSegmentCount;
                if (pathSegmentCount != 0)
                {
                    data[index].m_meshData = new RenderGroup.MeshData()
                    {
                        m_vertices = new Vector3[pathSegmentCount * 8],
                        m_normals = new Vector3[pathSegmentCount * 8],
                        m_tangents = new Vector4[pathSegmentCount * 8],
                        m_uvs = new Vector2[pathSegmentCount * 8],
                        m_uvs2 = new Vector2[pathSegmentCount * 8],
                        m_colors = new Color32[pathSegmentCount * 8],
                        m_triangles = new int[pathSegmentCount * 30]
                    };
                    ++length;
                }
            }
            var lineCurves = new Bezier3[curveCount];
            var curveOffsets = new Vector2[curveCount];
            int curveIndex = 0;
            float lengthScale = Mathf.Ceil(totalLength / 64f) / totalLength;
            float currentLength = 0.0f;
            Vector3 minPos;
            Vector3 maxPos;
            FillRouteSegments(m_route, ref data, lineCurves, curveOffsets, ref curveIndex, ref currentLength, lengthScale, out minPos, out maxPos, requireSurfaceLine, false);
            // FillPathNode adds the circles/nodes at the end of journeys.  I show them only for the final destination (last subJourney)
            if (m_endJourney && curveCount != 0)
            {
                if (requireSurfaceLine)
                {
                    int patchIndex = theTerrainManager.GetPatchIndex(endGridRef);
                    TransportLine.FillPathNode(endGridRef, data[patchIndex].m_meshData, data[patchIndex].m_pathSegmentIndex, 4f, requireSurfaceLine ? 5f : 20f, true);
                    ++data[patchIndex].m_pathSegmentIndex;
                }
                else
                {
                    TransportLine.FillPathNode(endGridRef, data[0].m_meshData, data[0].m_pathSegmentIndex, 4f, requireSurfaceLine ? 5f : 20f, false);
                    ++data[0].m_pathSegmentIndex;
                }
            }
            RenderGroup.MeshData[] meshDataArray = new RenderGroup.MeshData[length];
            int num2 = 0;
            for (int index = 0; index < data.Length; ++index)
            {
                if (data[index].m_meshData != null)
                {
                    data[index].m_meshData.UpdateBounds();
                    if (requireSurfaceLine)
                    {
                        Vector3 min = data[index].m_meshData.m_bounds.min;
                        Vector3 max = data[index].m_meshData.m_bounds.max;
                        max.y += 1024f;
                        data[index].m_meshData.m_bounds.SetMinMax(min, max);
                    }
                    meshDataArray[num2++] = data[index].m_meshData;
                }
            }
            // now I pass directly to Mesh[] (formerly done in UpdateMesh step, if m_meshData was not null, then leavinng it null)
            int meshDataArrayLength = meshDataArray.Length;
            m_RouteMeshes = new Mesh[meshDataArrayLength];
            for (int index = 0; index < meshDataArrayLength; ++index)
            {
                m_RouteMeshes[index] = new Mesh();
                m_RouteMeshes[index].vertices = meshDataArray[index].m_vertices;
                m_RouteMeshes[index].normals = meshDataArray[index].m_normals;
                m_RouteMeshes[index].tangents = meshDataArray[index].m_tangents;
                m_RouteMeshes[index].uv = meshDataArray[index].m_uvs;
                m_RouteMeshes[index].uv2 = meshDataArray[index].m_uvs2;
                m_RouteMeshes[index].colors32 = meshDataArray[index].m_colors;
                m_RouteMeshes[index].triangles = meshDataArray[index].m_triangles;
                m_RouteMeshes[index].bounds = meshDataArray[index].m_bounds;
            }
        }

        // CalculateJourneySegmentCount started life as a clone of TransportLine.CalculatePathSegmentCount, but here it is journey-fied
        // Here it seemed easiest to just include it here as a new JourneyVisualizer method rather than set up redirections or try to remap path information to journeys
        public static bool CalculateRouteSegmentCount(
                List<Waypoint> route,
                ref TransportLine.TempUpdateMeshData[] data,  // if !m_requireSurfaceLine, data is length 1 and data[0].m_pathSegmentCount has counted the segments (could have done that a lot quicker ...)
                ref int curveCount,  // gets incremented whenever the distance from end of A to start of B is >1, and incremented again if start of B to offsetted position is > 1  [[ 1 would be a straight line I believe ]]
                ref float totalLength,
                ref Vector3 gridPosition)  // gridPosition gets reassigned every step, so ends up pointing to the final lane-offseted journey step, ie end of subJourney (in PV, end of path chain)
        {
            NetManager theNetManager = Singleton<NetManager>.instance;
            TerrainManager theTerrainManager = Singleton<TerrainManager>.instance;
            bool isFirstStep = true;
            Vector3 previousGridPos = Vector3.zero;
            foreach (Waypoint thisWaypoint in route)
            {
                // here I have brought PathManager.GetLaneID in house. Set up netLaneID as the network global lane ID (eg 123456) corresponding to the lane number (eg 0 to 5) in thisStep.m_lane
                uint netLaneID = theNetManager.m_segments.m_buffer[thisWaypoint.Segment].m_lanes;
                for (int index = 0; index < thisWaypoint.Lane && netLaneID != 0U; ++index)
                {
                    netLaneID = theNetManager.m_lanes.m_buffer[netLaneID].m_nextLane;
                }

                // * 0.003921569f is /255 where 255 is the max m_offest value (often seen) -> maps 255 to 1, 128 to 0.5 ish, 0 to 0.  The arg to CalculatePosition is called laneOffset.
                Vector3 gridPosOffseted = theNetManager.m_lanes.m_buffer[netLaneID].CalculatePosition(thisWaypoint.Offset * 0.003921569f); 
                gridPosition = gridPosOffseted;
                if (isFirstStep)
                {
                    previousGridPos = gridPosOffseted;
                    isFirstStep = false;
                }
                else
                {
                    theNetManager.m_lanes.m_buffer[netLaneID].GetClosestPosition(previousGridPos, out Vector3 thisWaypointClosestGridPos, out float laneOffset);
                    float num2 = Vector3.Distance(thisWaypointClosestGridPos, previousGridPos);
                    float num3 = Vector3.Distance(gridPosOffseted, thisWaypointClosestGridPos);
                    if ((double)num2 > 1.0)
                    {
                        int index2 = 0;
                        if (data.Length > 1)
                            index2 = theTerrainManager.GetPatchIndex((previousGridPos + thisWaypointClosestGridPos) * 0.5f);
                        ++data[index2].m_pathSegmentCount;
                        ++curveCount;
                        totalLength += num2;
                        previousGridPos = thisWaypointClosestGridPos;
                    }
                    if ((double)num3 > 1.0)
                    {
                        int index2 = 0;
                        if (data.Length > 1)
                            index2 = theTerrainManager.GetPatchIndex((previousGridPos + gridPosOffseted) * 0.5f);  // I don't know what a PatchIndex is, all I know is we get it for the world gridref halfway along the crowfly from previous to now
                        ++data[index2].m_pathSegmentCount;
                        ++curveCount;
                        totalLength += num3;
                        previousGridPos = gridPosOffseted;
                    }
                }
            }
            return true;
        }

        public static void FillRouteSegments(
          List<Waypoint> route,
          ref TransportLine.TempUpdateMeshData[] data,
          Bezier3[] curves,
          Vector2[] curveOffsets,
          ref int curveIndex,
          ref float currentLength,
          float lengthScale,
          out Vector3 minPos,
          out Vector3 maxPos,
          bool ignoreY,
          bool useStopOffset)
        {
            NetManager theNetManager = Singleton<NetManager>.instance;
            TerrainManager theTerrainManager = Singleton<TerrainManager>.instance;
            bool isFirstStep = true;
            bool flag2 = true;
            bool previousPedestrianOrBikeLane = false;
            Waypoint previousWaypoint = route[0];  // to keep the compiler happy. It's not actually used until it has been properly assigned to something meaningful
            Vector3 previousWaypointGridRef = Vector3.zero;
            Vector3 previousWaypointDirection = Vector3.zero;
            minPos = new Vector3(100000f, 100000f, 100000f);
            maxPos = new Vector3(-100000f, -100000f, -100000f);

            int lastPoint = route.Count - 1;
            for (int pointIndex = 0; pointIndex < route.Count; ++pointIndex)
            {

                Waypoint thisWaypoint = route[pointIndex];
                bool onLastStep = pointIndex == lastPoint;

                NetInfo info = theNetManager.m_segments.m_buffer[thisWaypoint.Segment].Info;
                // PV checks here !info.m_lanes[thisStep.m_lane].CheckType(laneTypes, vehicleTypes) but in JV of course we
                // have citizens travelling along vehicle lanes (while they are in vehicles) so do not check lane appropriate for pedestrian
                if (info == null || info.m_lanes == null || (info.m_lanes.Length <= thisWaypoint.Lane))
                    return;
                NetInfo.LaneType laneType = info.m_lanes[thisWaypoint.Lane].m_laneType;
                VehicleInfo.VehicleType vehicleType = info.m_lanes[thisWaypoint.Lane].m_vehicleType;
                int PedOrBikeLane;
                switch (laneType)
                {
                    case NetInfo.LaneType.Vehicle:
                        PedOrBikeLane = vehicleType == VehicleInfo.VehicleType.Bicycle ? 1 : 0;
                        break;
                    case NetInfo.LaneType.Pedestrian:
                        PedOrBikeLane = 1;
                        break;
                    default:
                        PedOrBikeLane = 0;
                        break;
                }
                bool isPedOrBikeLine = PedOrBikeLane != 0;
                uint laneId = GetLaneID(thisWaypoint);
                float offsetProportion = thisWaypoint.Offset * 0.003921569f;  // ie divided by 255(ish)
                Vector3 thisWaypointGridRef;
                Vector3 thisWaypointDirection;
                theNetManager.m_lanes.m_buffer[laneId].CalculatePositionAndDirection(offsetProportion, out thisWaypointGridRef, out thisWaypointDirection);
                minPos = Vector3.Min(minPos, thisWaypointGridRef - new Vector3(4f, 4f, 4f));
                maxPos = Vector3.Max(maxPos, thisWaypointGridRef + new Vector3(4f, 4f, 4f));
                if (isFirstStep)
                {
                    previousWaypointGridRef = thisWaypointGridRef;
                    previousWaypointDirection = thisWaypointDirection;
                    isFirstStep = false;
                }
                else
                {
                    Vector3 thisWaypointClosestPosition;
                    float laneOffset;
                    theNetManager.m_lanes.m_buffer[laneId].GetClosestPosition(previousWaypointGridRef, out thisWaypointClosestPosition, out laneOffset);
                    Vector3 rhs = theNetManager.m_lanes.m_buffer[laneId].CalculateDirection(laneOffset);
                    minPos = Vector3.Min(minPos, thisWaypointClosestPosition - new Vector3(4f, 4f, 4f));
                    maxPos = Vector3.Max(maxPos, thisWaypointClosestPosition + new Vector3(4f, 4f, 4f));
                    float distanceClosestToPrevious = Vector3.Distance(thisWaypointClosestPosition, previousWaypointGridRef);
                    float distanceAlongThisStep = Vector3.Distance(thisWaypointGridRef, thisWaypointClosestPosition);
                    if ((double)distanceAlongThisStep > 1.0)
                    {
                        if ((double)offsetProportion < (double)laneOffset)
                        {
                            rhs = -rhs;
                            thisWaypointDirection = -thisWaypointDirection;
                        }
                    }
                    else if ((double)offsetProportion > 0.5)
                    {
                        rhs = -rhs;
                        thisWaypointDirection = -thisWaypointDirection;
                    }
                    // if both the following if blocks are commented out, no journeys are drawn at all (but the destination nodes DO show)
                    // if this next block is commented out, the curves connecting segments (eg where they join round corners) are not drawn
                    if ((double)distanceClosestToPrevious > 1.0)
                    {
                        ushort thisStartNode = theNetManager.m_segments.m_buffer[thisWaypoint.Segment].m_startNode;
                        ushort thisEndNode = theNetManager.m_segments.m_buffer[thisWaypoint.Segment].m_endNode;
                        ushort previousStartNode = theNetManager.m_segments.m_buffer[previousWaypoint.Segment].m_startNode;
                        ushort previousEndNode = theNetManager.m_segments.m_buffer[previousWaypoint.Segment].m_endNode;
                        bool changedTransport = (int)thisStartNode != (int)previousStartNode && (int)thisStartNode != (int)previousEndNode && (int)thisEndNode != (int)previousStartNode && (int)thisEndNode != (int)previousEndNode;
                        int index2 = 0;
                        if (data.Length > 1)
                            index2 = theTerrainManager.GetPatchIndex((previousWaypointGridRef + thisWaypointClosestPosition) * 0.5f);
                        float overallLength = currentLength + distanceClosestToPrevious;
                        Bezier3 bezier = new Bezier3();
                        if ((isPedOrBikeLine || previousPedestrianOrBikeLane) && previousWaypoint.Segment == thisWaypoint.Segment || changedTransport)
                        {
                            Vector3 vector3_3 = VectorUtils.NormalizeXZ(thisWaypointClosestPosition - previousWaypointGridRef);
                            bezier.a = previousWaypointGridRef - vector3_3;
                            bezier.b = previousWaypointGridRef * 0.7f + thisWaypointClosestPosition * 0.3f;
                            bezier.c = thisWaypointClosestPosition * 0.7f + previousWaypointGridRef * 0.3f;
                            bezier.d = thisWaypointClosestPosition + vector3_3;
                        }
                        else
                        {
                            bezier.a = previousWaypointGridRef;
                            bezier.b = previousWaypointGridRef + previousWaypointDirection.normalized * (distanceClosestToPrevious * 0.5f);
                            bezier.c = thisWaypointClosestPosition - rhs.normalized * (distanceClosestToPrevious * 0.5f);
                            bezier.d = thisWaypointClosestPosition;
                        }
                        // your basic line, with halfwidth 4f
                        TransportLine.FillPathSegment(bezier, data[index2].m_meshData, curves, data[index2].m_pathSegmentIndex, curveIndex, currentLength * lengthScale, overallLength * lengthScale, 4f, !ignoreY ? 10f : 40f, ignoreY);
                        if (curveOffsets != null)
                            curveOffsets[curveIndex] = new Vector2(currentLength * lengthScale, overallLength * lengthScale);
                        ++data[index2].m_pathSegmentIndex;
                        ++curveIndex;
                        currentLength = overallLength;
                        previousWaypointGridRef = thisWaypointClosestPosition;
                        previousWaypointDirection = rhs;
                        flag2 = false;
                    }
                    // if this next block is commented out, what should be straight lines remain as pronounced bezier curves
                    if ((double)distanceAlongThisStep > 1.0)
                    {
                        int index2 = 0;
                        if (data.Length > 1)
                            index2 = theTerrainManager.GetPatchIndex((previousWaypointGridRef + thisWaypointGridRef) * 0.5f);
                        float overallLength = currentLength + distanceAlongThisStep;
                        Bezier3 subCurve = theNetManager.m_lanes.m_buffer[laneId].GetSubCurve(laneOffset, offsetProportion);
                        if ((flag2 || onLastStep) && useStopOffset)
                        {
                            float num7 = theNetManager.m_segments.m_buffer[thisWaypoint.Segment].Info.m_lanes[thisWaypoint.Lane].m_stopOffset;
                            if ((theNetManager.m_segments.m_buffer[thisWaypoint.Segment].m_flags & NetSegment.Flags.Invert) != NetSegment.Flags.None)
                                num7 = -num7;
                            if ((double)offsetProportion < (double)laneOffset)
                                num7 = -num7;
                            if (flag2)
                                subCurve.a += Vector3.Cross(Vector3.up, rhs).normalized * num7;
                            if (onLastStep)
                                subCurve.d += Vector3.Cross(Vector3.up, thisWaypointDirection).normalized * num7;
                        }
                        TransportLine.FillPathSegment(subCurve, data[index2].m_meshData, curves, data[index2].m_pathSegmentIndex, curveIndex, currentLength * lengthScale, overallLength * lengthScale, 4f, !ignoreY ? 10f : 40f, ignoreY);
                        if (curveOffsets != null)
                            curveOffsets[curveIndex] = new Vector2(currentLength * lengthScale, overallLength * lengthScale);
                        ++data[index2].m_pathSegmentIndex;
                        ++curveIndex;
                        currentLength = overallLength;
                        previousWaypointGridRef = thisWaypointGridRef;
                        previousWaypointDirection = thisWaypointDirection;
                        flag2 = false;
                    }
                }
                previousPedestrianOrBikeLane = isPedOrBikeLine;
                previousWaypoint = thisWaypoint;
            }
        }

        // lookup function journeyfied from PathManager original
        public static uint GetLaneID(Waypoint waypoint)
        {
            NetManager theNetManager = Singleton<NetManager>.instance;
            uint num = theNetManager.m_segments.m_buffer[waypoint.Segment].m_lanes;
            for (int index = 0; index < waypoint.Lane && num != 0U; ++index)
                num = theNetManager.m_lanes.m_buffer[num].m_nextLane;
            return num;
        }

        // TODO - lots!!
        public void DrawTheMeshes(RenderManager.CameraInfo cameraInfo, Material material, bool heatmap = false, int denominator = 0, bool ignoreHidden = true)
        {
            NetManager theNetManager = Singleton<NetManager>.instance;
            if (m_RouteMeshes == null)
                return;
            material.color = m_Lineinfo.First().Value.m_lineColor;
            if (heatmap == false && denominator == 0 && ignoreHidden == true)
            {
                int length = m_RouteMeshes.Length;
                for (int index = 0; index < length; ++index)
                {
                    Mesh mesh = m_RouteMeshes[index];
                    if (mesh != null && cameraInfo.Intersect(mesh.bounds))
                    {
                        //if (thisSubJourney.m_requireSurfaceLine)
                        //    theTerrainManager.SetWaterMaterialProperties(mesh.bounds.center, material);
                        if (material.SetPass(0))
                        {
                            Debug.Log("about to call DrawMeshNow");
                            ++theNetManager.m_drawCallData.m_overlayCalls;
                            Graphics.DrawMeshNow(mesh, Matrix4x4.identity);
                        }
                    }
                }
            }
        }
    }
}
        // var sorted = myDictionary.OrderBy(x => x.Value);

