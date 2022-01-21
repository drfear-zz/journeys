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

public class PathVisualizer : MonoBehaviour
{
  private Dictionary<InstanceID, PathVisualizer.Path> m_paths;
  private FastList<PathVisualizer.Path> m_renderPaths;
  private FastList<PathVisualizer.Path> m_removePaths;
  private FastList<PathVisualizer.Path> m_stepPaths;
  private HashSet<InstanceID> m_targets;
  private InstanceID m_lastInstance;
  private bool m_pathsVisible;
  private int m_neededPathCount;
  private int m_pathRefreshFrame;
  private bool m_showPedestrians;
  private bool m_showCyclists;
  private bool m_showPrivateVehicles;
  private bool m_showPublicTransport;
  private bool m_showTrucks;
  private bool m_showCityServiceVehicles;
  private bool m_filterModified;
  private bool m_citizenPathChecked;

  public bool showPedestrians
  {
    get
    {
      return this.m_showPedestrians;
    }
    set
    {
      if (this.m_showPedestrians == value)
        return;
      this.m_showPedestrians = value;
      this.m_filterModified = true;
    }
  }

  public bool showCyclists
  {
    get
    {
      return this.m_showCyclists;
    }
    set
    {
      if (this.m_showCyclists == value)
        return;
      this.m_showCyclists = value;
      this.m_filterModified = true;
    }
  }

  public bool showPrivateVehicles
  {
    get
    {
      return this.m_showPrivateVehicles;
    }
    set
    {
      if (this.m_showPrivateVehicles == value)
        return;
      this.m_showPrivateVehicles = value;
      this.m_filterModified = true;
    }
  }

  public bool showPublicTransport
  {
    get
    {
      return this.m_showPublicTransport;
    }
    set
    {
      if (this.m_showPublicTransport == value)
        return;
      this.m_showPublicTransport = value;
      this.m_filterModified = true;
    }
  }

  public bool showTrucks
  {
    get
    {
      return this.m_showTrucks;
    }
    set
    {
      if (this.m_showTrucks == value)
        return;
      this.m_showTrucks = value;
      this.m_filterModified = true;
    }
  }

  public bool showCityServiceVehicles
  {
    get
    {
      return this.m_showCityServiceVehicles;
    }
    set
    {
      if (this.m_showCityServiceVehicles == value)
        return;
      this.m_showCityServiceVehicles = value;
      this.m_filterModified = true;
    }
  }

  private void Awake()
  {
    this.m_paths = new Dictionary<InstanceID, PathVisualizer.Path>();
    this.m_renderPaths = new FastList<PathVisualizer.Path>();
    this.m_removePaths = new FastList<PathVisualizer.Path>();
    this.m_stepPaths = new FastList<PathVisualizer.Path>();
    this.m_targets = new HashSet<InstanceID>();
    this.m_showPedestrians = true;
    this.m_showCyclists = true;
    this.m_showPrivateVehicles = true;
    this.m_showPublicTransport = true;
    this.m_showTrucks = true;
    this.m_showCityServiceVehicles = true;
  }

  private void OnDestroy()
  {
    this.DestroyPaths();
  }

  public bool PathsVisible
  {
    get
    {
      return this.m_pathsVisible;
    }
    set
    {
      this.m_pathsVisible = value;
    }
  }

  public void DestroyPaths()
  {
    do
      ;
    while (!Monitor.TryEnter((object) this.m_paths, SimulationManager.SYNCHRONIZE_TIMEOUT));
    try
    {
      using (Dictionary<InstanceID, PathVisualizer.Path>.Enumerator enumerator = this.m_paths.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          PathVisualizer.Path path = enumerator.Current.Value;
          Mesh[] meshes = path.m_meshes;
          if (meshes != null)
          {
            for (int index = 0; index < meshes.Length; ++index)
            {
              Mesh mesh = meshes[index];
              if ((UnityEngine.Object) mesh != (UnityEngine.Object) null)
                UnityEngine.Object.Destroy((UnityEngine.Object) mesh);
            }
            path.m_meshes = (Mesh[]) null;
          }
        }
      }
      this.m_paths.Clear();
    }
    finally
    {
      Monitor.Exit((object) this.m_paths);
    }
  }

  public void RenderPath(
    RenderManager.CameraInfo cameraInfo,
    PathVisualizer.Path path,
    bool secondary)
  {
    NetManager instance1 = Singleton<NetManager>.instance;
    TerrainManager instance2 = Singleton<TerrainManager>.instance;
    TransportManager instance3 = Singleton<TransportManager>.instance;
    if (path.m_meshData != null)
      this.UpdateMesh(path);
    Mesh[] meshes = path.m_meshes;
    Material material = !secondary ? path.m_material : path.m_material2;
    if (meshes == null || !((UnityEngine.Object) material != (UnityEngine.Object) null))
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
        if ((double) u1 <= 0.990000009536743 && ((double) num2 >= (double) num1 || (double) num1 <= (double) num3))
        {
          if ((double) num5 < (double) num4 && path.m_curveIndex + 1 < lineCurves.Length)
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
      if ((UnityEngine.Object) mesh != (UnityEngine.Object) null && cameraInfo.Intersect(mesh.bounds))
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

  public void RenderPaths(RenderManager.CameraInfo cameraInfo, int layerMask)
  {
    if (!this.m_pathsVisible)
      return;
    do
      ;
    while (!Monitor.TryEnter((object) this.m_paths, SimulationManager.SYNCHRONIZE_TIMEOUT));
    try
    {
      using (Dictionary<InstanceID, PathVisualizer.Path>.Enumerator enumerator = this.m_paths.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          PathVisualizer.Path path = enumerator.Current.Value;
          if (path.m_canRelease)
            this.m_removePaths.Add(path);
          else if ((layerMask & 1 << path.m_layer) != 0 || path.m_layer2 != -1 && (layerMask & 1 << path.m_layer2) != 0)
            this.m_renderPaths.Add(path);
        }
      }
      for (int index = 0; index < this.m_removePaths.m_size; ++index)
        this.m_paths.Remove(this.m_removePaths.m_buffer[index].m_id);
    }
    finally
    {
      Monitor.Exit((object) this.m_paths);
    }
    for (int index1 = 0; index1 < this.m_removePaths.m_size; ++index1)
    {
      Mesh[] meshes = this.m_removePaths.m_buffer[index1].m_meshes;
      if (meshes != null)
      {
        for (int index2 = 0; index2 < meshes.Length; ++index2)
        {
          Mesh mesh = meshes[index2];
          if ((UnityEngine.Object) mesh != (UnityEngine.Object) null)
            UnityEngine.Object.Destroy((UnityEngine.Object) mesh);
        }
      }
    }
    this.m_removePaths.Clear();
    for (int index = 0; index < this.m_renderPaths.m_size; ++index)
    {
      PathVisualizer.Path path = this.m_renderPaths.m_buffer[index];
      this.RenderPath(cameraInfo, path, (layerMask & 1 << path.m_layer) == 0);
    }
    this.m_renderPaths.Clear();
  }

  private void UpdateMesh(PathVisualizer.Path path)
  {
    do
      ;
    while (!Monitor.TryEnter((object) path, SimulationManager.SYNCHRONIZE_TIMEOUT));
    RenderGroup.MeshData[] meshData;
    try
    {
      meshData = path.m_meshData;
      path.m_curveIndex = path.m_startCurveIndex;
      path.m_meshData = (RenderGroup.MeshData[]) null;
    }
    finally
    {
      Monitor.Exit((object) path);
    }
    if (meshData == null)
      return;
    path.m_color = Color.black;
    if (path.m_id.Vehicle != (ushort) 0)
    {
      VehicleManager instance = Singleton<VehicleManager>.instance;
      VehicleInfo info = instance.m_vehicles.m_buffer[(int) path.m_id.Vehicle].Info;
      if ((UnityEngine.Object) info != (UnityEngine.Object) null)
        path.m_color = info.m_vehicleAI.GetColor(path.m_id.Vehicle, ref instance.m_vehicles.m_buffer[(int) path.m_id.Vehicle], InfoManager.InfoMode.TrafficRoutes);
    }
    else if (path.m_id.CitizenInstance != (ushort) 0)
    {
      CitizenManager instance = Singleton<CitizenManager>.instance;
      CitizenInfo info = instance.m_instances.m_buffer[(int) path.m_id.CitizenInstance].Info;
      if ((UnityEngine.Object) info != (UnityEngine.Object) null)
        path.m_color = info.m_citizenAI.GetColor(path.m_id.CitizenInstance, ref instance.m_instances.m_buffer[(int) path.m_id.CitizenInstance], InfoManager.InfoMode.TrafficRoutes);
    }
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
        UnityEngine.Object.Destroy((UnityEngine.Object) meshArray1[index]);
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

  public void SimulationStep(int subStep)
  {
    if (!this.m_pathsVisible)
      return;
    InstanceID instanceId = Singleton<InstanceManager>.instance.GetSelectedInstance();
    if (instanceId.Citizen != 0U)
    {
      CitizenManager instance1 = Singleton<CitizenManager>.instance;
      VehicleManager instance2 = Singleton<VehicleManager>.instance;
      ushort instance3 = instance1.m_citizens.m_buffer[(IntPtr) instanceId.Citizen].m_instance;
      ushort vehicle = instance1.m_citizens.m_buffer[(IntPtr) instanceId.Citizen].m_vehicle;
      if (instance3 != (ushort) 0 && instance1.m_instances.m_buffer[(int) instance3].m_path != 0U)
        instanceId.CitizenInstance = instance3;
      else if (vehicle != (ushort) 0 && instance2.m_vehicles.m_buffer[(int) vehicle].m_path != 0U)
        instanceId.Vehicle = vehicle;
    }
    if (instanceId.Vehicle != (ushort) 0)
    {
      CitizenManager instance1 = Singleton<CitizenManager>.instance;
      VehicleManager instance2 = Singleton<VehicleManager>.instance;
      instanceId.Vehicle = instance2.m_vehicles.m_buffer[(int) instanceId.Vehicle].GetFirstVehicle(instanceId.Vehicle);
      VehicleInfo info = instance2.m_vehicles.m_buffer[(int) instanceId.Vehicle].Info;
      if (info.m_vehicleType == VehicleInfo.VehicleType.Bicycle)
      {
        InstanceID ownerId = info.m_vehicleAI.GetOwnerID(instanceId.Vehicle, ref instance2.m_vehicles.m_buffer[(int) instanceId.Vehicle]);
        if (ownerId.Citizen != 0U)
          ownerId.CitizenInstance = instance1.m_citizens.m_buffer[(IntPtr) ownerId.Citizen].m_instance;
        if (ownerId.CitizenInstance != (ushort) 0)
          instanceId = ownerId;
      }
    }
    if (instanceId != this.m_lastInstance || this.m_filterModified)
    {
      this.m_filterModified = false;
      this.PreAddInstances();
      if (instanceId.Vehicle != (ushort) 0 || instanceId.CitizenInstance != (ushort) 0)
      {
        Singleton<GuideManager>.instance.m_routeButton.Disable();
        if (instanceId.CitizenInstance != (ushort) 0 && !this.m_citizenPathChecked && Singleton<SimulationManager>.instance.m_metaData.m_disableAchievements != SimulationMetaData.MetaBool.True)
        {
          this.m_citizenPathChecked = true;
          ColossalFramework.Threading.ThreadHelper.dispatcher.Dispatch((System.Action) (() =>
          {
            if (PlatformService.achievements["Reporting"].achieved)
              return;
            PlatformService.achievements["Reporting"].Unlock();
          }));
        }
        this.AddInstance(instanceId);
      }
      else if (instanceId.NetSegment != (ushort) 0 || instanceId.Building != (ushort) 0 || (instanceId.District != (byte) 0 || instanceId.Park != (byte) 0))
        this.AddPaths(instanceId, 0, 256);
      this.PostAddInstances();
      this.m_lastInstance = instanceId;
      this.m_pathRefreshFrame = 0;
    }
    else if (instanceId.NetSegment != (ushort) 0 || instanceId.Building != (ushort) 0 || (instanceId.District != (byte) 0 || instanceId.Park != (byte) 0))
    {
      if (this.m_pathRefreshFrame == 0)
        this.PreAddInstances();
      this.AddPaths(instanceId, this.m_pathRefreshFrame, this.m_pathRefreshFrame + 1);
      ++this.m_pathRefreshFrame;
      if (this.m_pathRefreshFrame >= 256)
      {
        this.PostAddInstances();
        this.m_pathRefreshFrame = 0;
      }
    }
    for (int index = 0; index < this.m_stepPaths.m_size; ++index)
      this.StepPath(this.m_stepPaths.m_buffer[index]);
  }

  private void PreAddInstances()
  {
    do
      ;
    while (!Monitor.TryEnter((object) this.m_paths, SimulationManager.SYNCHRONIZE_TIMEOUT));
    try
    {
      using (Dictionary<InstanceID, PathVisualizer.Path>.Enumerator enumerator = this.m_paths.GetEnumerator())
      {
        while (enumerator.MoveNext())
          enumerator.Current.Value.m_stillNeeded = false;
      }
    }
    finally
    {
      Monitor.Exit((object) this.m_paths);
    }
    this.m_neededPathCount = 0;
  }

  private void PostAddInstances()
  {
    this.m_stepPaths.Clear();
    do
      ;
    while (!Monitor.TryEnter((object) this.m_paths, SimulationManager.SYNCHRONIZE_TIMEOUT));
    try
    {
      using (Dictionary<InstanceID, PathVisualizer.Path>.Enumerator enumerator = this.m_paths.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          PathVisualizer.Path path = enumerator.Current.Value;
          path.m_canRelease = !path.m_stillNeeded;
          if (path.m_stillNeeded)
            this.m_stepPaths.Add(path);
        }
      }
    }
    finally
    {
      Monitor.Exit((object) this.m_paths);
    }
  }

  private void AddPaths(InstanceID target, int min, int max)
  {
    this.m_targets.Clear();
    if (target.Building != (ushort) 0)
    {
      BuildingManager instance1 = Singleton<BuildingManager>.instance;
      NetManager instance2 = Singleton<NetManager>.instance;
      int num1 = 0;
      while (target.Building != (ushort) 0)
      {
        this.m_targets.Add(target);
        ushort num2 = instance1.m_buildings.m_buffer[(int) target.Building].m_netNode;
        int num3 = 0;
        while (num2 != (ushort) 0)
        {
          if (instance2.m_nodes.m_buffer[(int) num2].Info.m_class.m_layer != ItemClass.Layer.PublicTransport)
          {
            for (int index = 0; index < 8; ++index)
            {
              ushort segment = instance2.m_nodes.m_buffer[(int) num2].GetSegment(index);
              if (segment != (ushort) 0 && (int) instance2.m_segments.m_buffer[(int) segment].m_startNode == (int) num2 && (instance2.m_segments.m_buffer[(int) segment].m_flags & NetSegment.Flags.Untouchable) != NetSegment.Flags.None)
              {
                InstanceID empty = InstanceID.Empty;
                empty.NetSegment = segment;
                this.m_targets.Add(empty);
              }
            }
          }
          num2 = instance2.m_nodes.m_buffer[(int) num2].m_nextBuildingNode;
          if (++num3 > 32768)
          {
            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
            break;
          }
        }
        target.Building = instance1.m_buildings.m_buffer[(int) target.Building].m_subBuilding;
        if (++num1 > 49152)
        {
          CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
          break;
        }
      }
    }
    else
      this.m_targets.Add(target);
    this.AddPathsImpl(min, max);
  }

  private void AddPathsImpl(int min, int max)
  {
    PathManager instance1 = Singleton<PathManager>.instance;
    NetManager instance2 = Singleton<NetManager>.instance;
    BuildingManager instance3 = Singleton<BuildingManager>.instance;
    DistrictManager instance4 = Singleton<DistrictManager>.instance;
    if (this.m_showPrivateVehicles || this.m_showPublicTransport || (this.m_showTrucks || this.m_showCityServiceVehicles))
    {
      VehicleManager instance5 = Singleton<VehicleManager>.instance;
      int num1 = min * 16384 >> 8;
      int num2 = max * 16384 >> 8;
      for (int index1 = num1; index1 < num2; ++index1)
      {
        if ((instance5.m_vehicles.m_buffer[index1].m_flags & (Vehicle.Flags.Created | Vehicle.Flags.Deleted | Vehicle.Flags.WaitingPath)) == Vehicle.Flags.Created)
        {
          VehicleInfo info1 = instance5.m_vehicles.m_buffer[index1].Info;
          ItemClass.Service service = info1.m_class.m_service;
          switch (service)
          {
            case ItemClass.Service.Residential:
              if (this.m_showPrivateVehicles)
                break;
              continue;
            case ItemClass.Service.Industrial:
              if (this.m_showTrucks)
                break;
              continue;
            default:
              if (service == ItemClass.Service.PublicTransport)
              {
                if (info1.m_class.m_subService == ItemClass.SubService.PublicTransportPost)
                {
                  if (this.m_showCityServiceVehicles)
                    break;
                  continue;
                }
                if (this.m_showPublicTransport)
                  break;
                continue;
              }
              if (this.m_showCityServiceVehicles)
                break;
              continue;
          }
          bool flag1 = false;
          int pathPositionIndex = (int) instance5.m_vehicles.m_buffer[index1].m_pathPositionIndex;
          int num3 = pathPositionIndex != (int) byte.MaxValue ? pathPositionIndex >> 1 : 0;
          uint num4 = instance5.m_vehicles.m_buffer[index1].m_path;
          bool flag2 = false;
          int num5 = 0;
          while (num4 != 0U && !flag1 && !flag2)
          {
            int positionCount = (int) instance1.m_pathUnits.m_buffer[(IntPtr) num4].m_positionCount;
            for (int index2 = num3; index2 < positionCount; ++index2)
            {
              PathUnit.Position position = instance1.m_pathUnits.m_buffer[(IntPtr) num4].GetPosition(index2);
              InstanceID empty = InstanceID.Empty;
              empty.NetSegment = position.m_segment;
              if (this.m_targets.Contains(empty))
              {
                if (instance2.m_segments.m_buffer[(int) position.m_segment].m_modifiedIndex < instance1.m_pathUnits.m_buffer[(IntPtr) num4].m_buildIndex)
                {
                  NetInfo info2 = instance2.m_segments.m_buffer[(int) position.m_segment].Info;
                  if (info2.m_lanes != null && (int) position.m_lane < info2.m_lanes.Length && (info2.m_lanes[(int) position.m_lane].m_laneType & (NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle)) != NetInfo.LaneType.None)
                  {
                    flag1 = true;
                    break;
                  }
                }
                flag2 = true;
                break;
              }
            }
            num3 = 0;
            num4 = instance1.m_pathUnits.m_buffer[(IntPtr) num4].m_nextPathUnit;
            if (++num5 >= 262144)
            {
              CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
              break;
            }
          }
          InstanceID targetId = info1.m_vehicleAI.GetTargetID((ushort) index1, ref instance5.m_vehicles.m_buffer[index1]);
          bool flag3 = flag1 | this.m_targets.Contains(targetId);
          if (targetId.Building != (ushort) 0)
          {
            Vector3 position = instance3.m_buildings.m_buffer[(int) targetId.Building].m_position;
            InstanceID empty = InstanceID.Empty;
            empty.District = instance4.GetDistrict(position);
            bool flag4 = flag3 | this.m_targets.Contains(empty);
            empty.Park = instance4.GetPark(position);
            flag3 = flag4 | this.m_targets.Contains(empty);
          }
          if (targetId.NetNode != (ushort) 0)
          {
            Vector3 position = instance2.m_nodes.m_buffer[(int) targetId.NetNode].m_position;
            InstanceID empty = InstanceID.Empty;
            empty.District = instance4.GetDistrict(position);
            bool flag4 = flag3 | this.m_targets.Contains(empty);
            empty.Park = instance4.GetPark(position);
            flag3 = flag4 | this.m_targets.Contains(empty);
          }
          if (flag3)
          {
            InstanceID empty = InstanceID.Empty;
            empty.Vehicle = (ushort) index1;
            this.AddInstance(empty);
            if (this.m_neededPathCount >= 100)
              return;
          }
        }
      }
    }
    if (!this.m_showPedestrians && !this.m_showCyclists)
      return;
    CitizenManager instance6 = Singleton<CitizenManager>.instance;
    VehicleManager instance7 = Singleton<VehicleManager>.instance;
    int num6 = min * 65536 >> 8;
    int num7 = max * 65536 >> 8;
    for (int index1 = num6; index1 < num7; ++index1)
    {
      if ((instance6.m_instances.m_buffer[index1].m_flags & (CitizenInstance.Flags.Created | CitizenInstance.Flags.Deleted | CitizenInstance.Flags.WaitingPath)) == CitizenInstance.Flags.Created)
      {
        VehicleInfo.VehicleType vehicleType = VehicleInfo.VehicleType.None;
        uint citizen = instance6.m_instances.m_buffer[index1].m_citizen;
        if (citizen != 0U)
        {
          ushort vehicle = instance6.m_citizens.m_buffer[(IntPtr) citizen].m_vehicle;
          if (vehicle != (ushort) 0)
            vehicleType = instance7.m_vehicles.m_buffer[(int) vehicle].Info.m_vehicleType;
        }
        switch (vehicleType)
        {
          case VehicleInfo.VehicleType.None:
            if (this.m_showPedestrians)
              break;
            continue;
          case VehicleInfo.VehicleType.Bicycle:
            if (this.m_showCyclists)
              break;
            continue;
          default:
            continue;
        }
        bool flag1 = false;
        int pathPositionIndex = (int) instance6.m_instances.m_buffer[index1].m_pathPositionIndex;
        int num1 = pathPositionIndex != (int) byte.MaxValue ? pathPositionIndex >> 1 : 0;
        uint num2 = instance6.m_instances.m_buffer[index1].m_path;
        bool flag2 = false;
        int num3 = 0;
        while (num2 != 0U && !flag1 && !flag2)
        {
          int positionCount = (int) instance1.m_pathUnits.m_buffer[(IntPtr) num2].m_positionCount;
          for (int index2 = num1; index2 < positionCount; ++index2)
          {
            PathUnit.Position position = instance1.m_pathUnits.m_buffer[(IntPtr) num2].GetPosition(index2);
            InstanceID empty = InstanceID.Empty;
            empty.NetSegment = position.m_segment;
            if (this.m_targets.Contains(empty))
            {
              if (instance2.m_segments.m_buffer[(int) position.m_segment].m_modifiedIndex < instance1.m_pathUnits.m_buffer[(IntPtr) num2].m_buildIndex)
              {
                NetInfo info = instance2.m_segments.m_buffer[(int) position.m_segment].Info;
                if (info.m_lanes != null && (int) position.m_lane < info.m_lanes.Length && (info.m_lanes[(int) position.m_lane].m_laneType == NetInfo.LaneType.Pedestrian || info.m_lanes[(int) position.m_lane].m_laneType == NetInfo.LaneType.Vehicle && info.m_lanes[(int) position.m_lane].m_vehicleType == VehicleInfo.VehicleType.Bicycle))
                {
                  flag1 = true;
                  break;
                }
              }
              flag2 = true;
              break;
            }
          }
          num1 = 0;
          num2 = instance1.m_pathUnits.m_buffer[(IntPtr) num2].m_nextPathUnit;
          if (++num3 >= 262144)
          {
            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
            break;
          }
        }
        InstanceID targetId = instance6.m_instances.m_buffer[index1].Info.m_citizenAI.GetTargetID((ushort) index1, ref instance6.m_instances.m_buffer[index1]);
        bool flag3 = flag1 | this.m_targets.Contains(targetId);
        if (targetId.Building != (ushort) 0)
        {
          Vector3 position = instance3.m_buildings.m_buffer[(int) targetId.Building].m_position;
          InstanceID empty = InstanceID.Empty;
          empty.District = instance4.GetDistrict(position);
          bool flag4 = flag3 | this.m_targets.Contains(empty);
          empty.Park = instance4.GetPark(position);
          flag3 = flag4 | this.m_targets.Contains(empty);
        }
        if (targetId.NetNode != (ushort) 0)
        {
          Vector3 position = instance2.m_nodes.m_buffer[(int) targetId.NetNode].m_position;
          InstanceID empty = InstanceID.Empty;
          empty.District = instance4.GetDistrict(position);
          bool flag4 = flag3 | this.m_targets.Contains(empty);
          empty.Park = instance4.GetPark(position);
          flag3 = flag4 | this.m_targets.Contains(empty);
        }
        if (flag3)
        {
          InstanceID empty = InstanceID.Empty;
          empty.CitizenInstance = (ushort) index1;
          this.AddInstance(empty);
          if (this.m_neededPathCount >= 100)
            break;
        }
      }
    }
  }

  private void AddInstance(InstanceID id)
  {
    do
      ;
    while (!Monitor.TryEnter((object) this.m_paths, SimulationManager.SYNCHRONIZE_TIMEOUT));
    try
    {
      PathVisualizer.Path path1;
      if (this.m_paths.TryGetValue(id, out path1))
      {
        if (!path1.m_stillNeeded)
        {
          path1.m_stillNeeded = true;
          ++this.m_neededPathCount;
        }
        path1.m_canRelease = false;
      }
      else
      {
        PathVisualizer.Path path2 = new PathVisualizer.Path();
        path2.m_id = id;
        path2.m_stillNeeded = true;
        path2.m_refreshRequired = true;
        this.m_paths.Add(id, path2);
        this.m_stepPaths.Add(path2);
        ++this.m_neededPathCount;
      }
    }
    finally
    {
      Monitor.Exit((object) this.m_paths);
    }
  }

  private void StepPath(PathVisualizer.Path path)
  {
    InstanceID id = path.m_id;
    uint num = 0;
    if (id.CitizenInstance != (ushort) 0)
      num = Singleton<CitizenManager>.instance.m_instances.m_buffer[(int) id.CitizenInstance].m_path;
    else if (id.Vehicle != (ushort) 0)
      num = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[(int) id.Vehicle].m_path;
    if (num != 0U)
    {
      if ((int) num == (int) path.m_nextPathUnit)
      {
        PathManager instance = Singleton<PathManager>.instance;
        path.m_pathUnit = num;
        path.m_nextPathUnit = instance.m_pathUnits.m_buffer[(IntPtr) num].m_nextPathUnit;
      }
      else if ((int) num != (int) path.m_pathUnit)
        path.m_refreshRequired = true;
    }
    if (!path.m_refreshRequired || !this.RefreshPath(path))
      return;
    path.m_refreshRequired = false;
  }

  private TransportInfo GetTransportInfo(PathVisualizer.Path path)
  {
    if (path.m_id.Vehicle != (ushort) 0)
    {
      VehicleInfo info = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[(int) path.m_id.Vehicle].Info;
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
    else if (path.m_id.CitizenInstance != (ushort) 0)
      return Singleton<TransportManager>.instance.GetTransportInfo(TransportInfo.TransportType.Bus);
    return (TransportInfo) null;
  }

  private bool RefreshPath(PathVisualizer.Path path)
  {
    TransportManager instance1 = Singleton<TransportManager>.instance;
    NetManager instance2 = Singleton<NetManager>.instance;
    PathManager instance3 = Singleton<PathManager>.instance;
    TerrainManager instance4 = Singleton<TerrainManager>.instance;
    VehicleManager instance5 = Singleton<VehicleManager>.instance;
    CitizenManager instance6 = Singleton<CitizenManager>.instance;
    if (path.m_id.Vehicle != (ushort) 0)
    {
      if ((instance5.m_vehicles.m_buffer[(int) path.m_id.Vehicle].m_flags & Vehicle.Flags.WaitingPath) != ~(Vehicle.Flags.Created | Vehicle.Flags.Deleted | Vehicle.Flags.Spawned | Vehicle.Flags.Inverted | Vehicle.Flags.TransferToTarget | Vehicle.Flags.TransferToSource | Vehicle.Flags.Emergency1 | Vehicle.Flags.Emergency2 | Vehicle.Flags.WaitingPath | Vehicle.Flags.Stopped | Vehicle.Flags.Leaving | Vehicle.Flags.Arriving | Vehicle.Flags.Reversed | Vehicle.Flags.TakingOff | Vehicle.Flags.Flying | Vehicle.Flags.Landing | Vehicle.Flags.WaitingSpace | Vehicle.Flags.WaitingCargo | Vehicle.Flags.GoingBack | Vehicle.Flags.WaitingTarget | Vehicle.Flags.Importing | Vehicle.Flags.Exporting | Vehicle.Flags.Parking | Vehicle.Flags.CustomName | Vehicle.Flags.OnGravel | Vehicle.Flags.WaitingLoading | Vehicle.Flags.Congestion | Vehicle.Flags.DummyTraffic | Vehicle.Flags.Underground | Vehicle.Flags.Transition | Vehicle.Flags.InsideBuilding | Vehicle.Flags.LeftHandDrive))
        return false;
    }
    else if (path.m_id.CitizenInstance != (ushort) 0 && (instance6.m_instances.m_buffer[(int) path.m_id.CitizenInstance].m_flags & CitizenInstance.Flags.WaitingPath) != CitizenInstance.Flags.None)
      return false;
    TransportInfo transportInfo = this.GetTransportInfo(path);
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
    if (path.m_id.Vehicle != (ushort) 0)
    {
      VehicleInfo info = instance5.m_vehicles.m_buffer[(int) path.m_id.Vehicle].Info;
      if (info != null)
      {
        laneTypes = NetInfo.LaneType.Vehicle | NetInfo.LaneType.Parking | NetInfo.LaneType.CargoVehicle | NetInfo.LaneType.TransportVehicle;
        vehicleTypes = info.m_vehicleType;
      }
      path1 = instance5.m_vehicles.m_buffer[(int) path.m_id.Vehicle].m_path;
      int pathPositionIndex = (int) instance5.m_vehicles.m_buffer[(int) path.m_id.Vehicle].m_pathPositionIndex;
      startIndex = pathPositionIndex != (int) byte.MaxValue ? pathPositionIndex >> 1 : 0;
    }
    else if (path.m_id.CitizenInstance != (ushort) 0)
    {
      laneTypes = NetInfo.LaneType.Vehicle | NetInfo.LaneType.Pedestrian;
      vehicleTypes = VehicleInfo.VehicleType.Bicycle;
      path1 = instance6.m_instances.m_buffer[(int) path.m_id.CitizenInstance].m_path;
      int pathPositionIndex = (int) instance6.m_instances.m_buffer[(int) path.m_id.CitizenInstance].m_pathPositionIndex;
      startIndex = pathPositionIndex != (int) byte.MaxValue ? pathPositionIndex >> 1 : 0;
    }
    else
      path1 = 0U;
    path.m_pathUnit = path1;
    byte num1 = 0;
    if (path1 != 0U)
    {
      path.m_nextPathUnit = instance3.m_pathUnits.m_buffer[(IntPtr) path1].m_nextPathUnit;
      num1 = instance3.m_pathUnits.m_buffer[(IntPtr) path1].m_pathFindFlags;
      if (((int) num1 & 4) != 0)
        TransportLine.CalculatePathSegmentCount(path1, startIndex, laneTypes, vehicleTypes, ref data, ref curveCount, ref totalLength, ref zero);
    }
    else
      path.m_nextPathUnit = 0U;
    if (curveCount != 0)
    {
      if (path.m_requireSurfaceLine)
        ++data[instance4.GetPatchIndex(zero)].m_pathSegmentCount;
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
    if (path1 != 0U && ((int) num1 & 4) != 0)
    {
      Vector3 minPos;
      Vector3 maxPos;
      TransportLine.FillPathSegments(path1, startIndex, laneTypes, vehicleTypes, ref data, path.m_lineCurves, path.m_curveOffsets, ref curveIndex, ref currentLength, lengthScale, out minPos, out maxPos, path.m_requireSurfaceLine, false);
    }
    if (curveCount != 0)
    {
      if (path.m_requireSurfaceLine)
      {
        int patchIndex = instance4.GetPatchIndex(zero);
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
    do
      ;
    while (!Monitor.TryEnter((object) path, SimulationManager.SYNCHRONIZE_TIMEOUT));
    try
    {
      path.m_meshData = meshDataArray;
      path.m_startCurveIndex = 0;
    }
    finally
    {
      Monitor.Exit((object) path);
    }
    return true;
  }

  public void UpdateData()
  {
    do
      ;
    while (!Monitor.TryEnter((object) this.m_paths, SimulationManager.SYNCHRONIZE_TIMEOUT));
    try
    {
      using (Dictionary<InstanceID, PathVisualizer.Path>.Enumerator enumerator = this.m_paths.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          PathVisualizer.Path path = enumerator.Current.Value;
          path.m_stillNeeded = false;
          path.m_canRelease = true;
        }
      }
    }
    finally
    {
      Monitor.Exit((object) this.m_paths);
    }
  }

  public bool IsPathVisible(InstanceID id)
  {
    do
      ;
    while (!Monitor.TryEnter((object) this.m_paths, SimulationManager.SYNCHRONIZE_TIMEOUT));
    try
    {
      return this.m_paths.ContainsKey(id);
    }
    finally
    {
      Monitor.Exit((object) this.m_paths);
    }
  }

  public class Path
  {
    public RenderGroup.MeshData[] m_meshData;
    public Bezier3[] m_lineCurves;
    public Vector2[] m_curveOffsets;
    public Mesh[] m_meshes;
    public Material m_material;
    public Material m_material2;
    public Color m_color;
    public InstanceID m_id;
    public float m_pathOffset;
    public int m_layer;
    public int m_layer2;
    public int m_curveIndex;
    public int m_startCurveIndex;
    public uint m_pathUnit;
    public uint m_nextPathUnit;
    public bool m_requireSurfaceLine;
    public bool m_refreshRequired;
    public bool m_stillNeeded;
    public bool m_canRelease;
  }
}
