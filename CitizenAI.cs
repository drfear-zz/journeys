// Decompiled with JetBrains decompiler
// Type: CitizenAI
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 9724B8F4-19DD-48C3-AE02-CDA150D75CEC
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Cities_Skylines\Cities_Data\Managed\Assembly-CSharp.dll

using ColossalFramework;
using ColossalFramework.Math;
using System;
using UnityEngine;

public class CitizenAI : PrefabAI
{
  [NonSerialized]
  public CitizenInfo m_info;

  public virtual void InitializeAI()
  {
  }

  public virtual void ReleaseAI()
  {
  }

  public virtual Color GetColor(
    ushort instanceID,
    ref CitizenInstance data,
    InfoManager.InfoMode infoMode)
  {
    switch (infoMode)
    {
      case InfoManager.InfoMode.None:
        if ((data.m_flags & CitizenInstance.Flags.CustomColor) != CitizenInstance.Flags.None)
          return (Color) data.m_color;
        switch (new Randomizer((int) instanceID).Int32(4U))
        {
          case 0:
            return this.m_info.m_color0;
          case 1:
            return this.m_info.m_color1;
          case 2:
            return this.m_info.m_color2;
          case 3:
            return this.m_info.m_color3;
          default:
            return this.m_info.m_color0;
        }
      case InfoManager.InfoMode.TrafficRoutes:
        if (Singleton<InfoManager>.instance.CurrentSubMode == InfoManager.SubInfoMode.Default)
        {
          InstanceID empty = InstanceID.Empty;
          empty.CitizenInstance = instanceID;
          if (Singleton<NetManager>.instance.PathVisualizer.IsPathVisible(empty))
          {
            uint citizen = data.m_citizen;
            if (citizen != 0U)
            {
              ushort vehicle = Singleton<CitizenManager>.instance.m_citizens.m_buffer[(IntPtr) citizen].m_vehicle;
              if (vehicle != (ushort) 0)
              {
                VehicleInfo info = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[(int) vehicle].Info;
                if ((UnityEngine.Object) info != (UnityEngine.Object) null && info.m_vehicleType == VehicleInfo.VehicleType.Bicycle)
                  return Singleton<InfoManager>.instance.m_properties.m_routeColors[1];
                return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
              }
            }
            return Singleton<InfoManager>.instance.m_properties.m_routeColors[0];
          }
          break;
        }
        break;
    }
    return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
  }

  public virtual void SetRenderParameters(
    RenderManager.CameraInfo cameraInfo,
    ushort instanceID,
    ref CitizenInstance data,
    Vector3 position,
    Quaternion rotation,
    Vector3 velocity,
    Color color,
    bool underground)
  {
    this.m_info.SetRenderParameters(position, rotation, velocity, color, 0, underground);
  }

  public virtual string GetLocalizedStatus(
    ushort instanceID,
    ref CitizenInstance data,
    out InstanceID target)
  {
    target = InstanceID.Empty;
    return (string) null;
  }

  public virtual string GetLocalizedStatus(uint citizenID, ref Citizen data, out InstanceID target)
  {
    target = InstanceID.Empty;
    return (string) null;
  }

  public virtual string GetDebugString(ushort instanceID, ref CitizenInstance data)
  {
    return StringUtils.SafeFormat("\nWorkplace: {0}\nHome: {1}\nEducation: {2}", (object) Singleton<CitizenManager>.instance.m_citizens.m_buffer[(IntPtr) data.m_citizen].m_workBuilding, (object) Singleton<CitizenManager>.instance.m_citizens.m_buffer[(IntPtr) data.m_citizen].m_homeBuilding, (object) Singleton<CitizenManager>.instance.m_citizens.m_buffer[(IntPtr) data.m_citizen].EducationLevel);
  }

  public virtual void SimulationStep(uint citizenID, ref Citizen data)
  {
  }

  public virtual void SimulationStep(uint homeID, ref CitizenUnit data)
  {
  }

  public virtual void StartTransfer(
    uint citizenID,
    ref Citizen data,
    TransferManager.TransferReason material,
    TransferManager.TransferOffer offer)
  {
  }

  public virtual void CreateInstance(ushort instanceID, ref CitizenInstance data)
  {
  }

  public virtual void ReleaseInstance(ushort instanceID, ref CitizenInstance data)
  {
    this.SetSource(instanceID, ref data, (ushort) 0);
    this.SetTarget(instanceID, ref data, (ushort) 0);
  }

  public virtual void SimulationStep(
    ushort instanceID,
    ref CitizenInstance data,
    Vector3 physicsLodRefPos)
  {
    if ((data.m_flags & CitizenInstance.Flags.Character) == CitizenInstance.Flags.None)
      return;
    CitizenInstance.Frame lastFrameData = data.GetLastFrameData();
    int gridX1 = Mathf.Clamp((int) ((double) lastFrameData.m_position.x / 8.0 + 1080.0), 0, 2159);
    int gridZ1 = Mathf.Clamp((int) ((double) lastFrameData.m_position.z / 8.0 + 1080.0), 0, 2159);
    bool lodPhysics = (double) Vector3.SqrMagnitude(physicsLodRefPos - lastFrameData.m_position) >= 62500.0;
    this.SimulationStep(instanceID, ref data, ref lastFrameData, lodPhysics);
    int gridX2 = Mathf.Clamp((int) ((double) lastFrameData.m_position.x / 8.0 + 1080.0), 0, 2159);
    int gridZ2 = Mathf.Clamp((int) ((double) lastFrameData.m_position.z / 8.0 + 1080.0), 0, 2159);
    if ((gridX2 != gridX1 || gridZ2 != gridZ1) && (data.m_flags & CitizenInstance.Flags.Character) != CitizenInstance.Flags.None)
    {
      Singleton<CitizenManager>.instance.RemoveFromGrid(instanceID, ref data, gridX1, gridZ1);
      Singleton<CitizenManager>.instance.AddToGrid(instanceID, ref data, gridX2, gridZ2);
    }
    if (data.m_flags == CitizenInstance.Flags.None)
      return;
    data.SetFrameData(Singleton<SimulationManager>.instance.m_currentFrameIndex, lastFrameData);
  }

  public virtual void SimulationStep(
    ushort instanceID,
    ref CitizenInstance citizenData,
    ref CitizenInstance.Frame frameData,
    bool lodPhysics)
  {
  }

  public virtual void LoadInstance(ushort instanceID, ref CitizenInstance data)
  {
  }

  public virtual void SetSource(ushort instanceID, ref CitizenInstance data, ushort sourceBuilding)
  {
  }

  public void SetTarget(ushort instanceID, ref CitizenInstance data, ushort targetBuilding)
  {
    this.SetTarget(instanceID, ref data, targetBuilding, false);
  }

  public virtual void SetTarget(
    ushort instanceID,
    ref CitizenInstance data,
    ushort targetIndex,
    bool targetIsNode)
  {
  }

  public virtual void JoinTarget(ushort instanceID, ref CitizenInstance data, ushort otherInstance)
  {
  }

  public virtual void BuildingRelocated(
    ushort instanceID,
    ref CitizenInstance data,
    ushort building)
  {
  }

  public virtual bool TransportArriveAtSource(
    ushort instanceID,
    ref CitizenInstance citizenData,
    Vector3 currentPos,
    Vector3 nextTarget)
  {
    return false;
  }

  public virtual bool TransportArriveAtTarget(
    ushort instanceID,
    ref CitizenInstance citizenData,
    Vector3 currentPos,
    Vector3 nextTarget,
    ref TransportPassengerData passengerData,
    bool forceUnload)
  {
    return true;
  }

  public virtual bool SetCurrentVehicle(
    ushort instanceID,
    ref CitizenInstance citizenData,
    ushort vehicleID,
    uint unitID,
    Vector3 position)
  {
    return false;
  }

  public virtual bool AddWind(
    ushort instanceID,
    ref CitizenInstance citizenData,
    Vector3 wind,
    InstanceManager.Group group)
  {
    return false;
  }

  public virtual void EnterParkArea(
    ushort instanceID,
    ref CitizenInstance citizenData,
    byte park,
    ushort gateID)
  {
  }

  protected Vector4 GetPathTargetPosition(
    ushort instanceID,
    ref CitizenInstance citizenData,
    ref CitizenInstance.Frame frameData,
    float minSqrDistance)
  {
    PathManager instance1 = Singleton<PathManager>.instance;
    NetManager instance2 = Singleton<NetManager>.instance;
    Vector4 vector4 = citizenData.m_targetPos;
    if ((double) VectorUtils.LengthSqrXZ((Vector3) citizenData.m_targetPos - frameData.m_position) >= (double) minSqrDistance)
      return vector4;
    if (citizenData.m_pathPositionIndex == byte.MaxValue)
    {
      citizenData.m_pathPositionIndex = (byte) 0;
      if (!Singleton<PathManager>.instance.m_pathUnits.m_buffer[(IntPtr) citizenData.m_path].CalculatePathPositionOffset((int) citizenData.m_pathPositionIndex >> 1, (Vector3) vector4, out citizenData.m_lastPathOffset))
      {
        this.InvalidPath(instanceID, ref citizenData);
        return vector4;
      }
    }
    PathUnit.Position position1;
    if (!instance1.m_pathUnits.m_buffer[(IntPtr) citizenData.m_path].GetPosition((int) citizenData.m_pathPositionIndex >> 1, out position1))
    {
      this.InvalidPath(instanceID, ref citizenData);
      return vector4;
    }
    if (((int) citizenData.m_pathPositionIndex & 1) == 0)
    {
      int index = ((int) citizenData.m_pathPositionIndex >> 1) + 1;
      uint num = citizenData.m_path;
      if (index >= (int) instance1.m_pathUnits.m_buffer[(IntPtr) num].m_positionCount)
      {
        index = 0;
        num = instance1.m_pathUnits.m_buffer[(IntPtr) num].m_nextPathUnit;
      }
      PathUnit.Position position2;
      if (num != 0U && instance1.m_pathUnits.m_buffer[(IntPtr) num].GetPosition(index, out position2) && (int) position2.m_segment == (int) position1.m_segment)
      {
        NetInfo info = instance2.m_segments.m_buffer[(int) position1.m_segment].Info;
        if (info.m_lanes.Length > (int) position1.m_lane && info.m_lanes.Length > (int) position2.m_lane && (double) Mathf.Abs(info.m_lanes[(int) position1.m_lane].m_position - info.m_lanes[(int) position2.m_lane].m_position) < 4.0)
        {
          citizenData.m_pathPositionIndex = (byte) (index << 1);
          position1 = position2;
          if ((int) num != (int) citizenData.m_path)
            Singleton<PathManager>.instance.ReleaseFirstUnit(ref citizenData.m_path);
        }
      }
    }
    uint num1 = PathManager.GetLaneID(position1);
    float num2 = (float) new Randomizer((int) instanceID).Int32(-500, 500) * (1f / 1000f);
    NetInfo info1;
    float num3;
    int index1;
    uint num4;
    PathUnit.Position position3;
    Bezier3 bezier;
    Vector3 direction1;
    Segment3 segment3;
    Vector3 vector3_1;
    while (true)
    {
      info1 = instance2.m_segments.m_buffer[(int) position1.m_segment].Info;
      if (info1.m_lanes.Length > (int) position1.m_lane)
      {
        float width1 = info1.m_lanes[(int) position1.m_lane].m_width;
        num3 = Mathf.Max(0.0f, width1 - 1f) * num2;
        float num5 = info1.m_lanes[(int) position1.m_lane].m_useTerrainHeight || (citizenData.m_flags & CitizenInstance.Flags.OnPath) == CitizenInstance.Flags.None ? 1f : 0.0f;
        if (((int) citizenData.m_pathPositionIndex & 1) == 0)
        {
          bool flag = true;
          int num6 = (int) position1.m_offset - (int) citizenData.m_lastPathOffset;
          while (num6 != 0)
          {
            if (flag)
            {
              flag = false;
            }
            else
            {
              float num7 = Mathf.Sqrt(minSqrDistance) - VectorUtils.LengthXZ((Vector3) vector4 - frameData.m_position);
              int num8 = (double) num7 >= 0.0 ? 4 + Mathf.CeilToInt((float) ((double) num7 * 256.0 / ((double) instance2.m_lanes.m_buffer[(IntPtr) num1].m_length + 1.0))) : 4;
              if (num6 < 0)
                citizenData.m_lastPathOffset = (byte) Mathf.Max((int) citizenData.m_lastPathOffset - num8, (int) position1.m_offset);
              else if (num6 > 0)
                citizenData.m_lastPathOffset = (byte) Mathf.Min((int) citizenData.m_lastPathOffset + num8, (int) position1.m_offset);
            }
            Vector3 position2;
            Vector3 direction2;
            instance2.m_lanes.m_buffer[(IntPtr) num1].CalculatePositionAndDirection((float) citizenData.m_lastPathOffset * 0.003921569f, out position2, out direction2);
            vector4 = (Vector4) position2;
            vector4.w = num5;
            Vector3 vector3_2 = Vector3.Cross(Vector3.up, direction2).normalized * num3;
            if (num6 > 0)
            {
              vector4.x += vector3_2.x;
              vector4.z += vector3_2.z;
            }
            else
            {
              vector4.x -= vector3_2.x;
              vector4.z -= vector3_2.z;
            }
            if ((double) VectorUtils.LengthSqrXZ((Vector3) vector4 - frameData.m_position) >= (double) minSqrDistance)
            {
              citizenData.m_flags = citizenData.m_flags & ~(CitizenInstance.Flags.Underground | CitizenInstance.Flags.InsideBuilding | CitizenInstance.Flags.Transition) | info1.m_setCitizenFlags;
              return vector4;
            }
            num6 = (int) position1.m_offset - (int) citizenData.m_lastPathOffset;
            if ((citizenData.m_flags & CitizenInstance.Flags.OnPath) == CitizenInstance.Flags.None)
            {
              citizenData.m_flags |= CitizenInstance.Flags.OnPath;
              if ((citizenData.m_flags & CitizenInstance.Flags.RidingBicycle) == CitizenInstance.Flags.None && (instance2.m_segments.m_buffer[(int) position1.m_segment].m_flags & NetSegment.Flags.BikeBan) == NetSegment.Flags.None)
                this.SpawnBicycle(instanceID, ref citizenData, position1);
            }
          }
          ++citizenData.m_pathPositionIndex;
          citizenData.m_lastPathOffset = (byte) 0;
        }
        index1 = ((int) citizenData.m_pathPositionIndex >> 1) + 1;
        num4 = citizenData.m_path;
        if (index1 >= (int) instance1.m_pathUnits.m_buffer[(IntPtr) citizenData.m_path].m_positionCount)
        {
          index1 = 0;
          num4 = instance1.m_pathUnits.m_buffer[(IntPtr) citizenData.m_path].m_nextPathUnit;
          if (num4 == 0U)
            goto label_37;
        }
        if (instance1.m_pathUnits.m_buffer[(IntPtr) num4].GetPosition(index1, out position3))
        {
          NetInfo info2 = instance2.m_segments.m_buffer[(int) position3.m_segment].Info;
          if (info2.m_lanes.Length > (int) position3.m_lane)
          {
            int index2 = index1 + 1;
            uint num6 = num4;
            uint num7 = 0;
            if (index2 >= (int) instance1.m_pathUnits.m_buffer[(IntPtr) num4].m_positionCount)
            {
              index2 = 0;
              num6 = instance1.m_pathUnits.m_buffer[(IntPtr) num4].m_nextPathUnit;
            }
            PathUnit.Position position2;
            if (num6 != 0U && instance1.m_pathUnits.m_buffer[(IntPtr) num6].GetPosition(index2, out position2) && ((int) position2.m_segment == (int) position3.m_segment && info2.m_lanes.Length > (int) position2.m_lane) && (double) Mathf.Abs(info2.m_lanes[(int) position3.m_lane].m_position - info2.m_lanes[(int) position2.m_lane].m_position) < 4.0)
            {
              num7 = PathManager.GetLaneID(position3);
              index1 = index2;
              position3 = position2;
              num4 = num6;
            }
            NetInfo.LaneType laneType = info2.m_lanes[(int) position3.m_lane].m_laneType;
            uint laneId = PathManager.GetLaneID(position3);
            float num8 = !info2.m_lanes[(int) position3.m_lane].m_useTerrainHeight ? 0.0f : 1f;
            bool flag = false;
            ushort startNode1 = instance2.m_segments.m_buffer[(int) position1.m_segment].m_startNode;
            ushort endNode1 = instance2.m_segments.m_buffer[(int) position1.m_segment].m_endNode;
            ushort startNode2 = instance2.m_segments.m_buffer[(int) position3.m_segment].m_startNode;
            ushort endNode2 = instance2.m_segments.m_buffer[(int) position3.m_segment].m_endNode;
            if ((int) startNode2 != (int) startNode1 && (int) startNode2 != (int) endNode1 && ((int) endNode2 != (int) startNode1 && (int) endNode2 != (int) endNode1))
            {
              uint lane1 = instance2.m_nodes.m_buffer[(int) startNode1].m_lane;
              uint lane2 = instance2.m_nodes.m_buffer[(int) startNode2].m_lane;
              uint lane3 = instance2.m_nodes.m_buffer[(int) endNode1].m_lane;
              uint lane4 = instance2.m_nodes.m_buffer[(int) endNode2].m_lane;
              if ((int) lane1 == (int) laneId || (int) lane2 == (int) num1 || ((int) lane3 == (int) laneId || (int) lane4 == (int) num1) || num7 != 0U && ((int) lane1 == (int) num7 || (int) lane3 == (int) num7))
              {
                if (((instance2.m_nodes.m_buffer[(int) startNode1].m_flags | instance2.m_nodes.m_buffer[(int) endNode1].m_flags) & NetNode.Flags.Disabled) != NetNode.Flags.None || ((instance2.m_nodes.m_buffer[(int) startNode2].m_flags | instance2.m_nodes.m_buffer[(int) endNode2].m_flags) & NetNode.Flags.Disabled) == NetNode.Flags.None)
                  flag = true;
                else
                  goto label_50;
              }
              else
                goto label_48;
            }
            if ((laneType & (NetInfo.LaneType.PublicTransport | NetInfo.LaneType.EvacuationTransport)) == NetInfo.LaneType.None)
            {
              if ((laneType & (NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle)) != NetInfo.LaneType.None)
              {
                if ((info2.m_lanes[(int) position3.m_lane].m_vehicleType & VehicleInfo.VehicleType.Bicycle) != VehicleInfo.VehicleType.None)
                {
                  if ((citizenData.m_flags & CitizenInstance.Flags.RidingBicycle) != CitizenInstance.Flags.None || this.SpawnBicycle(instanceID, ref citizenData, position3))
                    citizenData.m_flags |= CitizenInstance.Flags.OnBikeLane;
                  else
                    goto label_63;
                }
                else
                  goto label_65;
              }
              else if (laneType == NetInfo.LaneType.Pedestrian)
              {
                citizenData.m_flags &= ~CitizenInstance.Flags.OnBikeLane;
                if ((citizenData.m_flags & CitizenInstance.Flags.RidingBicycle) != CitizenInstance.Flags.None && (instance2.m_segments.m_buffer[(int) position3.m_segment].m_flags & NetSegment.Flags.BikeBan) != NetSegment.Flags.None)
                {
                  if (citizenData.m_citizen != 0U)
                    Singleton<CitizenManager>.instance.m_citizens.m_buffer[(IntPtr) citizenData.m_citizen].SetVehicle(citizenData.m_citizen, (ushort) 0, 0U);
                  citizenData.m_flags &= ~CitizenInstance.Flags.RidingBicycle;
                }
              }
              else
                goto label_71;
              byte offset;
              PathUnit.CalculatePathPositionOffset(laneId, (Vector3) vector4, out offset);
              if ((int) position3.m_segment != (int) position1.m_segment)
              {
                if ((instance2.m_segments.m_buffer[(int) position3.m_segment].m_flags & (NetSegment.Flags.Collapsed | NetSegment.Flags.Flooded)) == NetSegment.Flags.None)
                {
                  bezier = new Bezier3();
                  instance2.m_lanes.m_buffer[(IntPtr) num1].CalculatePositionAndDirection((float) position1.m_offset * 0.003921569f, out bezier.a, out direction1);
                  Vector3 direction2;
                  instance2.m_lanes.m_buffer[(IntPtr) laneId].CalculatePositionAndDirection((float) offset * 0.003921569f, out bezier.d, out direction2);
                  if (position1.m_offset == (byte) 0)
                    direction1 = -direction1;
                  if ((int) offset < (int) position3.m_offset)
                    direction2 = -direction2;
                  direction1.Normalize();
                  direction2.Normalize();
                  float distance;
                  NetSegment.CalculateMiddlePoints(bezier.a, direction1, bezier.d, direction2, true, true, out bezier.b, out bezier.c, out distance);
                  if ((double) distance >= 1.0)
                  {
                    if ((double) distance <= 64.0)
                    {
                      if (citizenData.m_lastPathOffset != (byte) 0 || this.CheckSegmentChange(instanceID, ref citizenData, position1, position3, (int) position1.m_offset, (int) offset, bezier))
                      {
                        float min = Mathf.Min(bezier.a.y, bezier.d.y);
                        float max = Mathf.Max(bezier.a.y, bezier.d.y);
                        bezier.b.y = Mathf.Clamp(bezier.b.y, min, max);
                        bezier.c.y = Mathf.Clamp(bezier.c.y, min, max);
                        float width2 = info2.m_lanes[(int) position3.m_lane].m_width;
                        while (citizenData.m_lastPathOffset < byte.MaxValue)
                        {
                          float num9 = Mathf.Sqrt(minSqrDistance) - VectorUtils.LengthXZ((Vector3) vector4 - frameData.m_position);
                          int num10 = (double) num9 >= 0.0 ? 8 + Mathf.CeilToInt((float) ((double) num9 * 256.0 / ((double) distance + 1.0))) : 8;
                          citizenData.m_lastPathOffset = (byte) Mathf.Min((int) citizenData.m_lastPathOffset + num10, (int) byte.MaxValue);
                          float t = (float) citizenData.m_lastPathOffset * 0.003921569f;
                          vector4 = (Vector4) bezier.Position(t);
                          vector4.w = num5 + (num8 - num5) * t;
                          float num11 = Mathf.Max(0.0f, Mathf.Lerp(width1, width2, t) - 1f) * num2;
                          Vector3 vector3_2 = Vector3.Cross(Vector3.up, bezier.Tangent(t)).normalized * num11;
                          vector4.x += vector3_2.x;
                          vector4.z += vector3_2.z;
                          if ((double) VectorUtils.LengthSqrXZ((Vector3) vector4 - frameData.m_position) >= (double) minSqrDistance)
                          {
                            CitizenInstance.Flags flags = citizenData.m_flags & ~(CitizenInstance.Flags.Underground | CitizenInstance.Flags.InsideBuilding | CitizenInstance.Flags.Transition) | info1.m_setCitizenFlags & info2.m_setCitizenFlags | (info1.m_setCitizenFlags | info2.m_setCitizenFlags) & CitizenInstance.Flags.Transition;
                            if ((flags & CitizenInstance.Flags.Underground) == CitizenInstance.Flags.None && ((info1.m_setCitizenFlags | info2.m_setCitizenFlags) & CitizenInstance.Flags.Underground) != CitizenInstance.Flags.None)
                              flags |= CitizenInstance.Flags.Transition;
                            citizenData.m_flags = flags;
                            return vector4;
                          }
                          if ((citizenData.m_flags & CitizenInstance.Flags.OnPath) == CitizenInstance.Flags.None)
                          {
                            citizenData.m_flags |= CitizenInstance.Flags.OnPath;
                            if ((citizenData.m_flags & CitizenInstance.Flags.RidingBicycle) == CitizenInstance.Flags.None && (instance2.m_segments.m_buffer[(int) position3.m_segment].m_flags & NetSegment.Flags.BikeBan) == NetSegment.Flags.None)
                              this.SpawnBicycle(instanceID, ref citizenData, position1);
                          }
                        }
                      }
                      else
                        goto label_87;
                    }
                    else
                      goto label_85;
                  }
                }
                else
                  goto label_78;
              }
              else if ((int) laneId != (int) num1)
              {
                int prevOffset = position1.m_offset < (byte) 128 ? 0 : (int) byte.MaxValue;
                int nextOffset = offset < (byte) 128 ? 0 : (int) byte.MaxValue;
                Vector3 direction2;
                instance2.m_lanes.m_buffer[(IntPtr) num1].CalculatePositionAndDirection((float) prevOffset * 0.003921569f, out segment3.a, out direction2);
                switch (prevOffset)
                {
                  case 0:
                    segment3.a -= direction2.normalized * 1.5f;
                    break;
                  case (int) byte.MaxValue:
                    segment3.a += direction2.normalized * 1.5f;
                    break;
                }
                Vector3 direction3;
                instance2.m_lanes.m_buffer[(IntPtr) laneId].CalculatePositionAndDirection((float) nextOffset * 0.003921569f, out segment3.b, out direction3);
                switch (nextOffset)
                {
                  case 0:
                    segment3.b -= direction3.normalized * 1.5f;
                    break;
                  case (int) byte.MaxValue:
                    segment3.b += direction3.normalized * 1.5f;
                    break;
                }
                vector3_1 = Vector3.Cross(Vector3.up, segment3.b - segment3.a).normalized * num2;
                if (citizenData.m_lastPathOffset != (byte) 0 || (int) num1 == (int) laneId || this.CheckLaneChange(instanceID, ref citizenData, position1, position3, prevOffset, nextOffset))
                {
                  float b = Mathf.Abs(info1.m_lanes[(int) position1.m_lane].m_position - info2.m_lanes[(int) position3.m_lane].m_position);
                  float num9 = (info1.m_halfWidth - info1.m_pavementWidth) / Mathf.Max(1f, b);
                  float num10 = info1.m_surfaceLevel - info1.m_lanes[(int) position1.m_lane].m_verticalOffset;
                  while (citizenData.m_lastPathOffset < byte.MaxValue)
                  {
                    float num11 = Mathf.Sqrt(minSqrDistance) - VectorUtils.LengthXZ((Vector3) vector4 - frameData.m_position);
                    int num12 = (double) num11 >= 0.0 ? 8 + Mathf.CeilToInt((float) ((double) num11 * 256.0 / ((double) b + 1.0))) : 8;
                    citizenData.m_lastPathOffset = (byte) Mathf.Min((int) citizenData.m_lastPathOffset + num12, (int) byte.MaxValue);
                    float t = (float) citizenData.m_lastPathOffset * 0.003921569f;
                    vector4 = (Vector4) (segment3.Position(t) + vector3_1);
                    vector4.w = num5 + (num8 - num5) * t;
                    if ((double) Mathf.Abs(t - 0.5f) < (double) num9)
                      vector4.y += num10;
                    if ((double) VectorUtils.LengthSqrXZ((Vector3) vector4 - frameData.m_position) >= (double) minSqrDistance)
                    {
                      CitizenInstance.Flags flags = citizenData.m_flags & ~(CitizenInstance.Flags.Underground | CitizenInstance.Flags.InsideBuilding | CitizenInstance.Flags.Transition) | info1.m_setCitizenFlags & info2.m_setCitizenFlags | (info1.m_setCitizenFlags | info2.m_setCitizenFlags) & CitizenInstance.Flags.Transition;
                      if ((flags & CitizenInstance.Flags.Underground) == CitizenInstance.Flags.None && ((info1.m_setCitizenFlags | info2.m_setCitizenFlags) & CitizenInstance.Flags.Underground) != CitizenInstance.Flags.None)
                        flags |= CitizenInstance.Flags.Transition;
                      citizenData.m_flags = flags;
                      return vector4;
                    }
                    if ((citizenData.m_flags & CitizenInstance.Flags.OnPath) == CitizenInstance.Flags.None)
                    {
                      citizenData.m_flags |= CitizenInstance.Flags.OnPath;
                      if ((instance2.m_segments.m_buffer[(int) position3.m_segment].m_flags & NetSegment.Flags.BikeBan) == NetSegment.Flags.None && (citizenData.m_flags & CitizenInstance.Flags.RidingBicycle) == CitizenInstance.Flags.None)
                        this.SpawnBicycle(instanceID, ref citizenData, position1);
                    }
                  }
                }
                else
                  goto label_105;
              }
              if ((instance2.m_segments.m_buffer[(int) position3.m_segment].m_flags & NetSegment.Flags.Untouchable) != NetSegment.Flags.None && ((instance2.m_segments.m_buffer[(int) position1.m_segment].m_flags & NetSegment.Flags.Untouchable) == NetSegment.Flags.None || flag))
              {
                ushort ownerBuilding = NetSegment.FindOwnerBuilding(position3.m_segment, 363f);
                if (ownerBuilding != (ushort) 0)
                {
                  ushort num9 = 0;
                  if ((instance2.m_segments.m_buffer[(int) position1.m_segment].m_flags & NetSegment.Flags.Untouchable) != NetSegment.Flags.None)
                    num9 = NetSegment.FindOwnerBuilding(position1.m_segment, 363f);
                  if ((int) ownerBuilding != (int) num9)
                  {
                    BuildingManager instance3 = Singleton<BuildingManager>.instance;
                    instance3.m_buildings.m_buffer[(int) ownerBuilding].Info.m_buildingAI.EnterBuildingSegment(ownerBuilding, ref instance3.m_buildings.m_buffer[(int) ownerBuilding], position3.m_segment, position3.m_offset, new InstanceID()
                    {
                      CitizenInstance = instanceID
                    });
                  }
                }
              }
              if ((int) num4 != (int) citizenData.m_path)
                Singleton<PathManager>.instance.ReleaseFirstUnit(ref citizenData.m_path);
              citizenData.m_pathPositionIndex = (byte) (index1 << 1);
              citizenData.m_lastPathOffset = offset;
              position1 = position3;
              num1 = laneId;
            }
            else
              goto label_53;
          }
          else
            goto label_41;
        }
        else
          goto label_39;
      }
      else
        break;
    }
    this.InvalidPath(instanceID, ref citizenData);
    return vector4;
label_37:
    Singleton<PathManager>.instance.ReleasePath(citizenData.m_path);
    citizenData.m_path = 0U;
    return vector4;
label_39:
    this.InvalidPath(instanceID, ref citizenData);
    return vector4;
label_41:
    this.InvalidPath(instanceID, ref citizenData);
    return vector4;
label_48:
    this.InvalidPath(instanceID, ref citizenData);
    return vector4;
label_50:
    this.InvalidPath(instanceID, ref citizenData);
    return vector4;
label_53:
    citizenData.m_flags |= CitizenInstance.Flags.WaitingTransport;
    citizenData.m_flags &= ~CitizenInstance.Flags.BoredOfWaiting;
    citizenData.m_waitCounter = (byte) 0;
    if ((int) num4 != (int) citizenData.m_path)
      Singleton<PathManager>.instance.ReleaseFirstUnit(ref citizenData.m_path);
    citizenData.m_pathPositionIndex = (byte) (index1 << 1);
    citizenData.m_lastPathOffset = position3.m_offset;
    citizenData.m_flags = citizenData.m_flags & ~(CitizenInstance.Flags.Underground | CitizenInstance.Flags.InsideBuilding | CitizenInstance.Flags.Transition | CitizenInstance.Flags.OnBikeLane) | info1.m_setCitizenFlags;
    if ((citizenData.m_flags & CitizenInstance.Flags.RidingBicycle) != CitizenInstance.Flags.None)
    {
      if (citizenData.m_citizen != 0U)
        Singleton<CitizenManager>.instance.m_citizens.m_buffer[(IntPtr) citizenData.m_citizen].SetVehicle(citizenData.m_citizen, (ushort) 0, 0U);
      citizenData.m_flags &= ~CitizenInstance.Flags.RidingBicycle;
    }
    return vector4;
label_63:
    this.InvalidPath(instanceID, ref citizenData);
    return vector4;
label_65:
    if ((int) num4 != (int) citizenData.m_path)
      Singleton<PathManager>.instance.ReleaseFirstUnit(ref citizenData.m_path);
    citizenData.m_pathPositionIndex = (byte) (index1 << 1);
    citizenData.m_lastPathOffset = position3.m_offset;
    if (!this.SpawnVehicle(instanceID, ref citizenData, position3))
      this.InvalidPath(instanceID, ref citizenData);
    return vector4;
label_71:
    this.InvalidPath(instanceID, ref citizenData);
    return vector4;
label_78:
    this.InvalidPath(instanceID, ref citizenData);
    return vector4;
label_85:
    this.InvalidPath(instanceID, ref citizenData);
    return vector4;
label_87:
    Vector3 vector3_3 = Vector3.Cross(Vector3.up, direction1).normalized * num3;
    return (Vector4) (bezier.a + vector3_3);
label_105:
    return (Vector4) (segment3.a + vector3_1);
  }

  protected virtual bool CheckSegmentChange(
    ushort instanceID,
    ref CitizenInstance citizenData,
    PathUnit.Position prevPos,
    PathUnit.Position nextPos,
    int prevOffset,
    int nextOffset,
    Bezier3 bezier)
  {
    return true;
  }

  protected virtual bool CheckLaneChange(
    ushort instanceID,
    ref CitizenInstance citizenData,
    PathUnit.Position prevPos,
    PathUnit.Position nextPos,
    int prevOffset,
    int nextOffset)
  {
    return true;
  }

  protected virtual bool SpawnVehicle(
    ushort instanceID,
    ref CitizenInstance citizenData,
    PathUnit.Position pathPos)
  {
    return false;
  }

  protected virtual bool SpawnBicycle(
    ushort instanceID,
    ref CitizenInstance citizenData,
    PathUnit.Position pathPos)
  {
    return false;
  }

  protected void CheckCollisions(
    ushort instanceID,
    ref CitizenInstance citizenData,
    Vector3 sourcePos,
    Vector3 targetPos,
    ushort buildingID,
    ref Vector3 pushAmount,
    ref float pushDivider)
  {
    Segment3 segment = new Segment3(sourcePos, targetPos);
    Vector3 min = segment.Min();
    min.x -= this.m_info.m_radius;
    min.z -= this.m_info.m_radius;
    Vector3 max = segment.Max();
    max.x += this.m_info.m_radius;
    max.y += this.m_info.m_height;
    max.z += this.m_info.m_radius;
    CitizenManager instance1 = Singleton<CitizenManager>.instance;
    int num1 = Mathf.Max((int) (((double) min.x - 3.0) / 8.0 + 1080.0), 0);
    int num2 = Mathf.Max((int) (((double) min.z - 3.0) / 8.0 + 1080.0), 0);
    int num3 = Mathf.Min((int) (((double) max.x + 3.0) / 8.0 + 1080.0), 2159);
    int num4 = Mathf.Min((int) (((double) max.z + 3.0) / 8.0 + 1080.0), 2159);
    for (int index1 = num2; index1 <= num4; ++index1)
    {
      for (int index2 = num1; index2 <= num3; ++index2)
      {
        ushort otherID = instance1.m_citizenGrid[index1 * 2160 + index2];
        int num5 = 0;
        while (otherID != (ushort) 0)
        {
          otherID = this.CheckCollisions(instanceID, ref citizenData, segment, min, max, otherID, ref instance1.m_instances.m_buffer[(int) otherID], ref pushAmount, ref pushDivider);
          if (++num5 > 65536)
          {
            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
            break;
          }
        }
      }
    }
    VehicleManager instance2 = Singleton<VehicleManager>.instance;
    int num6 = Mathf.Max((int) (((double) min.x - 10.0) / 32.0 + 270.0), 0);
    int num7 = Mathf.Max((int) (((double) min.z - 10.0) / 32.0 + 270.0), 0);
    int num8 = Mathf.Min((int) (((double) max.x + 10.0) / 32.0 + 270.0), 539);
    int num9 = Mathf.Min((int) (((double) max.z + 10.0) / 32.0 + 270.0), 539);
    for (int index1 = num7; index1 <= num9; ++index1)
    {
      for (int index2 = num6; index2 <= num8; ++index2)
      {
        ushort otherID = instance2.m_vehicleGrid[index1 * 540 + index2];
        int num5 = 0;
        while (otherID != (ushort) 0)
        {
          otherID = this.CheckCollisions(instanceID, ref citizenData, segment, min, max, otherID, ref instance2.m_vehicles.m_buffer[(int) otherID], ref pushAmount, ref pushDivider);
          if (++num5 > 16384)
          {
            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
            break;
          }
        }
      }
    }
    for (int index1 = num7; index1 <= num9; ++index1)
    {
      for (int index2 = num6; index2 <= num8; ++index2)
      {
        ushort otherID = instance2.m_parkedGrid[index1 * 540 + index2];
        int num5 = 0;
        while (otherID != (ushort) 0)
        {
          otherID = this.CheckCollisions(instanceID, ref citizenData, segment, min, max, otherID, ref instance2.m_parkedVehicles.m_buffer[(int) otherID], ref pushAmount, ref pushDivider);
          if (++num5 > 16384)
          {
            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
            break;
          }
        }
      }
    }
    if (buildingID == (ushort) 0)
      return;
    BuildingManager instance3 = Singleton<BuildingManager>.instance;
    BuildingInfo info = instance3.m_buildings.m_buffer[(int) buildingID].Info;
    if (info.m_props == null)
      return;
    Vector3 position = instance3.m_buildings.m_buffer[(int) buildingID].m_position;
    float angle = instance3.m_buildings.m_buffer[(int) buildingID].m_angle;
    int length = instance3.m_buildings.m_buffer[(int) buildingID].Length;
    Matrix4x4 matrix4x4 = new Matrix4x4();
    matrix4x4.SetTRS(Building.CalculateMeshPosition(info, position, angle, length), Quaternion.AngleAxis(angle * 57.29578f, Vector3.down), Vector3.one);
    for (int index = 0; index < info.m_props.Length; ++index)
    {
      BuildingInfo.Prop prop = info.m_props[index];
      Randomizer r = new Randomizer((int) buildingID << 6 | prop.m_index);
      if (r.Int32(100U) < prop.m_probability && length >= prop.m_requiredLength)
      {
        Vector3 vector3_1 = matrix4x4.MultiplyPoint(prop.m_position);
        if ((double) vector3_1.x >= (double) min.x - 2.0 && (double) vector3_1.x <= (double) max.x + 2.0 && ((double) vector3_1.z >= (double) min.z - 2.0 && (double) vector3_1.z <= (double) max.z + 2.0))
        {
          PropInfo finalProp = prop.m_finalProp;
          TreeInfo finalTree = prop.m_finalTree;
          float num5 = 0.0f;
          float num10 = 0.0f;
          if (finalProp != null)
          {
            PropInfo variation = finalProp.GetVariation(ref r);
            if (!variation.m_isMarker && !variation.m_isDecal && variation.m_hasRenderer)
            {
              num5 = variation.m_generatedInfo.m_size.x * 0.5f;
              num10 = variation.m_generatedInfo.m_size.y;
            }
            else
              continue;
          }
          else if (finalTree != null)
          {
            TreeInfo variation = finalTree.GetVariation(ref r);
            num5 = (float) (((double) variation.m_generatedInfo.m_size.x + (double) variation.m_generatedInfo.m_size.z) * 0.125);
            num10 = variation.m_generatedInfo.m_size.y;
          }
          if (!prop.m_fixedHeight)
            vector3_1.y = Singleton<TerrainManager>.instance.SampleDetailHeight(vector3_1);
          else if (info.m_requireHeightMap)
            vector3_1.y = Singleton<TerrainManager>.instance.SampleDetailHeight(vector3_1) + prop.m_position.y;
          if ((double) vector3_1.y + (double) num10 >= (double) min.y && (double) vector3_1.y <= (double) max.y)
          {
            float num11 = this.m_info.m_radius + num5;
            float u;
            float f = segment.DistanceSqr(vector3_1, out u);
            if ((double) f < (double) num11 * (double) num11)
            {
              float num12 = num11 - Mathf.Sqrt(f);
              float num13 = (float) (1.0 - (double) f / ((double) num11 * (double) num11));
              Vector3 vector3_2 = segment.Position(u * 0.9f);
              vector3_2.y = 0.0f;
              vector3_1.y = 0.0f;
              Vector3 lhs = Vector3.Normalize(vector3_2 - vector3_1);
              Vector3 rhs1 = Vector3.Normalize(new Vector3(segment.b.x - segment.a.x, 0.0f, segment.b.z - segment.a.z));
              Vector3 rhs2 = new Vector3(rhs1.z, 0.0f, -rhs1.x) * Mathf.Abs(Vector3.Dot(lhs, rhs1) * 0.5f);
              Vector3 vector3_3 = (double) Vector3.Dot(lhs, rhs2) < 0.0 ? lhs - rhs2 : lhs + rhs2;
              pushAmount += vector3_3 * (num12 * num13);
              pushDivider += num13;
            }
          }
        }
      }
    }
  }

  private ushort CheckCollisions(
    ushort instanceID,
    ref CitizenInstance citizenData,
    Segment3 segment,
    Vector3 min,
    Vector3 max,
    ushort otherID,
    ref CitizenInstance otherData,
    ref Vector3 pushAmount,
    ref float pushDivider)
  {
    if ((int) otherID == (int) instanceID || ((citizenData.m_flags | otherData.m_flags) & CitizenInstance.Flags.Transition) == CitizenInstance.Flags.None && (citizenData.m_flags & CitizenInstance.Flags.Underground) != (citizenData.m_flags & CitizenInstance.Flags.Underground))
      return otherData.m_nextGridInstance;
    CitizenInfo info = otherData.Info;
    CitizenInstance.Frame lastFrameData = otherData.GetLastFrameData();
    Segment3 segment1 = new Segment3(lastFrameData.m_position, lastFrameData.m_position + lastFrameData.m_velocity);
    Vector3 vector3_1 = segment1.Min();
    vector3_1.x -= info.m_radius;
    vector3_1.z -= info.m_radius;
    Vector3 vector3_2 = segment1.Max();
    vector3_2.x += info.m_radius;
    vector3_2.y += info.m_height;
    vector3_2.z += info.m_radius;
    if ((double) min.x < (double) vector3_2.x && (double) max.x > (double) vector3_1.x && ((double) min.z < (double) vector3_2.z && (double) max.z > (double) vector3_1.z) && ((double) min.y < (double) vector3_2.y && (double) max.y > (double) vector3_1.y))
    {
      float num1 = this.m_info.m_radius + info.m_radius;
      float u;
      float v;
      float f = segment.DistanceSqr(segment1, out u, out v);
      if ((double) f < (double) num1 * (double) num1)
      {
        float num2 = num1 - Mathf.Sqrt(f);
        float num3 = (float) (1.0 - (double) f / ((double) num1 * (double) num1));
        Vector3 vector3_3 = segment.Position(u * 0.9f);
        Vector3 vector3_4 = segment1.Position(v);
        vector3_3.y = 0.0f;
        vector3_4.y = 0.0f;
        Vector3 lhs = vector3_3 - vector3_4;
        Vector3 rhs = new Vector3(segment.b.z - segment.a.z, 0.0f, segment.a.x - segment.b.x);
        Vector3 vector3_5 = (double) Vector3.Dot(lhs, rhs) < 0.0 ? lhs - rhs : lhs + rhs;
        pushAmount += vector3_5.normalized * (num2 * num3);
        pushDivider += num3;
      }
    }
    return otherData.m_nextGridInstance;
  }

  private ushort CheckCollisions(
    ushort instanceID,
    ref CitizenInstance citizenData,
    Segment3 segment,
    Vector3 min,
    Vector3 max,
    ushort otherID,
    ref Vehicle otherData,
    ref Vector3 pushAmount,
    ref float pushDivider)
  {
    if (otherData.Info.m_vehicleType == VehicleInfo.VehicleType.Bicycle || (otherData.m_flags & Vehicle.Flags.Transition) == ~(Vehicle.Flags.Created | Vehicle.Flags.Deleted | Vehicle.Flags.Spawned | Vehicle.Flags.Inverted | Vehicle.Flags.TransferToTarget | Vehicle.Flags.TransferToSource | Vehicle.Flags.Emergency1 | Vehicle.Flags.Emergency2 | Vehicle.Flags.WaitingPath | Vehicle.Flags.Stopped | Vehicle.Flags.Leaving | Vehicle.Flags.Arriving | Vehicle.Flags.Reversed | Vehicle.Flags.TakingOff | Vehicle.Flags.Flying | Vehicle.Flags.Landing | Vehicle.Flags.WaitingSpace | Vehicle.Flags.WaitingCargo | Vehicle.Flags.GoingBack | Vehicle.Flags.WaitingTarget | Vehicle.Flags.Importing | Vehicle.Flags.Exporting | Vehicle.Flags.Parking | Vehicle.Flags.CustomName | Vehicle.Flags.OnGravel | Vehicle.Flags.WaitingLoading | Vehicle.Flags.Congestion | Vehicle.Flags.DummyTraffic | Vehicle.Flags.Underground | Vehicle.Flags.Transition | Vehicle.Flags.InsideBuilding | Vehicle.Flags.LeftHandDrive) && (citizenData.m_flags & CitizenInstance.Flags.Transition) == CitizenInstance.Flags.None && (otherData.m_flags & Vehicle.Flags.Underground) != ~(Vehicle.Flags.Created | Vehicle.Flags.Deleted | Vehicle.Flags.Spawned | Vehicle.Flags.Inverted | Vehicle.Flags.TransferToTarget | Vehicle.Flags.TransferToSource | Vehicle.Flags.Emergency1 | Vehicle.Flags.Emergency2 | Vehicle.Flags.WaitingPath | Vehicle.Flags.Stopped | Vehicle.Flags.Leaving | Vehicle.Flags.Arriving | Vehicle.Flags.Reversed | Vehicle.Flags.TakingOff | Vehicle.Flags.Flying | Vehicle.Flags.Landing | Vehicle.Flags.WaitingSpace | Vehicle.Flags.WaitingCargo | Vehicle.Flags.GoingBack | Vehicle.Flags.WaitingTarget | Vehicle.Flags.Importing | Vehicle.Flags.Exporting | Vehicle.Flags.Parking | Vehicle.Flags.CustomName | Vehicle.Flags.OnGravel | Vehicle.Flags.WaitingLoading | Vehicle.Flags.Congestion | Vehicle.Flags.DummyTraffic | Vehicle.Flags.Underground | Vehicle.Flags.Transition | Vehicle.Flags.InsideBuilding | Vehicle.Flags.LeftHandDrive) != ((citizenData.m_flags & CitizenInstance.Flags.Underground) != CitizenInstance.Flags.None))
      return otherData.m_nextGridVehicle;
    Segment3 segment1 = otherData.m_segment;
    Vector3 vector3_1 = Vector3.Min(segment1.Min(), (Vector3) otherData.m_targetPos1);
    --vector3_1.x;
    --vector3_1.z;
    Vector3 vector3_2 = Vector3.Max(segment1.Max(), (Vector3) otherData.m_targetPos1);
    ++vector3_2.x;
    ++vector3_2.y;
    ++vector3_2.z;
    if ((double) min.x < (double) vector3_2.x && (double) max.x > (double) vector3_1.x && ((double) min.z < (double) vector3_2.z && (double) max.z > (double) vector3_1.z) && ((double) min.y < (double) vector3_2.y && (double) max.y > (double) vector3_1.y))
    {
      float num1 = this.m_info.m_radius + 1f;
      float u;
      float v;
      float f1 = segment.DistanceSqr(segment1, out u, out v);
      if ((double) f1 < (double) num1 * (double) num1)
      {
        float num2 = num1 - Mathf.Sqrt(f1);
        float num3 = (float) (1.0 - (double) f1 / ((double) num1 * (double) num1));
        Vector3 vector3_3 = segment.Position(u * 0.9f);
        Vector3 vector3_4 = segment1.Position(v);
        vector3_3.y = 0.0f;
        vector3_4.y = 0.0f;
        Vector3 lhs = Vector3.Normalize(vector3_3 - vector3_4);
        Vector3 rhs1 = Vector3.Normalize(new Vector3(segment.b.x - segment.a.x, 0.0f, segment.b.z - segment.a.z));
        Vector3 rhs2 = new Vector3(rhs1.z, 0.0f, -rhs1.x) * Mathf.Abs(Vector3.Dot(lhs, rhs1) * 0.5f);
        Vector3 vector3_5 = (double) Vector3.Dot(lhs, rhs2) < 0.0 ? lhs - rhs2 : lhs + rhs2;
        pushAmount += vector3_5 * (num2 * num3);
        pushDivider += num3;
      }
      float magnitude = otherData.GetLastFrameVelocity().magnitude;
      if ((double) magnitude > 0.100000001490116)
      {
        float num2 = this.m_info.m_radius + 3f;
        segment1.a = segment1.b;
        segment1.b += Vector3.ClampMagnitude((Vector3) otherData.m_targetPos1 - segment1.b, magnitude * 4f);
        float f2 = segment.DistanceSqr(segment1, out u, out v);
        if ((double) f2 > (double) num1 * (double) num1 && (double) f2 < (double) num2 * (double) num2)
        {
          float num3 = num2 - Mathf.Sqrt(f2);
          float num4 = (float) (1.0 - (double) f2 / ((double) num2 * (double) num2));
          Vector3 vector3_3 = segment.Position(u * 0.9f);
          Vector3 vector3_4 = segment1.Position(v);
          vector3_3.y = 0.0f;
          vector3_4.y = 0.0f;
          Vector3 vector3_5 = vector3_3 - vector3_4;
          pushAmount += vector3_5.normalized * (num3 * num4);
          pushDivider += num4;
        }
      }
    }
    return otherData.m_nextGridVehicle;
  }

  private ushort CheckCollisions(
    ushort instanceID,
    ref CitizenInstance citizenData,
    Segment3 segment,
    Vector3 min,
    Vector3 max,
    ushort otherID,
    ref VehicleParked otherData,
    ref Vector3 pushAmount,
    ref float pushDivider)
  {
    VehicleInfo info = otherData.Info;
    Vector3 position = otherData.m_position;
    Vector3 vector3_1 = otherData.m_rotation * new Vector3(0.0f, 0.0f, Mathf.Max(0.5f, (float) ((double) info.m_generatedInfo.m_size.z * 0.5 - 1.0)));
    Segment3 segment1;
    segment1.a = position - vector3_1;
    segment1.b = position + vector3_1;
    Vector3 vector3_2 = segment1.Min();
    --vector3_2.x;
    --vector3_2.z;
    Vector3 vector3_3 = segment1.Max();
    ++vector3_3.x;
    ++vector3_3.y;
    ++vector3_3.z;
    if ((double) min.x < (double) vector3_3.x && (double) max.x > (double) vector3_2.x && ((double) min.z < (double) vector3_3.z && (double) max.z > (double) vector3_2.z) && ((double) min.y < (double) vector3_3.y && (double) max.y > (double) vector3_2.y))
    {
      float num1 = this.m_info.m_radius + 1f;
      float u;
      float v;
      float f = segment.DistanceSqr(segment1, out u, out v);
      if ((double) f < (double) num1 * (double) num1)
      {
        float num2 = num1 - Mathf.Sqrt(f);
        float num3 = (float) (1.0 - (double) f / ((double) num1 * (double) num1));
        Vector3 vector3_4 = segment.Position(u * 0.9f);
        Vector3 vector3_5 = segment1.Position(v);
        vector3_4.y = 0.0f;
        vector3_5.y = 0.0f;
        Vector3 lhs = Vector3.Normalize(vector3_4 - vector3_5);
        Vector3 rhs1 = Vector3.Normalize(new Vector3(segment.b.x - segment.a.x, 0.0f, segment.b.z - segment.a.z));
        Vector3 rhs2 = new Vector3(rhs1.z, 0.0f, -rhs1.x) * Mathf.Abs(Vector3.Dot(lhs, rhs1) * 0.5f);
        Vector3 vector3_6 = (double) Vector3.Dot(lhs, rhs2) < 0.0 ? lhs - rhs2 : lhs + rhs2;
        pushAmount += vector3_6 * (num2 * num3);
        pushDivider += num3;
      }
    }
    return otherData.m_nextGridParked;
  }

  protected void InvalidPath(ushort instanceID, ref CitizenInstance citizenData)
  {
    if (citizenData.m_path != 0U)
    {
      Singleton<PathManager>.instance.ReleasePath(citizenData.m_path);
      citizenData.m_path = 0U;
    }
    citizenData.m_flags &= ~(CitizenInstance.Flags.WaitingTransport | CitizenInstance.Flags.EnteringVehicle | CitizenInstance.Flags.BoredOfWaiting | CitizenInstance.Flags.WaitingTaxi);
    if (this.StartPathFind(instanceID, ref citizenData))
      return;
    citizenData.Unspawn(instanceID);
  }

  public virtual InstanceID GetTargetID(
    ushort instanceID,
    ref CitizenInstance citizenData)
  {
    InstanceID instanceId = new InstanceID();
    if ((citizenData.m_flags & CitizenInstance.Flags.TargetIsNode) != CitizenInstance.Flags.None)
      instanceId.NetNode = citizenData.m_targetBuilding;
    else
      instanceId.Building = citizenData.m_targetBuilding;
    return instanceId;
  }

  protected virtual bool StartPathFind(ushort instanceID, ref CitizenInstance citizenData)
  {
    return false;
  }

  protected bool StartPathFind(
    ushort instanceID,
    ref CitizenInstance citizenData,
    Vector3 startPos,
    Vector3 endPos,
    VehicleInfo vehicleInfo,
    bool enableTransport,
    bool ignoreCost)
  {
    NetInfo.LaneType laneTypes = NetInfo.LaneType.Pedestrian;
    VehicleInfo.VehicleType vehicleTypes = VehicleInfo.VehicleType.None;
    bool randomParking = false;
    bool combustionEngine = false;
        // cannot imagine why ever call with null vehicle, just a sanity check?
    if (vehicleInfo != null)
    {
      if (vehicleInfo.m_class.m_subService == ItemClass.SubService.PublicTransportTaxi)
      {
        if ((citizenData.m_flags & CitizenInstance.Flags.CannotUseTaxi) == CitizenInstance.Flags.None && Singleton<DistrictManager>.instance.m_districts.m_buffer[0].m_productionData.m_finalTaxiCapacity != 0U)
        {
          SimulationManager instance = Singleton<SimulationManager>.instance;
          if (instance.m_isNightTime || instance.m_randomizer.Int32(2U) == 0)
          {
            laneTypes |= NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle;
            vehicleTypes |= vehicleInfo.m_vehicleType;
          }
        }
      }
      else
      {
        laneTypes |= NetInfo.LaneType.Vehicle;
        vehicleTypes |= vehicleInfo.m_vehicleType;
        if (citizenData.m_targetBuilding != (ushort) 0)
        {
          if ((citizenData.m_flags & CitizenInstance.Flags.TargetIsNode) != CitizenInstance.Flags.None)
            randomParking = true;
          else if (Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int) citizenData.m_targetBuilding].Info.m_class.m_service > ItemClass.Service.Office)
            randomParking = true;
        }
        if (vehicleInfo.m_vehicleType == VehicleInfo.VehicleType.Car)
          combustionEngine = vehicleInfo.m_class.m_subService == ItemClass.SubService.ResidentialLow;
      }
    }
    CitizenManager instance1 = Singleton<CitizenManager>.instance;
    PathUnit.Position pathPos = new PathUnit.Position();                    // pathPos will be set to hold the out value of pathPos: calculated by FindPathPosition method
                                                                            // Note pathPos is a PathUnit.Position NOT a vector3 type position. pathPos holds an m_segment, m_lanes and m_offset (as indices for the PathManager lists)
    ushort parkedVehicle = instance1.m_citizens.m_buffer[(IntPtr) citizenData.m_citizen].m_parkedVehicle;
    if (parkedVehicle != (ushort) 0)
      PathManager.FindPathPosition(position: Singleton<VehicleManager>.instance.m_parkedVehicles.m_buffer[parkedVehicle].m_position, service: ItemClass.Service.Road, laneType: NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle, vehicleType: VehicleInfo.VehicleType.Car, allowUnderground: false, requireConnect: false, maxDistance: 32f, pathPos: out pathPos);

    bool allowUnderground = (citizenData.m_flags & (CitizenInstance.Flags.Underground | CitizenInstance.Flags.Transition)) != CitizenInstance.Flags.None;
    bool stablePath;
    float maxLength;
        // see practical difference with OnTour here.  If OnTour, much longer max length journeys and a stablePath (Not sure yet exactly what that means)
        // laneTypes already inclued Peddestrian and Vehicle (and maybe TransportVehicle) from above
        // currently I do not understand why PublicTransport is flagged for OnTour but not for eg home-work (but ahah see just below where it gets enabled)
    if ((citizenData.m_flags & CitizenInstance.Flags.OnTour) != CitizenInstance.Flags.None)
    {
      stablePath = true;
      maxLength = 160000f;
      laneTypes &= NetInfo.LaneType.Pedestrian | NetInfo.LaneType.Parking | NetInfo.LaneType.PublicTransport | NetInfo.LaneType.CargoVehicle | NetInfo.LaneType.EvacuationTransport | NetInfo.LaneType.Tour;
    }
    else
    {
      stablePath = false;
      maxLength = 20000f;
    }

    PathUnit.Position startPosA;
    PathUnit.Position endPosA;
    if (this.FindPathPosition(instanceID, ref citizenData, startPos, laneTypes, vehicleTypes, allowUnderground, out startPosA) && this.FindPathPosition(instanceID, ref citizenData, endPos, laneTypes, vehicleTypes, false, out endPosA))
    {
      if (enableTransport && (citizenData.m_flags & CitizenInstance.Flags.CannotUseTransport) == CitizenInstance.Flags.None)        // add flags to consider public transport here
      {
        laneTypes |= NetInfo.LaneType.PublicTransport;
        uint citizen = citizenData.m_citizen;
        if (citizen != 0U && (instance1.m_citizens.m_buffer[(IntPtr) citizen].m_flags & Citizen.Flags.Evacuating) != Citizen.Flags.None)
          laneTypes |= NetInfo.LaneType.EvacuationTransport;
      }
      PathUnit.Position nullPos = new PathUnit.Position();
      uint pathUnitID;
      if (Singleton<PathManager>.instance.CreatePath(unit: out pathUnitID, randomizer: ref Singleton<SimulationManager>.instance.m_randomizer, buildIndex: Singleton<SimulationManager>.instance.m_currentBuildIndex, 
          startPosA: startPosA, startPosB: nullPos, endPosA: endPosA, endPosB: nullPos, vehiclePosition: pathPos, laneTypes: laneTypes, vehicleTypes: vehicleTypes, 
          maxLength: maxLength, isHeavyVehicle: false, ignoreBlocked: false, stablePath: stablePath, skipQueue: false, randomParking: randomParking, ignoreFlooded: false, combustionEngine: combustionEngine, ignoreCost: ignoreCost))
      {
        if (citizenData.m_path != 0U)
          Singleton<PathManager>.instance.ReleasePath(citizenData.m_path);
        citizenData.m_path = pathUnitID;
        citizenData.m_flags |= CitizenInstance.Flags.WaitingPath;       // we have not actually *created* a path yet, we have just flagged NetManager (or Sim Manager) to create paths for this citizen when it is their turn
        return true;
      }
    }
    return false;
  }

  public virtual bool FindPathPosition(
    ushort instanceID,
    ref CitizenInstance citizenData,
    Vector3 pos,
    NetInfo.LaneType laneTypes,
    VehicleInfo.VehicleType vehicleTypes,
    bool allowUnderground,
    out PathUnit.Position position)
  {
    position = new PathUnit.Position();
    float num = 1E+10f;
    PathUnit.Position pathPosA1;
    PathUnit.Position pathPosB1;
    float distanceSqrA1;
    float distanceSqrB1;
    if (PathManager.FindPathPosition(pos, ItemClass.Service.Road, laneTypes, vehicleTypes, allowUnderground, false, 32f, out pathPosA1, out pathPosB1, out distanceSqrA1, out distanceSqrB1) && (double) distanceSqrA1 < (double) num)
    {
      num = distanceSqrA1;
      position = pathPosA1;
    }
    PathUnit.Position pathPosA2;
    PathUnit.Position pathPosB2;
    float distanceSqrA2;
    float distanceSqrB2;
    if (PathManager.FindPathPosition(pos, ItemClass.Service.Beautification, laneTypes, vehicleTypes, allowUnderground, false, 32f, out pathPosA2, out pathPosB2, out distanceSqrA2, out distanceSqrB2) && (double) distanceSqrA2 < (double) num)
    {
      num = distanceSqrA2;
      position = pathPosA2;
    }
    PathUnit.Position pathPosA3;
    PathUnit.Position pathPosB3;
    float distanceSqrA3;
    float distanceSqrB3;
    if ((citizenData.m_flags & CitizenInstance.Flags.CannotUseTransport) == CitizenInstance.Flags.None && PathManager.FindPathPosition(pos, ItemClass.Service.PublicTransport, laneTypes, vehicleTypes, allowUnderground, false, 32f, out pathPosA3, out pathPosB3, out distanceSqrA3, out distanceSqrB3) && (double) distanceSqrA3 < (double) num)
      position = pathPosA3;
    return position.m_segment != (ushort) 0;
  }

  public virtual bool IsAnimal()
  {
    return false;
  }

  public virtual bool IsSwimming()
  {
    return false;
  }

  public virtual string GenerateName(ushort instanceID, bool useCitizen)
  {
    CitizenManager instance = Singleton<CitizenManager>.instance;
    uint citizen = instance.m_instances.m_buffer[(int) instanceID].m_citizen;
    if (citizen != 0U && useCitizen)
      return CitizenAI.GenerateCitizenName(citizen, instance.m_citizens.m_buffer[(IntPtr) citizen].m_family);
    return (string) null;
  }

  public static string GenerateCitizenName(uint citizenID, byte family)
  {
    Randomizer randomizer1 = new Randomizer(citizenID);
    Randomizer randomizer2 = new Randomizer((int) family);
    string id1 = "NAME_FEMALE_FIRST";
    string id2 = "NAME_FEMALE_LAST";
    if (Citizen.GetGender(citizenID) == Citizen.Gender.Male)
    {
      id1 = "NAME_MALE_FIRST";
      id2 = "NAME_MALE_LAST";
    }
    string str = ColossalFramework.Globalization.Locale.Get(id1, randomizer1.Int32(ColossalFramework.Globalization.Locale.Count(id1)));
    return StringUtils.SafeFormat(ColossalFramework.Globalization.Locale.Get(id2, randomizer2.Int32(ColossalFramework.Globalization.Locale.Count(id2))), (object) str);
  }
}
