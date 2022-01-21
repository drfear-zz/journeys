using ColossalFramework;
using ColossalFramework.Math;
using System.Collections.Generic;
using UnityEngine;
using Journeys.RedirectionFramework.Attributes;


namespace Journeys
{

    [TargetType(typeof(PathVisualizer))]


    //
    // ************************** Journey objects: each Journey contains an array of subJourneys, each of which is an array of journeySteps
    //   a Journey is potentially longer than a PV PathUnit, it is basically a concatenation of the linked list of PathUnits 
    //        that constitute a journey from begin to end
    //   a SubJourney is a close equivalent of a PV PathUnit, but along all of which the same mode of transport is used
    //        unlike a PathUnit I use a dynamic fastlist to store as many JourneySteps as needed (PV hardcodes 12 positions and uses a link list for the next 12 if needed) 
    //        in JV the next part of the journey is simply the next item in the Journey's subJourney list, no linked listing required
    //   a JourneyStep is the equivalent of a PV path position, ie contains a single segment with its nodes and other info
    //



    public class Journey
    {
        // the indexer of the m_journeys Dictionary is InstanceID m_id, which in JV is necessarily a citizenInstance.
        // when a journey is first created (in AddInstances) it contains only m_id, m_refreshRequired and m_stillNeeded
        // all the rest of the members derive from the knowledge of m_id via its associated pathmanager path
        public InstanceID m_id;
        public List<ushort> m_steps;  // the journey encapsulated as a list of its JourneySteps (as reference indices to the JourneyStepMgr dictionary of JourneySteps)
        public bool m_refreshRequired;
        public bool m_stillNeeded;
        public bool m_canRelease;
        public uint m_pathUnit;  // we need to keep track of linking to pathmanager paths even though we don't directly use them again (see StepJourneys comments why)
        public uint m_nextPathUnit;  // it is so we can tell if the simulation's pathmanager path changed to a completely new path, or whether it just stepped along the chain
        public float m_arrowOffset;     // a quasi random number between 1 and 1.25 used to multiply offset of arrowheads on rendered journeys so there is an arrowhead per citizen

        // construct a skeleton journey (called while adding instances to the dictionary in AddInstance)
        public Journey(InstanceID id, bool refreshRequired, bool stillNeeded)
        {
            m_id = id;
            m_steps = new List<ushort>();
            m_arrowOffset = 1f + (id.CitizenInstance / 524280);
            m_refreshRequired = refreshRequired;
            m_stillNeeded = stillNeeded;
            m_pathUnit = 0;
            m_nextPathUnit = 0;
        }

        // for debugging
        //public void Dprint()
        //{
        //    Debug.Log(" -- Journey for citizenInstance ID " + m_id.CitizenInstance + " -- \n" +
        //        "m_pathUnit: " + m_pathUnit + "\n" +
        //        "m_nextPathUnit: " + m_nextPathUnit + "\n" +
        //        "m_refreshRequired" + m_refreshRequired + "\n" +
        //        "m_stillNeeded: " + m_stillNeeded + "\n" +
        //        m_subJourneys.Count + " subjourneys as follows");
        //    for (int index = 0; index < m_subJourneys.Count; index++)
        //    {
        //        Debug.Log("SubJourney " + index + ":");
        //        m_subJourneys[index].Dprint();
        //    }
        //}

        // Fill a Journey calculated by unravelling a path (ie from a starting PathUnit) into same-transport SubJourneys
        // setting the relevant metadata (line ID, color, material, etc etc) as we go
        //
        // when paths split to subjourneys, AB travels from step A to step B using B's journey type
        // eg for () denoting line changes, A(BC)(DE)F travel AB on B's line, then BC on C's line (but is same line, so like AC on C's line)
        // then from C to D (then E) on D's line, then E to F on F's line
        // the subjourneys here are ABC (on B line), CDE (on D line), EF (on F line), ie the subjourney end needs to be repeated as
        // the beginning of the next subjourney, then travel is on the line/Color of the 2nd component (which is seen as when line/color changes)
        //
        // furthermore, for a step AB made on public transport, it is converted to step(s) A' to B' (along normal map nodes rather then PT nodes)
        // this is checked and performed if necessary in AddStep

        // NOTE - because of the desire for a heatmap, I am almost completely breaking the original concept now.  Actually every segment is a subjourney,
        // in the old sense; except for public transport steps for which the subjourney is a set of map segments substituted for original single transport segment
        // I PROBABLY NEED TO REWRITE THIS WHOLE CLASS LIBRARY NOW?  for the time being left Classes as were, but note, nearly every Subjourney has only one JourneyStep

        public bool SetFromPath(uint pathID)
        {
            Debug.Log("Entered SetFromPath with pathID " + pathID);
            if (pathID == 0)
                return false;
            // I believe Journeys don't really need to track paths like PV does, but for now leaving this mechanism intact
            PathManager thePathManager = Singleton<PathManager>.instance;
            NetManager theNetManager = Singleton<NetManager>.instance;
            m_pathUnit = pathID;
            m_nextPathUnit = thePathManager.m_pathUnits.m_buffer[pathID].m_nextPathUnit;
            m_steps.Clear();  // failsafe, it should have been newly created anyway

            // here we go with the journey calculation itself
            List<Waypoint> itinerary = PathToWaypoints(pathID);
            if (itinerary == null)
                return false;
            List<Waypoint> route = new List<Waypoint>(); // working list of waypoints for each step
            List<Waypoint> landroute = new List<Waypoint>(); // working list of waypoints on land for ptrans steps
            Waypoint pointA = null;
            LineColorPair lineColorA;
            bool onlandA = true;
            Waypoint pointB = null;
            LineColorPair lineColorB;
            bool onlandB = true;
            bool firstStep = true;

            int itinIdx = 0;
            int itinLast = itinerary.Count - 1;
            while (true)
            {
                if (itinIdx == itinLast)
                    break;
                if (firstStep)
                {
                    pointA = itinerary[itinIdx];
                    lineColorA = new LineColorPair(pointA);
                    onlandA = lineColorA.m_travelmode < 32;
                    firstStep = false;
                }
                else
                {
                    if (onlandB)
                    {
                        pointA = pointB;
                        onlandA = true;
                    }
                }
                pointB = itinerary[++itinIdx];
                lineColorB = new LineColorPair(pointB);
                onlandB = lineColorB.m_travelmode < 32;
                if (onlandB)
                {
                    route.Add(pointA);
                    route.Add(pointB);
                    m_steps.Add(JourneyStepMgr.instance.Augment(route, m_id.CitizenInstance, lineColorB, itinIdx == itinLast));
                    route.Clear();
                }
                else
                {
                    landroute = PathToWaypoints(theNetManager.m_segments.m_buffer[pointB.Segment].m_path);
                    if (onlandA)
                    {
                        route.Add(pointA);
                        route.Add(landroute[0]);
                        m_steps.Add(JourneyStepMgr.instance.Augment(route, m_id.CitizenInstance, lineColorB, itinIdx == itinLast));
                        route.Clear();
                    }
                    m_steps.Add(JourneyStepMgr.instance.Augment(landroute, m_id.CitizenInstance, lineColorB, itinIdx == itinLast));
                    pointA = landroute[landroute.Count - 1];
                    onlandA = false;
                }
            }
            //Debug.Log("Exiting SetFromPath with journey as follows");
            //this.Dprint();
            string str = "";
            foreach (ushort i in m_steps)
            {
                str = str + i + ", ";
            }
            Debug.Log("m_steps for cim " + m_id + "constructed as: " + str);
            return true;
        }

        private List<Waypoint> PathToWaypoints(uint pathID)
        {
            if (pathID == 0)
                return null;
            PathManager thePathManager = Singleton<PathManager>.instance;
            byte pathFindFlags = thePathManager.m_pathUnits.m_buffer[pathID].m_pathFindFlags;
            if ((pathFindFlags & 4) == 0)
                return null;
            int loopCount = 0;
            List<Waypoint> outlist = new List<Waypoint>();
            while (pathID != 0)
            {
                PathUnit thisUnit = thePathManager.m_pathUnits.m_buffer[pathID];
                int positionCount = thisUnit.m_positionCount;
                for (int positionIndex = 0; positionIndex < positionCount; ++positionIndex)
                {
                    if (!thisUnit.GetPosition(positionIndex, out PathUnit.Position thisPathPosition))
                        return null;  // this could happen if a path gets modified in the sim while this loop is calculating
                    outlist.Add(new Waypoint(thisPathPosition));
                }
                pathID = thisUnit.m_nextPathUnit;
                if (++loopCount >= 262144)
                {
                    Debug.LogError("Invalid path detected!\n" + System.Environment.StackTrace);
                    return null;
                }
            }
            return outlist;
        }
    }

    public struct LineColorPair
    {
        public int m_travelmode;
        public Color m_lineColor;

        public LineColorPair(Waypoint waypoint)
        {
            NetManager theNetManager = Singleton<NetManager>.instance;
            TransportManager theTransportManager = Singleton<TransportManager>.instance;
            NetSegment netSegment = theNetManager.m_segments.m_buffer[waypoint.Segment];
            NetInfo thisSegmentInfo = netSegment.Info;
            if (thisSegmentInfo == null || thisSegmentInfo.m_lanes == null || (thisSegmentInfo.m_lanes.Length <= waypoint.Lane))
            {
                m_travelmode = 0;
                m_lineColor = Singleton<JourneysPanel>.instance.m_travelModeColors[0];
            }
            else
            {
                NetInfo.LaneType theLaneType = thisSegmentInfo.m_lanes[waypoint.Lane].m_laneType;
                switch (theLaneType)
                {
                    case NetInfo.LaneType.Pedestrian:
                        m_travelmode = 2;
                        m_lineColor = Singleton<JourneysPanel>.instance.m_travelModeColors[2];
                        break;
                    case NetInfo.LaneType.PublicTransport:
                        int line = theNetManager.m_nodes.m_buffer[netSegment.m_startNode].m_transportLine;
                        m_lineColor = line == 0 ? Color.black : theTransportManager.m_lines.m_buffer[line].GetColor();
                        m_travelmode = 32 + line;
                        break;
                    case NetInfo.LaneType.Vehicle:
                        if (thisSegmentInfo.m_lanes[waypoint.Lane].m_vehicleType == VehicleInfo.VehicleType.Bicycle)
                        {
                            m_travelmode = 3;
                            m_lineColor = Singleton<JourneysPanel>.instance.m_travelModeColors[3];
                        }
                        else
                        {
                            m_travelmode = 1;
                            m_lineColor = Singleton<JourneysPanel>.instance.m_travelModeColors[1];
                        }
                        break;
                    default:
                        m_travelmode = 4;
                        m_lineColor = Singleton<JourneysPanel>.instance.m_travelModeColors[4];
                        break;
                }
            }

        }
    }
}

        //public class SubJourney
        //{
        //    public List<JourneyStep> m_journeySteps;
        //    // public TransportInfo.TransportType m_transportType = TransportInfo.TransportType.Pedestrian; // I probably don't need this if I have lineID and color, although I will need the right Material
        //    //public ushort m_lineID;
        //    //public Color m_color;
        //    public LineColorPair m_lineColorPair;
        //    public RenderGroup.MeshData[] m_meshData;
        //    public Bezier3[] m_lineCurves;
        //    public Vector2[] m_curveOffsets;
        //    public Mesh[] m_meshes;
        //    public Material m_material;
        //    public Material m_material2;
        //    public bool m_requireSurfaceLine;
        //    public float m_pathOffset;
        //    public int m_layer;
        //    public int m_layer2;
        //    public int m_curveIndex;
        //    public int m_startCurveIndex;

        //    // for debugging
        //    public void Dprint()
        //    {
        //        string meshDataStatus;
        //        string meshesStatus;
        //        if (m_meshData == null)
        //            meshDataStatus = "\nm_meshData is null";
        //        else
        //        {
        //            meshDataStatus = "\nm_meshData length: " + m_meshData.Length;
        //        }
        //        if (m_meshes == null)
        //        {
        //            meshesStatus = "\nm_meshes is null";
        //        }
        //        else
        //        {
        //            meshesStatus = "\nm_meshes length: " + m_meshes.Length;
        //        }
        //        Debug.Log("\nm_lineID: " + m_lineColorPair.m_travelmode +
        //            "\ncolor: " + m_lineColorPair.m_lineColor.ToString() +
        //            "\nm_requireSufaceLine: " + m_requireSurfaceLine +
        //            "\nm_layer: " + m_layer +
        //            "\nm_layer2: " + m_layer2 + meshDataStatus + meshesStatus +
        //            "\n" + m_journeySteps.Count + " m_journeySteps as follows");
        //        foreach (var jstep in m_journeySteps)
        //        {
        //            jstep.Dprint();
        //        }
        //    }

        //    public SubJourney(JourneyStep jStep)
        //    {
        //        var newlist = new List<JourneyStep>();
        //        newlist.Add(jStep);
        //        m_journeySteps = newlist;
        //    }

        //    public JourneyStep GetLatestStep()
        //    {
        //        return m_journeySteps[m_journeySteps.Count - 1];
        //    }

        //    public void SetLineColor(LineColorPair lineColorPair)
        //    {
        //        m_lineColorPair = lineColorPair;
        //    }

        //    public void SetMaterials()
        //    {
        //        TransportInfo transportInfo = Singleton<TransportManager>.instance.GetTransportInfo(TransportInfo.TransportType.Metro); // hope this will fix going through buildings
        //        if (transportInfo != null)
        //        {
        //            m_material = transportInfo.m_lineMaterial2;
        //            m_material2 = transportInfo.m_secondaryLineMaterial2;
        //            m_requireSurfaceLine = transportInfo.m_requireSurfaceLine; // have experimented with just setting it directly - I COULD NOT SEE ANY DIFFERENCE AT ALL if false, but everything invisible if true (in comb with layer?)
        //            m_layer = transportInfo.m_prefabDataLayer; // transportInfo.m_secondaryLayer; experiment draw everything at secondary layer (I think/hope this means underground) - but no, draws nothing at all
        //            m_layer2 = transportInfo.m_secondaryLayer; // experiment draw everything at prefab layer - MADE NO DIFFERENCE
        //        }
        //    }


        //    public void UpdateMesh()
        //    {
        //        // function is only called when m_meshdata is not null; this meshData is read into m_meshes and then m_meshdata is cleared
        //        // (which is presumably so it does not keep redrawing the same meshes, m_meshdata!=null is a flag for needing to (re)draw)
        //        RenderGroup.MeshData[] meshData = m_meshData;
        //        m_curveIndex = m_startCurveIndex;   // which sets it to zero
        //        m_meshData = null;
        //        if (meshData == null)
        //            return;

        //        int mmeshesLengthOnCall;
        //        if (m_meshes != null)
        //        {
        //            mmeshesLengthOnCall = m_meshes.Length;
        //        }
        //        else
        //        {
        //            mmeshesLengthOnCall = 0;
        //        }
        //        int meshDataLength = meshData.Length;
        //        if (mmeshesLengthOnCall != meshDataLength)
        //        {
        //            Mesh[] meshArray2 = new Mesh[meshDataLength];
        //            int smallestLength = Mathf.Min(mmeshesLengthOnCall, meshDataLength);
        //            for (int index = 0; index < smallestLength; ++index)
        //                meshArray2[index] = m_meshes[index];
        //            for (int index = smallestLength; index < meshDataLength; ++index)
        //                meshArray2[index] = new Mesh();
        //            for (int index = smallestLength; index < mmeshesLengthOnCall; ++index)
        //                UnityEngine.Object.Destroy(m_meshes[index]);
        //            m_meshes = meshArray2;
        //        }
        //        // what we have just done is assign a dynamically sized array of meshes (to meshdataLength) to m_meshes - the content
        //        // is irrelevant and will now be overwritten per meshData
        //        for (int index = 0; index < meshDataLength; ++index)
        //        {
        //            m_meshes[index].Clear();
        //            m_meshes[index].vertices = meshData[index].m_vertices;
        //            m_meshes[index].normals = meshData[index].m_normals;
        //            m_meshes[index].tangents = meshData[index].m_tangents;
        //            m_meshes[index].uv = meshData[index].m_uvs;
        //            m_meshes[index].uv2 = meshData[index].m_uvs2;
        //            m_meshes[index].colors32 = meshData[index].m_colors;
        //            m_meshes[index].triangles = meshData[index].m_triangles;
        //            m_meshes[index].bounds = meshData[index].m_bounds;
        //        }
        //    }


        // really strictly speaking I don't need JourneyStep, I could just use PathUnit.Position objects, but I like more self-contained and possibly I might change them one day
        //public struct JourneyStep
        //{
        //    public ushort m_segment;
        //    public byte m_offset;
        //    public byte m_lane;

        //    // for debugging
        //    public void Dprint()
        //    {
        //        Debug.Log("\nm_segment: " + m_segment + ", m_offset: " + m_offset + ", m_lane: " + m_lane);
        //    }

        //    public JourneyStep(PathUnit.Position pathPosition)
        //    {
        //        m_segment = pathPosition.m_segment;
        //        m_offset = pathPosition.m_offset;
        //        m_lane = pathPosition.m_lane;
        //    }
        //}
        //    }
        //}