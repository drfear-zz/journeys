using ColossalFramework;
using ColossalFramework.Math;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
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

        // temporary for bug fix
        public List<Waypoint> checklandroute;

        public Journey()
        {
            m_id = 0;
            m_steps = new List<ushort>();
        }

        public void SetJourney(ushort citizenID, Landroute itinerary)
        {
            if (citizenID == 0 || itinerary == null || itinerary.m_landpoints == null || itinerary.m_landpoints.Count < 2)
                return;
            m_id = citizenID;
            JourneyVisualizer theJourneyVisualizer = Singleton<JourneyVisualizer>.instance;
            JourneyStepMgr theStepManager = theJourneyVisualizer.theStepManager;

            //checklandroute = JVutils.PathToWaypoints(pathID, fullroute: true);
            //Debug.Log("for citizen " + m_id + "overland itinerary is\n" + this.Printlist(checklandroute));

            bool showPTstops = theJourneyVisualizer.ShowPTStops;

            // itinIdx and itinLast are just for setting end journey, not needed for foreach control
            Landpoint first = itinerary.m_landpoints[0];
            Waypoint pointA = first.m_waypoint;
            bool onlandA = first.m_pathSegment == 0;
            LineColorPair lineColorA;
            if (onlandA)
                lineColorA = new LineColorPair(pointA, m_id);     // will only be used for journeys starting on transport (ie never?) but is difficult to deal with this not being assigned to something
            else
                lineColorA = new LineColorPair(first.m_pathSegment);
            Waypoint pointB;
            bool onlandB;
            LineColorPair lineColorB;
            itinerary.KillFirst();
            int itinIdx = 1;
            int itinLast = itinerary.m_landpoints.Count;
            foreach (Landpoint lpoint in itinerary.m_landpoints)
            {
                itinIdx++;
                pointB = lpoint.m_waypoint;
                onlandB = lpoint.m_pathSegment == 0;
                if (onlandB)
                    lineColorB = new LineColorPair(pointB, m_id);
                else if (onlandA)
                    lineColorB = new LineColorPair(lpoint.m_pathSegment);
                else
                    lineColorB = lineColorA;
                m_steps.AddRange(theStepManager.Augment(pointA, pointB, m_id, lineColorB, endJourney: itinIdx == itinLast));
                pointA = pointB;
                onlandA = onlandB;
                lineColorA = lineColorB;
            }
        }
    }

    public class Journey2
    {
        // the indexer of the m_journeys Dictionary is a citizenInstanceID, but a Journey also knows its own id (as m_cim)
        public ushort m_cim;
        public List<ushort> m_steps;  // the journey encapsulated as a list of its JourneySteps (as reference indices to the JourneyStepMgr dictionary of JourneySteps)
        public List<Waypoint> m_rawRoute;     //the journey encapsulated as a list of raw Waypoints (raw = pathmanager path except PathPositions transformed to Waypoints, transport segs are not expanded to landroute)

        public Journey2()
        {
            m_cim = 0;
            m_steps = new List<ushort>();
            m_rawRoute = new List<Waypoint>();
        }

        public Journey2(List<Waypoint> rawroute, ushort cim)
        {
            m_cim = cim;
            m_steps = new List<ushort>();
            m_rawRoute = new List<Waypoint>(rawroute);
        }

        public void SetSteps()
        {
            if (m_rawRoute == null || m_rawRoute.Count < 2)
            {
                Debug.LogError("JV Error: called SetSteps with deficient m_rawRoute");      // code should have already checked route is OK before creating the journey
                return;
            }
            TransportManager theTransportManager = Singleton<TransportManager>.instance;
            JourneyVisualizer theJourneyVisualizer = Singleton<JourneyVisualizer>.instance;
            JourneyStepMgr theStepManager = theJourneyVisualizer.theStepManager;

            bool showPTstops = theJourneyVisualizer.ShowPTStops;

            // itinIdx and itinLast are just for setting end journey, not needed for foreach control
            int itinIdx = 1;
            int itinLast = m_rawRoute.Count;
            // the initial pointA is only used in the loop if pointB is on PT, but we need to set default values to keep the compiler quiet
            Waypoint pointA = m_rawRoute.First();
            JourneyVisualizer.SegWay segwayA = theJourneyVisualizer.m_segways[pointA.Segment];
            int travelModeA = 0;
            Color colourA = JVutils.m_travelModeColors[0];
            bool onlandA = !segwayA.IsTransport;
            if (!onlandA)
            {
                travelModeA = 32 + segwayA.m_line;
                colourA = segwayA.m_line == 0 ? Color.black : theTransportManager.GetLineColor(segwayA.m_line); // I used to use .GetColor() but for line selections that can catch it while it is flashing
                pointA = segwayA.m_landroute.First();
                foreach (Waypoint pointB in segwayA.m_landroute.Skip(1))
                {
                    m_steps.AddRange(theStepManager.Augment(pointA, pointB, m_cim, travelModeA, colourA, endJourney: itinIdx == itinLast));
                    pointA = pointB;
                }
            }
            int travelModeB;
            Color colourB;
            foreach (Waypoint pointB in m_rawRoute.Skip(1))
            {
                itinIdx++;
                JourneyVisualizer.SegWay segwayB = theJourneyVisualizer.m_segways[pointB.Segment];
                bool onlandB = !segwayB.IsTransport;
                if (onlandB)
                {
                    travelModeB = GetNonPTTravelMode(pointB);
                    colourB = JVutils.m_travelModeColors[travelModeB];
                    m_steps.AddRange(theStepManager.Augment(pointA, pointB, m_cim, travelModeB, colourB, endJourney: itinIdx == itinLast));
                    pointA = pointB;
                }
                else if (onlandA)
                {
                    // here if we are starting a journey on PT
                    travelModeB = 32 + segwayB.m_line;
                    colourB = segwayB.m_line == 0 ? Color.black : theTransportManager.GetLineColor(segwayB.m_line);
                    foreach (Waypoint pointC in segwayB.m_landroute)
                    {
                        m_steps.AddRange(theStepManager.Augment(pointA, pointC, m_cim, travelModeB, colourB, endJourney: itinIdx == itinLast));
                        pointA = pointC;
                    }
                }
                else
                {
                    // here if we are continuing a journey on PT
                    travelModeB = travelModeA;
                    colourB = colourA;
                    foreach (Waypoint pointC in segwayB.m_landroute.Skip(1))
                    {
                        m_steps.AddRange(theStepManager.Augment(pointA, pointC, m_cim, travelModeB, colourB, endJourney: itinIdx == itinLast));
                        pointA = pointC;
                    }
                }
                onlandA = onlandB;
                travelModeA = travelModeB;
                colourA = colourB;
            }
        }

        public int GetNonPTTravelMode(Waypoint waypoint, ushort cim = 0)
        {
            NetManager theNetManager = Singleton<NetManager>.instance;

            NetSegment netSegment = theNetManager.m_segments.m_buffer[waypoint.Segment];
            NetInfo thisSegmentInfo = netSegment.Info;
            if (thisSegmentInfo == null || thisSegmentInfo.m_lanes == null || (thisSegmentInfo.m_lanes.Length <= waypoint.Lane))
                return 0;
            NetInfo.LaneType theLaneType = thisSegmentInfo.m_lanes[waypoint.Lane].m_laneType;
            switch (theLaneType)
            {
                case NetInfo.LaneType.Pedestrian:
                    if (cim == 0)                               // called with cim=0 if you do not care about bikes, and/or if there is no specific citizen relevant
                        return 2;
                    int retval = 2;
                    // check if on a bike (strictly speaking = at the moment) - if so, it is considered ALL their pedestrian steps are on bicycle
                    CitizenManager theCitizenManager = Singleton<CitizenManager>.instance;
                    uint cit = theCitizenManager.m_instances.m_buffer[cim].m_citizen;
                    ushort vehicle = theCitizenManager.m_citizens.m_buffer[cit].m_vehicle;
                    if (Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicle].Info.m_vehicleType == VehicleInfo.VehicleType.Bicycle)
                        retval = 3;
                    return retval;
                // case NetInfo.LaneType.PublicTransport: - cannot happen for non-transport segments, we would already know IsTransport and m_line, and not call this function
                case NetInfo.LaneType.Vehicle:
                    // A vehicle on a bicycle lane is ipso facto a bicycle, else vehicle must be a car (no other types occur in JV cos there are no citizens in any other nonpublic road transport)
                    if (thisSegmentInfo.m_lanes[waypoint.Lane].m_vehicleType == VehicleInfo.VehicleType.Bicycle)
                        return 3;
                    else
                        return 1;
                default:
                    return 4;
            }
        }

        public void DebugJourney()
        {
            JourneyVisualizer theJV = Singleton<JourneyVisualizer>.instance;
            JourneyStepMgr theStepManager = theJV.theStepManager;
            string ans = "Full journey for citizenInstance " + m_cim + ":\n";
            foreach (ushort idx in m_steps)
                ans = ans + theStepManager.GetStep(idx).Hashname + ",\n";
            ans = ans + "Masked journey for citizenInstance " + m_cim + ":\n";
            foreach (ushort stepIdx in theJV.MaskPT(theJV.MaskFromTo(m_steps, m_cim), m_cim))
                ans = ans + theStepManager.GetStep(stepIdx).Hashname + "\n";
            ans = ans + "FromToFlag is " + theJV.FromToFlag + ", OnlyPTStretches is " + theJV.OnlyPTstretches + "\n";
            Debug.Log(ans);
        }

    }

    // a "full route" aka "landroute" is every waypoint on the overland journey (that is: with pathunit transport segments converted to a list of map segments)
    // The Landroute object has this route plus parallel info on the original path segments generating each waypoint
    // This pathSegment is 0 if the landroute and path segments are the same, or for PT steps it is the original transport segment (which holds the landroute as its m_path)
    // Hence pathSegment can also be used as a binary flag (0 = not on PT, nonzero = on PT) if the segment info is not needed per se

    public struct Landpoint
    {
        public Waypoint m_waypoint;
        public ushort m_pathSegment;

        public Landpoint(Waypoint newWaypoint, ushort newPathSegment) : this()
        {
            m_waypoint = newWaypoint;
            m_pathSegment = newPathSegment;
        }
    }

    public class Landroute
    {
        public List<Landpoint> m_landpoints;

        public Landroute()
        {
            m_landpoints = new List<Landpoint>();
        }

        // a path may be legitimately partly circular (ie waypoint(s) repeated later) so we don't even check for this here (it is handled in step creation)
        // also a pathmanager path can have straightforwardly duplicated pathunits (ie exact same step twice in a row), for absolutely no reason [seems to affect about 1 in 100 paths]
        // again in the interest of speed for Landroute we do not check for this here (it is checked in journey creation else creates a useless null step)
        public void AddPoint(Waypoint newWaypoint, ushort newPathSegment)
        {
            m_landpoints.Add(new Landpoint(newWaypoint, newPathSegment));
        }

        // set of segments (with repeats suppressed) used in segment selection searching
        // no guarantee of sequence in hashset, but that doesn't matter for the JV application
        public HashSet<ushort> Segments()
        {
            HashSet<ushort> segset = new HashSet<ushort>();
            foreach (Landpoint lp in m_landpoints)
                segset.Add(lp.m_waypoint.Segment);
            return segset;
        }

        public void KillFirst()
        {
            m_landpoints.RemoveAt(0);
        }
    }

    public struct LineColorPair
    {
        public int m_travelmode;
        public Color m_lineColor;

        //special case constructing from known PT segment (from Landroute)
        public LineColorPair(ushort segmentID)
        {
            NetSegment netSegment = Singleton<NetManager>.instance.m_segments.m_buffer[segmentID];
            ushort line = Singleton<NetManager>.instance.m_nodes.m_buffer[netSegment.m_startNode].m_transportLine;
            m_lineColor = line == 0 ? Color.black : Singleton<TransportManager>.instance.GetLineColor(line); // I used to use .GetColor() but for line selections that can catch it while it is flashing
            m_travelmode = 32 + line;
        }

        public LineColorPair(Waypoint waypoint, ushort cim = 0)
        {
            NetManager theNetManager = Singleton<NetManager>.instance;
            TransportManager theTransportManager = Singleton<TransportManager>.instance;
            CitizenManager theCitizenManager = Singleton<CitizenManager>.instance;
            VehicleManager theVehicleManager = Singleton<VehicleManager>.instance;

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
                        // check if on a bike (strictly speaking = at the moment) - if so, it is considered ALL their pedestrian steps are on bicycle
                        if (cim == 0)
                        {
                            m_travelmode = 2;
                            m_lineColor = JVutils.m_travelModeColors[2];
                        }
                        else
                        {
                            uint cit = theCitizenManager.m_instances.m_buffer[cim].m_citizen;
                            ushort vehicle = theCitizenManager.m_citizens.m_buffer[cit].m_vehicle;
                            if (theVehicleManager.m_vehicles.m_buffer[vehicle].Info.m_vehicleType == VehicleInfo.VehicleType.Bicycle)
                            {
                                m_travelmode = 3;
                                m_lineColor = JVutils.m_travelModeColors[3];
                            }
                            else
                            {
                                m_travelmode = 2;
                                m_lineColor = JVutils.m_travelModeColors[2];
                            }
                        }
                        break;
                    case NetInfo.LaneType.PublicTransport:
                        ushort line = theNetManager.m_nodes.m_buffer[netSegment.m_startNode].m_transportLine;
                        //TransportLine tline = theTransportManager.m_lines.m_buffer[line];
                        //Color color = (tline.m_flags & TransportLine.Flags.CustomColor) == TransportLine.Flags.None ? theTransportManager.m_properties.m_transportColors[(int)tline.Info.m_transportType] : (Color)tline.m_color;
                        //m_lineColor = line == 0 ? Color.black : color; // I used to use .GetColor() but for line selections that can catch it while it is flashing
                        m_lineColor = line == 0 ? Color.black : theTransportManager.GetLineColor(line); // I used to use .GetColor() but for line selections that can catch it while it is flashing
                        m_travelmode = 32 + line;
                        break;
                    case NetInfo.LaneType.Vehicle:
                        // A vehicle on a bicycle lane is ipso facto a bicycle, else it's a car (no other types occur in JV cos there are no citizens in any other nonpublic road transport)
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

