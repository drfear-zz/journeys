using System.Collections.Generic;
using UnityEngine;
using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.UI;



namespace Journeys
{
    public static class JVutils
    {

        public static Dictionary<int, Color> m_travelModeColors;
        static JVutils()
        {
            m_travelModeColors = new Dictionary<int, Color>
            {
                { 0, Color.white },  // representing serious error condition (segment info null)
                { 1, ColorUtils.Desaturate(Color.magenta, 0.3f) },  // cars
                { 2, ColorUtils.Desaturate(Color.green, 0.3f) },  // pedestrians
                { 3, ColorUtils.Desaturate(Color.cyan, 0.3f) },  // bicycles
                { 4, Color.black },  // all other non-public-transport (mostly never selectable)
                { 5, Color.blue },  // generic for bus
                { 6, Color.green },  // metro
                { 7, Color.yellow },  // train
                { 8, Color.yellow },  // ship
                { 9, Color.yellow },  // airplane
                { 10, Color.yellow },  // taxi
                { 11, Color.magenta },  // tram
                { 12, Color.yellow },  // evac bus
                { 13, Color.yellow },  // monorail
                { 14, Color.yellow },  // cablecar
                { 15, Color.yellow },  // touristbus
                { 16, Color.yellow },  // hotairballon
                { 17, Color.yellow },  // post
                { 18, Color.yellow },  // trolleybus
                { 19, Color.yellow },  // fishing
                { 20, Color.yellow }  // helicopter
            };
        }

        public static RenderGroup.MeshData[] CreateMeshData(List<Waypoint> route, bool endJourney, float halfwidth, float halfheight)
        {
            if (route.Count == 0)
            {
                Debug.Log("JV Error: createmesh called with empty route");
                return null;
            }

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
            CalculateRouteSegmentCount(ref route, ref data, ref curveCount, ref totalLength, ref endGridRef); //path1 was uint pathID same as journey.m_pathUnit once set up
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
            FillRouteSegments(ref route, ref data, lineCurves, curveOffsets, ref curveIndex, ref currentLength, lengthScale, halfwidth, halfheight, out minPos, out maxPos, requireSurfaceLine, false);
            // FillPathNode adds the circles/nodes at the end of journeys.  I show them only for the final destination (last subJourney)
            if (endJourney && curveCount != 0)
            {
                if (requireSurfaceLine)
                {
                    int patchIndex = theTerrainManager.GetPatchIndex(endGridRef);
                    TransportLine.FillPathNode(endGridRef, data[patchIndex].m_meshData, data[patchIndex].m_pathSegmentIndex, halfwidth, requireSurfaceLine ? halfheight : 4f * halfheight, true);
                    ++data[patchIndex].m_pathSegmentIndex;
                }
                else
                {
                    TransportLine.FillPathNode(endGridRef, data[0].m_meshData, data[0].m_pathSegmentIndex, halfwidth, requireSurfaceLine ? halfheight : 4f * halfheight, false);
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
            return meshDataArray;
        }

        // CalculateJourneySegmentCount started life as a clone of TransportLine.CalculatePathSegmentCount, but here it is journey-fied
        // Here it seemed easiest to just include it here as a new JourneyVisualizer method rather than set up redirections or try to remap path information to journeys
        public static bool CalculateRouteSegmentCount(
                ref List<Waypoint> route,
                ref TransportLine.TempUpdateMeshData[] data,  // if !m_requireSurfaceLine, data is length 1 and data[0].m_pathSegmentCount has counted the segments (could have done that a lot quicker ...)
                ref int curveCount,  // gets incremented whenever the distance from end of A to start of B is >1, and incremented again if start of B to offsetted position is > 1  [[ 1 would be a straight line I believe ]]
                ref float totalLength,
                ref Vector3 gridPosition)  // gridPosition gets reassigned every step, so ends up pointing to the final lane-offseted journey step, ie end of subJourney (in PV, end of path chain)
        {
            //Debug.Log("JV: in CalcRouteSegCount. Waypoint 0 and 1:");
            //route[0].Dprint();
            //route[1].Dprint();
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
          ref List<Waypoint> route,
          ref TransportLine.TempUpdateMeshData[] data,
          Bezier3[] curves,
          Vector2[] curveOffsets,
          ref int curveIndex,
          ref float currentLength,
          float lengthScale,
          float halfwidth,
          float halfheight,
          out Vector3 minPos,
          out Vector3 maxPos,
          bool ignoreY,
          bool useStopOffset)
        {
            //Debug.Log("JV: in FillRouteSegments. Waypoint 0 and 1:");
            //route[0].Dprint();
            //route[1].Dprint();
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
                    //Debug.Log("Wpoint " + pointIndex + " grid ref " + VDprint(thisWaypointGridRef));
                    //Debug.Log("Wpoint " + pointIndex + " direction " + VDprint(thisWaypointDirection) + " - normalized: " + VDprint(thisWaypointDirection.normalized));
                minPos = Vector3.Min(minPos, thisWaypointGridRef - new Vector3(4f, 4f, 4f));
                maxPos = Vector3.Max(maxPos, thisWaypointGridRef + new Vector3(4f, 4f, 4f));
                if (isFirstStep)
                {
                    previousWaypointGridRef = thisWaypointGridRef;
                    if (offsetProportion == 0)
                        previousWaypointDirection = -thisWaypointDirection;
                    else
                        previousWaypointDirection = thisWaypointDirection;
                    isFirstStep = false;
                }
                else
                {
                    Vector3 thisWaypointClosestPosition;
                    float laneOffset;
                    theNetManager.m_lanes.m_buffer[laneId].GetClosestPosition(previousWaypointGridRef, out thisWaypointClosestPosition, out laneOffset);
                    Vector3 rhs = theNetManager.m_lanes.m_buffer[laneId].CalculateDirection(laneOffset);
                    //Debug.Log("Wpoint " + pointIndex + " thisWaypointClosestPosition " + VDprint(thisWaypointClosestPosition));
                    //Debug.Log("Wpoint " + pointIndex + " laneOffset " + laneOffset);
                    //Debug.Log("Wpoint " + pointIndex + " rhs " + VDprint(rhs) + " - normalized: " + VDprint(thisWaypointDirection.normalized));
                    minPos = Vector3.Min(minPos, thisWaypointClosestPosition - new Vector3(4f, 4f, 4f));
                    maxPos = Vector3.Max(maxPos, thisWaypointClosestPosition + new Vector3(4f, 4f, 4f));
                    float distanceClosestToPrevious = Vector3.Distance(thisWaypointClosestPosition, previousWaypointGridRef);
                    float distanceAlongThisStep = Vector3.Distance(thisWaypointGridRef, thisWaypointClosestPosition);
                    //Debug.Log("Wpoint " + pointIndex + " distanceClosestToPrevious " + distanceClosestToPrevious);
                    //Debug.Log("Wpoint " + pointIndex + " distanceAlongThisStep " + distanceAlongThisStep);
                    if (distanceAlongThisStep > 1.0f)
                    {
                        if (offsetProportion < laneOffset)
                        {
                            rhs = -rhs;
                            thisWaypointDirection = -thisWaypointDirection;
                        }
                    }
                    else if (offsetProportion > 0.5)
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
                        TransportLine.FillPathSegment(bezier, data[index2].m_meshData, curves, data[index2].m_pathSegmentIndex, curveIndex, currentLength * lengthScale, overallLength * lengthScale, halfwidth, !ignoreY ? halfheight : halfheight * 4f, ignoreY);
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
                        TransportLine.FillPathSegment(subCurve, data[index2].m_meshData, curves, data[index2].m_pathSegmentIndex, curveIndex, currentLength * lengthScale, overallLength * lengthScale, halfwidth, !ignoreY ? halfheight : halfheight * 4f, ignoreY);
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

        public static Mesh[] MeshFromData(RenderGroup.MeshData[] data)
        {
            int dataLength = data.Length;
            Mesh[] newmesh = new Mesh[dataLength];
            for (int index = 0; index < dataLength; ++index)
            {
                newmesh[index] = new Mesh
                {
                    vertices = data[index].m_vertices,
                    normals = data[index].m_normals,
                    tangents = data[index].m_tangents,
                    uv = data[index].m_uvs,
                    uv2 = data[index].m_uvs2,
                    colors32 = data[index].m_colors,
                    triangles = data[index].m_triangles,
                    bounds = data[index].m_bounds
                };
            }
            return newmesh;
        }

        public static Mesh[] MeshClone(Mesh[] data)
        {
            int dataLength = data.Length;
            Mesh[] newmesh = new Mesh[dataLength];
            for (int index = 0; index < dataLength; ++index)
            {
                newmesh[index] = new Mesh
                {
                    vertices = data[index].vertices,
                    normals = data[index].normals,
                    tangents = data[index].tangents,
                    uv = data[index].uv,
                    uv2 = data[index].uv2,
                    colors32 = data[index].colors32,
                    triangles = data[index].triangles,
                    bounds = data[index].bounds
                };
            }
            return newmesh;
        }

        // save redefining this every time we adjust a mesh
        public static readonly int[] starts = { 0, 1, 4, 5 };

        // instead of completely redrawing a mesh[] from scratch, just adjust it for a different halfwidth
        public static void MeshAdjust(Mesh[] meshes, float currentHalfwidth, float newHalfwidth, bool endNode)
        {
            float delta = currentHalfwidth - newHalfwidth;
            if (Mathf.Abs(delta) < 0.1)         // this makes the function generic, but in fact in JV halfwidths are integer, so delta will be either 0 or >=1
                return;
            float width = 2 * currentHalfwidth;
            float weight = width - delta;
            foreach (Mesh mesh in meshes)
            {
                Vector3[] newVertices = mesh.vertices;
                Vector3[] newNormals = mesh.normals;
                int eightIdx;
                int eightstop = (mesh.vertexCount / 8);
                if (endNode)
                    eightstop--;
                for (eightIdx = 0; eightIdx < eightstop; ++eightIdx)
                {
                    foreach (int idx in starts)
                    {
                        int absIdx1 = idx + (eightIdx * 8);
                        int absIdx2 = absIdx1 + 2;
                        Vector3 v1 = mesh.vertices[absIdx1];
                        Vector3 v2 = mesh.vertices[absIdx2];
                        Vector3 n1 = mesh.normals[absIdx1] + new Vector3(0f, 0f, delta);
                        Vector3 n2 = mesh.normals[absIdx2] + new Vector3(0f, 0f, delta);
                        newVertices[absIdx1] = ((weight * v1) + (delta * v2)) / width;
                        newVertices[absIdx2] = ((delta * v1) + (weight * v2)) / width;
                        newNormals[absIdx1] = n1;
                        newNormals[absIdx2] = n2;
                    }
                }
                // then for the filled nodes (end journeys)
                if (endNode)
                {
                    int idxOffset = eightstop * 8;
                    newVertices[idxOffset] = mesh.vertices[idxOffset] + new Vector3(delta, 0, delta);
                    newNormals[idxOffset] = mesh.normals[idxOffset] - new Vector3(0, 0, delta);
                    idxOffset++;
                    newVertices[idxOffset] = mesh.vertices[idxOffset] + new Vector3(delta, 0, delta);
                    newNormals[idxOffset] = mesh.normals[idxOffset] - new Vector3(0, 0, delta);
                    idxOffset++;
                    newVertices[idxOffset] = mesh.vertices[idxOffset] + new Vector3(-delta, 0, delta);
                    newNormals[idxOffset] = mesh.normals[idxOffset] - new Vector3(0, 0, delta);
                    idxOffset++;
                    newVertices[idxOffset] = mesh.vertices[idxOffset] + new Vector3(-delta, 0, delta);
                    newNormals[idxOffset] = mesh.normals[idxOffset] - new Vector3(0, 0, delta);
                    idxOffset++;
                    newVertices[idxOffset] = mesh.vertices[idxOffset] + new Vector3(delta, 0, -delta);
                    newNormals[idxOffset] = mesh.normals[idxOffset] - new Vector3(0, 0, delta);
                    idxOffset++;
                    newVertices[idxOffset] = mesh.vertices[idxOffset] + new Vector3(delta, 0, -delta);
                    newNormals[idxOffset] = mesh.normals[idxOffset] - new Vector3(0, 0, delta);
                    idxOffset++;
                    newVertices[idxOffset] = mesh.vertices[idxOffset] + new Vector3(-delta, 0, -delta);
                    newNormals[idxOffset] = mesh.normals[idxOffset] - new Vector3(0, 0, delta);
                    idxOffset++;
                    newVertices[idxOffset] = mesh.vertices[idxOffset] + new Vector3(-delta, 0, -delta);
                    newNormals[idxOffset] = mesh.normals[idxOffset] - new Vector3(0, 0, delta);
                }
                mesh.vertices = newVertices;
                mesh.normals = newNormals;
                mesh.RecalculateBounds();
            }
        }

        // Note: for all that this is called "turns corner", it in fact splits at any junction, including when the path is straight over the junction
        public static bool TurnsCorner(Waypoint wpointA, Waypoint wpointB, out Waypoint pointBprime)
        {
            pointBprime = null;
            if (wpointA.Segment == wpointB.Segment)
                return false;
            NetManager theNetManager = Singleton<NetManager>.instance;
            Vector3 gridrefA = theNetManager.m_lanes.m_buffer[GetLaneID(wpointA)].CalculatePosition(wpointA.Offset / 255);
            uint laneIDB = GetLaneID(wpointB);
            NetLane laneB = theNetManager.m_lanes.m_buffer[laneIDB];
            Vector3 gridrefB1 = laneB.CalculatePosition(0);
            Vector3 gridrefB2 = laneB.CalculatePosition(1);
            float distanceAB1 = Vector3.Distance(gridrefA, gridrefB1);
            float distanceAB2 = Vector3.Distance(gridrefA, gridrefB2);
            if (distanceAB1 > distanceAB2 && distanceAB2 > 1)
            {
                pointBprime = new Waypoint(wpointB.Segment, wpointB.Lane, 255, wpointB.TravelMode);
                return pointBprime.Offset != wpointB.Offset;        // sometimes pointB is already the start of its segment, so Bprime and B are the same; then do not double it
            }
            else if (distanceAB2 > distanceAB1 && distanceAB1 > 1)
            {
                pointBprime = new Waypoint(wpointB.Segment, wpointB.Lane, 0, wpointB.TravelMode);
                return pointBprime.Offset != wpointB.Offset;
            }
            return false;
        }

        // I removed this functionality because my implementation attempt does not properly identify cases when a PT passes a stop of another line, without itself having a stop there.
        // (and this is the only case in which it matters at all)
        // In other words: much, much more trouble than it is worth. You can in any case obtain the info using lane-line cycle.
        // To identify properly is very complicated, need to take account of positions and directions, whether same lane etc etc
        // current code looks only at Seg B but that is also not good enough, if route is A-000 to B-000 the stop will be on A (if A is forwards ...)
        //
        //public static bool PassesStop(Waypoint wpointA, Waypoint wpointB, out Waypoint pointBprime)
        //{
        //    pointBprime = null;
        //    if (wpointA.Offset != 128 && wpointB.Offset != 128 &&
        //            ((wpointA.Offset < 128 && wpointB.Offset > 128) || (wpointA.Offset > 128 && wpointB.Offset < 128)))
        //    {
        //        NetManager theNetManager = Singleton<NetManager>.instance;
        //        // PT stops are not on the PT lanes, they are on pedestrian lanes.  There seems no link built in so do the following,
        //        // which assumes if a stop is on the same side of the road as a PT lane, then it's a stop for the PT lane
        //        NetInfo.Lane[] thisInfoLanes = theNetManager.m_segments.m_buffer[wpointB.Segment].Info.m_lanes;
        //        bool pos = thisInfoLanes[wpointB.Lane].m_position > 0;
        //        // look for a lane on same side of the road with a PT stop
        //        for (int idx = 0; idx < thisInfoLanes.Length; idx++)
        //        {
        //            NetInfo.Lane lane = thisInfoLanes[idx];
        //            bool thispos = lane.m_position > 0;
        //            if (pos == thispos && lane.m_stopType > VehicleInfo.VehicleType.Car)
        //            {
        //                uint laneID = theNetManager.m_segments.m_buffer[wpointB.Segment].m_lanes;
        //                for (int jdx = 0; jdx < idx && laneID != 0U; ++jdx)
        //                    laneID = theNetManager.m_lanes.m_buffer[laneID].m_nextLane;
        //                ushort laneNode = theNetManager.m_lanes.m_buffer[laneID].m_nodes;
        //                if (laneNode != 0)
        //                {
        //                    pointBprime = new Waypoint(wpointB.Segment, wpointB.Lane, 128);
        //                    return true;
        //                }
        //            }
        //        }
        //        return false;
        //    }
        //    else
        //        return false;
        //}

        // starting values for the linewidth/heatmap categories
        // cutoffs[0] is never used, it just conveniently makes the array be zero-based while
        // we in fact always use it as one-based
        public static int[] cutoffs = new int[9] { 0, 2, 4, 8, 16, 32, 64, 128, 128 };

        public static int Categorize(int rawheat)
        {
            for (int i = 1; i < 8; i++)
                if (rawheat <= cutoffs[i])
                    return i;
            return 8;
        }

        public static Color[] cutoffsColor = new Color[9] {
              Color.white,
              Color.HSVToRGB(H: 0.7f, S: 0.9f, V: 0.9f),
              Color.HSVToRGB(H: 0.6f, S: 0.9f, V: 0.9f),
              Color.HSVToRGB(H: 0.5f, S: 0.9f, V: 0.9f),
              Color.HSVToRGB(H: 0.4f, S: 0.9f, V: 0.9f),
              Color.HSVToRGB(H: 0.225f, S: 0.9f, V: 0.9f),
              Color.HSVToRGB(H: 0.15f, S: 0.9f, V: 0.9f),
              Color.HSVToRGB(H: 0.075f, S: 0.9f, V: 0.9f),
              Color.HSVToRGB(H: 0f, S: 0.9f, V: 0.9f)
            };

        public static float HalfWidthHeat(int category)
        {
                return Mathf.Max(Singleton<JourneyVisualizer>.instance.MinHalfwidth, category);                                           // max halfwidth as 8, and discretized to integer
        }

        // PathToWaypoints is a quick direct conversion of a path into a list of waypoints, leaving PT segments as PT segments (ie not converting to landroutes)
        // If you need the landroute (which you most often do) use PathToLandroute (of which this is simply a cut-down version)
        public static List<Waypoint> PathToWaypoints(uint pathID)
        {
            if (pathID == 0)
                return null;
            PathManager thePathManager = Singleton<PathManager>.instance;
            NetManager theNetManager = Singleton<NetManager>.instance;
            int loopCount = 0;
            List<Waypoint> outlist = new List<Waypoint>();
            while (pathID != 0)
            {
                PathUnit thisUnit = thePathManager.m_pathUnits.m_buffer[pathID];
                int positionCount = thisUnit.m_positionCount;
                for (int positionIndex = 0; positionIndex < positionCount; ++positionIndex)
                {
                    if (!thisUnit.GetPosition(positionIndex, out PathUnit.Position thisPathPosition))
                        return null;  // this happens occaionally. EG if a path is captured half finished, or gets modified in the sim while this loop is calculating (extremely unlikely in JV, much more likely in PV)
                    outlist.Add(new Waypoint(thisPathPosition));
                }
                pathID = thisUnit.m_nextPathUnit;
                if (++loopCount >= 262144)
                {
                    Debug.LogError("JV Error: Invalid path (quasi-infinite loop in pathmanager pathunits) detected!\n" + System.Environment.StackTrace);
                    return null;
                }
            }
            return outlist;
        }

        public static List<Waypoint> PathToLandroute(uint pathID, ushort cim)
        {
            if (pathID == 0)
                return null;
            PathManager thePathManager = Singleton<PathManager>.instance;
            NetManager theNetManager = Singleton<NetManager>.instance;
            int loopCount = 0;
            bool afloat = false;
            Landroute outroute = new Landroute();
            while (pathID != 0)
            {
                PathUnit thisUnit = thePathManager.m_pathUnits.m_buffer[pathID];
                int positionCount = thisUnit.m_positionCount;
                for (int positionIndex = 0; positionIndex < positionCount; ++positionIndex)
                {
                    if (!thisUnit.GetPosition(positionIndex, out PathUnit.Position thisPathPosition))
                        return null;  // this is extremely unlikely to happen, means pathmanager path is corrupt in some way (in PV, with live updates, it could happen that path has been changed during sim updates)
                    //ushort thisSegment = thisPathPosition.m_segment;
                    uint landpath = theNetManager.m_segments.m_buffer[thisPathPosition.m_segment].m_path;
                    if (landpath == 0)
                    {
                        outroute.Add(new Waypoint(thisPathPosition, GetNonPTTravelMode(thisPathPosition.m_segment, thisPathPosition.m_lane, cim);
                        afloat = false;
                    }
                    else
                    {
                        List<Waypoint> landroute = PathToWaypoints(landpath);
                        // if part of the path is dud, forget the whole thing
                        if (landroute == null)
                            return null;
                        // avoid duplication of the step that ends one landroute but also begins the next
                        if (afloat)
                            landroute.RemoveAt(0);
                        foreach (Waypoint wpoint in landroute)
                        {
                            outroute.AddPoint(wpoint, thisSegment);
                        }
                        afloat = true;
                    }
                }
                pathID = thisUnit.m_nextPathUnit;
                if (++loopCount >= 262144)
                {
                    Debug.LogError("JV Error: Invalid path (quasi-infinite loop in pathmanager pathunits) detected!\n" + System.Environment.StackTrace);
                    return null;
                }
            }
            return outroute;
        }

        public static List<ushort> PW2(uint pathID)
        {
            if (pathID == 0)
                return null;
            PathManager thePathManager = Singleton<PathManager>.instance;
            NetManager theNetManager = Singleton<NetManager>.instance;
            int loopCount = 0;
            List<ushort> outlist = new List<ushort>();
            while (pathID != 0)
            {
                PathUnit thisUnit = thePathManager.m_pathUnits.m_buffer[pathID];
                int positionCount = thisUnit.m_positionCount;
                for (int positionIndex = 0; positionIndex < positionCount; ++positionIndex)
                {
                    if (!thisUnit.GetPosition(positionIndex, out PathUnit.Position thisPathPosition))
                        return null;  // this could happen if a path gets modified in the sim while this loop is calculating (extremely unlikely in JV, much more likely in PV)
                    outlist.Add(thisPathPosition.m_segment);
                }
                pathID = thisUnit.m_nextPathUnit;
                if (++loopCount >= 262144)
                {
                    Debug.LogError("JV Error: Invalid path (quasi-infinite loop in pathmanager pathunits) detected!\n" + System.Environment.StackTrace);
                    return null;
                }
            }
            return outlist;
        }


        public static List<ushort> PL2(uint pathID)
        {
            if (pathID == 0)
                return null;
            PathManager thePathManager = Singleton<PathManager>.instance;
            NetManager theNetManager = Singleton<NetManager>.instance;
            int loopCount = 0;
            bool afloat = false;
            List<ushort> outroute = new List<ushort>();
            while (pathID != 0)
            {
                PathUnit thisUnit = thePathManager.m_pathUnits.m_buffer[pathID];
                int positionCount = thisUnit.m_positionCount;
                for (int positionIndex = 0; positionIndex < positionCount; ++positionIndex)
                {
                    if (!thisUnit.GetPosition(positionIndex, out PathUnit.Position thisPathPosition))
                        return null;  // this could happen if a path gets modified in the sim while this loop is calculating
                    ushort thisSegment = thisPathPosition.m_segment;
                    uint landpath = theNetManager.m_segments.m_buffer[thisSegment].m_path;
                    if (landpath == 0)
                    {
                        outroute.Add(thisSegment);
                        afloat = false;
                    }
                    else
                    {
                        List<ushort> landroute = PW2(landpath);
                        // if part of the path is dud, forget the whole thing
                        if (landroute == null)
                            return null;
                        // avoid duplication of the step that ends one landroute but also begins the next
                        if (afloat)
                            landroute.RemoveAt(0);
                        foreach (ushort wpoint in landroute)
                        {
                            outroute.Add(thisSegment);
                        }
                        afloat = true;
                    }
                }
                pathID = thisUnit.m_nextPathUnit;
                if (++loopCount >= 262144)
                {
                    Debug.LogError("JV Error: Invalid path (quasi-infinite loop in pathmanager pathunits) detected!\n" + System.Environment.StackTrace);
                    return null;
                }
            }
            return outroute;
        }

        public static string VDprint(Vector3 _vector)
        {
            return $"({_vector.x:0.0#######}, {_vector.y:0.0#######}, {_vector.z:0.0#######})";
        }

        // code for drawing building overlays is copied and adapted (a tiny bit - simplified color to one parameter) from BuldingTools
        internal static void DrawBuildingOverlay(
              RenderManager.CameraInfo cameraInfo,
              ref Building building,
              Color color)
        {
            BuildingInfo info = building.Info;
            if (info == null)
                return;
            if (info.m_circular)
            {
                ++Singleton<ToolManager>.instance.m_drawCallData.m_overlayCalls;
                Singleton<RenderManager>.instance.OverlayEffect.DrawCircle(cameraInfo, color, building.m_position, Mathf.Max(info.m_cellWidth, building.Length) * 8f, building.m_position.y - 100f, building.m_position.y + info.m_size.y + 100.0f, false, false);
            }
            else
            {
                Vector3 vector3_1 = new Vector3(Mathf.Cos(building.m_angle), 0.0f, Mathf.Sin(building.m_angle));
                Vector3 vector3_2 = new Vector3(vector3_1.z, 0.0f, -vector3_1.x);
                vector3_1 *= info.m_cellWidth * 4f;
                Vector3 vector3_3 = vector3_2 * (building.Length * 4f);
                Quad3 quad = new Quad3
                {
                    a = building.m_position - vector3_1 - vector3_3,
                    b = building.m_position - vector3_1 + vector3_3,
                    c = building.m_position + vector3_1 + vector3_3,
                    d = building.m_position + vector3_1 - vector3_3
                };
                ++Singleton<ToolManager>.instance.m_drawCallData.m_overlayCalls;
                Singleton<RenderManager>.instance.OverlayEffect.DrawQuad(cameraInfo, color, quad, building.m_position.y - 100f, building.m_position.y + info.m_size.y + 100.0f, false, false);
            }
        }

        // the following code for drawing the red (and green) selected-segment overlays is stolen directly from TM:PE

        /// <summary>
        /// similar to NetTool.RenderOverlay()
        /// but with additional control over alphaBlend.
        /// </summary>
        internal static void DrawSegmentOverlay(
            RenderManager.CameraInfo cameraInfo,
            ushort segmentId,
            Color color,
            bool alphaBlend)
        {
            if (segmentId == 0)
            {
                return;
            }

            ref NetSegment segment =
                ref Singleton<NetManager>.instance.m_segments.m_buffer[segmentId];
            float width = segment.Info.m_halfWidth;

            NetNode[] nodeBuffer = Singleton<NetManager>.instance.m_nodes.m_buffer;

            bool IsMiddle(ushort nodeId) =>
                (nodeBuffer[nodeId].m_flags & NetNode.Flags.Middle) != 0;

            Bezier3 bezier;
            bezier.a = segment.m_startNode.ToNode().GetPositionOnTerrain();
            bezier.d = segment.m_endNode.ToNode().GetPositionOnTerrain();

            NetSegment.CalculateMiddlePoints(
                startPos: bezier.a,
                startDir: segment.m_startDirection,
                endPos: bezier.d,
                endDir: segment.m_endDirection,
                smoothStart: IsMiddle(segment.m_startNode),
                smoothEnd: IsMiddle(segment.m_endNode),
                middlePos1: out bezier.b,
                middlePos2: out bezier.c);

            Singleton<ToolManager>.instance.m_drawCallData.m_overlayCalls++;
            Singleton<RenderManager>.instance.OverlayEffect.DrawBezier(
                cameraInfo,
                color,
                bezier,
                size: width * 2f,
                cutStart: 0,
                cutEnd: 0,
                minY: -1f,
                maxY: 1280f,
                renderLimits: false,
                alphaBlend);
        }
        internal static Vector3 GetPositionOnTerrain(this ref NetNode node)
        {
            Vector3 pos = node.m_position;
            float terrainY = Singleton<TerrainManager>.instance.SampleDetailHeightSmooth(pos);
            if (terrainY > pos.y)
            {
                pos.y = terrainY;
            }

            return pos;
        }

        // embryo code for lane overlay (from TMPE). AFAIK bezier is simply the bezier of the netlane without adjustment
        ///// <summary>Renders lane overlay.</summary>
        //internal void RenderOverlay(RenderManager.CameraInfo cameraInfo, Color color, bool enlarge = false, bool renderLimits = false)
        //{
        //    float minH = Mathf.Min(Bezier.a.y, Bezier.d.y);
        //    float maxH = Mathf.Max(Bezier.a.y, Bezier.d.y);

        //    float overdrawHeight = IsUnderground || renderLimits ? 0f : 5f;
        //    ColossalFramework.Singleton<ToolManager>.instance.m_drawCallData.m_overlayCalls++;
        //    RenderManager.instance.OverlayEffect.DrawBezier(
        //        cameraInfo: cameraInfo,
        //        color: color,
        //        bezier: Bezier,
        //        size: enlarge ? Size * 1.41f : Size,
        //        cutStart: 0,
        //        cutEnd: 0,
        //        minY: minH - overdrawHeight,
        //        maxY: maxH + overdrawHeight,
        //        renderLimits: IsUnderground || renderLimits,
        //        alphaBlend: false);
        //}

    }

    public static class NetNodeExtensions
    {
        private static NetNode[] _nodeBuffer = Singleton<NetManager>.instance.m_nodes.m_buffer;

        internal static ref NetNode ToNode(this ushort nodeId) => ref _nodeBuffer[nodeId];
    }

    // the following code is stolen directly from an older version of IPT2

    public static class UIUtils
    {
        private static UIFont _font;

        public static UIFont Font
        {
            get
            {
                if ((Object)UIUtils._font == (Object)null)
                    UIUtils._font = GameObject.Find("(Library) PublicTransportInfoViewPanel").GetComponent<PublicTransportInfoViewPanel>().Find<UILabel>("Label").font;
                return UIUtils._font;
            }
        }

        public static bool IsFullyClippedFromParent(UIComponent component)
        {
            if ((Object)component.parent == (Object)null || (Object)component.parent == (Object)component)
                return false;
            UIScrollablePanel parent = component.parent as UIScrollablePanel;
            return (Object)parent != (Object)null && parent.clipChildren && ((double)component.relativePosition.x < 0.0 - (double)component.size.x - 1.0 || (double)component.relativePosition.x + (double)component.size.x > (double)component.parent.size.x + (double)component.size.x + 1.0 || ((double)component.relativePosition.y < 0.0 - (double)component.size.y - 1.0 || (double)component.relativePosition.y + (double)component.size.y > (double)component.parent.size.y + (double)component.size.y + 1.0));
        }

        public static UIButton CreateButton(UIComponent parent)
        {
            UIButton uiButton = parent.AddUIComponent<UIButton>();
            UIFont font = UIUtils.Font;
            uiButton.font = font;
            RectOffset rectOffset = new RectOffset(0, 0, 4, 0);
            uiButton.textPadding = rectOffset;
            string str1 = "ButtonMenu";
            uiButton.normalBgSprite = str1;
            string str2 = "ButtonMenuDisabled";
            uiButton.disabledBgSprite = str2;
            string str3 = "ButtonMenuHovered";
            uiButton.hoveredBgSprite = str3;
            string str4 = "ButtonMenu";
            uiButton.focusedBgSprite = str4;
            string str5 = "ButtonMenuPressed";
            uiButton.pressedBgSprite = str5;
            Color32 color32_1 = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
            uiButton.textColor = color32_1;
            Color32 color32_2 = new Color32((byte)7, (byte)7, (byte)7, byte.MaxValue);
            uiButton.disabledTextColor = color32_2;
            Color32 color32_3 = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
            uiButton.hoveredTextColor = color32_3;
            Color32 color32_4 = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
            uiButton.focusedTextColor = color32_4;
            Color32 color32_5 = new Color32((byte)30, (byte)30, (byte)44, byte.MaxValue);
            uiButton.pressedTextColor = color32_5;
            return uiButton;
        }

        public static UICheckBox CreateCheckBox(UIComponent parent)
        {
            UICheckBox uiCheckBox = parent.AddUIComponent<UICheckBox>();
            Vector2 size = parent.size;
            uiCheckBox.size = size;
            int num = 1;
            uiCheckBox.clipChildren = num != 0;
            UISprite uiSprite1 = uiCheckBox.AddUIComponent<UISprite>();
            uiSprite1.spriteName = "check-unchecked";
            uiSprite1.size = new Vector2(16f, 16f);
            uiSprite1.relativePosition = Vector3.zero;
            UISprite uiSprite2 = uiSprite1.AddUIComponent<UISprite>();
            uiCheckBox.checkedBoxObject = (UIComponent)uiSprite2;
            ((UISprite)uiCheckBox.checkedBoxObject).spriteName = "check-checked";
            uiCheckBox.checkedBoxObject.size = new Vector2(16f, 16f);
            uiCheckBox.checkedBoxObject.relativePosition = Vector3.zero;
            UILabel uiLabel = uiCheckBox.AddUIComponent<UILabel>();
            uiCheckBox.label = uiLabel;
            uiCheckBox.label.font = UIUtils.Font;
            uiCheckBox.label.textColor = (Color32)Color.white;
            uiCheckBox.label.textScale = 0.8f;
            uiCheckBox.label.relativePosition = new Vector3(22f, 2f);
            return uiCheckBox;
        }
    }
}
