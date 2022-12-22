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


namespace Journeys
{

    //[TargetType(typeof(PathVisualizer))]

    public class JourneyVisualizer : MonoBehaviour
    {

        public static JourneyVisualizer instance;
        public JourneyStepMgr theStepManager;
        public Dictionary<ushort, Journey> m_journeys;
        public HashSet<ushort> m_selectedCims;
        public HashSet<ushort> m_subselectedCims;
        public HashSet<ushort> m_subsubselectedCims;
        public HashSet<InstanceID> m_targets;
        public List<ushort> m_targetSteps;
        private List<ushort> m_subtargetSteps;
        private List<ushort> m_stepsList;
        private JourneyStepMgr.LaneLineSet m_LLTargetSteps;
        public List<ushort> m_BJcimsList;
        private int m_BJendIndex;
        private int m_BJcimIndex;
        private InstanceID m_lastInstance;
        private bool m_journeysVisible;
        private bool m_showAllCars;
        private int m_maxJourneysCount;
        private readonly object buildLock = new object();
        private readonly object renderLock = new object();
        private bool doneRender;
        private bool doneMeshes;

        public void ToggleAllCars()
        {
            m_showAllCars = !m_showAllCars;
            Debug.Log("toggled m_showAllCars to " + m_showAllCars);
            m_lastInstance = InstanceID.Empty;
            SimulationStep(0);
        }


        public ushort SelectedSegment { get; set; }
        public NetInfo SelectedSegInfo { get; set; }
        public byte CurrentLane { get; private set; }
        public byte NumLanes { get; set; }

        public bool HeatMap { get; set; }
        public int DiscreteHeats { get; set; }

        public int AbsoluteHeats { get; set; }

        public bool HeatOnlyAsSelected { get; set; }

        public bool ShowOnlyTransportSteps { get; set; }
        public byte FromToFlag { get; set; }
        public bool OnlyPTstretches { get; set; }
        public bool ShowPTStops { get; set; }
        public bool ShowBlended {  get; set; }
        public bool ByLaneInitiated { get; set; }
        public bool ByStepInitiated { get; set; }
        public bool ByJourneyInitiated { get; private set; }
        public int TargetStepsIdx { get; private set; }
        public bool SubselectByLaneLineInitiated { get; set; }



        public bool DoneRender
        {
            get
            {
                lock (renderLock)
                {
                    return doneRender;
                }
            }
            private set
            {
                doneRender = value;
            }
        }

        public bool DoneMeshes
        {
            get
            {
                lock (buildLock)
                {
                    return doneMeshes;
                }
            }
            private set
            {
                doneMeshes = value;
            }
        }


        //
        // ************************** initialization and destruction
        //

        private void Awake()
        {
            instance = this;
            Debug.Log("JV: Awake has set instance");
        }

        // there is no Init in PV, but that is because PV creation is done differently in NM

        public void Init()
        {
            theStepManager = new JourneyStepMgr();
            m_showAllCars = false;
            m_journeys = new Dictionary<ushort, Journey>();
            m_selectedCims = new HashSet<ushort>();
            m_subselectedCims = new HashSet<ushort>();
            m_subsubselectedCims = new HashSet<ushort>();
            m_maxJourneysCount = 5000;                       // maybe more or less would be better.  PV has (hardcoded) 100 but that is def not enough for eg a full train.
            m_targets = new HashSet<InstanceID>();
            m_targetSteps = new List<ushort>();
            m_subtargetSteps = new List<ushort>();
            m_stepsList = new List<ushort>();
            m_BJcimsList = new List<ushort>();
            m_journeysVisible = true;
            m_lastInstance = InstanceID.Empty;
            doneRender = true;                          // doneRender is always true, but DoneRender is locked during rendering (also just in case, RenderJourneys resets it true after rendering)
            doneMeshes = false;
            HeatMap = false;
            DiscreteHeats = 0;
            AbsoluteHeats = 0;
            ShowBlended = false;
            HeatOnlyAsSelected = true;
            ShowOnlyTransportSteps = false;
            ShowPTStops = false;
            FromToFlag = 0;
            Debug.Log("JV: instance.Init has been run");
        }

        private void OnDestroy()
        {
            DestroyJourneys();
            Debug.Log("JV: OnDestroy has been run");
        }

        //
        // ************************** the main call
        //


        public void SimulationStep(int subStep)
        {
            if (!m_journeysVisible)
                return;
            lock (buildLock)
            {
                CitizenManager theCitizenManager = Singleton<CitizenManager>.instance;
                VehicleManager theVehicleManager = Singleton<VehicleManager>.instance;
                PathManager thePathManager = Singleton<PathManager>.instance;
                // PV selections are shown dynamically using a m_pathRefreshFrame mechanism to check for updates,
                // but JV freezes journey info at the time of selection
                // because it could then happen that a citizen appears more than once, ie with the same ref number recycled)
                InstanceID clickedInstance = Singleton<InstanceManager>.instance.GetSelectedInstance();
                if (clickedInstance == m_lastInstance || clickedInstance == null)
                    return;
                m_lastInstance = clickedInstance;
                doneMeshes = false;
                //Debug.Log("Starting new instance");
                if (clickedInstance.Citizen != 0U)
                {
                    // Debug.Log("Selected a citizen");
                    ushort citizenInstanceID = theCitizenManager.m_citizens.m_buffer[clickedInstance.Citizen].m_instance;
                    uint pathID = theCitizenManager.m_instances.m_buffer[citizenInstanceID].m_path;
                    if (citizenInstanceID != 0 && pathID != 0)
                    {
                        WipeSlate();
                        PathUnit pathunit = thePathManager.m_pathUnits.m_buffer[pathID];
                        int pathPositionIndex = theCitizenManager.m_instances.m_buffer[citizenInstanceID].m_pathPositionIndex;
                        if (!pathunit.GetPosition(pathPositionIndex >> 1, out PathUnit.Position thisPathPosition))
                            return;  // this could happen if a path gets modified in the sim while this loop is calculating
                        InstanceID newID = InstanceID.Empty;
                        newID.NetSegment = thisPathPosition.m_segment;
                        m_targets.Add(newID);
                        SelectedSegment = thisPathPosition.m_segment;
                        AddJourney(citizenInstanceID, pathID);
                    }
                }
                // now a major departure from PV for vehicles - which we here convert to a collection of their passengers
                // note PV has a special procedure for bikes (finds their owners). This is not needed in JV, ALL vehicles have their "passengers" in citizenUnits member
                else if (clickedInstance.Vehicle != 0)
                {
                    Debug.Log("Selected a new vehicle");
                    WipeSlate();
                    bool carPath = false;
                    bool firstloop = true;
                    int loopLimit = 0;
                    ushort leadingVehicleID = theVehicleManager.m_vehicles.m_buffer[clickedInstance.Vehicle].GetFirstVehicle(clickedInstance.Vehicle);   // make sure we start with the leading vehicle for trams etc
                    ushort vehicleID = leadingVehicleID;
                    uint pathID = theVehicleManager.m_vehicles.m_buffer[leadingVehicleID].m_path;
                    if (pathID != 0 && theVehicleManager.m_vehicles.m_buffer[leadingVehicleID].m_transportLine == 0)
                    {
                        carPath = true;
                    }
                    while (vehicleID != 0)
                    {
                        uint thisUnit = theVehicleManager.m_vehicles.m_buffer[vehicleID].m_citizenUnits;
                        int loopLimit2 = 0;
                        while (thisUnit != 0)
                        {
                            for (int index = 0; index < 5; ++index)
                            {
                                uint citizen = theCitizenManager.m_units.m_buffer[thisUnit].GetCitizen(index);
                                if (citizen != 0)
                                {
                                    ushort citInst = theCitizenManager.m_citizens.m_buffer[citizen].m_instance;
                                    uint citPathID = theCitizenManager.m_instances.m_buffer[citInst].m_path;
                                    if (firstloop)
                                    {
                                        PathUnit pathunit;
                                        PathUnit.Position thisPathPosition;
                                        int pathPositionIndex;
                                        if (pathID == 0)
                                        {
                                            // all bikes and some (not many) cars have zero m_path, pick up vehicle segment position from passenger path
                                            pathunit = thePathManager.m_pathUnits.m_buffer[citPathID];
                                            pathPositionIndex = theCitizenManager.m_instances.m_buffer[citInst].m_pathPositionIndex;
                                            if (!pathunit.GetPosition(pathPositionIndex >> 1, out thisPathPosition))
                                                return;
                                        }
                                        else
                                        {
                                            // public transport vehicles have a path for the vehicle (used just to find current segment)
                                            // cars (nearly all) have only a path for the vehicle
                                            pathPositionIndex = theVehicleManager.m_vehicles.m_buffer[leadingVehicleID].m_pathPositionIndex;
                                            pathunit = thePathManager.m_pathUnits.m_buffer[pathID];
                                            if (!pathunit.GetPosition(pathPositionIndex >> 1, out thisPathPosition))
                                                return;
                                        }
                                        SelectedSegment = thisPathPosition.m_segment;
                                        InstanceID newID = InstanceID.Empty;
                                        newID.NetSegment = thisPathPosition.m_segment;
                                        m_targets.Add(newID);
                                        Debug.Log("Done vehicle selection, vehicle on segment " + SelectedSegment);
                                        firstloop = false;
                                    }
                                    if (carPath)
                                    {
                                        AddJourney(citInst, pathID);
                                    }
                                    else
                                    {
                                        AddJourney(citInst, citPathID);
                                    }
                                }
                            }
                            thisUnit = theCitizenManager.m_units.m_buffer[thisUnit].m_nextUnit;
                            if (++loopLimit2 > 524288)
                            {
                                Debug.LogError("JV Error: Invalid list of citizen units detected!\n" + System.Environment.StackTrace);
                                break;
                            }
                        }
                        vehicleID = theVehicleManager.m_vehicles.m_buffer[vehicleID].m_trailingVehicle;
                        if (++loopLimit > 16384)
                        {
                            Debug.LogError("JV Error: Invalid list of leading/trailing vehicles detected!\n" + System.Environment.StackTrace);
                            break;
                        }
                    }
                }
                else if (clickedInstance.NetSegment != 0 || clickedInstance.Building != 0 || (clickedInstance.District != 0 || clickedInstance.Park != 0))
                {
                    WipeSlate();
                    AddJourneys(clickedInstance);
                    SelectedSegment = clickedInstance.NetSegment;       // which will set it to zero if not a segment selected
               }
                else if (clickedInstance.TransportLine != 0)
                {
                    Debug.Log("selected transport line " + clickedInstance.TransportLine);
                    return;
                }
                SelectedSegInfo = Singleton<NetManager>.instance.m_segments.m_buffer[SelectedSegment].Info;
                m_subselectedCims.Clear();
                foreach (ushort cim in m_selectedCims)
                    m_subselectedCims.Add(cim);
                m_targetSteps = theStepManager.GetTargetSteps();
                Debug.Log("Length of targetSteps: " + m_targetSteps.Count);
                foreach (ushort kdx in m_targetSteps)
                {
                    theStepManager.GetStep(kdx).DumpStep(kdx);
                }
                m_subtargetSteps = theStepManager.GetTargetSteps();
                FromToFlag = 0;
                if (theStepManager.StepCount > 0)
                    theStepManager.CalculateMeshes();
                //theStepManager.LogSteps();
                doneMeshes = true;
            }
        }


        // AddJourneys (and AddJourneysImpl) is what happens when you select a road segment or a building (or a region or a park)
        // so in this case the main argument InstanceID is called target (it cannot be a citizen or a vehicle else this would not be called)
        // note that AddJourneys does not itself add journeys!  It just handles target or non-null target building (and subnodes and subbuildings) adding to m_targets list
        // then it hands over to AddJourneysImpl to do the actual path adding, based on the m_targets (passed implicitly by sharing member)

        private void AddJourneys(InstanceID target)
        {
            // m_targets is a SET of targets, noting a building can have more than one node (and certainly a region or park does)
            m_targets.Clear();
            // if target is not a building, the only thing AddJourneys does is to add target to m_targets (after having cleared it first)
            switch (target.Building)
            {
                case 0:
                    m_targets.Add(target);  // all done with just add the target InstanceID to the targets list for a segment, District or Park
                    break;
                default:
                    {
                        BuildingManager theBuildingManager = Singleton<BuildingManager>.instance;
                        NetManager theNetManager = Singleton<NetManager>.instance;
                        int loopLimit = 0;
                        while (target.Building != 0)
                        {
                            // whatever else happens, add the target building to m_targets
                            m_targets.Add(target);
                            // lookup the netNode on which the building sits
                            ushort targetNetNode = theBuildingManager.m_buildings.m_buffer[target.Building].m_netNode;
                            int loopLimit2 = 0;
                            while (targetNetNode != 0)
                            {
                                // I do not understand why PV excludes public transport nodes here. I leave the restriction in for now but will try removing later
                                if (theNetManager.m_nodes.m_buffer[targetNetNode].Info.m_class.m_layer != ItemClass.Layer.PublicTransport)
                                {
                                    // check all 8 segments coming from this node (I assume some or most are null) - really check all 8 there is no break or continue
                                    for (int index = 0; index < 8; ++index)
                                    {
                                        ushort segment = theNetManager.m_nodes.m_buffer[targetNetNode].GetSegment(index);
                                        // it the segment starts at the target node (and is not null and flags are ok)
                                        if (segment != 0 && theNetManager.m_segments.m_buffer[segment].m_startNode == targetNetNode && (theNetManager.m_segments.m_buffer[segment].m_flags & NetSegment.Flags.Untouchable) != NetSegment.Flags.None)
                                        {
                                            InstanceID newID = InstanceID.Empty;
                                            newID.NetSegment = segment;
                                            m_targets.Add(newID);
                                        }
                                    }
                                }
                                targetNetNode = theNetManager.m_nodes.m_buffer[targetNetNode].m_nextBuildingNode;     // loop again for the next node of a big building that has multiple nodes
                                if (++loopLimit2 > 32768)
                                {
                                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                                    break;
                                }
                            }
                            target.Building = theBuildingManager.m_buildings.m_buffer[target.Building].m_subBuilding;   // outer loop again for any subbuilding of the target building
                            if (++loopLimit > 49152)
                            {
                                CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                                break;
                            }
                        }
                        break;
                    }
            }
            AddJourneysImpl();
        }

        // AddJourneysImpl is where we actually add the journeys that hit the target(s). This is where we
        // trace through entire journeys of all citizens (and vehicles, in PV) to see if they pass through m_targets
        // I am making a big departure from PV here in that I am only going to look at the paths of citizens
        // BECAUSE their paths include the time they spend in vehicles (and unlike PV, we will not be plotting vehicle paths per se)
        //

        private void AddJourneysImpl()
        {
            PathManager thePathManager = Singleton<PathManager>.instance;
            NetManager theNetManager = Singleton<NetManager>.instance;
            BuildingManager theBuildingManager = Singleton<BuildingManager>.instance;
            DistrictManager theDistrictManager = Singleton<DistrictManager>.instance;
            CitizenManager theCitizenManager = Singleton<CitizenManager>.instance;
            VehicleManager theVehicleManager = Singleton<VehicleManager>.instance;

            // loop through every citizen path (journey) looking to see if they hit a target in m_targets
            int addedJourneysCount = 0;
            for (int index1 = 0; index1 < 65536; ++index1)
            {
                // for all CitizenInstances that have been created but are are not marked as Deleted or WaitingPath
                //if (index1 == 65535)
                //    Debug.Log("cim index reached 65535");
                CitizenInstance thisCitInst = theCitizenManager.m_instances.m_buffer[index1];
                if ((thisCitInst.m_flags & (CitizenInstance.Flags.Created | CitizenInstance.Flags.Deleted | CitizenInstance.Flags.WaitingPath)) != CitizenInstance.Flags.Created)
                    continue;
                bool addable = false;
                uint path = thisCitInst.m_path;
                List<Waypoint> route = JVutils.PathToWaypoints(path, fullroute: true);
                if (route == null && m_showAllCars)
                {
                    ushort vehicle = theCitizenManager.m_citizens.m_buffer[thisCitInst.m_citizen].m_vehicle;
                    if (theVehicleManager.m_vehicles.m_buffer[vehicle].Info.m_vehicleType == VehicleInfo.VehicleType.Car)
                    {
                        path = theVehicleManager.m_vehicles.m_buffer[vehicle].m_path;
                        route = JVutils.PathToWaypoints(path, fullroute: true);
                    }
                }
                if (route != null)
                {
                    foreach (Waypoint maybematch in route)
                    {
                        InstanceID newID = InstanceID.Empty;
                        newID.NetSegment = maybematch.Segment;
                        if (m_targets.Contains(newID))
                        {
                            NetSegment segobject = theNetManager.m_segments.m_buffer[maybematch.Segment];
                            NetInfo info = segobject.Info;
                            addable = thePathManager.m_pathUnits.m_buffer[path].m_buildIndex > segobject.m_modifiedIndex
                                && info != null && info.m_lanes != null && maybematch.Lane < info.m_lanes.Length;
                            break;
                        }
                    }
                }

                InstanceID targetId = thisCitInst.Info.m_citizenAI.GetTargetID((ushort)index1, ref thisCitInst);
                addable |= m_targets.Contains(targetId);
                if (targetId.Building != 0)
                {
                    Vector3 position = theBuildingManager.m_buildings.m_buffer[targetId.Building].m_position;
                    InstanceID newID = InstanceID.Empty;
                    newID.District = theDistrictManager.GetDistrict(position);
                    addable |= m_targets.Contains(newID);
                    newID.Park = theDistrictManager.GetPark(position);
                    addable |= m_targets.Contains(newID);
                }
                if (targetId.NetNode != 0)
                {
                    Vector3 position = theNetManager.m_nodes.m_buffer[targetId.NetNode].m_position;
                    InstanceID newID = InstanceID.Empty;
                    newID.District = theDistrictManager.GetDistrict(position);
                    addable |= m_targets.Contains(newID);
                    newID.Park = theDistrictManager.GetPark(position);
                    addable |= m_targets.Contains(newID);
                }

                // here, finally, is the add to m_journeys
                if (addable)
                {
                    AddJourney((ushort)index1, path);
                    //Debug.Log("Just called AddJourney for index " + index1);
                    // PV stops at 100 paths, but this would not be enough for checking all passengers on metros for example
                    if (++addedJourneysCount > m_maxJourneysCount)
                        break;
                }
            }
            //Debug.Log("Done segment-type selection");
        }

        // AddJourney is where a new Journey is created and added to m_journeys
        // Creating the journey also creates and populates the corresponding JourneySteps, including the meshes
        // the only thing still to do is actually draw the meshes

        private void AddJourney(ushort citizenID, uint pathID)
        {
            //Debug.Log("about to add new journey");
            Journey journey = new Journey(citizenID, pathID);
            //Debug.Log("added new journey with m_steps.Count " + journey.m_steps.Count);
            if (journey.m_steps.Count > 0)
            {
                m_journeys.Add(citizenID, journey);
                m_selectedCims.Add(citizenID);
            }
        }

        public void ChangeHeatMap()
        {
            lock (buildLock)
            {
                doneMeshes = false;
                HeatMap = !HeatMap;
                theStepManager.ReheatMeshes();
                doneMeshes = true;
            }
        }

        public void ChangeDiscreteHeats()
        {
            DiscreteHeats++;
            if (DiscreteHeats > 8)
                DiscreteHeats = 0;
            AbsoluteHeats = 0;
            lock (buildLock)
            {
                doneMeshes = false;
                theStepManager.ReheatMeshes();
                doneMeshes = true;
            }
        }

        public void ChangeAbsoluteHeats()
        {
            AbsoluteHeats++;
            if (AbsoluteHeats > 6)
                AbsoluteHeats = 0;
            DiscreteHeats = 0;
            lock (buildLock)
            {
                doneMeshes = false;
                theStepManager.ReheatMeshes();
                doneMeshes = true;
            }
        }

        public void ChangeHeatOnlyAsSelected()
        {
            lock (buildLock)
            {
                doneMeshes = false;
                HeatOnlyAsSelected = !HeatOnlyAsSelected;
                theStepManager.ReheatMeshes();
                doneMeshes = true;
            }
        }

        public void ToggleTransportSteps()
        {
            ShowOnlyTransportSteps = !ShowOnlyTransportSteps;
            ByStepInitiated = false;
            ByLaneInitiated = false;
        }

        public void ToggleShowPTstretches()
        {
            OnlyPTstretches = !OnlyPTstretches;
            ShowJourneys();
        }
        public void ToggleShowPTstops()
        {
            ShowPTStops = !ShowPTStops;
            Debug.Log("ShowPTStops toggled to " + ShowPTStops);
            ShowJourneys();
        }

        public void ToggleShowBlended()
        {
            ShowBlended = !ShowBlended;
            theStepManager.ReheatMeshes();
        }

        public void SubSelectByLane()
        {
            lock (buildLock)
            {
                if (SelectedSegment == 0)
                    return;
                doneMeshes = false;     // not entirely true but stops RenderJourneys in the meantime
                if (!ByLaneInitiated)
                {
                    NumLanes = (byte)SelectedSegInfo.m_lanes.Length;
                    if (NumLanes == 0)
                        return;
                    CurrentLane = 0;
                    ByLaneInitiated = true;
                }
                if (CurrentLane == NumLanes)
                {
                    // in this case we return to showing all steps
                    m_subselectedCims.Clear();
                    foreach (ushort cim in m_selectedCims)
                        m_subselectedCims.Add(cim);
                }
                else
                {
                    bool notAllLanes = true;
                    if (ShowOnlyTransportSteps)
                    {
                        while (SelectedSegInfo.m_lanes[CurrentLane].m_vehicleType < VehicleInfo.VehicleType.Metro)
                        // NB this does not detect busses (or bus lanes), because their VehicleType is Car
                        {
                            CurrentLane++;
                            if (CurrentLane == NumLanes)
                            {
                                m_subselectedCims.Clear();
                                foreach (ushort cim in m_selectedCims)
                                    m_subselectedCims.Add(cim);
                                notAllLanes = false;
                                break;
                            }
                            Debug.Log("lane " + CurrentLane + " has vehicleType " + SelectedSegInfo.m_lanes[CurrentLane].m_vehicleType);
                        }
                    }
                    Debug.Log("JV hit segment selecting lane " + CurrentLane + " (out of " + NumLanes + ")");
                    if (notAllLanes)
                    {
                        m_subselectedCims = theStepManager.GetLaneCims(SelectedSegment, CurrentLane);
                    }
                }
            }
            CurrentLane++;
            if (CurrentLane > NumLanes)
                CurrentLane = 0;
            ByJourneyInitiated = false;
            ByStepInitiated = false;
            ShowJourneys();
            doneMeshes = true;
        }

        public void ShowJourneys()
        {
            //Debug.Log("Show journey, " + m_journeys.Count + " total journeys");
            //Debug.Log("Show journey, " + m_subselectedCims.Count + " selected cims/journeys");


            lock (buildLock)
            {
                theStepManager.HideAllCims();
                //Debug.Log("m_journeys.Count " + m_journeys.Count + ", m_subtargetSteps.Count " + m_subtargetSteps.Count + ", m_subselectedCims.Count " + m_subselectedCims.Count);

                if (m_journeys.Count == 0)
                    return;

                doneMeshes = false;

                if (FromToFlag == 0 || m_subtargetSteps.Count == 0)
                {
                    if (OnlyPTstretches == false)
                    {
                        foreach (ushort cim in m_subselectedCims)
                        {
                            Journey journey = m_journeys[cim];
                            foreach (ushort stepIdx in journey.m_steps)
                                theStepManager.GetStep(stepIdx).ShowCitizen(cim);
                        }
                    }
                    else
                    {
                        foreach (ushort cim in m_subselectedCims)
                        {
                            Journey journey = m_journeys[cim];
                        foreach (ushort stepIdx in MaskPT(journey.m_steps))
                            theStepManager.GetStep(stepIdx).ShowCitizen(cim);
                        }
                    }
                }
                else
                {
                    if (!OnlyPTstretches)
                    {
                        foreach (ushort cim in m_subselectedCims)
                        {
                            Journey journey = m_journeys[cim];
                            foreach (ushort stepIdx in MaskFromTo(journey.m_steps))
                                theStepManager.GetStep(stepIdx).ShowCitizen(cim);
                        }
                    }
                    else
                    {
                        foreach (ushort cim in m_subselectedCims)
                        {
                            Journey journey = m_journeys[cim];
                            foreach (ushort stepIdx in MaskPT(MaskFromTo(journey.m_steps)))
                                theStepManager.GetStep(stepIdx).ShowCitizen(cim);
                        }
                    }
                }

                theStepManager.CalculateMeshes();
                doneMeshes = true;
            }
        }

        private List<ushort> MaskFromTo(List<ushort> stepList)
        {
            if (FromToFlag == 1)
            {
                for (int idx = 0; idx < stepList.Count; idx++)
                    if (m_subtargetSteps.Contains(stepList[idx]))
                        return stepList.GetRange(idx, stepList.Count - idx);
            }
            else if (FromToFlag == 2)
            {
                for (int idx = 0; idx < stepList.Count; idx++)
                {
                    if (m_subtargetSteps.Contains(stepList[idx]))
                    {
                        // keep looking until we hit a non-target step (so multiple target steps will be INcluded in selection)
                        for (int jdx = idx + 1; jdx < stepList.Count; jdx++)
                        {
                            if (m_subtargetSteps.Contains(stepList[jdx]))
                            {
                                idx++;
                                continue;
                            }
                            break;
                        }
                        return stepList.GetRange(0, idx + 1);
                    }
                }
            }
            else
            {
                return stepList;
            }
            return new List<ushort>(); // this can occur for To cases, for vehicle selections, falling through because the cim path position is (wrongly) in advance of the vehicle
        }

        private List<ushort> MaskPT(List<ushort> stepList)
        {
            if (!OnlyPTstretches || stepList.Count == 0)
                return stepList;
            List<ushort> maybeConnections = new List<ushort>();
            List<ushort> retList = new List<ushort>();
            bool caughtPT = false;
            foreach (ushort stepIdx in stepList)
            {
                JourneyStep jStep = theStepManager.GetStep(stepIdx);
                if (jStep.IsTransportStep())
                {
                    retList.AddRange(maybeConnections);
                    retList.Add(stepIdx);
                    maybeConnections.Clear();
                    caughtPT = true;
                }
                else
                {
                    if (caughtPT)
                    {
                        maybeConnections.Add(stepIdx);
                    }
                }
            }
            return retList;
        }


        public void ToggleFromToHere()
        {
            FromToFlag++;
            if (FromToFlag == 3)
                FromToFlag = 0;

            ShowJourneys();
        }

        public void SubselectByLaneLine(bool forwards = true)
        {
            lock (buildLock)
            {
                doneMeshes = false;     // not entirely true but stops RenderJourneys in the meantime
                if (!SubselectByLaneLineInitiated)
                {
                    m_LLTargetSteps = new JourneyStepMgr.LaneLineSet(m_targetSteps, SelectedSegment, SelectedSegInfo);
                    SubselectByLaneLineInitiated = true;
                }

                m_subselectedCims = m_LLTargetSteps.GetNextLaneLineCims(forwards, ShowOnlyTransportSteps);
                ShowJourneys();
                doneMeshes = true;
            }
        }

        public void SubSelectByStep()
        {
            if (m_targetSteps.Count == 0)
                return;
            lock (buildLock)
            {
                doneMeshes = false;     // not entirely true but stops RenderJourneys in the meantime

                if (!ByStepInitiated)
                {
                    m_stepsList = new List<ushort>();
                    if (ShowOnlyTransportSteps)
                    {
                        foreach (ushort stepID in m_targetSteps)
                            if (theStepManager.GetStep(stepID).IsTransportStep())
                                m_stepsList.Add(stepID);
                    }
                    else
                    {
                        m_stepsList.AddRange(m_targetSteps);
                    }

                    if (m_stepsList.Count == 0)
                    {
                        Debug.Log("stepsList.Count is zero; targetSteps.Count is " + m_targetSteps.Count);
                        theStepManager.HideAllCims();
                        theStepManager.CalculateMeshes();
                        return;
                    }
                    ByStepInitiated = true;
                    TargetStepsIdx = m_stepsList.Count;
                }

                TargetStepsIdx++;
                if (TargetStepsIdx == m_stepsList.Count)
                {
                    // in this case we return to showing all steps and all cims
                    m_subselectedCims.Clear();
                    foreach (ushort cim in m_selectedCims)
                        m_subselectedCims.Add(cim);
                    m_subtargetSteps.Clear();
                    m_subtargetSteps.AddRange(m_targetSteps);
                }
                else
                {
                    if (TargetStepsIdx > m_stepsList.Count)
                        TargetStepsIdx = 0;
                    ushort m_selectedStep = m_stepsList[TargetStepsIdx];
                    JourneyStep jstep = theStepManager.GetStep(m_selectedStep);
                    m_subselectedCims = jstep.GetCims();
                    m_subtargetSteps.Clear();
                    m_subtargetSteps.Add(m_selectedStep);
                    // m_subtargetSteps.Add(m_selectedStep); seems to be no point at all in this line, it means the loop would never end
                    //jstep.DumpStep(m_selectedStep);
                }
            }
            ByJourneyInitiated = false;
            ByLaneInitiated = false;
            ShowJourneys();
            doneMeshes = true;
        }

        public void ByJourney()
        {
            lock (buildLock)
            {
                doneMeshes = false;
                if (m_subselectedCims.Count == 0)
                    return;
                if (!ByJourneyInitiated)
                {
                    m_BJcimsList = m_subselectedCims.ToList();
                    m_BJendIndex = m_BJcimsList.Count;
                    m_BJcimIndex = 0;
                    ByJourneyInitiated = true;
                }

                theStepManager.HideAllCims();

                if (m_BJcimIndex == m_BJendIndex)
                {
                    //m_BJcimsList.Clear();
                    //foreach (ushort cim in m_selectedCims)
                    //    m_BJcimsList.Add(cim);
                    //foreach (ushort cim in m_subselectedCims)
                    //{
                    //    Journey journey = m_journeys[cim];
                    //    foreach (ushort stepIdx in journey.m_steps)
                    //        theStepManager.GetStep(stepIdx).ShowCitizen(cim);
                    //}
                    m_subselectedCims.Clear();
                    foreach (ushort cim in m_BJcimsList)
                        m_subselectedCims.Add(cim);
                    m_BJcimIndex = 0;
                }
                else
                {
                    m_subselectedCims.Clear();
                    m_subselectedCims.Add(m_BJcimsList[m_BJcimIndex]);
                    m_BJcimIndex++;
                }
            }
            ShowJourneys();
            doneMeshes = true;
        }

        public void ShowAllJourneys()
        {
            lock (buildLock)
            {
                CitizenManager theCitizenManager = Singleton<CitizenManager>.instance;
                VehicleManager theVehicleManager = Singleton<VehicleManager>.instance;
                doneMeshes = false;
                WipeSlate();
                CitizenInstance thisCitInst;
                for (int citizen = 1; citizen < 65535; ++citizen)
                {
                    try
                    {
                        thisCitInst = theCitizenManager.m_instances.m_buffer[citizen];
                    }
                    catch
                    {
                        Debug.Log("there was an exception");
                        continue;
                    }
                    if ((thisCitInst.m_flags & (CitizenInstance.Flags.Created | CitizenInstance.Flags.Deleted | CitizenInstance.Flags.WaitingPath)) != CitizenInstance.Flags.Created)
                        continue;
                    uint path = thisCitInst.m_path;
                    Debug.Log("for index " + citizen + " path is " + path + " with m_showAllCars = " + m_showAllCars);
                    if (path == 0)
                    {
                        if (m_showAllCars)
                        {
                            ushort vehicle = theCitizenManager.m_citizens.m_buffer[thisCitInst.m_citizen].m_vehicle;
                            if (theVehicleManager.m_vehicles.m_buffer[vehicle].Info.m_vehicleType == VehicleInfo.VehicleType.Car)
                            {
                                path = theVehicleManager.m_vehicles.m_buffer[vehicle].m_path;
                                if (path == 0)
                                    continue;
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                    ushort ushcitizen = (ushort)citizen;
                    Journey journey = new Journey(ushcitizen, path);
                    if (journey.m_steps.Count > 0)
                    {
                        m_journeys.Add(ushcitizen, journey);
                        m_selectedCims.Add(ushcitizen);
                        m_subselectedCims.Add(ushcitizen);
                    }

                }
                FromToFlag = 0;
                if (theStepManager.StepCount <= 0)
                    Debug.Log("all journeys finds stepcount " + theStepManager.StepCount);
                if (theStepManager.StepCount > 0)
                    theStepManager.CalculateMeshes();
                doneMeshes = true;
            }
        }


        // m_journeysVisible is this is the fundamental controller of whether SimulationStep happens or not (as well as the obvious meaning)
        // I thought there might be calls to PathsVisible from other managers, but there do not seem to be.  Still, I left it here for now.
        public bool PathsVisible
        {
            get
            {
                Debug.Log("JV: called PathsVisible.get with value = " + m_journeysVisible);
                return m_journeysVisible;
            }
            set
            {
                Debug.Log("JV: Called PathsVisible.set with value = " + value);
                m_journeysVisible = value;
            }
        }

        public void DestroyJourneys()
        {
            if (DoneMeshes && DoneRender)
            {
                theStepManager.DestroyAll();
                DoneMeshes = false;         // this stops a wasted RenderJourneys
            }
            lock (m_journeys)
            {
                m_journeys.Clear();
            }
        }


        // RenderJourneys is called from outside JV in the manager sim steps at high level
        // (by redirection from PV.RenderPaths)

        public void RenderJourneys(RenderManager.CameraInfo cameraInfo, int layerMask)
        {
            //Debug.Log("JV: called RenderJourneys");
            if (!m_journeysVisible || !DoneMeshes)
                return;
            //Debug.Log("JV: Entered active part of RenderJourneys");
            lock (renderLock)
            {
                theStepManager.DrawTheMeshes(cameraInfo, layerMask);
                doneRender = true;
            }
        }

        public void WipeSlate()
        {
            Debug.Log("JV: called JV.WipeSlate");
            theStepManager.WipeSlate();
            Debug.Log("JV: done call to theStepManager.WipeSlate");
            m_journeys.Clear();
            m_targets.Clear();
            m_selectedCims.Clear();
            m_targetSteps.Clear();  
            m_subtargetSteps.Clear();
            ByLaneInitiated = false;
            ByStepInitiated = false;
            ByJourneyInitiated = false;
            SubselectByLaneLineInitiated = false;
        Debug.Log("JV: finished JV.WipeSlate");
        }

        // network manager insists on calling this, so it must exist. But it's only called on quitting the PV
        public void UpdateData()
        {
            Debug.Log("JV: called UpdateData");
        }


        // IsPathVisble is a helper function called by ALL the vehicleAI classes individually, to determine if the
        // vehicle should be drawn the same colour as its path (makes sense for PV, not always for JV)

        public bool IsPathVisible(InstanceID id)
        {
            return false;
        }

    }
}


