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
        private Dictionary<ushort, Journey> m_journeys;
        public int m_journeysCount;
        private HashSet<InstanceID> m_targets;
        private InstanceID m_lastInstance;
        private bool m_journeysVisible;
        private int m_maxJourneysCount;
        private int m_journeyRefreshFrame;
        private readonly object buildLock = new object();
        private readonly object renderLock = new object();
        private bool doneRender;
        private bool doneMeshes;

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
            m_journeys = new Dictionary<ushort, Journey>();
            m_journeysCount = 0;
            m_maxJourneysCount = 500;                       // maybe more or less would be better.  PV has (hardcoded) 100 but that is def not enough for eg a full train.
            m_targets = new HashSet<InstanceID>();
            m_journeysVisible = true;
            m_lastInstance = InstanceID.Empty;
            doneRender = true;                          // doneRender is always true, but DoneRender is locked during rendering (also just in case, RenderJourneys resets it true after rendering)
            doneMeshes = false; 
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
                // for now, selections are frozen (making the whole of the m_journeyRefreshFrame mechanism redundant)
                // but I have left all the mechanisms in place for when I switch to allowing journeys to build up from a segment selection
                // (that is a big departure, because it could then happen that a citizen appears more than once, either actually the same citizen or
                // maybe a new one with the same ref number recycled)
                InstanceID clickedInstance = Singleton<InstanceManager>.instance.GetSelectedInstance();
                if (clickedInstance == m_lastInstance)
                    return;
                doneMeshes = false;
                Debug.Log("Starting new instance");
                if (clickedInstance.Citizen != 0U && clickedInstance != m_lastInstance)
                {
                    // Debug.Log("Selected a citizen");
                    ushort citizenInstanceID = theCitizenManager.m_citizens.m_buffer[clickedInstance.Citizen].m_instance;
                    uint pathID = theCitizenManager.m_instances.m_buffer[citizenInstanceID].m_path;
                    if (citizenInstanceID != 0 && pathID != 0)
                    {
                        WipeSlate();
                        AddJourney(citizenInstanceID);
                        m_lastInstance = clickedInstance;
                        m_journeyRefreshFrame = 0;
                    }
                }
                // now a major departure from PV for vehicles - which we here convert to a collection of their passengers
                // note PV has a special procedure for bikes (finds their owners). This is not needed in JV, ALL vehicles have their "passengers" in citizenUnits member
                else if (clickedInstance.Vehicle != 0 && clickedInstance != m_lastInstance)
                {
                    Debug.Log("Selected a new vehicle");
                    WipeSlate();
                    ushort vehicleID = theVehicleManager.m_vehicles.m_buffer[clickedInstance.Vehicle].GetFirstVehicle(clickedInstance.Vehicle);   // make sure we start with the leading vehicle for trams etc
                    int loopLimit = 0;
                    while (vehicleID != 0)
                    {
                        uint thisUnit = theVehicleManager.m_vehicles.m_buffer[vehicleID].m_citizenUnits;
                        int loopLimit2 = 0;
                        while (thisUnit != 0)
                        {
                            uint nextUnit = theCitizenManager.m_units.m_buffer[thisUnit].m_nextUnit;
                            for (int index = 0; index < 5; ++index)
                            {
                                uint citizen = theCitizenManager.m_units.m_buffer[thisUnit].GetCitizen(index);
                                if (citizen != 0)
                                {
                                    AddJourney(theCitizenManager.m_citizens.m_buffer[citizen].m_instance);
                                    //Debug.Log("added passenger " + theCitizenManager.m_citizens.m_buffer[citizen].m_instance);
                                }
                            }
                            thisUnit = nextUnit;
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
                    Debug.Log("Done vehicle selection");
                    m_lastInstance = clickedInstance;
                    m_journeyRefreshFrame = 0;
                }
                else if (clickedInstance.NetSegment != 0 || clickedInstance.Building != 0 || (clickedInstance.District != 0 || clickedInstance.Park != 0))
                {
                    if (clickedInstance != m_lastInstance)
                    {
                        WipeSlate();
                        AddJourneys(clickedInstance, 0, 256);
                        m_lastInstance = clickedInstance;
                        m_journeyRefreshFrame = 0;
                    }
                    else
                    {
                        if (m_journeyRefreshFrame == 0)
                        {
                            WipeSlate();
                        }
                        AddJourneys(clickedInstance, m_journeyRefreshFrame, m_journeyRefreshFrame + 1);
                        ++m_journeyRefreshFrame;
                        if (m_journeyRefreshFrame >= 256)
                            m_journeyRefreshFrame = 0;
                    }
                }
                if (theStepManager.StepCount > 0)
                    theStepManager.CalculateMeshes();
                doneMeshes = true;
            }
        }


        // AddJourneys (and AddJourneysImpl) is what happens when you select a road segment or a building (or a region or a park)
        // so in this case the main argument InstanceID is called target (it cannot be a citizen or a vehicle else this would not be called)
        // note that AddJourneys does not itself add journeys!  It just handles target or non-null target building (and subnodes and subbuildings) adding to m_targets list
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

        // AddJourneysImpl is where we actually add the journeys that hit the target(s). This is where we
        // trace through entire journeys of all citizens (and vehicles, in PV) to see if they pass through m_targets
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

            // loop through every citizen path (journey) looking to see if they hit a target in m_targets
            int addedJourneysCount = 0;
            int min256 = (min * 65536) >> 8;    // I am not entirely sure why min and max per calling args are manipulated like this instead of just being set in caller (effect is to multiply by 256, ie by \x100)
            int max256 = (max * 65536) >> 8;    // when called by simulation step for a target segment or building, effect is indices here run from 0 to 65536 (64K, \x10000)
            for (int index1 = min256; index1 < max256; ++index1)
            {
                // for all CitizenInstances that have been created but are are not marked as Deleted or WaitingPath
                if (index1 == 65535)
                    Debug.Log("cim index reached 65535");
                CitizenInstance thisCitInst = theCitizenManager.m_instances.m_buffer[index1];
                if ((thisCitInst.m_flags & (CitizenInstance.Flags.Created | CitizenInstance.Flags.Deleted | CitizenInstance.Flags.WaitingPath)) != CitizenInstance.Flags.Created)
                    continue;
                bool addable = false;
                List<Waypoint> route = PathToWaypoints(thisCitInst.m_path, fullroute: true);
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
                            addable = thePathManager.m_pathUnits.m_buffer[thisCitInst.m_path].m_buildIndex > segobject.m_modifiedIndex
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
                    AddJourney((ushort)index1);
                    //Debug.Log("Just called AddJourney for index " + index1);
                    // PV stops at 100 paths, but this would not be enough for checking all passengers on metros for example
                    if (++addedJourneysCount > m_maxJourneysCount)
                        break;
                }
            }
            Debug.Log("Done segment-type selection");
        }

        // AddJourney is where a new Journey is created and added to m_journeys
        // Creating the journey also creates and populates the corresponding JourneySteps, including the meshes
        // the only thing still to do is actually draw the meshes

        private void AddJourney(ushort citizenID)
        {
            Journey journey = new Journey(citizenID);
            if (journey.m_steps.Count > 0)
            {
                m_journeys.Add(citizenID, journey);
                ++m_journeysCount;                              // not sure if this is (now) used often enough in anger to be worth maintaining when m_journeys.Count would do
            }
        }


        // a "fullroute" is every waypoint on the overland journey; it does not distinguish where this substituted for public transport steps
        // as such it is difficult to process in journey creation - but very good for use with a segment or district selection looking for targets
        // there is unfortunate redundancy in that when we are looking for segments as targets, we calculate the fullroute, but then
        // later for those citizens selected, we recalculate the raw list (FYI PV also runs through the paths twice; in fact PV does it a third time
        // when creating the meshes)
        public List<Waypoint> PathToWaypoints(uint pathID, bool fullroute = false)
        {
            if (pathID == 0)
                return null;
            PathManager thePathManager = Singleton<PathManager>.instance;
            NetManager theNetManager = Singleton<NetManager>.instance;
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
                    if (fullroute)
                    {
                        var landpath = theNetManager.m_segments.m_buffer[thisPathPosition.m_segment].m_path;
                        if (landpath == 0)
                            outlist.Add(new Waypoint(thisPathPosition));
                        else
                        {
                            var landroute = PathToWaypoints(landpath);
                            if (landroute == null)
                                return null;
                            if (outlist == null)
                                outlist = landroute;
                            else
                                outlist.AddRange(landroute);
                        }
                    }
                    else
                    {
                        outlist.Add(new Waypoint(thisPathPosition));
                    }
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
            if (DoneRender)
                theStepManager.WipeSlate();
            Debug.Log("JV: done call to theStepManager.WipeSlate");
            lock (m_journeys)
            {
                m_journeys.Clear();
                m_journeysCount = 0;
            }
            Debug.Log("JV: finished JV.WipeSlate");
        }

        // network manager insists on calling this, so it must exist
        // if I were to allow for threading, this could be where to tell the current JSteps to calculate their route meshes
        public void UpdateData()
        {
            Debug.Log("JV: called UpdateData");
            lock (m_journeys)
            {
                ushort dummy;
                foreach (Journey j in m_journeys.Values)
                    dummy = j.m_id;
            }
            //while (!Monitor.TryEnter(JourneyStepMgr.instance, SimulationManager.SYNCHRONIZE_TIMEOUT)) { }
            //try
            //{
            //    foreach (JourneyStep jStep in JourneyStepMgr.instance.StepCollection.Values)
            //    {
            //        string dummy = jStep.Hashname;
            //    }
            //}
            //finally
            //{
            //    Monitor.Exit(JourneyStepMgr.instance);
            //}
        }


        // IsPathVisble is a helper function called by ALL the vehicleAI classes individually, to determine if the
        // vehicle should be drawn the same colour as its path (makes sense for PV, not always for JV)

        public bool IsPathVisible(InstanceID id)
        {
            return false;
        }

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




