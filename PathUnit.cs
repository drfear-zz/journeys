// Decompiled with JetBrains decompiler
// Type: PathUnit
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 9724B8F4-19DD-48C3-AE02-CDA150D75CEC
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Cities_Skylines\Cities_Data\Managed\Assembly-CSharp.dll

using ColossalFramework;
using System;
using UnityEngine;

public struct PathUnit
{
  public const byte FLAG_CREATED = 1;
  public const byte FLAG_IGNORE_FLOODED = 2;
  public const byte FLAG_COMBUSTION = 4;
  public const byte FLAG_IGNORE_COST = 8;
  public const byte FLAG_IS_HEAVY = 16;
  public const byte FLAG_IGNORE_BLOCKED = 32;
  public const byte FLAG_STABLE_PATH = 64;
  public const byte FLAG_RANDOM_PARKING = 128;
  public const byte FLAG_QUEUED = 1;
  public const byte FLAG_CALCULATING = 2;
  public const byte FLAG_READY = 4;
  public const byte FLAG_FAILED = 8;
  public const int MAX_POSITIONS = 12;
  public PathUnit.Position m_position00;            // positions 00 to 03 are used to hold start and target when creating a path, but they change when the path has been created
  public PathUnit.Position m_position01;            // basically the path goes from 00 to 01 to 02 ... up to 11.  If needs more, then a pointer is set in m_nextPathUnit to the
  public PathUnit.Position m_position02;            // next PathUnit in the linked list
  public PathUnit.Position m_position03;            //
  public PathUnit.Position m_position04;            // The sim's current path position is held in the Instance, not here in the PathUnit eg CitizenInstance.m_pathPosition = 4
  public PathUnit.Position m_position05;
  public PathUnit.Position m_position06;
  public PathUnit.Position m_position07;
  public PathUnit.Position m_position08;
  public PathUnit.Position m_position09;
  public PathUnit.Position m_position10;
  public PathUnit.Position m_position11;            // vehiclePosition as used by CreatePath, but in later use simply the last of the path positions
  public uint m_buildIndex;                         // datestamp on path so you can test it if is earlier than edits of the grid
  public uint m_nextPathUnit;                       // the next unit in the linked list
  public float m_length;
  public ushort m_vehicleTypes;                     // all the vehicle types that can be used for the journey (wrapped into bit flags in one ushort)
  public byte m_simulationFlags;                    // things like randomparking, ignoreblocked, and stablePath and ignoreCost
  public byte m_pathFindFlags;
  public byte m_laneTypes;                          // all the lane types that can be used on the journey (wrapped up into one byte)
  public byte m_positionCount;                      // the length of the usefully populated list (there can be junk in positions beyond this)
  public byte m_referenceCount;
  public byte m_speed;

  public void SetPosition(int index, PathUnit.Position position)
  {
    switch (index)
    {
      case 0:
        this.m_position00 = position;
        break;
      case 1:
        this.m_position01 = position;
        break;
      case 2:
        this.m_position02 = position;
        break;
      case 3:
        this.m_position03 = position;
        break;
      case 4:
        this.m_position04 = position;
        break;
      case 5:
        this.m_position05 = position;
        break;
      case 6:
        this.m_position06 = position;
        break;
      case 7:
        this.m_position07 = position;
        break;
      case 8:
        this.m_position08 = position;
        break;
      case 9:
        this.m_position09 = position;
        break;
      case 10:
        this.m_position10 = position;
        break;
      case 11:
        this.m_position11 = position;
        break;
    }
  }

  public PathUnit.Position GetPosition(int index)
  {
    switch (index)
    {
      case 0:
        return this.m_position00;
      case 1:
        return this.m_position01;
      case 2:
        return this.m_position02;
      case 3:
        return this.m_position03;
      case 4:
        return this.m_position04;
      case 5:
        return this.m_position05;
      case 6:
        return this.m_position06;
      case 7:
        return this.m_position07;
      case 8:
        return this.m_position08;
      case 9:
        return this.m_position09;
      case 10:
        return this.m_position10;
      case 11:
        return this.m_position11;
      default:
        return new PathUnit.Position();
    }
  }

  public bool GetPosition(int index, out PathUnit.Position position)
  {
    position = this.GetPosition(index);
    NetManager instance = Singleton<NetManager>.instance;
    return position.m_segment != (ushort) 0 && (instance.m_segments.m_buffer[(int) position.m_segment].m_flags & (NetSegment.Flags.Created | NetSegment.Flags.Deleted)) == NetSegment.Flags.Created && instance.m_segments.m_buffer[(int) position.m_segment].m_modifiedIndex < this.m_buildIndex;
  }

  public bool GetNextPosition(int index, out PathUnit.Position position)
  {
    if (index < (int) this.m_positionCount - 1)
      return this.GetPosition(index + 1, out position);
    if (this.m_nextPathUnit != 0U)
      return Singleton<PathManager>.instance.m_pathUnits.m_buffer[(IntPtr) this.m_nextPathUnit].GetPosition(0, out position);
    position = new PathUnit.Position();
    return false;
  }

  public void MoveLastPosition(uint unitID, float distance)
  {
    PathManager instance1 = Singleton<PathManager>.instance;
    NetManager instance2 = Singleton<NetManager>.instance;
    uint num1 = unitID;
    uint nextPathUnit = instance1.m_pathUnits.m_buffer[(IntPtr) num1].m_nextPathUnit;
    PathUnit.Position position1;
    if (!this.GetPosition(0, out position1))
      return;
    int num2 = 0;
    while (nextPathUnit != 0U)
    {
      if (!instance1.m_pathUnits.m_buffer[(IntPtr) num1].GetPosition((int) instance1.m_pathUnits.m_buffer[(IntPtr) num1].m_positionCount - 1, out position1))
        return;
      num1 = nextPathUnit;
      nextPathUnit = instance1.m_pathUnits.m_buffer[(IntPtr) num1].m_nextPathUnit;
      if (++num2 >= 262144)
      {
        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
        return;
      }
    }
    int positionCount = (int) instance1.m_pathUnits.m_buffer[(IntPtr) num1].m_positionCount;
    PathUnit.Position position2;
    if (positionCount > 1 && !instance1.m_pathUnits.m_buffer[(IntPtr) num1].GetPosition(positionCount - 2, out position1) || (!instance1.m_pathUnits.m_buffer[(IntPtr) num1].GetPosition(positionCount - 1, out position2) || position1.m_segment == (ushort) 0) || position2.m_segment == (ushort) 0)
      return;
    uint laneId1 = PathManager.GetLaneID(position1);
    uint laneId2 = PathManager.GetLaneID(position2);
    Vector3 position3 = instance2.m_lanes.m_buffer[(IntPtr) laneId1].CalculatePosition((float) position1.m_offset * 0.003921569f);
    byte offset;
    PathUnit.CalculatePathPositionOffset(laneId2, position3, out offset);
    offset = (int) offset > (int) position2.m_offset ? (byte) Mathf.Clamp(Mathf.RoundToInt((1f - instance2.m_lanes.m_buffer[(IntPtr) laneId2].m_bezier.Invert().Travel((float) (1.0 - (double) position2.m_offset * 0.00392156885936856), distance)) * (float) byte.MaxValue), 0, (int) byte.MaxValue) : (byte) Mathf.Clamp(Mathf.RoundToInt(instance2.m_lanes.m_buffer[(IntPtr) laneId2].m_bezier.Travel((float) position2.m_offset * 0.003921569f, distance) * (float) byte.MaxValue), 0, (int) byte.MaxValue);
    if ((int) offset == (int) position2.m_offset)
      return;
    position2.m_offset = offset;
    instance1.m_pathUnits.m_buffer[(IntPtr) num1].SetPosition(positionCount - 1, position2);
  }

  public bool GetLastPosition(out PathUnit.Position position)
  {
    uint num1 = this.m_nextPathUnit;
    if (num1 == 0U)
      return this.GetPosition((int) this.m_positionCount - 1, out position);
    PathManager instance = Singleton<PathManager>.instance;
    int num2 = 0;
    uint nextPathUnit = instance.m_pathUnits.m_buffer[(IntPtr) num1].m_nextPathUnit;
    while (nextPathUnit != 0U)
    {
      num1 = nextPathUnit;
      nextPathUnit = instance.m_pathUnits.m_buffer[(IntPtr) num1].m_nextPathUnit;
      if (++num2 >= 262144)
      {
        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
        position = new PathUnit.Position();
        return false;
      }
    }
    return instance.m_pathUnits.m_buffer[(IntPtr) num1].GetLastPosition(out position);
  }

  public bool CalculatePathPositionOffset(int index, Vector3 refPos, out byte offset)
  {
    PathUnit.Position position;
    if (this.GetPosition(index, out position))
    {
      PathUnit.CalculatePathPositionOffset(PathManager.GetLaneID(position), refPos, out offset);
      return true;
    }
    offset = (byte) 0;
    return false;
  }

  public static void CalculatePathPositionOffset(uint laneID, Vector3 refPos, out byte offset)
  {
    Vector3 position;
    float laneOffset;
    Singleton<NetManager>.instance.m_lanes.m_buffer[(IntPtr) laneID].GetClosestPosition(refPos, out position, out laneOffset);
    offset = (byte) Mathf.Clamp(Mathf.RoundToInt(laneOffset * (float) byte.MaxValue), 0, (int) byte.MaxValue);
  }

  public static bool GetNextPosition(
    ref uint unitID,
    ref int index,
    out PathUnit.Position position,
    out bool invalid)
  {
    PathManager instance = Singleton<PathManager>.instance;
    if (index < (int) instance.m_pathUnits.m_buffer[(IntPtr) unitID].m_positionCount - 1)
    {
      ++index;
      invalid = !instance.m_pathUnits.m_buffer[(IntPtr) unitID].GetPosition(index, out position);
      return !invalid;
    }
    unitID = instance.m_pathUnits.m_buffer[(IntPtr) unitID].m_nextPathUnit;
    if (unitID != 0U)
    {
      index = 0;
      invalid = !instance.m_pathUnits.m_buffer[(IntPtr) unitID].GetPosition(index, out position);
      return !invalid;
    }
    position = new PathUnit.Position();
    invalid = false;
    return false;
  }

  public struct Position
  {
    public ushort m_segment;
    public byte m_offset;
    public byte m_lane;
  }
}
