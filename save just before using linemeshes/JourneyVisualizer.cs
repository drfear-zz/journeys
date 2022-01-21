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

    [TargetType(typeof(PathVisualizer))]

    public class JourneyVisualizer : MonoBehaviour
    {

        public static JourneyVisualizer instance;
        private Dictionary<InstanceID, Journey> m_journeys;
        private FastList<Journey> m_renderJourneys;
        private FastList<Journey> m_removeJourneys;
        private FastList<Journey> m_stepJourneys;
        private HashSet<InstanceID> m_targets;
        private InstanceID m_lastInstance;
        private bool m_journeysVisible;
        private int m_neededJourneysCount;
        private int m_journeyRefreshFrame;


        //
        // ************************** initialization and destruction
        //

        private void Awake()
        {
            instance = this;
            Debug.Log("JV Awake has set instance");
        }

        // there is no Init in PV, but that is because PV creation is done differently in NM

        public void Init()
        {
            m_journeys = new Dictionary<InstanceID, Journey>();
            m_renderJourneys = new FastList<Journey>();
            m_removeJourneys = new FastList<Journey>();
            m_stepJourneys = new FastList<Journey>();
            m_targets = new HashSet<InstanceID>();
            m_journeysVisible = true;
            Debug.Log("JV.instance.Init has been run");
        }

        private void OnDestroy()
        {
            DestroyJourneys();
            Debug.Log("JV OnDestroy has been run");
        }

        //
        // ************************** the main call
        //


        public void SimulationStep(int subStep)
        {
            //            Debug.Log("PV sim step in JV.instance now entered x " + count);
            //            ++count;
            if (!m_journeysVisible)
                return;
            CitizenManager theCitizenManager = Singleton<CitizenManager>.instance;
            VehicleManager theVehicleManager = Singleton<VehicleManager>.instance;
            InstanceID clickedInstance = Singleton<InstanceManager>.instance.GetSelectedInstance();
            // clickedInstance.Citizen returns either a citizen id (if if is a citizen) else 0 (= not a citizen)
            if (clickedInstance.Citizen != 0U && clickedInstance != m_lastInstance)
            {
                // Debug.Log("Selected a citizen");
                ushort citizenInstanceID = theCitizenManager.m_citizens.m_buffer[clickedInstance.Citizen].m_instance;
                if (citizenInstanceID != 0 && theCitizenManager.m_instances.m_buffer[citizenInstanceID].m_path != 0U)
                {
                    clickedInstance.CitizenInstance = citizenInstanceID;
                    /* PV adds here a test that if a citizen has no m_path (is 0) but their m_vehicle has a path, then PV selects their vehicle
                     * I do not know in precisely what circumstances this could happen (it is NOT for a bicyle or a car, they click as vehicles), 
                     * but I don't want citizens without their own paths, so I just drop them */
                    PreAddInstances();
                    AddInstance(clickedInstance);
                    PostAddInstances();
                    m_lastInstance = clickedInstance;
                    m_journeyRefreshFrame = 0;
                    // Debug.Log("Done pre add and postadd");
                }
            }
            // now a major departure from PV for vehicles - which we here convert to a collection of their passengers
            // note PV has a special procedure for bikes (finds their owners). This is not needed in JV, ALL vehicles have their "passengers" in citizenUnits member
            else if (clickedInstance.Vehicle != 0 && clickedInstance != m_lastInstance)
            {
                // Debug.Log("Selected a new vehicle");
                PreAddInstances();
                ushort vehicleID = theVehicleManager.m_vehicles.m_buffer[clickedInstance.Vehicle].GetFirstVehicle(clickedInstance.Vehicle);   // make sure we start with the leading vehicle for trams etc
                int loopLimit = 0;
                while (vehicleID != 0)
                {
                    uint thisUnit = theVehicleManager.m_vehicles.m_buffer[vehicleID].m_citizenUnits;
                    int loopLimit2 = 0;
                    while (thisUnit != 0U)
                    {
                        uint nextUnit = theCitizenManager.m_units.m_buffer[thisUnit].m_nextUnit;
                        for (int index = 0; index < 5; ++index)
                        {
                            uint citizen = theCitizenManager.m_units.m_buffer[thisUnit].GetCitizen(index);
                            if (citizen != 0U)
                            {
                                InstanceID newID = InstanceID.Empty;
                                newID.CitizenInstance = theCitizenManager.m_citizens.m_buffer[citizen].m_instance;
                                AddInstance(newID);
                                Debug.Log("added citizenInstance " + newID.CitizenInstance);
                            }
                        }
                        thisUnit = nextUnit;
                        if (++loopLimit2 > 524288)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                            break;
                        }
                    }
                    vehicleID = theVehicleManager.m_vehicles.m_buffer[vehicleID].m_trailingVehicle;
                    if (++loopLimit > 16384)
                    {
                        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                        break;
                    }
                }
                PostAddInstances();
                m_lastInstance = clickedInstance;
                m_journeyRefreshFrame = 0;
                // Debug.Log("Done pre add and postadd");
            }
            else if (clickedInstance.NetSegment != 0 || clickedInstance.Building != 0 || (clickedInstance.District != 0 || clickedInstance.Park != 0))
            {
                if (clickedInstance != m_lastInstance)
                {
                    PreAddInstances();
                    AddJourneys(clickedInstance, 0, 256);
                    PostAddInstances();
                    m_lastInstance = clickedInstance;
                    m_journeyRefreshFrame = 0;
                }
                else
                {
                    if (m_journeyRefreshFrame == 0)
                    {
                        PreAddInstances();
                    }
                    AddJourneys(clickedInstance, m_journeyRefreshFrame, m_journeyRefreshFrame + 1);
                    ++m_journeyRefreshFrame;
                    if (m_journeyRefreshFrame >= 256)
                    {
                        PostAddInstances();
                        m_journeyRefreshFrame = 0;
                    }
                }
            }
            // regardless of whether we actually did anything above, always update the step path chain
            // Debug.Log("stepjouryes.m_size: " + m_stepJourneys.m_size);
            for (int index = 0; index < m_stepJourneys.m_size; ++index)
            {
                StepJourney(m_stepJourneys.m_buffer[index]);
            }
        }


        // PreAddInstances is normally called prior to AddInstances - 
        // it marks everything currently in m_paths as not still needed, and m_neededPathCount set to zero

        private void PreAddInstances()
        {
            do
                ;
            while (!Monitor.TryEnter(m_journeys, SimulationManager.SYNCHRONIZE_TIMEOUT));
            try
            {
                using (Dictionary<InstanceID, Journey>.Enumerator enumerator = m_journeys.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                        enumerator.Current.Value.m_stillNeeded = false;
                }
            }
            finally
            {
                Monitor.Exit(m_journeys);
            }
            m_neededJourneysCount = 0;
        }

        // PostAddInstances is called after AddInstances (normally) -
        // mark all the m_paths that are not stillNeeded as canRelease
        // also clear then recreate m_StepPaths with all the Paths that are stillNeeded

        private void PostAddInstances()
        {
            m_stepJourneys.Clear();
            do
                ;
            while (!Monitor.TryEnter(m_journeys, SimulationManager.SYNCHRONIZE_TIMEOUT));
            try
            {
                using (Dictionary<InstanceID, Journey>.Enumerator enumerator = m_journeys.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        Journey journey = enumerator.Current.Value;
                        journey.m_canRelease = !journey.m_stillNeeded;
                        if (journey.m_stillNeeded)
                            m_stepJourneys.Add(journey);
                    }
                }
            }
            finally
            {
                Monitor.Exit(m_journeys);
            }
        }

        // AddJourneys (and AddJourneysImpl) is what happens when you select a road segment or a building (or, apparently, a region or a park)
        // so in this case the main argument InstanceID is called target (it cannot be a citizen or a vehicle else this would not be called)
        // note that AddJourneys does not itself add paths!  It just handles target or non-null target building (and subnodes and subbuildings) adding to m_targets list
        // then it hands over to AddJourneysImpl to do the actual path adding, based on the m_targets (passed implicitly by sharing member)

        private void AddJourneys(InstanceID target, int min, int max)
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

            AddJourneysImpl(min, max);
        }

        // AddJourneysImpl is where we actually add instances to m_paths. This is where we
        // trace through entire journeys of all citizens (and vehicles, in PV) to see if they pass through m_targets
        // the journeys are not rendered at this time (kind of obviously given so many of them are not relevant)
        //
        // I am making a big departure from PV here in that I am only going to look at the paths of citizens
        // BECAUSE their paths include the time they spend in vehicles (and unlike PV, we will not be plotting vehicle paths per se)
        //

        private void AddJourneysImpl(int min, int max)
        {
            PathManager thePathManager = Singleton<PathManager>.instance;
            NetManager theNetManager = Singleton<NetManager>.instance;
            BuildingManager theBuildingManager = Singleton<BuildingManager>.instance;
            DistrictManager theDistrictManager = Singleton<DistrictManager>.instance;
            CitizenManager theCitizenManager = Singleton<CitizenManager>.instance;

            // loop through every citizen journey looking to see if they hit a target in m_targets

            int min256 = (min * 65536) >> 8;    // I am not entirely sure why min and max per calling args are manipulated like this instead of just being set in caller (effect is to multiply by 256, ie by \x100)
            int max256 = (max * 65536) >> 8;    // when called by simulation step for a target segment or building, effect is indices here run from 0 to 65536 (64K, \x10000)
            for (int index1 = min256; index1 < max256; ++index1)
            {
                // for all CitizenInstances that have been created but are are not marked as Deleted or WaitingPath
                CitizenInstance thisCitInst = theCitizenManager.m_instances.m_buffer[index1];
                if ((thisCitInst.m_flags & (CitizenInstance.Flags.Created | CitizenInstance.Flags.Deleted | CitizenInstance.Flags.WaitingPath)) != CitizenInstance.Flags.Created)
                    continue;
                // changed here from PV - JV always starts drawing paths from the very first position, regardless of current position.  Always the most history possible.
                int index2StartVal = 0; // really I do not need to declare this variable now, TBD tidy up
                uint pathChainIdx = thisCitInst.m_path;
                bool addable = false;
                bool hitTarget = false;
                int loopLimit = 0;
                // note that we search through paths in PathManager, not Journeys (we only later create Journeys, for those we need to show)
                while (pathChainIdx != 0U && !addable && !hitTarget)
                {
                    PathUnit thisPathUnit = thePathManager.m_pathUnits.m_buffer[pathChainIdx];
                    int positionCount = thisPathUnit.m_positionCount;
                    for (int index2 = index2StartVal; index2 < positionCount; ++index2)
                    {
                        PathUnit.Position position = thisPathUnit.GetPosition(index2);
                        InstanceID newID = InstanceID.Empty;
                        newID.NetSegment = position.m_segment;      // created a NetSegment InstanceID according to the current pathposition
                        if (m_targets.Contains(newID))    // ie if hit the target (or more accurately, hit one of the targets)
                        {
                            // exclude if the segment has been modified since the path was built (it is then likely the path is not valid any more)
                            if (theNetManager.m_segments.m_buffer[position.m_segment].m_modifiedIndex < thisPathUnit.m_buildIndex)
                            {
                                // In PV there are checks that this is pedestrian or bicycle lane (and for vehicles, PV checks for vehicle lanes)
                                // but for now I omit these checks and just say if you get to the node at all, it is a hit
                                addable = true;
                                break;
                            }
                            hitTarget = true;
                            break;
                        }
                    }
                    index2StartVal = 0;     // when we reach the end of the pathpositions (or if hit target), set up to start the next pathunit in the chain
                    pathChainIdx = thisPathUnit.m_nextPathUnit;     // this is a kind of clunky way to end the loop, but it must eventually hit pathChainIdx==0 (or of course addable or hitTarget) to break out of the while
                    if (++loopLimit >= 262144)
                    {
                        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                        break;
                    }
                }

                // TODO check this next code, surely this simple check if the citizen has the target as their target should come before trailing all the way down their path?
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
                    InstanceID newID = InstanceID.Empty;
                    newID.CitizenInstance = (ushort)index1;
                    AddInstance(newID);
                    // PV stops at 100 paths, but this would not be enough for checking all passengers on metros for example
                    if (m_neededJourneysCount >= 250)
                        break;
                }
            }
        }

        // AddInstance is where a new Journey is created and added to m_journeys
        // (OR: if the journey already exists in m_journeys, it is marked as stillNeeded and not for release)
        // if the journey is new it is also added to stepjourney list (and neededjourneyCount incremented)
        // NOTE - with this call, NO journey or path information is reported (not even start and next unit) - it is just the InstanceID and list maintenance flags

        private void AddInstance(InstanceID id)
        {
            // Debug.Log("Entering AddInstance");
            do
                ;
            while (!Monitor.TryEnter(m_journeys, SimulationManager.SYNCHRONIZE_TIMEOUT));
            {
                try
                {
                    if (m_journeys.TryGetValue(id, out Journey journey1))
                    {
                        journey1.m_stillNeeded = true;
                        journey1.m_canRelease = false;
                        ++m_neededJourneysCount;
                    }
                    else
                    {
                        Journey journey2 = new Journey(id, true, true);  // which sets m_refreshRequired = m_stillNeeded = true
                        m_journeys.Add(id, journey2);
                        m_stepJourneys.Add(journey2);
                        ++m_neededJourneysCount;
                    }
                }
                finally
                {
                    Monitor.Exit(m_journeys);
                }
            }
        }

        // StepJourney is called by SimulationStep (only)
        //   of itself it checks just one Path versus its Journey (ie handles the pathUnit and nextPathUnit indices)
        //   but note it is called in a loop for all the Paths in m_stepPaths, at every simstep
        // If it finds a path that needs refreshing (ie has changed, or was never set, ie is new) then it calls RefreshJourney

        // StepJourney is slightly less critical in JV than in PV.  There, it is always necessary to keep track of when a unit overshoots its current path, and change to the next PathUnit
        // this aspect is absent in JV because the entire path-chain info is stored locally in m_journeys and kept until the journey ends
        // Still, it might happen that as the sim runs, a citizen gets asigned to a new, different path to their currently stored journey (eg the map changes, a transport line is deleted, whatever)
        // In order to be able to test for this as being different to overshoot path, we need to maintain links to current pathUnit and its nextpathunit just like PV does
        // (an unfortunate but necessary overhead, I had thought I could do without this, but can't)

        private void StepJourney(Journey journey)
        {
            // set pathID as the path (index of first unit) the citizen is actually on right now according to the CitizenManager
            uint pathID = Singleton<CitizenManager>.instance.m_instances.m_buffer[journey.m_id.CitizenInstance].m_path;
            // compare it to the pathUnit and nextPathUnit in the stepPaths buffer for that citizen
            if (pathID != 0U)
            {
                if ((int)pathID != (int)journey.m_nextPathUnit)        // if they are not now/yet on the next path unit in the steps chain **OR** m_nextPathUnit is still zero
                {
                    if ((int)pathID != (int)journey.m_pathUnit)        // and they are not still on the current path unit **OR** m_pathUnit is still zero
                        journey.m_refreshRequired = true;              // then they have walked right off the stepPath chain (OR not been rendered yet) and we will have to reset their path/journey from scratch as it were
                }
                else
                // else all good, they have reached the end of the current path unit/started on the next, so we set them up on the next in the chain (which will already have been rendered in this case)
                {
                    journey.m_pathUnit = pathID;
                    journey.m_nextPathUnit = Singleton<PathManager>.instance.m_pathUnits.m_buffer[pathID].m_nextPathUnit;
                }
            }
            // Debug.Log("pathID: " + pathID + " m_nextpathunit: " + journey.m_nextPathUnit + " m_pathunit: " + journey.m_pathUnit+ " refreshRequired: " + journey.m_refreshRequired);
            if (!journey.m_refreshRequired || !RefreshJourney(journey))      // note the 'hidden' call to RefreshJourney here (and use of its return value to set flags accordingly)
                return;
            journey.m_refreshRequired = false;
        }


        // RefreshJourney is called by StepPath if the citizen has wandered off the known path chain in m_stepPaths OR the Path has not been fully set yet
        // (eg for a newly added path for a new selection, or if in between simulation step calls to here, they have been assigned a brand new path)
        // 
        // This function does not itself do any rendering, but the first critical setup of m_meshes() is done here

    // This is also where a Journey first gets filled out from being just m_id and admin flags populated, to having all its components set
    private bool RefreshJourney(Journey journey)
        {
            PathManager thePathManager = Singleton<PathManager>.instance;
            TerrainManager theTerrainManager = Singleton<TerrainManager>.instance;
            CitizenManager theCitizenManager = Singleton<CitizenManager>.instance;

            // Initial failsafe recheck for citizen in limbo still waiting for path, quick return false if so
            CitizenInstance theCitInst = theCitizenManager.m_instances.m_buffer[journey.m_id.CitizenInstance];
            if ((theCitInst.m_flags & CitizenInstance.Flags.WaitingPath) != CitizenInstance.Flags.None)
            {
                return false;
            }

            // Set up the journey (other than its meshes).  The journey already exists in m_journeys, 
            // but now we fill it out with its subjourneys and journeysteps, materials - everything except the mesh, which is set below this
            if (!journey.SetFromPath(theCitInst.m_path))
            {
                return false;
            }

            // Now PV deals with a PV.Path, but in JV we follow each SubJourney
            for (int sjIndex = 0; sjIndex < journey.m_subJourneys.Count; ++sjIndex)
            {
                Journey.SubJourney thisSubJourney = journey.m_subJourneys[sjIndex];


                // ********************************************************************************************************
                // AT THIS POINT NEED TO INSERT TEST FOR SUBJOURNEY ON PUBLIC TRANSPORT and if so do something different
                // ********************************************************************************************************

                // I have no idea at all what a TempUpdateMeshData is for
                TransportLine.TempUpdateMeshData[] data = !thisSubJourney.m_requireSurfaceLine ? new TransportLine.TempUpdateMeshData[1] : new TransportLine.TempUpdateMeshData[81];
                int curveCount = 0;
                float totalLength = 0.0f;
                Vector3 endGridRef = Vector3.zero;

                byte pfFlags = thePathManager.m_pathUnits.m_buffer[journey.m_pathUnit].m_pathFindFlags;  // check for paths found flags in the pathmanager pathunit itself
                if ((pfFlags & 4) != 0)
                    CalculateSubJourneySegmentCount(thisSubJourney, ref data, ref curveCount, ref totalLength, ref endGridRef); //path1 was uint pathID same as journey.m_pathUnit once set up

                if (curveCount != 0)
                {
                    if (thisSubJourney.m_requireSurfaceLine)
                        ++data[theTerrainManager.GetPatchIndex(endGridRef)].m_pathSegmentCount;
                    else
                        ++data[0].m_pathSegmentCount;  // I have no idea why, but anyway: add 1 to the segment count if there were (any number of) curves
                }

                // and now we create the skeleton mesh (as unpopulated members of TempUpdateMeshData). This is a very huge array if you require a surface line (although not 81 full unless every PatchIndex possible got used)
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

                thisSubJourney.m_lineCurves = new Bezier3[curveCount];
                thisSubJourney.m_curveOffsets = new Vector2[curveCount];
                int curveIndex = 0;
                float lengthScale = Mathf.Ceil(totalLength / 64f) / totalLength;
                float currentLength = 0.0f;
                // if (thisSubJourney != 0U && (num1 & 4) != 0) // no idea why we would be here with no path, it does not seem to make sense to test this again
                if ((pfFlags & 4) != 0)
                {
                    Vector3 minPos;
                    Vector3 maxPos;
                    FillSubJourneySegments(thisSubJourney, ref data, thisSubJourney.m_lineCurves, thisSubJourney.m_curveOffsets, ref curveIndex, ref currentLength, lengthScale, out minPos, out maxPos, thisSubJourney.m_requireSurfaceLine, false);
                }

                // FillPathNode adds the circles/nodes at the end of journeys.  I show them only for the final destination (last subJourney)
                if (sjIndex == journey.m_subJourneys.Count - 1 && curveCount != 0)
                {
                    if (thisSubJourney.m_requireSurfaceLine)
                    {
                        int patchIndex = theTerrainManager.GetPatchIndex(endGridRef);
                        TransportLine.FillPathNode(endGridRef, data[patchIndex].m_meshData, data[patchIndex].m_pathSegmentIndex, 4f, !thisSubJourney.m_requireSurfaceLine ? 5f : 20f, true);
                        ++data[patchIndex].m_pathSegmentIndex;
                    }
                    else
                    {
                        TransportLine.FillPathNode(endGridRef, data[0].m_meshData, data[0].m_pathSegmentIndex, 4f, !thisSubJourney.m_requireSurfaceLine ? 5f : 20f, false);
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
                        if (thisSubJourney.m_requireSurfaceLine)
                        {
                            Vector3 min = data[index].m_meshData.m_bounds.min;
                            Vector3 max = data[index].m_meshData.m_bounds.max;
                            max.y += 1024f;
                            data[index].m_meshData.m_bounds.SetMinMax(min, max);
                        }
                        meshDataArray[num2++] = data[index].m_meshData;
                    }
                }
                do
                    ;
                while (!Monitor.TryEnter(thisSubJourney, SimulationManager.SYNCHRONIZE_TIMEOUT));
                try
                {
                    thisSubJourney.m_meshData = meshDataArray;
                    thisSubJourney.m_startCurveIndex = 0;
                    journey.m_subJourneys[sjIndex] = thisSubJourney;
                }
                finally
                {
                    Monitor.Exit(thisSubJourney);
                }
            }
            return true;
        }



        // CalculateJourneySegmentCount started life as a clone of TransportLine.CalculatePathSegmentCount, but here it is journey-fied
        // Here it seemed easiest to just include it here as a new JourneyVisualizer method rather than set up redirections or try to remap path information to journeys
        public static bool CalculateSubJourneySegmentCount(
            Journey.SubJourney subJourney,
            ref TransportLine.TempUpdateMeshData[] data,  // if !m_requireSurfaceLine, data is length 1 and data[0].m_pathSegmentCount has counted the segments (could have done that a lot quicker ...)
            ref int curveCount,  // gets incremented whenever the distance from end of A to start of B is >1, and incremented again if start of B to offsetted position is > 1  [[ 1 would be a straight line I believe ]]
            ref float totalLength,
            ref Vector3 gridPosition)  // gridPosition gets reassigned every step, so ends up pointing to the final lane-offseted journey step, ie end of subJourney (in PV, end of path chain)
        {
            NetManager theNetManager = Singleton<NetManager>.instance;
            TerrainManager theTerrainManager = Singleton<TerrainManager>.instance;
            bool isFirstStep = true;
            Vector3 previousGridPos = Vector3.zero;
            foreach (Journey.SubJourney.JourneyStep thisStep in subJourney.m_journeySteps)
            {
                // here I have brought PathManager.GetLaneID in house. Set up netLaneID as the network global lane ID (eg 123456) corresponding to the lane number (eg 0 to 5) in thisStep.m_lane
                uint netLaneID = theNetManager.m_segments.m_buffer[thisStep.m_segment].m_lanes;
                for (int index = 0; index < thisStep.m_lane && netLaneID != 0U; ++index)
                {
                    netLaneID = theNetManager.m_lanes.m_buffer[netLaneID].m_nextLane;
                }

                // * 0.003921569f is /255 where 255 is the max m_offest value (often seen) -> maps 255 to 1, 128 to 0.5 ish, 0 to 0.  The arg to CalculatePosition is called laneOffset.
                Vector3 gridPosOffseted = theNetManager.m_lanes.m_buffer[netLaneID].CalculatePosition(thisStep.m_offset * 0.003921569f);  // leave for now cos I don't really understand but note in general I do not want current pos offset
                gridPosition = gridPosOffseted;
                if (isFirstStep)
                {
                    previousGridPos = gridPosOffseted;
                    isFirstStep = false;
                }
                else
                {
                    theNetManager.m_lanes.m_buffer[netLaneID].GetClosestPosition(previousGridPos, out Vector3 thisStepClosestGridPos, out float laneOffset);
                    float num2 = Vector3.Distance(thisStepClosestGridPos, previousGridPos);
                    float num3 = Vector3.Distance(gridPosOffseted, thisStepClosestGridPos);
                    if ((double)num2 > 1.0)
                    {
                        int index2 = 0;
                        if (data.Length > 1)
                            index2 = theTerrainManager.GetPatchIndex((previousGridPos + thisStepClosestGridPos) * 0.5f);
                        ++data[index2].m_pathSegmentCount;
                        ++curveCount;
                        totalLength += num2;
                        previousGridPos = thisStepClosestGridPos;
                    }
                    if ((double)num3 > 1.0)
                    {
                        int index2 = 0;
                        if (data.Length > 1)
                            index2 = theTerrainManager.GetPatchIndex((previousGridPos + gridPosOffseted) * 0.5f);  // I don't know what a PatchIndex is, all I know is we get it for the world gridref halfway along the new crowfly from previous to now
                        ++data[index2].m_pathSegmentCount;
                        ++curveCount;
                        totalLength += num3;
                        previousGridPos = gridPosOffseted;
                    }
                }
            }
            return true;
        }

        public static void FillSubJourneySegments(
          Journey.SubJourney subJourney,
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
            bool isFirstStep = true;
            bool flag2 = true;
            bool previousPedestrianOrBikeLane = false;
            var previousStep = new Journey.SubJourney.JourneyStep();
            Vector3 previousStepGridRef = Vector3.zero;
            Vector3 previousStepDirection = Vector3.zero;
            minPos = new Vector3(100000f, 100000f, 100000f);
            maxPos = new Vector3(-100000f, -100000f, -100000f);
            NetManager theNetManager = Singleton<NetManager>.instance;
            TerrainManager theTerrainManager = Singleton<TerrainManager>.instance;

            for (int stepIndex = 0; stepIndex < subJourney.m_journeySteps.Count; ++stepIndex)
            {

                Journey.SubJourney.JourneyStep thisStep = subJourney.m_journeySteps[stepIndex];
                bool onLastStep = stepIndex == subJourney.m_journeySteps.Count - 1;

                NetInfo info = theNetManager.m_segments.m_buffer[thisStep.m_segment].Info;
                // PV checks here !info.m_lanes[thisStep.m_lane].CheckType(laneTypes, vehicleTypes) but in JV of course we
                // have citizens travelling along vehicle lanes (while they are in vehicles) so do not check lane appropriate for pedestrian
                if (info == null || info.m_lanes == null || (info.m_lanes.Length <= thisStep.m_lane))
                    return;
                NetInfo.LaneType laneType = info.m_lanes[thisStep.m_lane].m_laneType;
                VehicleInfo.VehicleType vehicleType = info.m_lanes[thisStep.m_lane].m_vehicleType;
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
                uint laneId = GetLaneID(thisStep);
                float offsetProportion = thisStep.m_offset * 0.003921569f;
                Vector3 thisStepGridRef;
                Vector3 thisStepDirection;
                theNetManager.m_lanes.m_buffer[laneId].CalculatePositionAndDirection(offsetProportion, out thisStepGridRef, out thisStepDirection);
                minPos = Vector3.Min(minPos, thisStepGridRef - new Vector3(4f, 4f, 4f));
                maxPos = Vector3.Max(maxPos, thisStepGridRef + new Vector3(4f, 4f, 4f));
                if (isFirstStep)
                {
                    previousStepGridRef = thisStepGridRef;
                    previousStepDirection = thisStepDirection;
                    isFirstStep = false;
                }
                else
                {
                    Vector3 thisStepClosestPosition;
                    float laneOffset;
                    theNetManager.m_lanes.m_buffer[laneId].GetClosestPosition(previousStepGridRef, out thisStepClosestPosition, out laneOffset);
                    Vector3 rhs = theNetManager.m_lanes.m_buffer[laneId].CalculateDirection(laneOffset);
                    minPos = Vector3.Min(minPos, thisStepClosestPosition - new Vector3(4f, 4f, 4f));
                    maxPos = Vector3.Max(maxPos, thisStepClosestPosition + new Vector3(4f, 4f, 4f));
                    float distanceClosestToPrevious = Vector3.Distance(thisStepClosestPosition, previousStepGridRef);
                    float distanceAlongThisStep = Vector3.Distance(thisStepGridRef, thisStepClosestPosition);
                    if ((double)distanceAlongThisStep > 1.0)
                    {
                        if ((double)offsetProportion < (double)laneOffset)
                        {
                            rhs = -rhs;
                            thisStepDirection = -thisStepDirection;
                        }
                    }
                    else if ((double)offsetProportion > 0.5)
                    {
                        rhs = -rhs;
                        thisStepDirection = -thisStepDirection;
                    }
                    // if both the following if blocks are commented out, no journeys are drawn at all (but the destination nodes DO show)
                    // if this next block is commented out, the curves connecting segments (eg where they join round corners) are not drawn
                    if ((double)distanceClosestToPrevious > 1.0)
                    {
                        ushort thisStartNode = theNetManager.m_segments.m_buffer[thisStep.m_segment].m_startNode;
                        ushort thisEndNode = theNetManager.m_segments.m_buffer[thisStep.m_segment].m_endNode;
                        ushort previousStartNode = theNetManager.m_segments.m_buffer[previousStep.m_segment].m_startNode;
                        ushort previousEndNode = theNetManager.m_segments.m_buffer[previousStep.m_segment].m_endNode;
                        bool changedTransport = (int)thisStartNode != (int)previousStartNode && (int)thisStartNode != (int)previousEndNode && (int)thisEndNode != (int)previousStartNode && (int)thisEndNode != (int)previousEndNode;
                        int index2 = 0;
                        if (data.Length > 1)
                            index2 = theTerrainManager.GetPatchIndex((previousStepGridRef + thisStepClosestPosition) * 0.5f);
                        float overallLength = currentLength + distanceClosestToPrevious;
                        Bezier3 bezier = new Bezier3();
                        if ((isPedOrBikeLine || previousPedestrianOrBikeLane) && previousStep.m_segment == thisStep.m_segment || changedTransport)
                        {
                            Vector3 vector3_3 = VectorUtils.NormalizeXZ(thisStepClosestPosition - previousStepGridRef);
                            bezier.a = previousStepGridRef - vector3_3;
                            bezier.b = previousStepGridRef * 0.7f + thisStepClosestPosition * 0.3f;
                            bezier.c = thisStepClosestPosition * 0.7f + previousStepGridRef * 0.3f;
                            bezier.d = thisStepClosestPosition + vector3_3;
                        }
                        else
                        {
                            bezier.a = previousStepGridRef;
                            bezier.b = previousStepGridRef + previousStepDirection.normalized * (distanceClosestToPrevious * 0.5f);
                            bezier.c = thisStepClosestPosition - rhs.normalized * (distanceClosestToPrevious * 0.5f);
                            bezier.d = thisStepClosestPosition;
                        }
                        TransportLine.FillPathSegment(bezier, data[index2].m_meshData, curves, data[index2].m_pathSegmentIndex, curveIndex, currentLength * lengthScale, overallLength * lengthScale, 4f, !ignoreY ? 5f : 20f, ignoreY);
                        Debug.Log("cuveindex: " + curveIndex + ", currentLength: " + currentLength + ", lengthScale: " + lengthScale);
                        if (curveOffsets != null)
                            curveOffsets[curveIndex] = new Vector2(currentLength * lengthScale, overallLength * lengthScale);
                        ++data[index2].m_pathSegmentIndex;
                        ++curveIndex;
                        currentLength = overallLength;
                        previousStepGridRef = thisStepClosestPosition;
                        previousStepDirection = rhs;
                        flag2 = false;
                    }
                    // if this next block is commented out, what should be straight lines remain as pronounced bezier curves
                    if ((double)distanceAlongThisStep > 1.0)
                    {
                        int index2 = 0;
                        if (data.Length > 1)
                            index2 = theTerrainManager.GetPatchIndex((previousStepGridRef + thisStepGridRef) * 0.5f);
                        float overallLength = currentLength + distanceAlongThisStep;
                        Bezier3 subCurve = theNetManager.m_lanes.m_buffer[laneId].GetSubCurve(laneOffset, offsetProportion);
                        if ((flag2 || onLastStep) && useStopOffset)
                        {
                            float num7 = theNetManager.m_segments.m_buffer[thisStep.m_segment].Info.m_lanes[thisStep.m_lane].m_stopOffset;
                            if ((theNetManager.m_segments.m_buffer[thisStep.m_segment].m_flags & NetSegment.Flags.Invert) != NetSegment.Flags.None)
                                num7 = -num7;
                            if ((double)offsetProportion < (double)laneOffset)
                                num7 = -num7;
                            if (flag2)
                                subCurve.a += Vector3.Cross(Vector3.up, rhs).normalized * num7;
                            if (onLastStep)
                                subCurve.d += Vector3.Cross(Vector3.up, thisStepDirection).normalized * num7;
                        }
                        TransportLine.FillPathSegment(subCurve, data[index2].m_meshData, curves, data[index2].m_pathSegmentIndex, curveIndex, currentLength * lengthScale, overallLength * lengthScale, 4f, !ignoreY ? 5f : 20f, ignoreY);
                        if (curveOffsets != null)
                            curveOffsets[curveIndex] = new Vector2(currentLength * lengthScale, overallLength * lengthScale);
                        ++data[index2].m_pathSegmentIndex;
                        ++curveIndex;
                        currentLength = overallLength;
                        previousStepGridRef = thisStepGridRef;
                        previousStepDirection = thisStepDirection;
                        flag2 = false;
                    }
                }
                previousPedestrianOrBikeLane = isPedOrBikeLine;
                previousStep = thisStep;
            }
        }

        // lookup function journeyfied from PathManager original
        public static uint GetLaneID(Journey.SubJourney.JourneyStep theStep)
        {
            NetManager theNetManager = Singleton<NetManager>.instance;
            uint num = theNetManager.m_segments.m_buffer[theStep.m_segment].m_lanes;
            for (int index = 0; index < theStep.m_lane && num != 0U; ++index)
                num = theNetManager.m_lanes.m_buffer[num].m_nextLane;
            return num;
        }



        // UpdateData marks the entire m_journeys list as not stillNeeded and canRelease (without any testing)
        // a better name for it might be "MarkDataForRelease"

        public void UpdateData()
        {
            do
                ;
            while (!Monitor.TryEnter(m_journeys, SimulationManager.SYNCHRONIZE_TIMEOUT));
            try
            {
                using (Dictionary<InstanceID, Journey>.Enumerator enumerator = m_journeys.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        Journey journey = enumerator.Current.Value;
                        journey.m_stillNeeded = false;
                        journey.m_canRelease = true;
                    }
                }
            }
            finally
            {
                Monitor.Exit(m_journeys);
            }
        }

        // map a Vehicle.Type into a TransportInfo object - this is going to have to change, JV does not have vehicle IDs in the path
        // am going to have to write a new function that returns the info based on a citizen ID and the pathposition-segment determining which vehicle type they will be on

        private TransportInfo GetTransportInfo(Journey path)
        {
            if (path.m_id.Vehicle != 0)
            {
                VehicleInfo info = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[path.m_id.Vehicle].Info;
                if (info != null)
                {
                    VehicleInfo.VehicleType vehicleType = info.m_vehicleType;
                    switch (vehicleType)
                    {
                        case VehicleInfo.VehicleType.Metro:
                            return Singleton<TransportManager>.instance.GetTransportInfo(TransportInfo.TransportType.Metro);
                        case VehicleInfo.VehicleType.Train:
                            return Singleton<TransportManager>.instance.GetTransportInfo(TransportInfo.TransportType.Train);
                        default:
                            if (vehicleType != VehicleInfo.VehicleType.Ship)
                            {
                                if (vehicleType != VehicleInfo.VehicleType.Plane)
                                {
                                    if (vehicleType == VehicleInfo.VehicleType.Tram)
                                        return Singleton<TransportManager>.instance.GetTransportInfo(TransportInfo.TransportType.Tram);
                                    if (vehicleType != VehicleInfo.VehicleType.Helicopter)
                                    {
                                        if (vehicleType != VehicleInfo.VehicleType.Ferry)
                                        {
                                            if (vehicleType == VehicleInfo.VehicleType.Monorail)
                                                return Singleton<TransportManager>.instance.GetTransportInfo(TransportInfo.TransportType.Monorail);
                                            if (vehicleType == VehicleInfo.VehicleType.CableCar)
                                                return Singleton<TransportManager>.instance.GetTransportInfo(TransportInfo.TransportType.CableCar);
                                            if (vehicleType != VehicleInfo.VehicleType.Blimp)
                                                return Singleton<TransportManager>.instance.GetTransportInfo(TransportInfo.TransportType.Bus);
                                        }
                                        else
                                            goto label_13;
                                    }
                                }
                                return Singleton<TransportManager>.instance.GetTransportInfo(TransportInfo.TransportType.Airplane);
                            }
                        label_13:
                            return Singleton<TransportManager>.instance.GetTransportInfo(TransportInfo.TransportType.Ship);
                    }
                }
            }
            else if (path.m_id.CitizenInstance != 0)
                return Singleton<TransportManager>.instance.GetTransportInfo(TransportInfo.TransportType.Bus);
            return null;
        }



        // this is the fundamental controller of whether SimulationStep happens or not (as well as the obvious meaning)
        public bool PathsVisible
        {
            get
            {
                Debug.Log("called PathsVisible.get, it had value = " + m_journeysVisible);
                return m_journeysVisible;
            }
            set
            {
                Debug.Log("Called PathsVisible.set with value = " + value);
                m_journeysVisible = value;
            }
        }


        public void DestroyJourneys()
        {
            do
                ;
            while (!Monitor.TryEnter(m_journeys, SimulationManager.SYNCHRONIZE_TIMEOUT));
            try
            {
                using (Dictionary<InstanceID, Journey>.Enumerator enumerator = m_journeys.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        Journey journey = enumerator.Current.Value;
                        for (int sjIndex = 0; sjIndex < journey.m_subJourneys.Count; ++sjIndex)
                        {
                            Journey.SubJourney thisSubJourney = journey.m_subJourneys[sjIndex];
                            Mesh[] meshes = thisSubJourney.m_meshes;
                            if (meshes != null)
                            {
                                for (int index = 0; index < meshes.Length; ++index)
                                {
                                    Mesh mesh = meshes[index];
                                    if (mesh != null) Destroy(mesh);
                                }
                                thisSubJourney.m_meshes = null;
                            }
                        }
                    }
                }
                m_journeys.Clear();
            }
            finally
            {
                Monitor.Exit(m_journeys);
            }
        }

        // draws all the subjourneys for one journey

        public void RenderJourney(RenderManager.CameraInfo cameraInfo, Journey journey, int layerMask)
        {
            NetManager theNetManager = Singleton<NetManager>.instance;
            TerrainManager theTerrainManager = Singleton<TerrainManager>.instance;
            TransportManager theTransportManager = Singleton<TransportManager>.instance;


            if (journey.m_subJourneys == null)
            {
                Debug.Log("subjourneys is null");
                return;
            }

            for (int sjIndex = 0; sjIndex < journey.m_subJourneys.Count; ++sjIndex)
            {
                Journey.SubJourney thisSubJourney = journey.m_subJourneys[sjIndex];
                // a new journey (not yet rendered) has m_meshdata non-null and m_meshes null; these are switched by UpdateMesh
                if (thisSubJourney.m_meshData != null)
                {
                    thisSubJourney.UpdateMesh();
                }
                Mesh[] meshes = thisSubJourney.m_meshes;
                // Material material = (layerMask & 1 << thisSubJourney.m_layer) != 0 ? thisSubJourney.m_material : thisSubJourney.m_material2;
                Material material = thisSubJourney.m_material2;
                if (meshes == null || !(material != null))
                {
                    return;
                }

                // pretty sure all of this commented out is just about setting the trail behind moving citizens or vehicles (only relevant in PV)

                //Bezier3[] lineCurves = thisSubJourney.m_lineCurves;
                //Vector2[] curveOffsets = thisSubJourney.m_curveOffsets;
                //Vector3 position;
                //Quaternion rotation;
                //Vector3 size;
                //if (lineCurves != null && curveOffsets != null && lineCurves.Length == curveOffsets.Length && InstanceManager.GetPosition(journey.m_id, out position, out rotation, out size))
                //{
                //    for (; lineCurves.Length > thisSubJourney.m_curveIndex; thisSubJourney.m_pathOffset = curveOffsets[thisSubJourney.m_curveIndex++].y)
                //    {
                //        Bezier3 bezier3_1 = lineCurves[thisSubJourney.m_curveIndex];
                //        Bezier3 bezier3_2 = thisSubJourney.m_curveIndex + 1 >= lineCurves.Length ? bezier3_1 : lineCurves[thisSubJourney.m_curveIndex + 1];
                //        float num1;
                //        float num2;
                //        float num3;
                //        float u1;
                //        float num4;
                //        float u2;
                //        float num5;
                //        if (thisSubJourney.m_requireSurfaceLine)
                //        {
                //            num1 = VectorUtils.LengthSqrXZ(position - bezier3_1.a);
                //            num2 = VectorUtils.LengthSqrXZ(position - bezier3_1.d);
                //            num3 = VectorUtils.LengthSqrXZ(bezier3_1.d - bezier3_1.a);
                //            Bezier2 bezier2 = new Bezier2(VectorUtils.XZ(bezier3_1.a), VectorUtils.XZ(bezier3_1.b), VectorUtils.XZ(bezier3_1.c), VectorUtils.XZ(bezier3_1.d));
                //            num4 = bezier2.DistanceSqr(VectorUtils.XZ(position), out u1);
                //            bezier2 = new Bezier2(VectorUtils.XZ(bezier3_2.a), VectorUtils.XZ(bezier3_2.b), VectorUtils.XZ(bezier3_2.c), VectorUtils.XZ(bezier3_2.d));
                //            num5 = bezier2.DistanceSqr(VectorUtils.XZ(position), out u2);
                //        }
                //        else
                //        {
                //            num1 = Vector3.SqrMagnitude(position - bezier3_1.a);
                //            num2 = Vector3.SqrMagnitude(position - bezier3_1.d);
                //            num3 = Vector3.SqrMagnitude(bezier3_1.d - bezier3_1.a);
                //            num4 = bezier3_1.DistanceSqr(position, out u1);
                //            num5 = bezier3_2.DistanceSqr(position, out u2);
                //        }
                //        if (u1 <= 0.990000009536743 && (num2 >= (double)num1 || num1 <= (double)num3))
                //        {
                //            if (num5 < (double)num4 && thisSubJourney.m_curveIndex + 1 < lineCurves.Length)
                //            {
                //                Vector2 vector2 = curveOffsets[++thisSubJourney.m_curveIndex];
                //                thisSubJourney.m_pathOffset = vector2.x + (vector2.y - vector2.x) * u2;
                //                break;
                //            }
                //            Vector2 vector2_1 = curveOffsets[thisSubJourney.m_curveIndex];
                //            thisSubJourney.m_pathOffset = vector2_1.x + (vector2_1.y - vector2_1.x) * u1;
                //            break;
                //        }
                //    }
                //}
                // material.SetFloat(instance3.ID_StartOffset, path.m_pathOffset); trying out draw segment from beginning to end, not from current offset position
                material.color = thisSubJourney.m_lineColorPair.m_color;
                int length = meshes.Length;
                for (int index = 0; index < length; ++index)
                {
                    Mesh mesh = meshes[index];
                    if (mesh != null && cameraInfo.Intersect(mesh.bounds))
                    {
                        material.SetFloat(theTransportManager.ID_StartOffset, -1000f);
                        if (thisSubJourney.m_requireSurfaceLine)
                            theTerrainManager.SetWaterMaterialProperties(mesh.bounds.center, material);
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

        // RenderJourneys is called from outside JV in the manager sim steps at high level
        // (by redirection from PV.RenderPaths)
        // draw all the paths for all the journeys, drawing each subjourney.m_paths (including remove journeys marked for release)

        public void RenderJourneys(RenderManager.CameraInfo cameraInfo, int layerMask)
        {
            // Debug.Log("entering RenderPaths with flag m_pathsVisible = " + m_journeysVisible);
            if (!m_journeysVisible || m_journeys == null || m_journeys.Count == 0)
                return;
            // split m_paths into m_removePaths and m_renderPaths
            do
                ;
            while (!Monitor.TryEnter(m_journeys, SimulationManager.SYNCHRONIZE_TIMEOUT));
            try
            {
                using (Dictionary<InstanceID, Journey>.Enumerator enumerator = m_journeys.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        Journey journey = enumerator.Current.Value;
                        if (journey.m_canRelease)
                            m_removeJourneys.Add(journey);
                        else
                        // else if ((layerMask & 1 << journey.m_layer) != 0 || journey.m_layer2 != -1 && (layerMask & 1 << journey.m_layer2) != 0)
                        // I do not understand what the condition is testing and in any case cannot test for layer at journey level (only can do a subjourney level)
                            m_renderJourneys.Add(journey);
                    }
                }
                for (int index = 0; index < m_removeJourneys.m_size; ++index)
                    m_journeys.Remove(m_removeJourneys.m_buffer[index].m_id);
            }
            finally
            {
                Monitor.Exit(m_journeys);
            }
            // remove paths in m_removePaths and then clear m_removePaths
            for (int index1 = 0; index1 < m_removeJourneys.m_size; ++index1)
            {
                Journey thisJourney = m_removeJourneys.m_buffer[index1];
                for (int sjIndex = 0; sjIndex < thisJourney.m_subJourneys.Count; ++sjIndex)
                {
                    Journey.SubJourney thisSubJourney = thisJourney.m_subJourneys[sjIndex];
                    Mesh[] meshes = thisSubJourney.m_meshes;
                    if (meshes != null)
                    {
                        for (int index2 = 0; index2 < meshes.Length; ++index2)
                        {
                            Mesh mesh = meshes[index2];
                            if (mesh != null) Destroy(mesh);
                        }
                    }
                }
            }
            m_removeJourneys.Clear();

            // draw journeys in m_renderPaths and then clear m_renderPaths when done
            for (int index = 0; index < m_renderJourneys.m_size; ++index)
            {
                Journey journey = m_renderJourneys.m_buffer[index];
                RenderJourney(cameraInfo, journey, layerMask);
            }
            m_renderJourneys.Clear();
        }



        // this is my own function, but it is not enough at the moment, it is not so simple when we really want paths to change colour
        private Color JourneyColor(Journey path)
        {
            //// colour the path per the vehicle if the person is in a vehicle
            //if (path.m_id.Vehicle != 0)
            //{
            //    VehicleManager instance = Singleton<VehicleManager>.instance;
            //    VehicleInfo info = instance.m_vehicles.m_buffer[path.m_id.Vehicle].Info;
            //    if (info != null)
            //    {
            //        switch (info.m_vehicleType)
            //        {
            //            case VehicleInfo.VehicleType.Metro:
            //                return Color.yellow;
            //            case VehicleInfo.VehicleType.Train:
            //                return new Color(1f, 0.5f, 0.0f);
            //            case VehicleInfo.VehicleType.Bicycle:
            //                return new Color(0.0f, 0.5f, 0.0f);
            //            case VehicleInfo.VehicleType.Car:
            //                return new Color(0.5f, 0.0f, 1f);
            //            case VehicleInfo.VehicleType.Tram:
            //                return Color.magenta;
            //            default:
            //                return Color.blue;  // bus is not a VehicleType, very bizarre!  Colour everything like monorails, helicopters, blimps and planes in blue for now
            //        }
            //    }
            //}
            //else if (path.m_id.CitizenInstance != 0)
            //{
            return Color.green; // I am not checking for bikes here, they should be picked up as vehicles above (cos I am not mapping them to citizen like PV does)
                                //}
                                //else return Color.black;
        }


        // IsPathVisble is a helper function called by ALL the vehicleAI classes individually, to determine if the
        // vehicle should be drawn the same colour as its path (makes sense for PV, probabaly makes no sense for JV)

        public bool IsPathVisible(InstanceID id)
        {
            return false;
            //do
            //    ;
            //while (!Monitor.TryEnter(m_paths, SimulationManager.SYNCHRONIZE_TIMEOUT));
            //try
            //{
            //    return m_paths.ContainsKey(id);
            //}
            //finally
            //{
            //    Monitor.Exit(m_paths);
            //}
        }

    }

  //  public static void FillPathSegment(
  //Bezier3 bezier,
  //RenderGroup.MeshData meshData,
  //Bezier3[] curves,
  //int segmentIndex,
  //int curveIndex,
  //float startOffset,
  //float endOffset,
  //float halfWidth,
  //float halfHeight,
  //bool ignoreY)
  //  {
  //      if (curves != null)
  //          curves[curveIndex] = bezier;
  //      if (ignoreY)
  //      {
  //          bezier.a.y = 0.0f;
  //          bezier.b.y = 0.0f;
  //          bezier.c.y = 0.0f;
  //          bezier.d.y = 0.0f;
  //      }
  //      int index1 = segmentIndex * 8;
  //      int num1 = segmentIndex * 30;
  //      float b1 = Mathf.Abs(bezier.a.y - bezier.d.y) * 0.5f;
  //      Vector3 vector3_1 = new Vector3(0.0f, Mathf.Max(Mathf.Max(0.0f, bezier.b.y - bezier.a.y) + halfHeight, b1), 0.0f);
  //      Vector3 vector3_2 = new Vector3(0.0f, Mathf.Min(Mathf.Min(0.0f, bezier.b.y - bezier.a.y) - halfHeight, -b1), 0.0f);
  //      Vector3 vector3_3 = new Vector3(0.0f, Mathf.Max(Mathf.Max(0.0f, bezier.c.y - bezier.d.y) + halfHeight, b1), 0.0f);
  //      Vector3 vector3_4 = new Vector3(0.0f, Mathf.Min(Mathf.Min(0.0f, bezier.c.y - bezier.d.y) - halfHeight, -b1), 0.0f);
  //      Vector3 vector3_5 = new Vector3(startOffset, endOffset, -halfWidth);
  //      Vector4 vector4 = new Vector4(bezier.a.x, bezier.b.x, bezier.c.x, bezier.d.x);
  //      Vector2 vector2_1 = new Vector2(bezier.a.z, bezier.b.z);
  //      Vector2 vector2_2 = new Vector2(bezier.c.z, bezier.d.z);
  //      Vector3 normalized1 = (bezier.d - bezier.a).normalized;
  //      Vector3 normalized2 = Vector3.Cross(Vector3.up, normalized1).normalized;
  //      float a = Vector3.Dot(normalized2, bezier.b - bezier.a);
  //      float b2 = Vector3.Dot(normalized2, bezier.c - bezier.a);
  //      float num2 = Mathf.Min(0.0f, Mathf.Min(a, b2)) - halfWidth;
  //      float num3 = Mathf.Max(0.0f, Mathf.Max(a, b2)) + halfWidth;
  //      Vector3 vector3_6 = bezier.a + normalized2 * num2 - normalized1 * 4f;
  //      Vector3 vector3_7 = bezier.a + normalized2 * num3 - normalized1 * 4f;
  //      Vector3 vector3_8 = bezier.d + normalized2 * num2 + normalized1 * 4f;
  //      Vector3 vector3_9 = bezier.d + normalized2 * num3 + normalized1 * 4f;
  //      meshData.m_vertices[index1] = vector3_6 + vector3_2;
  //      meshData.m_colors[index1] = TransportLine.CalculateVertexColor(meshData.m_vertices[index1], bezier);
  //      meshData.m_normals[index1] = vector3_5;
  //      meshData.m_tangents[index1] = vector4;
  //      meshData.m_uvs[index1] = vector2_1;
  //      meshData.m_uvs2[index1] = vector2_2;
  //      int index2 = index1 + 1;
  //      meshData.m_vertices[index2] = vector3_6 + vector3_1;
  //      meshData.m_colors[index2] = TransportLine.CalculateVertexColor(meshData.m_vertices[index2], bezier);
  //      meshData.m_normals[index2] = vector3_5;
  //      meshData.m_tangents[index2] = vector4;
  //      meshData.m_uvs[index2] = vector2_1;
  //      meshData.m_uvs2[index2] = vector2_2;
  //      int index3 = index2 + 1;
  //      meshData.m_vertices[index3] = vector3_7 + vector3_2;
  //      meshData.m_colors[index3] = TransportLine.CalculateVertexColor(meshData.m_vertices[index3], bezier);
  //      meshData.m_normals[index3] = vector3_5;
  //      meshData.m_tangents[index3] = vector4;
  //      meshData.m_uvs[index3] = vector2_1;
  //      meshData.m_uvs2[index3] = vector2_2;
  //      int index4 = index3 + 1;
  //      meshData.m_vertices[index4] = vector3_7 + vector3_1;
  //      meshData.m_colors[index4] = TransportLine.CalculateVertexColor(meshData.m_vertices[index4], bezier);
  //      meshData.m_normals[index4] = vector3_5;
  //      meshData.m_tangents[index4] = vector4;
  //      meshData.m_uvs[index4] = vector2_1;
  //      meshData.m_uvs2[index4] = vector2_2;
  //      int index5 = index4 + 1;
  //      meshData.m_vertices[index5] = vector3_8 + vector3_4;
  //      meshData.m_colors[index5] = TransportLine.CalculateVertexColor(meshData.m_vertices[index5], bezier);
  //      meshData.m_normals[index5] = vector3_5;
  //      meshData.m_tangents[index5] = vector4;
  //      meshData.m_uvs[index5] = vector2_1;
  //      meshData.m_uvs2[index5] = vector2_2;
  //      int index6 = index5 + 1;
  //      meshData.m_vertices[index6] = vector3_8 + vector3_3;
  //      meshData.m_colors[index6] = TransportLine.CalculateVertexColor(meshData.m_vertices[index6], bezier);
  //      meshData.m_normals[index6] = vector3_5;
  //      meshData.m_tangents[index6] = vector4;
  //      meshData.m_uvs[index6] = vector2_1;
  //      meshData.m_uvs2[index6] = vector2_2;
  //      int index7 = index6 + 1;
  //      meshData.m_vertices[index7] = vector3_9 + vector3_4;
  //      meshData.m_colors[index7] = TransportLine.CalculateVertexColor(meshData.m_vertices[index7], bezier);
  //      meshData.m_normals[index7] = vector3_5;
  //      meshData.m_tangents[index7] = vector4;
  //      meshData.m_uvs[index7] = vector2_1;
  //      meshData.m_uvs2[index7] = vector2_2;
  //      int index8 = index7 + 1;
  //      meshData.m_vertices[index8] = vector3_9 + vector3_3;
  //      meshData.m_colors[index8] = TransportLine.CalculateVertexColor(meshData.m_vertices[index8], bezier);
  //      meshData.m_normals[index8] = vector3_5;
  //      meshData.m_tangents[index8] = vector4;
  //      meshData.m_uvs[index8] = vector2_1;
  //      meshData.m_uvs2[index8] = vector2_2;
  //      int num4 = index8 + 1;
  //      int[] triangles1 = meshData.m_triangles;
  //      int index9 = num1;
  //      int num5 = index9 + 1;
  //      int num6 = num4 - 8;
  //      triangles1[index9] = num6;
  //      int[] triangles2 = meshData.m_triangles;
  //      int index10 = num5;
  //      int num7 = index10 + 1;
  //      int num8 = num4 - 7;
  //      triangles2[index10] = num8;
  //      int[] triangles3 = meshData.m_triangles;
  //      int index11 = num7;
  //      int num9 = index11 + 1;
  //      int num10 = num4 - 6;
  //      triangles3[index11] = num10;
  //      int[] triangles4 = meshData.m_triangles;
  //      int index12 = num9;
  //      int num11 = index12 + 1;
  //      int num12 = num4 - 6;
  //      triangles4[index12] = num12;
  //      int[] triangles5 = meshData.m_triangles;
  //      int index13 = num11;
  //      int num13 = index13 + 1;
  //      int num14 = num4 - 7;
  //      triangles5[index13] = num14;
  //      int[] triangles6 = meshData.m_triangles;
  //      int index14 = num13;
  //      int num15 = index14 + 1;
  //      int num16 = num4 - 5;
  //      triangles6[index14] = num16;
  //      int[] triangles7 = meshData.m_triangles;
  //      int index15 = num15;
  //      int num17 = index15 + 1;
  //      int num18 = num4 - 6;
  //      triangles7[index15] = num18;
  //      int[] triangles8 = meshData.m_triangles;
  //      int index16 = num17;
  //      int num19 = index16 + 1;
  //      int num20 = num4 - 5;
  //      triangles8[index16] = num20;
  //      int[] triangles9 = meshData.m_triangles;
  //      int index17 = num19;
  //      int num21 = index17 + 1;
  //      int num22 = num4 - 2;
  //      triangles9[index17] = num22;
  //      int[] triangles10 = meshData.m_triangles;
  //      int index18 = num21;
  //      int num23 = index18 + 1;
  //      int num24 = num4 - 2;
  //      triangles10[index18] = num24;
  //      int[] triangles11 = meshData.m_triangles;
  //      int index19 = num23;
  //      int num25 = index19 + 1;
  //      int num26 = num4 - 5;
  //      triangles11[index19] = num26;
  //      int[] triangles12 = meshData.m_triangles;
  //      int index20 = num25;
  //      int num27 = index20 + 1;
  //      int num28 = num4 - 1;
  //      triangles12[index20] = num28;
  //      int[] triangles13 = meshData.m_triangles;
  //      int index21 = num27;
  //      int num29 = index21 + 1;
  //      int num30 = num4 - 7;
  //      triangles13[index21] = num30;
  //      int[] triangles14 = meshData.m_triangles;
  //      int index22 = num29;
  //      int num31 = index22 + 1;
  //      int num32 = num4 - 3;
  //      triangles14[index22] = num32;
  //      int[] triangles15 = meshData.m_triangles;
  //      int index23 = num31;
  //      int num33 = index23 + 1;
  //      int num34 = num4 - 5;
  //      triangles15[index23] = num34;
  //      int[] triangles16 = meshData.m_triangles;
  //      int index24 = num33;
  //      int num35 = index24 + 1;
  //      int num36 = num4 - 5;
  //      triangles16[index24] = num36;
  //      int[] triangles17 = meshData.m_triangles;
  //      int index25 = num35;
  //      int num37 = index25 + 1;
  //      int num38 = num4 - 3;
  //      triangles17[index25] = num38;
  //      int[] triangles18 = meshData.m_triangles;
  //      int index26 = num37;
  //      int num39 = index26 + 1;
  //      int num40 = num4 - 1;
  //      triangles18[index26] = num40;
  //      int[] triangles19 = meshData.m_triangles;
  //      int index27 = num39;
  //      int num41 = index27 + 1;
  //      int num42 = num4 - 8;
  //      triangles19[index27] = num42;
  //      int[] triangles20 = meshData.m_triangles;
  //      int index28 = num41;
  //      int num43 = index28 + 1;
  //      int num44 = num4 - 4;
  //      triangles20[index28] = num44;
  //      int[] triangles21 = meshData.m_triangles;
  //      int index29 = num43;
  //      int num45 = index29 + 1;
  //      int num46 = num4 - 7;
  //      triangles21[index29] = num46;
  //      int[] triangles22 = meshData.m_triangles;
  //      int index30 = num45;
  //      int num47 = index30 + 1;
  //      int num48 = num4 - 7;
  //      triangles22[index30] = num48;
  //      int[] triangles23 = meshData.m_triangles;
  //      int index31 = num47;
  //      int num49 = index31 + 1;
  //      int num50 = num4 - 4;
  //      triangles23[index31] = num50;
  //      int[] triangles24 = meshData.m_triangles;
  //      int index32 = num49;
  //      int num51 = index32 + 1;
  //      int num52 = num4 - 3;
  //      triangles24[index32] = num52;
  //      int[] triangles25 = meshData.m_triangles;
  //      int index33 = num51;
  //      int num53 = index33 + 1;
  //      int num54 = num4 - 2;
  //      triangles25[index33] = num54;
  //      int[] triangles26 = meshData.m_triangles;
  //      int index34 = num53;
  //      int num55 = index34 + 1;
  //      int num56 = num4 - 1;
  //      triangles26[index34] = num56;
  //      int[] triangles27 = meshData.m_triangles;
  //      int index35 = num55;
  //      int num57 = index35 + 1;
  //      int num58 = num4 - 4;
  //      triangles27[index35] = num58;
  //      int[] triangles28 = meshData.m_triangles;
  //      int index36 = num57;
  //      int num59 = index36 + 1;
  //      int num60 = num4 - 4;
  //      triangles28[index36] = num60;
  //      int[] triangles29 = meshData.m_triangles;
  //      int index37 = num59;
  //      int num61 = index37 + 1;
  //      int num62 = num4 - 1;
  //      triangles29[index37] = num62;
  //      int[] triangles30 = meshData.m_triangles;
  //      int index38 = num61;
  //      int num63 = index38 + 1;
  //      int num64 = num4 - 3;
  //      triangles30[index38] = num64;
  //  }


}

