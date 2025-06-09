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
    //   a JourneyStep (qv) is the journey from one PV (Path Visualizer) path position to the next

    public class Journey
    {
        // the indexer of the m_journeys Dictionary is a citizenInstanceID, but a Journey also knows its own id (as m_cim)
        public ushort m_cim;
        public List<ushort> m_steps;  // the journey encapsulated as a list of its JourneySteps (as reference indices to the JourneyStepMgr dictionary of JourneySteps)
        public List<Waypoint> m_rawRoute;     //the journey encapsulated as a list of raw Waypoints (raw = pathmanager path except PathPositions transformed to Waypoints, transport segs are not expanded to landroute)

        public Journey()
        {
            m_cim = 0;
            m_steps = new List<ushort>();
            m_rawRoute = new List<Waypoint>();
        }

        public Journey(List<Waypoint> rawroute, ushort cim)
        {
            m_cim = cim;
            m_steps = new List<ushort>();
            m_rawRoute = new List<Waypoint>(rawroute);
        }

        public void SetSteps()
        {
            if (m_rawRoute == null || m_rawRoute.Count < 2)
            {
                Debug.LogError("JV Error: called SetSteps with deficient m_rawRoute");      // failsafe: code should have already checked route is OK before creating the journey
                return;
            }
            TransportManager theTransportManager = Singleton<TransportManager>.instance;
            JourneyVisualizer theJourneyVisualizer = Singleton<JourneyVisualizer>.instance;
            JourneyStepMgr theStepManager = theJourneyVisualizer.theStepManager;

            // itinIdx and itinLast are just for setting end journey, not needed for foreach control
            int itinIdx = 1;
            int itinLast = m_rawRoute.Count;
            // the initial pointA is only used in the loop if pointB is on PT, but we need to set default values to keep the compiler quiet
            Waypoint pointA = m_rawRoute.First();
            SegWay segwayA = theJourneyVisualizer.m_segways[pointA.Segment];
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
                SegWay segwayB = theJourneyVisualizer.m_segways[pointB.Segment];
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

    //// a "full route" aka "landroute" is every waypoint on the overland journey (that is: with pathunit transport segments converted to a list of map segments)
    //// The Landroute object has this route plus parallel info on the original path segments generating each waypoint
    //// This m_pathSegment is 0 if the landroute and path segments are the same, or for PT steps it is the original transport segment (which holds the landroute as its m_path)
    //// Hence m_pathSegment can also be used as a binary flag (0 = not on PT, nonzero = on PT) even if the rest of the segment info is not needed per se

    //public struct Landpoint
    //{
    //    public Waypoint m_waypoint;
    //    public ushort m_pathSegment;

    //    public Landpoint(Waypoint newWaypoint, ushort newPathSegment)
    //    {
    //        m_waypoint = newWaypoint;
    //        m_pathSegment = newPathSegment;
    //    }
    //}

    //public class Landroute
    //{
    //    public List<Landpoint> m_landpoints;

    //    public Landroute()
    //    {
    //        m_landpoints = new List<Landpoint>();
    //    }

    //    // a path may be legitimately partly circular (ie waypoint(s) repeated later) so we don't even check for this here (it is handled in step creation)
    //    // also a pathmanager path can have straightforwardly duplicated pathunits (ie exact same step twice in a row), for absolutely no reason [seems to affect about 1 in 100 paths]
    //    // again in the interest of speed for Landroute we do not check for this here (it is checked in journey creation else creates a useless null step)
    //    public void AddPoint(Waypoint newWaypoint, ushort newPathSegment)
    //    {
    //        m_landpoints.Add(new Landpoint(newWaypoint, newPathSegment));
    //    }
    //}

}

