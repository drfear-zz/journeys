using ColossalFramework;
using ColossalFramework.Math;
using System.Collections.Generic;
using UnityEngine;
using Journeys.RedirectionFramework.Attributes;


namespace Journeys
{

    //
    // ************************** Journey objects: each Journey contains a list of ushort references to its JourneySteps
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
        public ushort m_id;
        public List<ushort> m_steps;  // the journey encapsulated as a list of its JourneySteps (as reference indices to the JourneyStepMgr dictionary of JourneySteps)

        public Journey(ushort citizenID)
        {
            if (citizenID == 0)
                return;
            m_id = citizenID;
            m_steps = new List<ushort>();
            uint pathID = Singleton<CitizenManager>.instance.m_instances.m_buffer[citizenID].m_path;
            if (pathID == 0)
                return;
            // here we go with the journey calculation itself
            JourneyVisualizer theJourneyVisualizer = Singleton<JourneyVisualizer>.instance;
            JourneyStepMgr theStepManager = theJourneyVisualizer.theStepManager;
            List<Waypoint> itinerary = theJourneyVisualizer.PathToWaypoints(pathID);
            if (itinerary == null || itinerary.Count < 2)
                return;
            List<Waypoint> route = new List<Waypoint>(); // working list of waypoints for each step
            List<Waypoint> landroute; // working list of waypoints on land for ptrans steps
            Waypoint pointA = null;
            LineColorPair lineColorA;
            bool onlandA = true;
            Waypoint pointB = null;
            LineColorPair lineColorB;
            bool onlandB = true;
            bool firstStep = true;

            int itinIdx = 0;
            int itinLast = itinerary.Count - 1;
            while (itinIdx < itinLast)
            {
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
                    m_steps.Add(theStepManager.Augment(route, m_id, lineColorB, itinIdx == itinLast));
                    route.Clear();
                }
                else
                {
                    landroute = theJourneyVisualizer.PathToWaypoints(Singleton<NetManager>.instance.m_segments.m_buffer[pointB.Segment].m_path);
                    if (landroute == null || landroute.Count < 2)
                    {
                        // connect with a straight line in the event of a landroute failure
                        route.Add(pointA);
                        route.Add(pointB);
                        m_steps.Add(theStepManager.Augment(route, m_id, lineColorB, itinIdx == itinLast));
                        route.Clear();
                        pointA = pointB;
                        onlandA = false;
                    }
                    else
                    {
                        if (onlandA)
                        {
                            route.Add(pointA);
                            route.Add(landroute[0]);
                            m_steps.Add(theStepManager.Augment(route, m_id, lineColorB, itinIdx == itinLast));
                            route.Clear();
                        }
                        m_steps.Add(theStepManager.Augment(landroute, m_id, lineColorB, itinIdx == itinLast));
                        pointA = landroute[landroute.Count - 1];
                        onlandA = false;
                    }
                }
            }
            //Debug.Log("Exiting SetFromPath with journey as follows");
            //this.Dprint();
            //string str = "";
            //foreach (ushort i in m_steps)
            //{
            //    str = str + i + ", ";
            //}
            //Debug.Log("m_steps for cim " + m_id + "constructed as: " + str);
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
            //JourneysPanel JVutils = Singleton<JourneysPanel>.instance;
            NetSegment netSegment = theNetManager.m_segments.m_buffer[waypoint.Segment];
            NetInfo thisSegmentInfo = netSegment.Info;
            if (thisSegmentInfo == null || thisSegmentInfo.m_lanes == null || (thisSegmentInfo.m_lanes.Length <= waypoint.Lane))
            {
                m_travelmode = 0;
                m_lineColor = JVutils.m_travelModeColors[0];
            }
            else
            {
                NetInfo.LaneType theLaneType = thisSegmentInfo.m_lanes[waypoint.Lane].m_laneType;
                switch (theLaneType)
                {
                    case NetInfo.LaneType.Pedestrian:
                        m_travelmode = 2;
                        m_lineColor = JVutils.m_travelModeColors[2];
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
                            m_lineColor = JVutils.m_travelModeColors[3];
                        }
                        else
                        {
                            m_travelmode = 1;
                            m_lineColor = JVutils.m_travelModeColors[1];
                        }
                        break;
                    default:
                        m_travelmode = 4;
                        m_lineColor = JVutils.m_travelModeColors[4];
                        break;
                }
            }

        }
    }
}

