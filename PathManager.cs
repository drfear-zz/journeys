// Decompiled with JetBrains decompiler
// Type: PathManager
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 9724B8F4-19DD-48C3-AE02-CDA150D75CEC
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Cities_Skylines\Cities_Data\Managed\Assembly-CSharp.dll

using ColossalFramework;
using ColossalFramework.IO;
using ColossalFramework.Math;
using System;
using System.Threading;
using UnityEngine;

public class PathManager : SimulationManagerBase<PathManager, PathProperties>, ISimulationManager
{
  public const int MAX_PATHUNIT_COUNT = 262144;
  public int m_pathUnitCount;
  public int m_renderPathGizmo;
  [NonSerialized]
  public Array32<PathUnit> m_pathUnits;
  [NonSerialized]
  public object m_bufferLock;
  private PathFind[] m_pathfinds;
  private bool m_terminated;

  protected override void Awake()
  {
    base.Awake();
    this.m_pathUnits = new Array32<PathUnit>(262144U);
    this.m_bufferLock = new object();
    uint num;
    this.m_pathUnits.CreateItem(out num);
    int length = Mathf.Clamp(SystemInfo.processorCount / 2, 1, 4);
    this.m_pathfinds = new PathFind[length];
    for (int index = 0; index < length; ++index)
      this.m_pathfinds[index] = this.gameObject.AddComponent<PathFind>();
  }

  private void OnDestroy()
  {
    this.m_terminated = true;
  }

  private void OnDrawGizmosSelected()
  {
    Gizmos.color = Color.red;
    int num = 0;
    for (uint index1 = 0; index1 < this.m_pathUnits.m_size; ++index1)
    {
      if (((int) this.m_pathUnits.m_buffer[(IntPtr) index1].m_pathFindFlags & 4) != 0)
      {
        int positionCount = (int) this.m_pathUnits.m_buffer[(IntPtr) index1].m_positionCount;
        if (positionCount >= 1 && num++ == this.m_renderPathGizmo)
        {
          PathUnit.Position position1;
          if (!this.m_pathUnits.m_buffer[(IntPtr) index1].GetPosition(0, out position1))
            break;
          Vector3 vector3 = PathManager.CalculatePosition(position1);
          Gizmos.DrawSphere(vector3, 8f);
          for (int index2 = 1; index2 < positionCount; ++index2)
          {
            if (this.m_pathUnits.m_buffer[(IntPtr) index1].GetPosition(index2, out position1))
            {
              Vector3 position2 = PathManager.CalculatePosition(position1);
              Gizmos.DrawLine(vector3, position2);
              vector3 = position2;
            }
          }
          if (positionCount < 2)
            break;
          Gizmos.DrawSphere(vector3, 8f);
          break;
        }
      }
    }
  }

  public static bool FindPathPosition(
    Vector3 position,
    ItemClass.Service service,
    NetInfo.LaneType laneType,
    VehicleInfo.VehicleType vehicleType,
    bool allowUnderground,
    bool requireConnect,
    float maxDistance,
    out PathUnit.Position pathPos)
  {
    PathUnit.Position pathPosB;
    float distanceSqrA;
    float distanceSqrB;
    return PathManager.FindPathPosition(position, service, service, laneType, vehicleType, VehicleInfo.VehicleType.None, allowUnderground, requireConnect, maxDistance, out pathPos, out pathPosB, out distanceSqrA, out distanceSqrB);
  }

  public static bool FindPathPosition(
    Vector3 position,
    ItemClass.Service service,
    NetInfo.LaneType laneType,
    VehicleInfo.VehicleType vehicleType,
    bool allowUnderground,
    bool requireConnect,
    float maxDistance,
    out PathUnit.Position pathPosA,
    out PathUnit.Position pathPosB,
    out float distanceSqrA,
    out float distanceSqrB)
  {
    return PathManager.FindPathPosition(position, service, service, laneType, vehicleType, VehicleInfo.VehicleType.None, allowUnderground, requireConnect, maxDistance, out pathPosA, out pathPosB, out distanceSqrA, out distanceSqrB);
  }

  public static bool FindPathPosition(
    Vector3 position,
    ItemClass.Service service,
    NetInfo.LaneType laneType,
    VehicleInfo.VehicleType vehicleType,
    VehicleInfo.VehicleType stopType,
    bool allowUnderground,
    bool requireConnect,
    float maxDistance,
    out PathUnit.Position pathPosA,
    out PathUnit.Position pathPosB,
    out float distanceSqrA,
    out float distanceSqrB)
  {
    return PathManager.FindPathPosition(position, service, service, laneType, vehicleType, stopType, allowUnderground, requireConnect, maxDistance, out pathPosA, out pathPosB, out distanceSqrA, out distanceSqrB);
  }

  public static bool FindPathPosition(
    Vector3 position,
    ItemClass.Service service,
    ItemClass.Service service2,
    NetInfo.LaneType laneType,
    VehicleInfo.VehicleType vehicleType,
    VehicleInfo.VehicleType stopType,
    bool allowUnderground,
    bool requireConnect,
    float maxDistance,
    out PathUnit.Position pathPosA,
    out PathUnit.Position pathPosB,
    out float distanceSqrA,
    out float distanceSqrB)
  {
    Bounds bounds = new Bounds(position, new Vector3(maxDistance * 2f, maxDistance * 2f, maxDistance * 2f));
    int num1 = Mathf.Max((int) (((double) bounds.min.x - 64.0) / 64.0 + 135.0), 0);
    int num2 = Mathf.Max((int) (((double) bounds.min.z - 64.0) / 64.0 + 135.0), 0);
    int num3 = Mathf.Min((int) (((double) bounds.max.x + 64.0) / 64.0 + 135.0), 269);
    int num4 = Mathf.Min((int) (((double) bounds.max.z + 64.0) / 64.0 + 135.0), 269);
    NetManager instance = Singleton<NetManager>.instance;
    pathPosA.m_segment = (ushort) 0;
    pathPosA.m_lane = (byte) 0;
    pathPosA.m_offset = (byte) 0;
    distanceSqrA = 1E+10f;
    pathPosB.m_segment = (ushort) 0;
    pathPosB.m_lane = (byte) 0;
    pathPosB.m_offset = (byte) 0;
    distanceSqrB = 1E+10f;
    float num5 = maxDistance * maxDistance;
    for (int index1 = num2; index1 <= num4; ++index1)
    {
      for (int index2 = num1; index2 <= num3; ++index2)
      {
        ushort nextGridSegment = instance.m_segmentGrid[index1 * 270 + index2];
        int num6 = 0;
        while (nextGridSegment != (ushort) 0)
        {
          NetInfo info = instance.m_segments.m_buffer[(int) nextGridSegment].Info;
          if (info != null && (info.m_class.m_service == service || info.m_class.m_service == service2) && ((instance.m_segments.m_buffer[(int) nextGridSegment].m_flags & (NetSegment.Flags.Collapsed | NetSegment.Flags.Flooded)) == NetSegment.Flags.None && (allowUnderground || !info.m_netAI.IsUnderground())))
          {
            ushort startNode = instance.m_segments.m_buffer[(int) nextGridSegment].m_startNode;
            ushort endNode = instance.m_segments.m_buffer[(int) nextGridSegment].m_endNode;
            Vector3 position1 = instance.m_nodes.m_buffer[(int) startNode].m_position;
            Vector3 position2 = instance.m_nodes.m_buffer[(int) endNode].m_position;
            float num7 = Mathf.Max(Mathf.Max(bounds.min.x - 64f - position1.x, bounds.min.z - 64f - position1.z), Mathf.Max((float) ((double) position1.x - (double) bounds.max.x - 64.0), (float) ((double) position1.z - (double) bounds.max.z - 64.0)));
            float num8 = Mathf.Max(Mathf.Max(bounds.min.x - 64f - position2.x, bounds.min.z - 64f - position2.z), Mathf.Max((float) ((double) position2.x - (double) bounds.max.x - 64.0), (float) ((double) position2.z - (double) bounds.max.z - 64.0)));
            Vector3 positionA;
            int laneIndexA;
            float laneOffsetA;
            Vector3 positionB;
            int laneIndexB;
            float laneOffsetB;
            if (((double) num7 < 0.0 || (double) num8 < 0.0) && (instance.m_segments.m_buffer[(int) nextGridSegment].m_bounds.Intersects(bounds) && instance.m_segments.m_buffer[(int) nextGridSegment].GetClosestLanePosition(position, laneType, vehicleType, stopType, requireConnect, out positionA, out laneIndexA, out laneOffsetA, out positionB, out laneIndexB, out laneOffsetB)))
            {
              float num9 = Vector3.SqrMagnitude(position - positionA);
              if ((double) num9 < (double) num5)
              {
                num5 = num9;
                pathPosA.m_segment = nextGridSegment;
                pathPosA.m_lane = (byte) laneIndexA;
                pathPosA.m_offset = (byte) Mathf.Clamp(Mathf.RoundToInt(laneOffsetA * (float) byte.MaxValue), 0, (int) byte.MaxValue);
                distanceSqrA = num9;
                float num10 = Vector3.SqrMagnitude(position - positionB);
                if (laneIndexB == -1 || (double) num10 >= (double) maxDistance * (double) maxDistance)
                {
                  pathPosB.m_segment = (ushort) 0;
                  pathPosB.m_lane = (byte) 0;
                  pathPosB.m_offset = (byte) 0;
                  distanceSqrB = 1E+10f;
                }
                else
                {
                  pathPosB.m_segment = nextGridSegment;
                  pathPosB.m_lane = (byte) laneIndexB;
                  pathPosB.m_offset = (byte) Mathf.Clamp(Mathf.RoundToInt(laneOffsetB * (float) byte.MaxValue), 0, (int) byte.MaxValue);
                  distanceSqrB = num10;
                }
              }
            }
          }
          nextGridSegment = instance.m_segments.m_buffer[(int) nextGridSegment].m_nextGridSegment;
          if (++num6 >= 36864)
          {
            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
            break;
          }
        }
      }
    }
    return pathPosA.m_segment != (ushort) 0;
  }

  public static Vector3 CalculatePosition(PathUnit.Position pathPos)
  {
    NetManager instance = Singleton<NetManager>.instance;
    uint num = instance.m_segments.m_buffer[(int) pathPos.m_segment].m_lanes;
    for (int index = 0; index < (int) pathPos.m_lane && num != 0U; ++index)
      num = instance.m_lanes.m_buffer[(IntPtr) num].m_nextLane;
    if (num != 0U)
      return instance.m_lanes.m_buffer[(IntPtr) num].CalculatePosition((float) pathPos.m_offset * 0.003921569f);
    return Vector3.zero;
  }

  public static uint GetLaneID(PathUnit.Position pathPos)
  {
    NetManager instance = Singleton<NetManager>.instance;
    uint num = instance.m_segments.m_buffer[(int) pathPos.m_segment].m_lanes;
    for (int index = 0; index < (int) pathPos.m_lane && num != 0U; ++index)
      num = instance.m_lanes.m_buffer[(IntPtr) num].m_nextLane;
    return num;
  }

  public bool CreatePath(
    out uint unit,
    ref Randomizer randomizer,
    uint buildIndex,
    PathUnit.Position startPos,
    PathUnit.Position endPos,
    NetInfo.LaneType laneTypes,
    VehicleInfo.VehicleType vehicleTypes,
    float maxLength)
  {
    PathUnit.Position position = new PathUnit.Position();
    return this.CreatePath(out unit, ref randomizer, buildIndex, startPos, position, endPos, position, position, laneTypes, vehicleTypes, maxLength, false, false, false, false, false, false, false, false);
  }

  public bool CreatePath(
    out uint unit,
    ref Randomizer randomizer,
    uint buildIndex,
    PathUnit.Position startPosA,
    PathUnit.Position startPosB,
    PathUnit.Position endPosA,
    PathUnit.Position endPosB,
    NetInfo.LaneType laneTypes,
    VehicleInfo.VehicleType vehicleTypes,
    float maxLength)
  {
    PathUnit.Position vehiclePosition = new PathUnit.Position();
    return this.CreatePath(out unit, ref randomizer, buildIndex, startPosA, startPosB, endPosA, endPosB, vehiclePosition, laneTypes, vehicleTypes, maxLength, false, false, false, false, false, false, false, false);
  }

  public bool CreatePath(
    out uint unit,
    ref Randomizer randomizer,
    uint buildIndex,
    PathUnit.Position startPosA,
    PathUnit.Position startPosB,
    PathUnit.Position endPosA,
    PathUnit.Position endPosB,
    NetInfo.LaneType laneTypes,
    VehicleInfo.VehicleType vehicleTypes,
    float maxLength,
    bool isHeavyVehicle,
    bool ignoreBlocked,
    bool stablePath,
    bool skipQueue)
  {
    PathUnit.Position vehiclePosition = new PathUnit.Position();
    return this.CreatePath(out unit, ref randomizer, buildIndex, startPosA, startPosB, endPosA, endPosB, vehiclePosition, laneTypes, vehicleTypes, maxLength, isHeavyVehicle, ignoreBlocked, stablePath, skipQueue, false, false, false, false);
  }

  public bool CreatePath(
    out uint unit,
    ref Randomizer randomizer,
    uint buildIndex,
    PathUnit.Position startPosA,
    PathUnit.Position startPosB,
    PathUnit.Position endPosA,
    PathUnit.Position endPosB,
    PathUnit.Position vehiclePosition,
    NetInfo.LaneType laneTypes,
    VehicleInfo.VehicleType vehicleTypes,
    float maxLength,
    bool isHeavyVehicle,
    bool ignoreBlocked,
    bool stablePath,
    bool skipQueue,
    bool randomParking)
  {
    return this.CreatePath(out unit, ref randomizer, buildIndex, startPosA, startPosB, endPosA, endPosB, vehiclePosition, laneTypes, vehicleTypes, maxLength, isHeavyVehicle, ignoreBlocked, stablePath, skipQueue, randomParking, false, false, false);
  }

  public bool CreatePath(
    out uint unit,
    ref Randomizer randomizer,
    uint buildIndex,
    PathUnit.Position startPosA,
    PathUnit.Position startPosB,
    PathUnit.Position endPosA,
    PathUnit.Position endPosB,
    PathUnit.Position vehiclePosition,
    NetInfo.LaneType laneTypes,
    VehicleInfo.VehicleType vehicleTypes,
    float maxLength,
    bool isHeavyVehicle,
    bool ignoreBlocked,
    bool stablePath,
    bool skipQueue,
    bool randomParking,
    bool ignoreFlooded)
  {
    return this.CreatePath(out unit, ref randomizer, buildIndex, startPosA, startPosB, endPosA, endPosB, vehiclePosition, laneTypes, vehicleTypes, maxLength, isHeavyVehicle, ignoreBlocked, stablePath, skipQueue, randomParking, ignoreFlooded, false, false);
  }

  public bool CreatePath(
    out uint unit,
    ref Randomizer randomizer,
    uint buildIndex,
    PathUnit.Position startPosA,
    PathUnit.Position startPosB,
    PathUnit.Position endPosA,
    PathUnit.Position endPosB,
    PathUnit.Position vehiclePosition,
    NetInfo.LaneType laneTypes,
    VehicleInfo.VehicleType vehicleTypes,
    float maxLength,
    bool isHeavyVehicle,
    bool ignoreBlocked,
    bool stablePath,
    bool skipQueue,
    bool randomParking,
    bool ignoreFlooded,
    bool combustionEngine)
  {
    return this.CreatePath(out unit, ref randomizer, buildIndex, startPosA, startPosB, endPosA, endPosB, vehiclePosition, laneTypes, vehicleTypes, maxLength, isHeavyVehicle, ignoreBlocked, stablePath, skipQueue, randomParking, ignoreFlooded, combustionEngine, false);
  }

    // this method adds a new path to the m_pathUnits buffer
    // a number (but certainly not all) of the pathUnit's journey-global members are set here too
  public bool CreatePath(
    out uint unit,
    ref Randomizer randomizer,
    uint buildIndex,
    PathUnit.Position startPosA,
    PathUnit.Position startPosB,
    PathUnit.Position endPosA,
    PathUnit.Position endPosB,
    PathUnit.Position vehiclePosition,
    NetInfo.LaneType laneTypes,
    VehicleInfo.VehicleType vehicleTypes,
    float maxLength,
    bool isHeavyVehicle,
    bool ignoreBlocked,
    bool stablePath,
    bool skipQueue,
    bool randomParking,
    bool ignoreFlooded,
    bool combustionEngine,
    bool ignoreCost)
  {
    while (!Monitor.TryEnter(this.m_bufferLock, SimulationManager.SYNCHRONIZE_TIMEOUT));
    uint num1;
    try
    {
      if (!this.m_pathUnits.CreateItem(out num1, ref randomizer))
      {
        unit = 0U;
        return false;
      }
      this.m_pathUnitCount = (int) this.m_pathUnits.ItemCount() - 1;
    }
    finally
    {
      Monitor.Exit(this.m_bufferLock);
    }
    unit = num1;
    this.m_pathUnits.m_buffer[unit].m_simulationFlags = (byte) 1;
    if (isHeavyVehicle)
      this.m_pathUnits.m_buffer[(IntPtr) unit].m_simulationFlags |= (byte) 16;
    if (ignoreBlocked)
      this.m_pathUnits.m_buffer[(IntPtr) unit].m_simulationFlags |= (byte) 32;
    if (stablePath)
      this.m_pathUnits.m_buffer[(IntPtr) unit].m_simulationFlags |= (byte) 64;
    if (randomParking)
      this.m_pathUnits.m_buffer[(IntPtr) unit].m_simulationFlags |= (byte) 128;
    if (ignoreFlooded)
      this.m_pathUnits.m_buffer[(IntPtr) unit].m_simulationFlags |= (byte) 2;
    if (combustionEngine)
      this.m_pathUnits.m_buffer[(IntPtr) unit].m_simulationFlags |= (byte) 4;
    if (ignoreCost)
      this.m_pathUnits.m_buffer[(IntPtr) unit].m_simulationFlags |= (byte) 8;
    this.m_pathUnits.m_buffer[(IntPtr) unit].m_pathFindFlags = (byte) 0;
    this.m_pathUnits.m_buffer[(IntPtr) unit].m_buildIndex = buildIndex;
    this.m_pathUnits.m_buffer[(IntPtr) unit].m_position00 = startPosA;
    this.m_pathUnits.m_buffer[(IntPtr) unit].m_position01 = endPosA;
    this.m_pathUnits.m_buffer[(IntPtr) unit].m_position02 = startPosB;
    this.m_pathUnits.m_buffer[(IntPtr) unit].m_position03 = endPosB;
    this.m_pathUnits.m_buffer[(IntPtr) unit].m_position11 = vehiclePosition;
    this.m_pathUnits.m_buffer[(IntPtr) unit].m_nextPathUnit = 0U;
    this.m_pathUnits.m_buffer[(IntPtr) unit].m_laneTypes = (byte) laneTypes;
    this.m_pathUnits.m_buffer[(IntPtr) unit].m_vehicleTypes = (ushort) vehicleTypes;
    this.m_pathUnits.m_buffer[(IntPtr) unit].m_length = maxLength;
    this.m_pathUnits.m_buffer[(IntPtr) unit].m_positionCount = (byte) 20;       // still trying to understand this one. Helpful to know initialized to 0x14 (ie 0001 0100 in bits)
    this.m_pathUnits.m_buffer[(IntPtr) unit].m_referenceCount = (byte) 1;
    int toplimit = 10000000;
    PathFind pathFind = (PathFind) null;
    for (int index = 0; index < this.m_pathfinds.Length; ++index)
    {
      PathFind pathfind = this.m_pathfinds[index];
      if (pathfind.IsAvailable && pathfind.m_queuedPathFindCount < toplimit)
      {
        toplimit = pathfind.m_queuedPathFindCount;
        pathFind = pathfind;
      }
    }
    if (pathFind != null && pathFind.CalculatePath(unit, skipQueue))
      return true;
    this.ReleasePath(unit);
    return false;
  }

  public void WaitForAllPaths()
  {
    for (int index = 0; index < this.m_pathfinds.Length; ++index)
      this.m_pathfinds[index].WaitForAllPaths();
  }

  public bool AddPathReference(uint unit)
  {
    if (unit == 0U || this.m_pathUnits.m_buffer[(IntPtr) unit].m_simulationFlags == (byte) 0)
      return false;
    do
      ;
    while (!Monitor.TryEnter(this.m_bufferLock, SimulationManager.SYNCHRONIZE_TIMEOUT));
    try
    {
      if (this.m_pathUnits.m_buffer[(IntPtr) unit].m_referenceCount >= byte.MaxValue)
        return false;
      ++this.m_pathUnits.m_buffer[(IntPtr) unit].m_referenceCount;
      return true;
    }
    finally
    {
      Monitor.Exit(this.m_bufferLock);
    }
  }

  public void ReleaseFirstUnit(ref uint unit)
  {
    if (unit == 0U)
      return;
    if (this.m_pathUnits.m_buffer[(IntPtr) unit].m_simulationFlags == (byte) 0)
    {
      unit = 0U;
    }
    else
    {
      do
        ;
      while (!Monitor.TryEnter(this.m_bufferLock, SimulationManager.SYNCHRONIZE_TIMEOUT));
      try
      {
        uint nextPathUnit = this.m_pathUnits.m_buffer[(IntPtr) unit].m_nextPathUnit;
        if (this.m_pathUnits.m_buffer[(IntPtr) unit].m_referenceCount <= (byte) 1)
        {
          this.m_pathUnits.m_buffer[(IntPtr) unit].m_simulationFlags = (byte) 0;
          this.m_pathUnits.m_buffer[(IntPtr) unit].m_pathFindFlags = (byte) 0;
          this.m_pathUnits.m_buffer[(IntPtr) unit].m_nextPathUnit = 0U;
          this.m_pathUnits.m_buffer[(IntPtr) unit].m_referenceCount = (byte) 0;
          this.m_pathUnits.ReleaseItem(unit);
          unit = nextPathUnit;
        }
        else
        {
          --this.m_pathUnits.m_buffer[(IntPtr) unit].m_referenceCount;
          if (this.m_pathUnits.m_buffer[(IntPtr) nextPathUnit].m_referenceCount < byte.MaxValue)
          {
            ++this.m_pathUnits.m_buffer[(IntPtr) nextPathUnit].m_referenceCount;
            unit = nextPathUnit;
          }
          else
            unit = 0U;
        }
        this.m_pathUnitCount = (int) this.m_pathUnits.ItemCount() - 1;
      }
      finally
      {
        Monitor.Exit(this.m_bufferLock);
      }
    }
  }

  public void ReleasePath(uint unit)
  {
    if (this.m_pathUnits.m_buffer[(IntPtr) unit].m_simulationFlags == (byte) 0)
      return;
    do
      ;
    while (!Monitor.TryEnter(this.m_bufferLock, SimulationManager.SYNCHRONIZE_TIMEOUT));
    try
    {
      int num = 0;
      while (unit != 0U)
      {
        if (this.m_pathUnits.m_buffer[(IntPtr) unit].m_referenceCount <= (byte) 1)
        {
          uint nextPathUnit = this.m_pathUnits.m_buffer[(IntPtr) unit].m_nextPathUnit;
          this.m_pathUnits.m_buffer[(IntPtr) unit].m_simulationFlags = (byte) 0;
          this.m_pathUnits.m_buffer[(IntPtr) unit].m_pathFindFlags = (byte) 0;
          this.m_pathUnits.m_buffer[(IntPtr) unit].m_nextPathUnit = 0U;
          this.m_pathUnits.m_buffer[(IntPtr) unit].m_referenceCount = (byte) 0;
          this.m_pathUnits.ReleaseItem(unit);
          unit = nextPathUnit;
          if (++num >= 262144)
          {
            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
            break;
          }
        }
        else
        {
          --this.m_pathUnits.m_buffer[(IntPtr) unit].m_referenceCount;
          break;
        }
      }
      this.m_pathUnitCount = (int) this.m_pathUnits.ItemCount() - 1;
    }
    finally
    {
      Monitor.Exit(this.m_bufferLock);
    }
  }

  protected override void SimulationStepImpl(int subStep)
  {
    int a = 0;
    for (int index = 0; index < this.m_pathfinds.Length; ++index)
      a = Mathf.Max(a, this.m_pathfinds[index].m_queuedPathFindCount);
    if (a < 100 || this.m_terminated)
      return;
    Thread.Sleep((a - 100) / 100 + 1);
  }

  public override void GetData(FastList<IDataContainer> data)
  {
    base.GetData(data);
    data.Add((IDataContainer) new PathManager.Data());
  }

  string ISimulationManager.GetName()
  {
    return this.GetName();
  }

  ThreadProfiler ISimulationManager.GetSimulationProfiler()
  {
    return this.GetSimulationProfiler();
  }

  void ISimulationManager.SimulationStep(int subStep)
  {
    this.SimulationStep(subStep);
  }

  public class Data : IDataContainer
  {
    public void Serialize(DataSerializer s)
    {
      Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.BeginSerialize(s, nameof (PathManager));
      PathManager instance = Singleton<PathManager>.instance;
      instance.WaitForAllPaths();
      PathUnit[] buffer = instance.m_pathUnits.m_buffer;
      int length = buffer.Length;
      EncodedArray.Byte @byte = EncodedArray.Byte.BeginWrite(s);
      for (int index = 1; index < length; ++index)
        @byte.Write(buffer[index].m_simulationFlags);
      @byte.EndWrite();
      for (int index1 = 1; index1 < length; ++index1)
      {
        if (buffer[index1].m_simulationFlags != (byte) 0)
        {
          s.WriteUInt8((uint) buffer[index1].m_pathFindFlags);
          s.WriteUInt8((uint) buffer[index1].m_laneTypes);
          s.WriteUInt16((uint) buffer[index1].m_vehicleTypes);
          s.WriteUInt8((uint) buffer[index1].m_positionCount);
          s.WriteUInt24(buffer[index1].m_nextPathUnit);
          s.WriteUInt32(buffer[index1].m_buildIndex);
          s.WriteFloat(buffer[index1].m_length);
          s.WriteUInt8((uint) buffer[index1].m_speed);
          int positionCount = (int) buffer[index1].m_positionCount;
          for (int index2 = 0; index2 < positionCount; ++index2)
          {
            PathUnit.Position position = buffer[index1].GetPosition(index2);
            s.WriteUInt16((uint) position.m_segment);
            s.WriteUInt8((uint) position.m_offset);
            s.WriteUInt8((uint) position.m_lane);
          }
        }
      }
      Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.EndSerialize(s, nameof (PathManager));
    }

    public void Deserialize(DataSerializer s)
    {
      Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.BeginDeserialize(s, nameof (PathManager));
      PathManager instance = Singleton<PathManager>.instance;
      instance.WaitForAllPaths();
      PathUnit[] buffer = instance.m_pathUnits.m_buffer;
      int num = buffer.Length;
      instance.m_pathUnits.ClearUnused();
      if (s.version < 123U)
        num = 131072;
      if (s.version >= 46U)
      {
        EncodedArray.Byte @byte = EncodedArray.Byte.BeginRead(s);
        for (int index = 1; index < num; ++index)
          buffer[index].m_simulationFlags = @byte.Read();
        @byte.EndRead();
      }
      else
      {
        for (int index = 1; index < num; ++index)
          buffer[index].m_simulationFlags = (byte) 0;
      }
      for (int index1 = 1; index1 < num; ++index1)
      {
        buffer[index1].m_referenceCount = (byte) 0;
        if (buffer[index1].m_simulationFlags != (byte) 0)
        {
          buffer[index1].m_pathFindFlags = (byte) s.ReadUInt8();
          buffer[index1].m_laneTypes = s.version < 49U ? (byte) 3 : (byte) s.ReadUInt8();
          buffer[index1].m_vehicleTypes = s.version < 311U ? (ushort) (byte) s.ReadUInt8() : (ushort) s.ReadUInt16();
          buffer[index1].m_positionCount = (byte) s.ReadUInt8();
          buffer[index1].m_nextPathUnit = s.ReadUInt24();
          buffer[index1].m_buildIndex = s.ReadUInt32();
          buffer[index1].m_length = s.ReadFloat();
          buffer[index1].m_speed = s.version >= 110026U && s.version < 111000U || s.version >= 111012U ? (byte) s.ReadUInt8() : (byte) 100;
          int positionCount = (int) buffer[index1].m_positionCount;
          for (int index2 = 0; index2 < positionCount; ++index2)
          {
            PathUnit.Position position;
            position.m_segment = (ushort) s.ReadUInt16();
            position.m_offset = (byte) s.ReadUInt8();
            position.m_lane = (byte) s.ReadUInt8();
            buffer[index1].SetPosition(index2, position);
          }
        }
        else
        {
          buffer[index1].m_pathFindFlags = (byte) 0;
          buffer[index1].m_laneTypes = (byte) 0;
          buffer[index1].m_vehicleTypes = (ushort) 0;
          buffer[index1].m_positionCount = (byte) 0;
          buffer[index1].m_nextPathUnit = 0U;
          buffer[index1].m_buildIndex = 0U;
          buffer[index1].m_length = 0.0f;
          buffer[index1].m_speed = (byte) 0;
          instance.m_pathUnits.ReleaseItem((uint) index1);
        }
      }
      if (s.version < 123U)
      {
        int length = buffer.Length;
        for (int index = num; index < length; ++index)
        {
          buffer[index].m_referenceCount = (byte) 0;
          buffer[index].m_simulationFlags = (byte) 0;
          buffer[index].m_pathFindFlags = (byte) 0;
          buffer[index].m_laneTypes = (byte) 0;
          buffer[index].m_vehicleTypes = (ushort) 0;
          buffer[index].m_positionCount = (byte) 0;
          buffer[index].m_nextPathUnit = 0U;
          buffer[index].m_buildIndex = 0U;
          buffer[index].m_length = 0.0f;
          instance.m_pathUnits.ReleaseItem((uint) index);
        }
      }
      Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.EndDeserialize(s, nameof (PathManager));
    }

    public void AfterDeserialize(DataSerializer s)
    {
      Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.BeginAfterDeserialize(s, nameof (PathManager));
      Singleton<LoadingManager>.instance.WaitUntilEssentialScenesLoaded();
      PathManager instance = Singleton<PathManager>.instance;
      PathUnit[] buffer = instance.m_pathUnits.m_buffer;
      int length = buffer.Length;
      for (int index = 1; index < length; ++index)
      {
        if (buffer[index].m_simulationFlags != (byte) 0 && buffer[index].m_nextPathUnit != 0U)
          ++buffer[(IntPtr) buffer[index].m_nextPathUnit].m_referenceCount;
      }
      instance.m_pathUnitCount = (int) instance.m_pathUnits.ItemCount() - 1;
      Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.EndAfterDeserialize(s, nameof (PathManager));
    }
  }
}
