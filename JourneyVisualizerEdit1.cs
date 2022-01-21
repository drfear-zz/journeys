// Decompiled with JetBrains decompiler
// Type: PathVisualizer
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 9724B8F4-19DD-48C3-AE02-CDA150D75CEC
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Cities_Skylines\Cities_Data\Managed\Assembly-CSharp.dll

using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.PlatformServices;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Journeys.RedirectionFramework.Attributes;


namespace Journeys {

    [TargetType(typeof(PathVisualizer))]
    public class JourneyVisualizer : MonoBehaviour
    {
        private Dictionary<InstanceID, Path> m_paths;
        private FastList<Path> m_renderPaths;
        private FastList<Path> m_removePaths;
        private FastList<Path> m_stepPaths;
        // m_targets is used by AddPaths to store a list of target buildings (with subbuildings and subnodes)
        private HashSet<InstanceID> m_targets;
        private InstanceID m_lastInstance;
        private bool m_pathsVisible;
        private int m_neededPathCount;
        private int m_pathRefreshFrame;


        private void Awake()
          {
            m_paths = new Dictionary<InstanceID, Path>();
            m_renderPaths = new FastList<Path>();
            m_removePaths = new FastList<Path>();
            m_stepPaths = new FastList<Path>();
            m_targets = new HashSet<InstanceID>();
          }

        private void OnDestroy()
          {
            DestroyPaths();
          }

        // For normal PV, PathsVisible is set in InfoManager.cs 
        // Singleton<NetManager>.instance.PathVisualizer.PathsVisible = flag16;
        // where flag16 is set for InfoManager.Infomode.TraficRoutes with submode InfoManager.SubInfoMode.Default
        // we need to call SetActualMode(InfoManager.InfoMode.Underground, InfoManager.SubInfoMode.Default)
        // but that is a private method... the public interface is SetCurrentMode
        // setting a new type of mode would seriously upset the toggling of the UI - better to go for TrafficRoutes mode
        // but if I can disable PathVisualizer.PathsVisible (and probably DestroyPaths) as soon as I can

        // this is the fundamental controller of whether SimulationStep happens or not (as well as the obvious meaning)
        public bool PathsVisible
          {
            get
            {
              return m_pathsVisible;
            }
            set
            {
              m_pathsVisible = value;
            }
          }

        public void DestroyPaths()
          {
            do
              ;
            while (!Monitor.TryEnter(m_paths, SimulationManager.SYNCHRONIZE_TIMEOUT));
            try
            {
              using (Dictionary<InstanceID, Path>.Enumerator enumerator = m_paths.GetEnumerator())
              {
                while (enumerator.MoveNext())
                {
                  Path path = enumerator.Current.Value;
                  Mesh[] meshes = path.m_meshes;
                  if (meshes != null)
                  {
                    for (int index = 0; index < meshes.Length; ++index)
                    {
                      Mesh mesh = meshes[index];
                      if (mesh != null) Destroy(mesh);
                    }
                    path.m_meshes = null;
                  }
                }
              }
              m_paths.Clear();
            }
            finally
            {
              Monitor.Exit(m_paths);
            }
          }

        // draws a single path (one node to the next)
        public void RenderPath(RenderManager.CameraInfo cameraInfo, Path path, bool secondary)
          {
            NetManager instance1 = Singleton<NetManager>.instance;
            TerrainManager instance2 = Singleton<TerrainManager>.instance;
            TransportManager instance3 = Singleton<TransportManager>.instance;
            if (path.m_meshData != null)
              UpdateMesh(path);
            Mesh[] meshes = path.m_meshes;
            Material material = !secondary ? path.m_material : path.m_material2;
            if (meshes == null || !(material != null))
              return;
            Bezier3[] lineCurves = path.m_lineCurves;
            Vector2[] curveOffsets = path.m_curveOffsets;
            Vector3 position;
            Quaternion rotation;
            Vector3 size;
            if (lineCurves != null && curveOffsets != null && (lineCurves.Length == curveOffsets.Length && InstanceManager.GetPosition(path.m_id, out position, out rotation, out size)))
            {
              for (; lineCurves.Length > path.m_curveIndex; path.m_pathOffset = curveOffsets[path.m_curveIndex++].y)
              {
                Bezier3 bezier3_1 = lineCurves[path.m_curveIndex];
                Bezier3 bezier3_2 = path.m_curveIndex + 1 >= lineCurves.Length ? bezier3_1 : lineCurves[path.m_curveIndex + 1];
                float num1;
                float num2;
                float num3;
                float u1;
                float num4;
                float u2;
                float num5;
                if (path.m_requireSurfaceLine)
                {
                  num1 = VectorUtils.LengthSqrXZ(position - bezier3_1.a);
                  num2 = VectorUtils.LengthSqrXZ(position - bezier3_1.d);
                  num3 = VectorUtils.LengthSqrXZ(bezier3_1.d - bezier3_1.a);
                  Bezier2 bezier2 = new Bezier2(VectorUtils.XZ(bezier3_1.a), VectorUtils.XZ(bezier3_1.b), VectorUtils.XZ(bezier3_1.c), VectorUtils.XZ(bezier3_1.d));
                  num4 = bezier2.DistanceSqr(VectorUtils.XZ(position), out u1);
                  bezier2 = new Bezier2(VectorUtils.XZ(bezier3_2.a), VectorUtils.XZ(bezier3_2.b), VectorUtils.XZ(bezier3_2.c), VectorUtils.XZ(bezier3_2.d));
                  num5 = bezier2.DistanceSqr(VectorUtils.XZ(position), out u2);
                }
                else
                {
                  num1 = Vector3.SqrMagnitude(position - bezier3_1.a);
                  num2 = Vector3.SqrMagnitude(position - bezier3_1.d);
                  num3 = Vector3.SqrMagnitude(bezier3_1.d - bezier3_1.a);
                  num4 = bezier3_1.DistanceSqr(position, out u1);
                  num5 = bezier3_2.DistanceSqr(position, out u2);
                }
                if (u1 <= 0.990000009536743 && (num2 >= (double) num1 || num1 <= (double) num3))
                {
                  if (num5 < (double) num4 && path.m_curveIndex + 1 < lineCurves.Length)
                  {
                    Vector2 vector2 = curveOffsets[++path.m_curveIndex];
                    path.m_pathOffset = vector2.x + (vector2.y - vector2.x) * u2;
                    break;
                  }
                  Vector2 vector2_1 = curveOffsets[path.m_curveIndex];
                  path.m_pathOffset = vector2_1.x + (vector2_1.y - vector2_1.x) * u1;
                  break;
                }
              }
            }
            material.color = path.m_color;
            material.SetFloat(instance3.ID_StartOffset, path.m_pathOffset);
            int length = meshes.Length;
            for (int index = 0; index < length; ++index)
            {
              Mesh mesh = meshes[index];
              if (mesh != null && cameraInfo.Intersect(mesh.bounds))
              {
                if (path.m_requireSurfaceLine)
                  instance2.SetWaterMaterialProperties(mesh.bounds.center, material);
                if (material.SetPass(0))
                {
                  ++instance1.m_drawCallData.m_overlayCalls;
                  Graphics.DrawMeshNow(mesh, Matrix4x4.identity);
                }
              }
            }
          }

        // draw all the paths in m_paths (including remove those marked for release)
        public void RenderPaths(RenderManager.CameraInfo cameraInfo, int layerMask)
          {
            if (!m_pathsVisible)
              return;
            // split m_paths into m_removePaths and m_renderPaths
            while (!Monitor.TryEnter(m_paths, SimulationManager.SYNCHRONIZE_TIMEOUT))
            {
                try
                {
                  using (Dictionary<InstanceID, Path>.Enumerator enumerator = m_paths.GetEnumerator())
                  {
                    while (enumerator.MoveNext())
                    {
                      Path path = enumerator.Current.Value;
                      if (path.m_canRelease)
                        m_removePaths.Add(path);
                      else if ((layerMask & 1 << path.m_layer) != 0 || path.m_layer2 != -1 && (layerMask & 1 << path.m_layer2) != 0)
                        m_renderPaths.Add(path);
                    }
                  }
                  for (int index = 0; index < m_removePaths.m_size; ++index)
                    m_paths.Remove(m_removePaths.m_buffer[index].m_id);
                }
                finally
                {
                  Monitor.Exit(m_paths);
                }
            }
            // remove paths in m_removePaths and then clear m_removePaths
            for (int index1 = 0; index1 < m_removePaths.m_size; ++index1)
            {
              Mesh[] meshes = m_removePaths.m_buffer[index1].m_meshes;
              if (meshes != null)
              {
                for (int index2 = 0; index2 < meshes.Length; ++index2)
                {
                  Mesh mesh = meshes[index2];
                  if (mesh != null) Destroy(mesh);
                }
              }
            }
            m_removePaths.Clear();
            // draw paths in m_renderPaths and then clear m_renderPaths when done
            for (int index = 0; index < m_renderPaths.m_size; ++index)
            {
              Path path = m_renderPaths.m_buffer[index];
              RenderPath(cameraInfo, path, (layerMask & 1 << path.m_layer) == 0);
            }
            m_renderPaths.Clear();
          }

        // it is the mesh that is actually drawn, the mesh is set up from the path
        private void UpdateMesh(Path path)
          {
            while (!Monitor.TryEnter(path, SimulationManager.SYNCHRONIZE_TIMEOUT))
            {
                RenderGroup.MeshData[] meshData;
                try
                {
                  meshData = path.m_meshData;
                  path.m_curveIndex = path.m_startCurveIndex;
                  path.m_meshData = null;
                }
                finally
                {
                  Monitor.Exit(path);
                }
            }
            if (meshData == null)
              return;

            // here is where we set the colour to draw the curve - this I do differently to PV
            path.m_color = JourneyColor(path);

            // then back to PV code
            Mesh[] meshArray1 = path.m_meshes;
            int a = 0;
            if (meshArray1 != null)
              a = meshArray1.Length;
            if (a != meshData.Length)
            {
              Mesh[] meshArray2 = new Mesh[meshData.Length];
              int num = Mathf.Min(a, meshArray2.Length);
              for (int index = 0; index < num; ++index)
                meshArray2[index] = meshArray1[index];
              for (int index = num; index < meshArray2.Length; ++index)
                meshArray2[index] = new Mesh();
              for (int index = num; index < a; ++index)
                UnityEngine.Object.Destroy(meshArray1[index]);
              meshArray1 = meshArray2;
              path.m_meshes = meshArray1;
            }
            for (int index = 0; index < meshData.Length; ++index)
            {
              meshArray1[index].Clear();
              meshArray1[index].vertices = meshData[index].m_vertices;
              meshArray1[index].normals = meshData[index].m_normals;
              meshArray1[index].tangents = meshData[index].m_tangents;
              meshArray1[index].uv = meshData[index].m_uvs;
              meshArray1[index].uv2 = meshData[index].m_uvs2;
              meshArray1[index].colors32 = meshData[index].m_colors;
              meshArray1[index].triangles = meshData[index].m_triangles;
              meshArray1[index].bounds = meshData[index].m_bounds;
            }
          }

        private Color JourneyColor(Path path)
        {
            // colour the path per the vehicle if the person is in a vehicle
            if (path.m_id.Vehicle != 0)
            {
                VehicleManager instance = Singleton<VehicleManager>.instance;
                VehicleInfo info = instance.m_vehicles.m_buffer[path.m_id.Vehicle].Info;
                if (info != null)
                {
                    switch (info.m_vehicleType)
                    {
                        case VehicleInfo.VehicleType.Metro:
                            return Color.yellow;
                        case VehicleInfo.VehicleType.Train:
                            return new Color(1f, 0.5f, 0.0f);
                        case VehicleInfo.VehicleType.Bicycle:
                            return new Color(0.0f, 0.5f, 0.0f);
                        case VehicleInfo.VehicleType.Car:
                            return new Color(0.5f, 0.0f, 1f);
                        case VehicleInfo.VehicleType.Tram:
                            return Color.magenta;
                        default:
                            return Color.blue;  // bus is not a VehicleType, very bizarre!  Colour everything like monorails, helicopters, blimps and planes in blue for now
                    }
                }
            }
            else if (path.m_id.CitizenInstance != 0)
            {
                return Color.green; // I am not checking for bikes here, they should be picked up as vehicles above (cos I am not mapping them to citizen like PV does)
            }
            else return Color.black;
        }

        // In this "main" routine we setup paths according to the current GUI selection
        public void SimulationStep(int subStep)
          {
            if (!m_pathsVisible)
              return;
            CitizenManager theCitizenManager = Singleton<CitizenManager>.instance;
            VehicleManager theVehicleManager = Singleton<VehicleManager>.instance;
            InstanceID instanceId = Singleton<InstanceManager>.instance.GetSelectedInstance();
            // I don't understand why the Selected should be a Citizen rather than a CitizenInstance, given the name of the function
            // I am also very baffled that it appears a citizen without a path (I can see that will sometimes happen, fair enough)
            // would be likely to be in a vehicle (because if you selected a vehicle, it would show as instanceID.Vehicle)
            // UNLESS a citizen on a bicycle selects as a citizen without a path (because the bicycle knows the path?)
            // I will assume this latter, and in this case assign the path back to the citizen - might work
            if (instanceId.Citizen != 0U)       // I am sticking with this per PV but I really expected a CitizenInstance here
            {
                ushort citizenInstanceID = theCitizenManager.m_citizens.m_buffer[instanceId.Citizen].m_instance;
                if (citizenInstanceID != 0 && theCitizenManager.m_instances.m_buffer[citizenInstanceID].m_path != 0U)
                {
                    instanceId.CitizenInstance = citizenInstanceID;
                }
                else
                {
                    ushort vehicleID = theCitizenManager.m_citizens.m_buffer[instanceId.Citizen].m_vehicle;
                    uint vehiclePath = theVehicleManager.m_vehicles.m_buffer[vehicleID].m_path;
                    if (vehicleID != 0 && vehiclePath != 0U)
                    {
                        instanceId.CitizenInstance = citizenInstanceID;
                        theCitizenManager.m_instances.m_buffer[citizenInstanceID].m_path = vehiclePath;
                    }
                }
                if (instanceId != m_lastInstance)
                {
                    PreAddInstances();
                    AddInstance(instanceId);
                    PostAddInstances();
                    m_lastInstance = instanceId;
                    m_pathRefreshFrame = 0;
                }
            }
            // now a major departure from PV for vehicles - which we here convert to a collection of their passengers
            else if (instanceId.Vehicle != 0)
            {
                if (instanceId != m_lastInstance)
                {
                    PreAddInstances();
                    ushort vehicleID = theVehicleManager.m_vehicles.m_buffer[instanceId.Vehicle].GetFirstVehicle(instanceId.Vehicle);   // make sure we start with the leading vehicle for trams etc
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
                    m_lastInstance = instanceId;
                    m_pathRefreshFrame = 0;
                }
            }
            else if (instanceId.NetSegment != 0 || instanceId.Building != 0 || (instanceId.District != 0 || instanceId.Park != 0))
            {
                if (instanceId != m_lastInstance)
                {
                    PreAddInstances();
                    AddPaths(instanceId, 0, 256);
                    PostAddInstances();
                    m_lastInstance = instanceId;
                    m_pathRefreshFrame = 0;
                }
                else
                {
                    if (m_pathRefreshFrame == 0)
                    {
                        PreAddInstances();
                    }
                    AddPaths(instanceId, m_pathRefreshFrame, m_pathRefreshFrame + 1);
                    ++m_pathRefreshFrame;
                    if (m_pathRefreshFrame >= 256)
                    {
                        PostAddInstances();
                        m_pathRefreshFrame = 0;
                    }
                }
            }
            // regardless of whether we actually did anything above, always update the step path chain
            for (int index = 0; index < m_stepPaths.m_size; ++index)
            {
                StepPath(m_stepPaths.m_buffer[index]);
            }
        }


        // PreAddInstances is sometimes called prior to AddInstances - 
        // it marks everything currently in m_paths as not still needed, and m_neededPathCount set to zero
        private void PreAddInstances()
          {
            while (!Monitor.TryEnter(m_paths, SimulationManager.SYNCHRONIZE_TIMEOUT));
            try
            {
              using (Dictionary<InstanceID, Path>.Enumerator enumerator = m_paths.GetEnumerator())
              {
                while (enumerator.MoveNext())
                  enumerator.Current.Value.m_stillNeeded = false;
              }
            }
            finally
            {
              Monitor.Exit(m_paths);
            }
            m_neededPathCount = 0;
          }

        // called after AddInstances (sometimes) -
        // mark all the m_paths that are no stillNeeded as canRelease
        // also clear then recreate m_StepPaths with all the Paths are stillNeeded
        private void PostAddInstances()
          {
            m_stepPaths.Clear();
            do
              ;
            while (!Monitor.TryEnter(m_paths, SimulationManager.SYNCHRONIZE_TIMEOUT));
            try
            {
              using (Dictionary<InstanceID, Path>.Enumerator enumerator = m_paths.GetEnumerator())
              {
                while (enumerator.MoveNext())
                {
                  Path path = enumerator.Current.Value;
                  path.m_canRelease = !path.m_stillNeeded;
                  if (path.m_stillNeeded)
                    m_stepPaths.Add(path);
                }
              }
            }
            finally
            {
              Monitor.Exit(m_paths);
            }
          }

        // AddPaths (and AddPathsImpl) is what happens when you select a road segment or a building (or, apparently, a region or a park)
        // so in this case the main argument InstanceID is called target (it cannot be a citizen or a vehicle else this would not be called)
        // note that AddPaths does not itself add paths!  It just handles target or non-null target building (and subnodes and subbuildings) adding to m_targets list
        // then it hands over to AddPathsImpl to do the actual path adding, based on the m_targets (passed implicitly by sharing member)
        private void AddPaths(InstanceID target, int min, int max)
         {
            // m_targets is a SET of targets, not quite sure why that is needed
            m_targets.Clear();
            // if target is not a building, the only thing AddPaths does is to add target to m_targets (after having cleared it first)
            if (target.Building == 0)
              m_targets.Add(target);
            else
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
            }

            AddPathsImpl(min, max);
        }

        //
        // AddPathsImpl is where we actually add instances to m_paths. This is where we
        // trace through entire journeys of all citizens (and vehicles, in PV) to see if they pass through m_targets
        // the journeys are not rendered at this time (kind of obviously given so many of them are not relevant)
        //
        // I am making my first big departure from PV here in that I am only going to look at the paths of citzens
        // making here the big assumption that their paths include the time they spend in vehicles
        //
        private void AddPathsImpl(int min, int max)
          {
            PathManager thePathManager = Singleton<PathManager>.instance;
            NetManager theNetManager = Singleton<NetManager>.instance;
            BuildingManager theBuildingManager = Singleton<BuildingManager>.instance;
            DistrictManager theDistrictManager = Singleton<DistrictManager>.instance;
            CitizenManager theCitizenManager = Singleton<CitizenManager>.instance;

            // loop through every citizen journey looking to see if they hit a target in m_targets

            int min256 = (min * 65536) >> 8;    // I have no idea why min and max per calling args are manipulated like this (effect is to multiply by 256)
            int max256 = (max * 65536) >> 8;
            for (int index1 = min256; index1 < max256; ++index1)
            {
                // for all CitizenInstances that have been created but are are not marked as Deleted or WaitingPath
                CitizenInstance thisCInst = theCitizenManager.m_instances.m_buffer[index1];
                if ((thisCInst.m_flags & (CitizenInstance.Flags.Created | CitizenInstance.Flags.Deleted | CitizenInstance.Flags.WaitingPath)) != CitizenInstance.Flags.Created)
                    continue;
                // Here in PV we only filter through citizens who are not in a vehicle (other than a bicycle) but I am not going to do that filtering
                int pathPositionIndex = thisCInst.m_pathPositionIndex;
                int index2StartVal = pathPositionIndex != byte.MaxValue ? pathPositionIndex >> 1 : 0;
                uint pathChainIdx = thisCInst.m_path;
                bool addable = false;
                bool hitTarget = false;
                int loopLimit = 0;
                while (pathChainIdx != 0U && !addable && !hitTarget)
                {
                    PathUnit thisPathUnit = thePathManager.m_pathUnits.m_buffer[pathChainIdx];
                    int positionCount = thisPathUnit.m_positionCount;
                    for (int index2 = index2StartVal; index2 < positionCount; ++index2)
                    {
                        PathUnit.Position position = thisPathUnit.GetPosition(index2);
                        InstanceID newID = InstanceID.Empty;
                        newID.NetSegment = position.m_segment;
                        if (m_targets.Contains(newID))    // hit the target
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
                    index2StartVal = 0;
                    pathChainIdx = thisPathUnit.m_nextPathUnit;
                    if (++loopLimit >= 262144)
                    {
                        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                        break;
                    }
                }
                // TODO surely this simple check if the citizen has the target as their target should come before trailing all the way down their path?
                InstanceID targetId = thisCInst.Info.m_citizenAI.GetTargetID((ushort) index1, ref thisCInst);
                bool addable = addable | m_targets.Contains(targetId);
                if (targetId.Building != 0)
                {
                    Vector3 position = theBuildingManager.m_buildings.m_buffer[targetId.Building].m_position;
                    InstanceID newID = InstanceID.Empty;
                    newID.District = theDistrictManager.GetDistrict(position);
                    bool addable = addable | m_targets.Contains(newID);
                    newID.Park = theDistrictManager.GetPark(position);
                    addable = addable | m_targets.Contains(newID);
                }
                if (targetId.NetNode != 0)
                {
                    Vector3 position = theNetManager.m_nodes.m_buffer[targetId.NetNode].m_position;
                    InstanceID newID = InstanceID.Empty;
                    newID.District = theDistrictManager.GetDistrict(position);
                    bool addable = addable | m_targets.Contains(newID);
                    newID.Park = theDistrictManager.GetPark(position);
                    addable = addable | m_targets.Contains(newID);
                }
                if (addable)
                {
                    InstanceID newID = InstanceID.Empty;
                    newID.CitizenInstance = (ushort) index1;
                    AddInstance(newID);     // here, finally, is the add to m_paths
                    // we stop at 100 paths (although I think this might not be enough for checking passengers on metros for example)
                    if (m_neededPathCount >= 100)
                    break;
                }
            }
        }

        // AddInstance is where a new Path is created and added to m_paths
        // (or if the path already exists in m_paths, it is marked as stillNeeded and not for release)
        // if the Path is new it is also added to stepPaths list (and neededPathCount incremented)
        // NOTE - at this point, NO path information is reported (not even start and next unit) - it is just the InstanceID and flags
        private void AddInstance(InstanceID id)
          {
            while (!Monitor.TryEnter(m_paths, SimulationManager.SYNCHRONIZE_TIMEOUT))
            {
                try
                {
                    if (m_paths.TryGetValue(id, out Path path1))
                    {
                        path1.m_stillNeeded = true;
                        path1.m_canRelease = false;
                        ++m_neededPathCount;
                    }
                    else
                    {
                        Path path2 = new Path
                        {
                            m_id = id,
                            m_stillNeeded = true,
                            m_refreshRequired = true
                        };
                        m_paths.Add(id, path2);
                        m_stepPaths.Add(path2);
                        ++m_neededPathCount;
                    }
                }
                finally
                {
                  Monitor.Exit(m_paths);
                }
            }
        }

        // StepPath is called by SimulationStep (only)
        // of itself it steps just one Path (ie handles the pathUnit and nextPathUnit indices - that is all it does)
        // but note it is called in a loop for all the Paths in m_stepPaths
        private void StepPath(Path path)
          {
            InstanceID id = path.m_id;      // for JV this can only be a CitizenInstance ID because nothing else is ever added to the list
            // set pathID as the path (index of first unit) the citizen is actually on right now
            uint pathID = Singleton<CitizenManager>.instance.m_instances.m_buffer[id.CitizenInstance].m_path;
            // compare it to the pathUnit and nextPathUnit in the stepPaths buffer for that citizen
            if (pathID != 0U)
            {
                if ((int)pathID != (int)path.m_nextPathUnit)        // if they are not now/yet on the next path unit in the steps chain
                {
                    if ((int)pathID != (int)path.m_pathUnit)        // and they are not still on the current path unit
                        path.m_refreshRequired = true;              // then they have walked right off the stepPath chain and we will have to reset their path from scratch as it were
                }
                else
                // else all good, they have reached the end of the current path unit/started on the next, so we set them up on the next in the chain
                {
                    path.m_pathUnit = pathID;
                    path.m_nextPathUnit = Singleton<PathManager>.instance.m_pathUnits.m_buffer[pathID].m_nextPathUnit;
                }
            }
            if (!path.m_refreshRequired || !RefreshPath(path))
              return;
            path.m_refreshRequired = false;
          }

        private TransportInfo GetTransportInfo(Path path)
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


        // RefreshPath is called by StepPath if the citizen has wandered off the known path chain in m_stepPaths
        // (eg in between simulation step calls to here, they have moved 2 or more steps down their path)
        private bool RefreshPath(Path path)
          {
            TransportManager theTransportManager = Singleton<TransportManager>.instance;
            NetManager theNetManager = Singleton<NetManager>.instance;
            PathManager thePathManager = Singleton<PathManager>.instance;
            TerrainManager theTerrainManager = Singleton<TerrainManager>.instance;
            VehicleManager theVehicleManager = Singleton<VehicleManager>.instance;
            CitizenManager theCitizenManager = Singleton<CitizenManager>.instance;
            if (path.m_id.Vehicle != 0)
            {
              if ((theVehicleManager.m_vehicles.m_buffer[path.m_id.Vehicle].m_flags & Vehicle.Flags.WaitingPath) != ~(Vehicle.Flags.Created | Vehicle.Flags.Deleted | Vehicle.Flags.Spawned | Vehicle.Flags.Inverted | Vehicle.Flags.TransferToTarget | Vehicle.Flags.TransferToSource | Vehicle.Flags.Emergency1 | Vehicle.Flags.Emergency2 | Vehicle.Flags.WaitingPath | Vehicle.Flags.Stopped | Vehicle.Flags.Leaving | Vehicle.Flags.Arriving | Vehicle.Flags.Reversed | Vehicle.Flags.TakingOff | Vehicle.Flags.Flying | Vehicle.Flags.Landing | Vehicle.Flags.WaitingSpace | Vehicle.Flags.WaitingCargo | Vehicle.Flags.GoingBack | Vehicle.Flags.WaitingTarget | Vehicle.Flags.Importing | Vehicle.Flags.Exporting | Vehicle.Flags.Parking | Vehicle.Flags.CustomName | Vehicle.Flags.OnGravel | Vehicle.Flags.WaitingLoading | Vehicle.Flags.Congestion | Vehicle.Flags.DummyTraffic | Vehicle.Flags.Underground | Vehicle.Flags.Transition | Vehicle.Flags.InsideBuilding | Vehicle.Flags.LeftHandDrive))
                return false;
            }
            else if (path.m_id.CitizenInstance != 0 && (theCitizenManager.m_instances.m_buffer[path.m_id.CitizenInstance].m_flags & CitizenInstance.Flags.WaitingPath) != CitizenInstance.Flags.None)
              return false;
            TransportInfo transportInfo = GetTransportInfo(path);
            if (transportInfo != null)
            {
              path.m_material = transportInfo.m_lineMaterial2;
              path.m_material2 = transportInfo.m_secondaryLineMaterial2;
              path.m_requireSurfaceLine = transportInfo.m_requireSurfaceLine;
              path.m_layer = transportInfo.m_prefabDataLayer;
              path.m_layer2 = transportInfo.m_secondaryLayer;
            }
            TransportLine.TempUpdateMeshData[] data = !path.m_requireSurfaceLine ? new TransportLine.TempUpdateMeshData[1] : new TransportLine.TempUpdateMeshData[81];
            int curveCount = 0;
            float totalLength = 0.0f;
            Vector3 zero = Vector3.zero;
            int startIndex = 0;
            NetInfo.LaneType laneTypes = NetInfo.LaneType.None;
            VehicleInfo.VehicleType vehicleTypes = VehicleInfo.VehicleType.None;
            uint path1;
            if (path.m_id.Vehicle != 0)
            {
              VehicleInfo info = theVehicleManager.m_vehicles.m_buffer[path.m_id.Vehicle].Info;
              if (info != null)
              {
                laneTypes = NetInfo.LaneType.Vehicle | NetInfo.LaneType.Parking | NetInfo.LaneType.CargoVehicle | NetInfo.LaneType.TransportVehicle;
                vehicleTypes = info.m_vehicleType;
              }
              path1 = theVehicleManager.m_vehicles.m_buffer[path.m_id.Vehicle].m_path;
              int pathPositionIndex = theVehicleManager.m_vehicles.m_buffer[path.m_id.Vehicle].m_pathPositionIndex;
              startIndex = pathPositionIndex != byte.MaxValue ? pathPositionIndex >> 1 : 0;
            }
            else if (path.m_id.CitizenInstance != 0)
            {
              laneTypes = NetInfo.LaneType.Vehicle | NetInfo.LaneType.Pedestrian;
              vehicleTypes = VehicleInfo.VehicleType.Bicycle;
              path1 = theCitizenManager.m_instances.m_buffer[path.m_id.CitizenInstance].m_path;
              int pathPositionIndex = theCitizenManager.m_instances.m_buffer[path.m_id.CitizenInstance].m_pathPositionIndex;
              startIndex = pathPositionIndex != byte.MaxValue ? pathPositionIndex >> 1 : 0;
            }
            else
              path1 = 0U;
            path.m_pathUnit = path1;
            byte num1 = 0;
            if (path1 != 0U)
            {
              path.m_nextPathUnit = thePathManager.m_pathUnits.m_buffer[path1].m_nextPathUnit;
              num1 = thePathManager.m_pathUnits.m_buffer[path1].m_pathFindFlags;
              if ((num1 & 4) != 0)
                TransportLine.CalculatePathSegmentCount(path1, startIndex, laneTypes, vehicleTypes, ref data, ref curveCount, ref totalLength, ref zero);
            }
            else
              path.m_nextPathUnit = 0U;
            if (curveCount != 0)
            {
              if (path.m_requireSurfaceLine)
                ++data[theTerrainManager.GetPatchIndex(zero)].m_pathSegmentCount;
              else
                ++data[0].m_pathSegmentCount;
            }
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
            path.m_lineCurves = new Bezier3[curveCount];
            path.m_curveOffsets = new Vector2[curveCount];
            int curveIndex = 0;
            float lengthScale = Mathf.Ceil(totalLength / 64f) / totalLength;
            float currentLength = 0.0f;
            if (path1 != 0U && (num1 & 4) != 0)
            {
              Vector3 minPos;
              Vector3 maxPos;
              TransportLine.FillPathSegments(path1, startIndex, laneTypes, vehicleTypes, ref data, path.m_lineCurves, path.m_curveOffsets, ref curveIndex, ref currentLength, lengthScale, out minPos, out maxPos, path.m_requireSurfaceLine, false);
            }
            if (curveCount != 0)
            {
              if (path.m_requireSurfaceLine)
              {
                int patchIndex = theTerrainManager.GetPatchIndex(zero);
                TransportLine.FillPathNode(zero, data[patchIndex].m_meshData, data[patchIndex].m_pathSegmentIndex, 4f, !path.m_requireSurfaceLine ? 5f : 20f, true);
                ++data[patchIndex].m_pathSegmentIndex;
              }
              else
              {
                TransportLine.FillPathNode(zero, data[0].m_meshData, data[0].m_pathSegmentIndex, 4f, !path.m_requireSurfaceLine ? 5f : 20f, false);
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
                if (path.m_requireSurfaceLine)
                {
                  Vector3 min = data[index].m_meshData.m_bounds.min;
                  Vector3 max = data[index].m_meshData.m_bounds.max;
                  max.y += 1024f;
                  data[index].m_meshData.m_bounds.SetMinMax(min, max);
                }
                meshDataArray[num2++] = data[index].m_meshData;
              }
            }
            while (!Monitor.TryEnter(path, SimulationManager.SYNCHRONIZE_TIMEOUT));
            try
            {
              path.m_meshData = meshDataArray;
              path.m_startCurveIndex = 0;
            }
            finally
            {
              Monitor.Exit(path);
            }
            return true;
          }

        // UpdateData marks the entire m_paths list as not stillNeeded and canRelease (without any testing)
        // a better name for it might be "MarkDataForRelease"
        public void UpdateData()
          {
            do
              ;
            while (!Monitor.TryEnter(m_paths, SimulationManager.SYNCHRONIZE_TIMEOUT));
            try
            {
              using (Dictionary<InstanceID, Path>.Enumerator enumerator = m_paths.GetEnumerator())
              {
                while (enumerator.MoveNext())
                {
                  Path path = enumerator.Current.Value;
                  path.m_stillNeeded = false;
                  path.m_canRelease = true;
                }
              }
            }
            finally
            {
              Monitor.Exit(m_paths);
            }
          }

        public bool IsPathVisible(InstanceID id)
          {
            while (!Monitor.TryEnter(m_paths, SimulationManager.SYNCHRONIZE_TIMEOUT));
            try
            {
              return m_paths.ContainsKey(id);
            }
            finally
            {
              Monitor.Exit(m_paths);
            }
          }

        public class Path
          {
            // the indexer of the m_paths Dictionary is InstanceID
            // when a path is first created (in AddInstances) it contains only InstanceID, m_refreshRequired and m_stillNeeded
            // all the rest of the members derive from the knowledge of InstanceID
            // NOTE: an InstanceID has just one member, called m_id - although calls like InstanceID.Citizen and InstanceID.Vehicle
            // might suggest it could contain two or more things.  It cannot.  It has *either* a citizen or a vehice (or a ...) never both
            // If it contains a vehicle then InstanceID.Vehicle returns the vehicle number (ie index of vehicle in vehicleManager)
            // if it is not a vehicle then InstanceID.Vehicle returns 0
            public InstanceID m_id;
            public bool m_refreshRequired;
            public bool m_stillNeeded;
            public bool m_canRelease;
            public RenderGroup.MeshData[] m_meshData;
            public Bezier3[] m_lineCurves;
            public Vector2[] m_curveOffsets;
            public Mesh[] m_meshes;
            public Material m_material;
            public Material m_material2;
            public Color m_color;
            public float m_pathOffset;
            public int m_layer;
            public int m_layer2;
            public int m_curveIndex;
            public int m_startCurveIndex;
            public uint m_pathUnit;
            public uint m_nextPathUnit;
            public bool m_requireSurfaceLine;
          }
    }
}