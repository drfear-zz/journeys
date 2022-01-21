// Decompiled with JetBrains decompiler
// Type: ResidentAI
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 9724B8F4-19DD-48C3-AE02-CDA150D75CEC
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Cities_Skylines\Cities_Data\Managed\Assembly-CSharp.dll

using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.PlatformServices;
using ColossalFramework.Threading;
using ICities;
using System;
using UnityEngine;

public class ResidentAI : HumanAI
{
  public const int UNIVERSITY_DURATION = 15;
  public const int BREED_INTERVAL = 12;
  public const int GAY_PROBABILITY = 5;
  public const int CAR_PROBABILITY_CHILD = 0;
  public const int CAR_PROBABILITY_TEEN = 5;
  public const int CAR_PROBABILITY_YOUNG = 15;
  public const int CAR_PROBABILITY_ADULT = 20;
  public const int CAR_PROBABILITY_SENIOR = 10;
  public const int BIKE_PROBABILITY_CHILD = 40;
  public const int BIKE_PROBABILITY_TEEN = 30;
  public const int BIKE_PROBABILITY_YOUNG = 20;
  public const int BIKE_PROBABILITY_ADULT = 10;
  public const int BIKE_PROBABILITY_SENIOR = 0;
  public const int TAXI_PROBABILITY_CHILD = 0;
  public const int TAXI_PROBABILITY_TEEN = 2;
  public const int TAXI_PROBABILITY_YOUNG = 2;
  public const int TAXI_PROBABILITY_ADULT = 4;
  public const int TAXI_PROBABILITY_SENIOR = 6;

  public override Color GetColor(
    ushort instanceID,
    ref CitizenInstance data,
    InfoManager.InfoMode infoMode)
  {
    if (infoMode != InfoManager.InfoMode.Health)
    {
      if (infoMode != InfoManager.InfoMode.Happiness)
        return base.GetColor(instanceID, ref data, infoMode);
      int happiness = Citizen.GetHappiness((int) Singleton<CitizenManager>.instance.m_citizens.m_buffer[(IntPtr) data.m_citizen].m_health, (int) Singleton<CitizenManager>.instance.m_citizens.m_buffer[(IntPtr) data.m_citizen].m_wellbeing);
      return Color.Lerp(Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int) infoMode].m_negativeColor, Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int) infoMode].m_targetColor, (float) Citizen.GetHappinessLevel(happiness) * 0.25f);
    }
    int health = (int) Singleton<CitizenManager>.instance.m_citizens.m_buffer[(IntPtr) data.m_citizen].m_health;
    return Color.Lerp(Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int) infoMode].m_negativeColor, Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int) infoMode].m_targetColor, (float) Citizen.GetHealthLevel(health) * 0.2f);
  }

  public override void SetRenderParameters(
    RenderManager.CameraInfo cameraInfo,
    ushort instanceID,
    ref CitizenInstance data,
    Vector3 position,
    Quaternion rotation,
    Vector3 velocity,
    Color color,
    bool underground)
  {
    if ((data.m_flags & CitizenInstance.Flags.AtTarget) != CitizenInstance.Flags.None)
    {
      if ((data.m_flags & CitizenInstance.Flags.SittingDown) != CitizenInstance.Flags.None)
      {
        this.m_info.SetRenderParameters(position, rotation, velocity, color, 2, underground);
        return;
      }
      if ((data.m_flags & (CitizenInstance.Flags.Panicking | CitizenInstance.Flags.Blown | CitizenInstance.Flags.Floating)) == CitizenInstance.Flags.Panicking)
      {
        this.m_info.SetRenderParameters(position, rotation, velocity, color, 1, underground);
        return;
      }
      if ((data.m_flags & (CitizenInstance.Flags.Blown | CitizenInstance.Flags.Floating | CitizenInstance.Flags.Cheering)) == CitizenInstance.Flags.Cheering)
      {
        this.m_info.SetRenderParameters(position, rotation, velocity, color, 5, underground);
        return;
      }
    }
    if ((data.m_flags & CitizenInstance.Flags.RidingBicycle) != CitizenInstance.Flags.None)
      this.m_info.SetRenderParameters(position, rotation, velocity, color, 3, underground);
    else if ((data.m_flags & (CitizenInstance.Flags.Blown | CitizenInstance.Flags.Floating)) != CitizenInstance.Flags.None)
      this.m_info.SetRenderParameters(position, rotation, Vector3.zero, color, 1, underground);
    else
      this.m_info.SetRenderParameters(position, rotation, velocity, color, (int) instanceID & 4, underground);
  }


    // the useful thing about following this through is that there are text string explanations of what the flags mean...
    // note there is a second version of this (below) with argument uint citizen which is for when the citizen is not clickable (eg at home)

    // the out arg target has nothing to do with setting citizen target - this routine is only called by UpdateBindings for HumanWorldInfoPanel
    // however, the way in which it is set is quite informative

  public override string GetLocalizedStatus(
    ushort instanceID,
    ref CitizenInstance data,
    out InstanceID target)
  {
    if ((data.m_flags & (CitizenInstance.Flags.Blown | CitizenInstance.Flags.Floating)) != CitizenInstance.Flags.None)
    {
      target = InstanceID.Empty;
      return ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_CONFUSED");
    }
    CitizenManager theCitizenManager = Singleton<CitizenManager>.instance;
    uint citizenID = data.m_citizen;
    bool isStudent = false;
    ushort homeID = 0;
    ushort workID = 0;
    ushort vehicleID = 0;
    if (citizenID != 0U)
    {
      homeID = theCitizenManager.m_citizens.m_buffer[citizenID].m_homeBuilding;
      workID = theCitizenManager.m_citizens.m_buffer[citizenID].m_workBuilding;
      vehicleID = theCitizenManager.m_citizens.m_buffer[citizenID].m_vehicle;
      isStudent = (theCitizenManager.m_citizens.m_buffer[citizenID].m_flags & Citizen.Flags.Student) != Citizen.Flags.None;
    }
    ushort targetBuilding = data.m_targetBuilding;  // this shows that a CitizenInstance.m_targetBuilding IS the target as reported by the GUI info panel
    if (targetBuilding != (ushort) 0)   // target building is (nearly) ALWAYS nonzero, else "confused"
    {
      if ((data.m_flags & CitizenInstance.Flags.TargetIsNode) != CitizenInstance.Flags.None)
      {
        if (vehicleID != (ushort) 0)
        {
          VehicleManager theVehicleManager = Singleton<VehicleManager>.instance;
          VehicleInfo info = theVehicleManager.m_vehicles.m_buffer[(int) vehicleID].Info;
          if (info.m_class.m_service == ItemClass.Service.Residential && info.m_vehicleType != VehicleInfo.VehicleType.Bicycle)
          {
            if ((int) info.m_vehicleAI.GetOwnerID(vehicleID, ref theVehicleManager.m_vehicles.m_buffer[(int) vehicleID]).Citizen == (int) citizenID)
            {
              target = InstanceID.Empty;
              target.NetNode = targetBuilding;
              return ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_DRIVINGTO");
            }
          }
          else if (info.m_class.m_service == ItemClass.Service.PublicTransport || info.m_class.m_service == ItemClass.Service.Disaster)
          {
            if ((data.m_flags & CitizenInstance.Flags.WaitingTaxi) != CitizenInstance.Flags.None)
            {
              target = InstanceID.Empty;
              return ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_WAITING_TAXI");
            }
            ushort transportLine = Singleton<NetManager>.instance.m_nodes.m_buffer[targetBuilding].m_transportLine;
            if ((int) theVehicleManager.m_vehicles.m_buffer[(int) vehicleID].m_transportLine != (int) transportLine)
            {
              target = InstanceID.Empty;
              target.NetNode = targetBuilding;
              return ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_TRAVELLINGTO");
            }
          }
        }
        if ((data.m_flags & CitizenInstance.Flags.OnTour) != CitizenInstance.Flags.None)
        {
          target = InstanceID.Empty;
          target.NetNode = targetBuilding;
          return ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_VISITING");
        }
        target = InstanceID.Empty;
        target.NetNode = targetBuilding;
        return ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_GOINGTO");
      }

      // you can only reach here if the target is a building [not a node] (all TargetIsNode paths are returned in the above enclosing if)
      bool isInOutBuilding = (Singleton<BuildingManager>.instance.m_buildings.m_buffer[targetBuilding].m_flags & Building.Flags.IncomingOutgoing) != Building.Flags.None;
      bool isHangAround = data.m_path == 0U && (data.m_flags & CitizenInstance.Flags.HangAround) != CitizenInstance.Flags.None;
      if (vehicleID != (ushort) 0)
      {
        VehicleManager instance2 = Singleton<VehicleManager>.instance;
        VehicleInfo info = instance2.m_vehicles.m_buffer[(int) vehicleID].Info;
        if (info.m_class.m_service == ItemClass.Service.Residential && info.m_vehicleType != VehicleInfo.VehicleType.Bicycle)
        {
          if ((int) info.m_vehicleAI.GetOwnerID(vehicleID, ref instance2.m_vehicles.m_buffer[(int) vehicleID]).Citizen == (int) citizenID)
          {
            if (isInOutBuilding)
            {
              target = InstanceID.Empty;
              return ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_DRIVINGTO_OUTSIDE");
            }
            if ((int) targetBuilding == (int) homeID)
            {
              target = InstanceID.Empty;
              return ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_DRIVINGTO_HOME");
            }
            if ((int) targetBuilding == (int) workID)
            {
              target = InstanceID.Empty;
              return ColossalFramework.Globalization.Locale.Get(!isStudent ? "CITIZEN_STATUS_DRIVINGTO_WORK" : "CITIZEN_STATUS_DRIVINGTO_SCHOOL");
            }
            target = InstanceID.Empty;
            target.Building = targetBuilding;
            return ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_DRIVINGTO");
          }
        }
        else if (info.m_class.m_service == ItemClass.Service.PublicTransport || info.m_class.m_service == ItemClass.Service.Disaster)
        {
          if ((data.m_flags & CitizenInstance.Flags.WaitingTaxi) != CitizenInstance.Flags.None)
          {
            target = InstanceID.Empty;
            return ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_WAITING_TAXI");
          }
          if (isInOutBuilding)
          {
            target = InstanceID.Empty;
            return ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_TRAVELLINGTO_OUTSIDE");
          }
          if ((int) targetBuilding == (int) homeID)
          {
            target = InstanceID.Empty;
            return ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_TRAVELLINGTO_HOME");
          }
          if ((int) targetBuilding == (int) workID)
          {
            target = InstanceID.Empty;
            return ColossalFramework.Globalization.Locale.Get(!isStudent ? "CITIZEN_STATUS_TRAVELLINGTO_WORK" : "CITIZEN_STATUS_TRAVELLINGTO_SCHOOL");
          }
          target = InstanceID.Empty;
          target.Building = targetBuilding;
          return ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_TRAVELLINGTO");
        }
      }
      if (isInOutBuilding)
      {
        target = InstanceID.Empty;
        return ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_GOINGTO_OUTSIDE");
      }
      if ((int) targetBuilding == (int) homeID)
      {
        if (isHangAround)
        {
          target = InstanceID.Empty;
          return ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_AT_HOME");
        }
        target = InstanceID.Empty;
        return ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_GOINGTO_HOME");
      }
      if ((int) targetBuilding == (int) workID)
      {
        if (isHangAround)
        {
          target = InstanceID.Empty;
          return ColossalFramework.Globalization.Locale.Get(!isStudent ? "CITIZEN_STATUS_AT_WORK" : "CITIZEN_STATUS_AT_SCHOOL");
        }
        target = InstanceID.Empty;
        return ColossalFramework.Globalization.Locale.Get(!isStudent ? "CITIZEN_STATUS_GOINGTO_WORK" : "CITIZEN_STATUS_GOINGTO_SCHOOL");
      }
      if (isHangAround)
      {
        target = InstanceID.Empty;
        target.Building = targetBuilding;
        return ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_VISITING");
      }
      target = InstanceID.Empty;
      target.Building = targetBuilding;
      return ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_GOINGTO");
    }
    target = InstanceID.Empty;
    return ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_CONFUSED");
  }

  public override string GetLocalizedStatus(
    uint citizenID,
    ref Citizen data,
    out InstanceID target)
  {
    CitizenManager instance1 = Singleton<CitizenManager>.instance;
    ushort instance2 = data.m_instance;
    if (instance2 != (ushort) 0)
      return this.GetLocalizedStatus(instance2, ref instance1.m_instances.m_buffer[(int) instance2], out target);
    Citizen.Location currentLocation = data.CurrentLocation;
    ushort homeBuilding = data.m_homeBuilding;
    ushort workBuilding = data.m_workBuilding;
    ushort visitBuilding = data.m_visitBuilding;
    bool flag = (data.m_flags & Citizen.Flags.Student) != Citizen.Flags.None;
    switch (currentLocation)
    {
      case Citizen.Location.Home:
        if (homeBuilding != (ushort) 0)
        {
          target = InstanceID.Empty;
          return ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_AT_HOME");
        }
        break;
      case Citizen.Location.Work:
        if (workBuilding != (ushort) 0)
        {
          target = InstanceID.Empty;
          return ColossalFramework.Globalization.Locale.Get(!flag ? "CITIZEN_STATUS_AT_WORK" : "CITIZEN_STATUS_AT_SCHOOL");
        }
        break;
      case Citizen.Location.Visit:
        if (visitBuilding != (ushort) 0)
        {
          target = InstanceID.Empty;
          target.Building = visitBuilding;
          return ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_VISITING");
        }
        break;
    }
    target = InstanceID.Empty;
    return ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_CONFUSED");
  }

  public override void LoadInstance(ushort instanceID, ref CitizenInstance data)
  {
    base.LoadInstance(instanceID, ref data);
    if (data.m_sourceBuilding != (ushort) 0)
      Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int) data.m_sourceBuilding].AddSourceCitizen(instanceID, ref data);
    if (data.m_targetBuilding == (ushort) 0)
      return;
    if ((data.m_flags & CitizenInstance.Flags.TargetIsNode) != CitizenInstance.Flags.None)
      Singleton<NetManager>.instance.m_nodes.m_buffer[(int) data.m_targetBuilding].AddTargetCitizen(instanceID, ref data);
    else
      Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int) data.m_targetBuilding].AddTargetCitizen(instanceID, ref data);
  }

  public override void SimulationStep(
    ushort instanceID,
    ref CitizenInstance citizenData,
    ref CitizenInstance.Frame frameData,
    bool lodPhysics)
  {
    if ((long) (Singleton<SimulationManager>.instance.m_currentFrameIndex >> 4 & 63U) == (long) ((int) instanceID & 63))
    {
      CitizenManager instance1 = Singleton<CitizenManager>.instance;
      uint citizen = citizenData.m_citizen;
      if (citizen != 0U && (instance1.m_citizens.m_buffer[(IntPtr) citizen].m_flags & Citizen.Flags.NeedGoods) != Citizen.Flags.None)
      {
        BuildingManager instance2 = Singleton<BuildingManager>.instance;
        ushort homeBuilding = instance1.m_citizens.m_buffer[(IntPtr) citizen].m_homeBuilding;
        ushort building = instance2.FindBuilding(frameData.m_position, 32f, ItemClass.Service.Commercial, ItemClass.SubService.None, Building.Flags.Created | Building.Flags.Active, Building.Flags.Deleted);
        if (homeBuilding != (ushort) 0 && building != (ushort) 0)
        {
          BuildingInfo info = instance2.m_buildings.m_buffer[(int) building].Info;
          int amountDelta = -100;
          info.m_buildingAI.ModifyMaterialBuffer(building, ref instance2.m_buildings.m_buffer[(int) building], TransferManager.TransferReason.Shopping, ref amountDelta);
          uint containingUnit = instance1.m_citizens.m_buffer[(IntPtr) citizen].GetContainingUnit(citizen, instance2.m_buildings.m_buffer[(int) homeBuilding].m_citizenUnits, CitizenUnit.Flags.Home);
          if (containingUnit != 0U)
            instance1.m_units.m_buffer[(IntPtr) containingUnit].m_goods += (ushort) -amountDelta;
          instance1.m_citizens.m_buffer[(IntPtr) citizen].m_flags &= Citizen.Flags.Unemployed | Citizen.Flags.Wealth | Citizen.Flags.Location | Citizen.Flags.NoElectricity | Citizen.Flags.NoWater | Citizen.Flags.NoSewage | Citizen.Flags.BadHealth | Citizen.Flags.Created | Citizen.Flags.Tourist | Citizen.Flags.Sick | Citizen.Flags.Dead | Citizen.Flags.Student | Citizen.Flags.MovingIn | Citizen.Flags.DummyTraffic | Citizen.Flags.Criminal | Citizen.Flags.Arrested | Citizen.Flags.Evacuating | Citizen.Flags.Collapsed | Citizen.Flags.Education1 | Citizen.Flags.Education2 | Citizen.Flags.Education3 | Citizen.Flags.Original | Citizen.Flags.CustomName;
        }
      }
    }
    base.SimulationStep(instanceID, ref citizenData, ref frameData, lodPhysics);
  }

    // this is a kind of "general maintenance updates" sim step - the first sim step called by CitizenManager
    // called sequentially for 256 citizens in the list (pretty sure this is for the most recent 256 citizens)
  public override void SimulationStep(uint citizenID, ref Citizen data)
  {
    if (!data.Dead && this.UpdateAge(citizenID, ref data))  //UpdateAge does a bit more than just update age.  It also checks for FinishSchoolOrWork, updates OldestOriginalResident
                                                            // and randomizes people to die. It only returns true if the person died - which also then ends the sim step
      return;
    if (!data.Dead)
      this.UpdateHome(citizenID, ref data);     // basically: if they do not have a home, TransferManager records a Priority 7 offer based on work location (if working) else map random
    if (!data.Sick && !data.Dead)
    {
      if (this.UpdateHealth(citizenID, ref data))   // this looks at a lot of stuff such as sewage, water, garbage as well as health per se.  Citizen randomized to die in this step also (returns true if die)
        return;
      this.UpdateWellbeing(citizenID, ref data);    // another long one depends lots of stuff like ameneties etc etc
      this.UpdateWorkplace(citizenID, ref data);    // does nothing if citizen has both a workBuilding and a homeBuilding, else engenders a transfer offer
    }
    UpdateLocation(citizenID, ref data);   // should be called "update according to whether home/work/visiting/moving". I need to follow this because can call StartMoving() and FindVisitPlace()
  }


    // this is sim step for family units at home - only ResidentAI has a nontrivial implementation of this
    //
  public override void SimulationStep(uint homeID, ref CitizenUnit data)
  {

        // first block is if there is a citizen0 and a citizen1 (=married) randomize for kids (but max 3)
    CitizenManager instance1 = Singleton<CitizenManager>.instance;
    ushort building = instance1.m_units.m_buffer[(IntPtr) homeID].m_building;
    if (data.m_citizen0 != 0U && data.m_citizen1 != 0U && (data.m_citizen2 == 0U || data.m_citizen3 == 0U || data.m_citizen4 == 0U))
    {
      bool flag1 = this.CanMakeBabies(data.m_citizen0, ref instance1.m_citizens.m_buffer[(IntPtr) data.m_citizen0]);
      bool flag2 = this.CanMakeBabies(data.m_citizen1, ref instance1.m_citizens.m_buffer[(IntPtr) data.m_citizen1]);
      if (flag1 && flag2 && Singleton<SimulationManager>.instance.m_randomizer.Int32(12U) == 0)
      {
        int family = (int) instance1.m_citizens.m_buffer[(IntPtr) data.m_citizen0].m_family;
        uint citizen;
        if (instance1.CreateCitizen(out citizen, 0, family, ref Singleton<SimulationManager>.instance.m_randomizer))
        {
          instance1.m_citizens.m_buffer[(IntPtr) citizen].SetHome(citizen, (ushort) 0, homeID);
          instance1.m_citizens.m_buffer[(IntPtr) citizen].m_flags |= Citizen.Flags.Original;
          if (building != (ushort) 0)
          {
            DistrictManager instance2 = Singleton<DistrictManager>.instance;
            Vector3 position = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int) building].m_position;
            byte district = instance2.GetDistrict(position);
            ++instance2.m_districts.m_buffer[(int) district].m_birthData.m_tempCount;
          }
        }
      }
    }
    // then we look at singles/single-parent families to seek a partner
    if (data.m_citizen0 != 0U && data.m_citizen1 == 0U)
      this.TryFindPartner(data.m_citizen0, ref instance1.m_citizens.m_buffer[(IntPtr) data.m_citizen0]);
    else if (data.m_citizen1 != 0U && data.m_citizen0 == 0U)
      this.TryFindPartner(data.m_citizen1, ref instance1.m_citizens.m_buffer[(IntPtr) data.m_citizen1]);
    if (data.m_citizen2 != 0U)
    // then we look to see if the kids are ready to leave home (higher priority the older they get)
      this.TryMoveAwayFromHome(data.m_citizen2, ref instance1.m_citizens.m_buffer[(IntPtr) data.m_citizen2]);
    if (data.m_citizen3 != 0U)
      this.TryMoveAwayFromHome(data.m_citizen3, ref instance1.m_citizens.m_buffer[(IntPtr) data.m_citizen3]);
    if (data.m_citizen4 != 0U)
      this.TryMoveAwayFromHome(data.m_citizen4, ref instance1.m_citizens.m_buffer[(IntPtr) data.m_citizen4]);

        // this next is important for whether they need to go shopping
        // if we find one who does, flag them (in the Citizen list, not the unit list) as Citizen.Flags.NeedGoods
        // (the bigger the family, the greater the chance of randomizing a family slot that exists)
    data.m_goods = (ushort) Mathf.Max(0, (int) data.m_goods - 20);
    if (data.m_goods < (ushort) 200)
    {
      int num = Singleton<SimulationManager>.instance.m_randomizer.Int32(5U);
      for (int index = 0; index < 5; ++index)
      {
        uint citizen = data.GetCitizen((num + index) % 5);
        if (citizen != 0U)
        {
          instance1.m_citizens.m_buffer[(IntPtr) citizen].m_flags |= Citizen.Flags.NeedGoods;
          break;
        }
      }
    }

    // really not sure about this next line, how could home be 0 now?  And if there are major or fatal problems, why not move? (perhaps handled elsewhere?)
    if (building == (ushort) 0 || (Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int) building].m_problems & (Notification.Problem.MajorProblem | Notification.Problem.FatalProblem)) == Notification.Problem.None)
      return;

    // if none of the family are dead, consider moving the family
    uint citizenID = 0;
    int familySize = 0;
    if (data.m_citizen4 != 0U && !instance1.m_citizens.m_buffer[(IntPtr) data.m_citizen4].Dead)
    {
      ++familySize;
      citizenID = data.m_citizen4;
    }
    if (data.m_citizen3 != 0U && !instance1.m_citizens.m_buffer[(IntPtr) data.m_citizen3].Dead)
    {
      ++familySize;
      citizenID = data.m_citizen3;
    }
    if (data.m_citizen2 != 0U && !instance1.m_citizens.m_buffer[(IntPtr) data.m_citizen2].Dead)
    {
      ++familySize;
      citizenID = data.m_citizen2;
    }
    if (data.m_citizen1 != 0U && !instance1.m_citizens.m_buffer[(IntPtr) data.m_citizen1].Dead)
    {
      ++familySize;
      citizenID = data.m_citizen1;
    }
    if (data.m_citizen0 != 0U && !instance1.m_citizens.m_buffer[(IntPtr) data.m_citizen0].Dead)
    {
      ++familySize;
      citizenID = data.m_citizen0;
    }
    if (citizenID == 0U)
      return;
    this.TryMoveFamily(citizenID, ref instance1.m_citizens.m_buffer[(IntPtr) citizenID], familySize);
  }

  protected override void PathfindSuccess(ushort instanceID, ref CitizenInstance data)
  {
    uint citizen = data.m_citizen;
    if (citizen != 0U && (Singleton<CitizenManager>.instance.m_citizens.m_buffer[(IntPtr) citizen].m_flags & (Citizen.Flags.MovingIn | Citizen.Flags.DummyTraffic)) == Citizen.Flags.MovingIn)
      Singleton<StatisticsManager>.instance.Acquire<StatisticInt32>(StatisticType.MoveRate).Add(1);
    base.PathfindSuccess(instanceID, ref data);
  }

  protected override void Spawn(ushort instanceID, ref CitizenInstance data)
  {
    if ((data.m_flags & CitizenInstance.Flags.Character) != CitizenInstance.Flags.None)
      return;
    data.Spawn(instanceID);
    uint citizen = data.m_citizen;
    ushort targetBuilding = data.m_targetBuilding;
    if (citizen == 0U || targetBuilding == (ushort) 0)
      return;
    Randomizer randomizer = new Randomizer(citizen);
    if (randomizer.Int32(20U) != 0)
      return;
    CitizenManager instance1 = Singleton<CitizenManager>.instance;
    DistrictManager instance2 = Singleton<DistrictManager>.instance;
    Vector3 worldPos = (data.m_flags & CitizenInstance.Flags.TargetIsNode) == CitizenInstance.Flags.None ? Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int) targetBuilding].m_position : Singleton<NetManager>.instance.m_nodes.m_buffer[(int) targetBuilding].m_position;
    byte district1 = instance2.GetDistrict((Vector3) data.m_targetPos);
    byte district2 = instance2.GetDistrict(worldPos);
    if (((instance2.m_districts.m_buffer[(int) district1].m_servicePolicies | instance2.m_districts.m_buffer[(int) district2].m_servicePolicies) & DistrictPolicies.Services.PetBan) != DistrictPolicies.Services.None)
      return;
    CitizenInfo groupAnimalInfo = instance1.GetGroupAnimalInfo(ref randomizer, this.m_info.m_class.m_service, this.m_info.m_class.m_subService);
    ushort instance3;
    if (groupAnimalInfo == null || !instance1.CreateCitizenInstance(out instance3, ref randomizer, groupAnimalInfo, 0U))
      return;
    groupAnimalInfo.m_citizenAI.SetSource(instance3, ref instance1.m_instances.m_buffer[(int) instance3], instanceID);
    groupAnimalInfo.m_citizenAI.SetTarget(instance3, ref instance1.m_instances.m_buffer[(int) instance3], instanceID);
  }

  private bool UpdateAge(uint citizenID, ref Citizen data)
  {
    int num = data.Age + 1;
    if (num <= 45)
    {
      if (num == 15 || num == 45)
        this.FinishSchoolOrWork(citizenID, ref data);
    }
    else if (num == 90 || num == 180)
      this.FinishSchoolOrWork(citizenID, ref data);
    else if ((data.m_flags & Citizen.Flags.Student) != Citizen.Flags.None && num % 15 == 0)
      this.FinishSchoolOrWork(citizenID, ref data);
    if ((data.m_flags & Citizen.Flags.Original) != Citizen.Flags.None)
    {
      CitizenManager instance = Singleton<CitizenManager>.instance;
      if (instance.m_tempOldestOriginalResident < num)
        instance.m_tempOldestOriginalResident = num;
      if (num == 240)
        Singleton<StatisticsManager>.instance.Acquire<StatisticInt32>(StatisticType.FullLifespans).Add(1);
    }
    data.Age = num;
    if (num >= 240 && data.CurrentLocation != Citizen.Location.Moving && (data.m_vehicle == (ushort) 0 && Singleton<SimulationManager>.instance.m_randomizer.Int32(240, (int) byte.MaxValue) <= num))
    {
      this.Die(citizenID, ref data);
      if (Singleton<SimulationManager>.instance.m_randomizer.Int32(2U) == 0)
      {
        Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
        return true;
      }
    }
    return false;
  }

  private void Die(uint citizenID, ref Citizen data)
  {
    data.Sick = false;
    data.Dead = true;
    data.SetParkedVehicle(citizenID, (ushort) 0);
    if ((data.m_flags & Citizen.Flags.MovingIn) != Citizen.Flags.None)
      return;
    ushort num = data.GetBuildingByLocation();
    if (num == (ushort) 0)
      num = data.m_homeBuilding;
    if (num == (ushort) 0)
      return;
    DistrictManager instance = Singleton<DistrictManager>.instance;
    Vector3 position = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int) num].m_position;
    byte district = instance.GetDistrict(position);
    ++instance.m_districts.m_buffer[(int) district].m_deathData.m_tempCount;
  }

  private void UpdateHome(uint citizenID, ref Citizen data)
  {
    if (data.m_homeBuilding != (ushort) 0 || (data.m_flags & Citizen.Flags.DummyTraffic) != Citizen.Flags.None)
      return;
    TransferManager.TransferOffer offer = new TransferManager.TransferOffer();
    offer.Priority = 7;
    offer.Citizen = citizenID;
    offer.Amount = 1;
    offer.Active = true;
    if (data.m_workBuilding != (ushort) 0)
    {
      BuildingManager instance = Singleton<BuildingManager>.instance;
      offer.Position = instance.m_buildings.m_buffer[(int) data.m_workBuilding].m_position;
    }
    else
    {
      offer.PositionX = Singleton<SimulationManager>.instance.m_randomizer.Int32(256U);
      offer.PositionZ = Singleton<SimulationManager>.instance.m_randomizer.Int32(256U);
    }
    if (Singleton<SimulationManager>.instance.m_randomizer.Int32(2U) == 0)
    {
      switch (data.EducationLevel)
      {
        case Citizen.Education.Uneducated:
          Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single0, offer);
          break;
        case Citizen.Education.OneSchool:
          Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single1, offer);
          break;
        case Citizen.Education.TwoSchools:
          Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single2, offer);
          break;
        case Citizen.Education.ThreeSchools:
          Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single3, offer);
          break;
      }
    }
    else
    {
      switch (data.EducationLevel)
      {
        case Citizen.Education.Uneducated:
          Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single0B, offer);
          break;
        case Citizen.Education.OneSchool:
          Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single1B, offer);
          break;
        case Citizen.Education.TwoSchools:
          Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single2B, offer);
          break;
        case Citizen.Education.ThreeSchools:
          Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single3B, offer);
          break;
      }
    }
  }

  private void UpdateWorkplace(uint citizenID, ref Citizen data)
  {
    if (data.m_workBuilding != (ushort) 0 || data.m_homeBuilding == (ushort) 0)
      return;
    Vector3 position = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int) data.m_homeBuilding].m_position;
    DistrictManager instance = Singleton<DistrictManager>.instance;
    byte district = instance.GetDistrict(position);
    DistrictPolicies.Services servicePolicies = instance.m_districts.m_buffer[(int) district].m_servicePolicies;
    int age = data.Age;
    TransferManager.TransferReason material = TransferManager.TransferReason.None;
    switch (Citizen.GetAgeGroup(age))
    {
      case Citizen.AgeGroup.Child:
        if (!data.Education1)
        {
          material = TransferManager.TransferReason.Student1;
          break;
        }
        break;
      case Citizen.AgeGroup.Teen:
        if (data.Education1 && !data.Education2)
        {
          material = TransferManager.TransferReason.Student2;
          break;
        }
        break;
      case Citizen.AgeGroup.Young:
      case Citizen.AgeGroup.Adult:
        if (data.Education1 && data.Education2 && !data.Education3)
        {
          material = TransferManager.TransferReason.Student3;
          break;
        }
        break;
    }
    if (data.Unemployed != 0 && ((servicePolicies & DistrictPolicies.Services.EducationBoost) == DistrictPolicies.Services.None || material != TransferManager.TransferReason.Student3 || age % 5 > 2))
    {
      TransferManager.TransferOffer offer = new TransferManager.TransferOffer();
      offer.Priority = Singleton<SimulationManager>.instance.m_randomizer.Int32(8U);
      offer.Citizen = citizenID;
      offer.Position = position;
      offer.Amount = 1;
      offer.Active = true;
      switch (data.EducationLevel)
      {
        case Citizen.Education.Uneducated:
          Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Worker0, offer);
          break;
        case Citizen.Education.OneSchool:
          Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Worker1, offer);
          break;
        case Citizen.Education.TwoSchools:
          Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Worker2, offer);
          break;
        case Citizen.Education.ThreeSchools:
          Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Worker3, offer);
          break;
      }
    }
    switch (material)
    {
      case TransferManager.TransferReason.Student3:
        if ((servicePolicies & DistrictPolicies.Services.SchoolsOut) != DistrictPolicies.Services.None && age % 5 <= 1)
          return;
        break;
      case TransferManager.TransferReason.None:
        return;
    }
    Singleton<TransferManager>.instance.AddOutgoingOffer(material, new TransferManager.TransferOffer()
    {
      Priority = Singleton<SimulationManager>.instance.m_randomizer.Int32(8U),
      Citizen = citizenID,
      Position = position,
      Amount = 1,
      Active = true
    });
  }

  private bool UpdateHealth(uint citizenID, ref Citizen data)
  {
    if (data.m_homeBuilding == (ushort) 0)
      return false;
    int num1 = 20;
    BuildingManager instance1 = Singleton<BuildingManager>.instance;
    BuildingInfo info = instance1.m_buildings.m_buffer[(int) data.m_homeBuilding].Info;
    Vector3 position = instance1.m_buildings.m_buffer[(int) data.m_homeBuilding].m_position;
    DistrictManager instance2 = Singleton<DistrictManager>.instance;
    byte district = instance2.GetDistrict(position);
    DistrictPolicies.Services servicePolicies = instance2.m_districts.m_buffer[(int) district].m_servicePolicies;
    DistrictPolicies.CityPlanning planningPolicies = instance2.m_districts.m_buffer[(int) district].m_cityPlanningPolicies;
    if ((servicePolicies & DistrictPolicies.Services.SmokingBan) != DistrictPolicies.Services.None)
      num1 += 10;
    if (data.Age >= 180 && (planningPolicies & DistrictPolicies.CityPlanning.AntiSlip) != DistrictPolicies.CityPlanning.None)
      num1 += 10;
    int amount;
    int max;
    info.m_buildingAI.GetMaterialAmount(data.m_homeBuilding, ref instance1.m_buildings.m_buffer[(int) data.m_homeBuilding], TransferManager.TransferReason.Garbage, out amount, out max);
    int num2 = amount / 1000;
    if (num2 <= 2)
      num1 += 12;
    else if (num2 >= 4)
      num1 -= num2 - 3;
    int healthCareRequirement = Citizen.GetHealthCareRequirement(Citizen.GetAgePhase(data.EducationLevel, data.Age));
    int local1;
    int total;
    Singleton<ImmaterialResourceManager>.instance.CheckResource(ImmaterialResourceManager.Resource.HealthCare, position, out local1, out total);
    if (healthCareRequirement != 0)
    {
      if (local1 != 0)
        num1 += ImmaterialResourceManager.CalculateResourceEffect(local1, healthCareRequirement, 500, 20, 40);
      if (total != 0)
        num1 += ImmaterialResourceManager.CalculateResourceEffect(total, healthCareRequirement >> 1, 250, 5, 20);
    }
    int local2;
    Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(ImmaterialResourceManager.Resource.NoisePollution, position, out local2);
    if (local2 != 0)
    {
      if (info.m_class.m_subService == ItemClass.SubService.ResidentialLowEco || info.m_class.m_subService == ItemClass.SubService.ResidentialHighEco)
        num1 -= local2 * 150 / (int) byte.MaxValue;
      else
        num1 -= local2 * 100 / (int) byte.MaxValue;
    }
    int local3;
    Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(ImmaterialResourceManager.Resource.CrimeRate, position, out local3);
    if (local3 > 3)
    {
      if (local3 <= 30)
        num1 -= 2;
      else if (local3 <= 70)
        num1 -= 5;
      else
        num1 -= 15;
    }
    bool water;
    bool sewage;
    byte waterPollution;
    Singleton<WaterManager>.instance.CheckWater(position, out water, out sewage, out waterPollution);
    if (water)
    {
      num1 += 12;
      data.NoWater = 0;
    }
    else
    {
      int noWater = data.NoWater;
      if (noWater < 2)
        data.NoWater = noWater + 1;
      else
        num1 -= 5;
    }
    if (sewage)
    {
      num1 += 12;
      data.NoSewage = 0;
    }
    else
    {
      int noSewage = data.NoSewage;
      if (noSewage < 2)
        data.NoSewage = noSewage + 1;
      else
        num1 -= 5;
    }
    int num3 = waterPollution >= (byte) 35 ? num1 - ((int) waterPollution * 2 - 35) : num1 - (int) waterPollution;
    byte groundPollution;
    Singleton<NaturalResourceManager>.instance.CheckPollution(position, out groundPollution);
    if (groundPollution != (byte) 0)
    {
      if (info.m_class.m_subService == ItemClass.SubService.ResidentialLowEco || info.m_class.m_subService == ItemClass.SubService.ResidentialHighEco)
        num3 -= (int) groundPollution * 200 / (int) byte.MaxValue;
      else
        num3 -= (int) groundPollution * 100 / (int) byte.MaxValue;
    }
    if (data.m_workBuilding != (ushort) 0)
    {
      byte park = instance2.GetPark(instance1.m_buildings.m_buffer[(int) data.m_workBuilding].m_position);
      DistrictPolicies.Park parkPolicies = instance2.m_parks.m_buffer[(int) park].m_parkPolicies;
      if ((parkPolicies & DistrictPolicies.Park.WorkSafety) != DistrictPolicies.Park.None && instance1.m_buildings.m_buffer[(int) data.m_workBuilding].Info.m_class.m_service == ItemClass.Service.PlayerIndustry)
        num3 += 10;
      if ((parkPolicies & DistrictPolicies.Park.StudentHealthcare) != DistrictPolicies.Park.None && instance1.m_buildings.m_buffer[(int) data.m_workBuilding].Info.m_class.m_service == ItemClass.Service.PlayerEducation)
        num3 += 10;
    }
    int num4 = Mathf.Clamp(num3, 0, 100);
    data.m_health = (byte) num4;
    int num5 = 0;
    if (num4 <= 10)
    {
      int badHealth = data.BadHealth;
      if (badHealth < 3)
      {
        num5 = 15;
        data.BadHealth = badHealth + 1;
      }
      else
        num5 = total != 0 ? 50 : 75;
    }
    else if (num4 <= 25)
    {
      data.BadHealth = 0;
      num5 += 10;
    }
    else if (num4 <= 50)
    {
      data.BadHealth = 0;
      num5 += 3;
    }
    else
      data.BadHealth = 0;
    if (data.CurrentLocation != Citizen.Location.Moving && data.m_vehicle == (ushort) 0 && (num5 != 0 && Singleton<SimulationManager>.instance.m_randomizer.Int32(100U) < num5))
    {
      if (Singleton<SimulationManager>.instance.m_randomizer.Int32(3U) == 0)
      {
        this.Die(citizenID, ref data);
        if (Singleton<SimulationManager>.instance.m_randomizer.Int32(2U) == 0)
        {
          Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
          return true;
        }
      }
      else
        data.Sick = true;
    }
    return false;
  }

  private void UpdateWellbeing(uint citizenID, ref Citizen data)
  {
    if (data.m_homeBuilding == (ushort) 0)
      return;
    int num1 = 0;
    BuildingManager instance1 = Singleton<BuildingManager>.instance;
    BuildingInfo info1 = instance1.m_buildings.m_buffer[(int) data.m_homeBuilding].Info;
    Vector3 position1 = instance1.m_buildings.m_buffer[(int) data.m_homeBuilding].m_position;
    ItemClass itemClass = info1.m_class;
    DistrictManager instance2 = Singleton<DistrictManager>.instance;
    byte district = instance2.GetDistrict(position1);
    DistrictPolicies.Services servicePolicies = instance2.m_districts.m_buffer[(int) district].m_servicePolicies;
    DistrictPolicies.Taxation taxationPolicies = instance2.m_districts.m_buffer[(int) district].m_taxationPolicies;
    DistrictPolicies.CityPlanning planningPolicies = instance2.m_districts.m_buffer[(int) district].m_cityPlanningPolicies;
    int health = (int) data.m_health;
    if (health > 80)
      num1 += 10;
    else if (health > 60)
      num1 += 5;
    int num2 = num1 - Mathf.Clamp(50 - health, 0, 30);
    if ((servicePolicies & DistrictPolicies.Services.PetBan) != DistrictPolicies.Services.None)
      num2 -= 5;
    if ((servicePolicies & DistrictPolicies.Services.SmokingBan) != DistrictPolicies.Services.None)
      num2 -= 15;
    if (instance1.m_buildings.m_buffer[(int) data.m_homeBuilding].GetLastFrameData().m_fireDamage != (byte) 0)
      num2 -= 15;
    Citizen.Wealth wealthLevel = data.WealthLevel;
    Citizen.AgePhase agePhase = Citizen.GetAgePhase(data.EducationLevel, data.Age);
    if ((planningPolicies & DistrictPolicies.CityPlanning.WorkersUnion) != DistrictPolicies.CityPlanning.None && agePhase >= Citizen.AgePhase.Adult0 && agePhase < Citizen.AgePhase.Senior0)
      num2 += 10;
    if ((servicePolicies & DistrictPolicies.Services.ForProfitEducation) != DistrictPolicies.Services.None)
      num2 -= 20;
    if (data.m_workBuilding != (ushort) 0)
    {
      BuildingInfo info2 = instance1.m_buildings.m_buffer[(int) data.m_workBuilding].Info;
      Vector3 position2 = instance1.m_buildings.m_buffer[(int) data.m_workBuilding].m_position;
      ItemClass.Service service = info2.m_class.m_service;
      byte park = instance2.GetPark(position2);
      DistrictPolicies.Park parkPolicies = instance2.m_parks.m_buffer[(int) park].m_parkPolicies;
      if (service == ItemClass.Service.PlayerEducation)
      {
        if ((parkPolicies & DistrictPolicies.Park.FreeLunch) != DistrictPolicies.Park.None)
          num2 += 5;
        if ((parkPolicies & DistrictPolicies.Park.UniversalEducation) != DistrictPolicies.Park.None)
          num2 += 15;
      }
    }
    int taxRate = Singleton<EconomyManager>.instance.GetTaxRate(itemClass, taxationPolicies);
    int num3 = (int) (8 - wealthLevel);
    int num4 = (int) (11 - wealthLevel);
    if (itemClass.m_subService == ItemClass.SubService.ResidentialHigh)
    {
      ++num3;
      ++num4;
    }
    if (taxRate < num3)
      num2 += num3 - taxRate;
    if (taxRate > num4)
      num2 -= taxRate - num4;
    int departmentRequirement1 = Citizen.GetPoliceDepartmentRequirement(agePhase);
    if (departmentRequirement1 != 0)
    {
      int local;
      int total;
      Singleton<ImmaterialResourceManager>.instance.CheckResource(ImmaterialResourceManager.Resource.PoliceDepartment, position1, out local, out total);
      if (local != 0)
        num2 += ImmaterialResourceManager.CalculateResourceEffect(local, departmentRequirement1, 500, 20, 40);
      if (total != 0)
        num2 += ImmaterialResourceManager.CalculateResourceEffect(total, departmentRequirement1 >> 1, 250, 5, 20);
    }
    int departmentRequirement2 = Citizen.GetFireDepartmentRequirement(agePhase);
    if (departmentRequirement2 != 0)
    {
      int local;
      int total;
      Singleton<ImmaterialResourceManager>.instance.CheckResource(ImmaterialResourceManager.Resource.FireDepartment, position1, out local, out total);
      if (local != 0)
        num2 += ImmaterialResourceManager.CalculateResourceEffect(local, departmentRequirement2, 500, 20, 40);
      if (total != 0)
        num2 += ImmaterialResourceManager.CalculateResourceEffect(total, departmentRequirement2 >> 1, 250, 5, 20);
    }
    int local1;
    int total1;
    Singleton<ImmaterialResourceManager>.instance.CheckResource(ImmaterialResourceManager.Resource.EducationLibrary, position1, out local1, out total1);
    if (local1 > 0)
      num2 += 5;
    int educationRequirement = Citizen.GetEducationRequirement(agePhase);
    if (educationRequirement != 0)
    {
      int num5 = Singleton<SimulationManager>.instance.m_randomizer.Int32(10000U);
      int local2;
      int total2;
      if (agePhase < Citizen.AgePhase.Teen0)
      {
        Singleton<ImmaterialResourceManager>.instance.CheckResource(ImmaterialResourceManager.Resource.EducationElementary, position1, out local2, out total2);
        if (local2 > 1000 && !data.Education1 && Singleton<SimulationManager>.instance.m_randomizer.Int32(9000U) < local2 - 1000)
          data.Education1 = true;
      }
      else if (agePhase < Citizen.AgePhase.Young0)
      {
        Singleton<ImmaterialResourceManager>.instance.CheckResource(ImmaterialResourceManager.Resource.EducationElementary, position1, out local2, out total2);
        if (local2 > 1000 && !data.Education1 && Singleton<SimulationManager>.instance.m_randomizer.Int32(9000U) < local2 - 1000)
          data.Education1 = true;
        Singleton<ImmaterialResourceManager>.instance.CheckResource(ImmaterialResourceManager.Resource.EducationHighSchool, position1, out local2, out total2);
        if (local2 > 1000 && data.Education1 && (!data.Education2 && Singleton<SimulationManager>.instance.m_randomizer.Int32(9000U) < local2 - 1000))
          data.Education2 = true;
        if (local1 > 80)
        {
          if (!data.Education1)
          {
            if (num5 < 25)
              data.Education1 = true;
          }
          else if (data.Education1 && !data.Education2 && num5 < 25)
            data.Education2 = true;
        }
      }
      else
      {
        Singleton<ImmaterialResourceManager>.instance.CheckResource(ImmaterialResourceManager.Resource.EducationElementary, position1, out local2, out total2);
        if (local2 > 1000 && !data.Education1 && Singleton<SimulationManager>.instance.m_randomizer.Int32(9000U) < local2 - 1000)
          data.Education1 = true;
        Singleton<ImmaterialResourceManager>.instance.CheckResource(ImmaterialResourceManager.Resource.EducationHighSchool, position1, out local2, out total2);
        if (local2 > 1000 && data.Education1 && (!data.Education2 && Singleton<SimulationManager>.instance.m_randomizer.Int32(9000U) < local2 - 1000))
          data.Education2 = true;
        Singleton<ImmaterialResourceManager>.instance.CheckResource(ImmaterialResourceManager.Resource.EducationUniversity, position1, out local2, out total2);
        if (local2 > 1000 && data.Education1 && (data.Education2 && !data.Education3) && Singleton<SimulationManager>.instance.m_randomizer.Int32(9000U) < local2 - 1000)
          data.Education3 = true;
        if (local1 > 80)
        {
          if (!data.Education1)
          {
            if (num5 < 25)
              data.Education1 = true;
          }
          else if (data.Education1 && !data.Education2)
          {
            if (num5 < 25)
              data.Education2 = true;
          }
          else if (data.Education2 && !data.Education3 && num5 < 25)
            data.Education3 = true;
        }
      }
      if (local2 != 0)
        num2 += ImmaterialResourceManager.CalculateResourceEffect(local2, educationRequirement, 500, 20, 40);
      if (total2 != 0)
        num2 += ImmaterialResourceManager.CalculateResourceEffect(total2, educationRequirement >> 1, 250, 5, 20);
    }
    int entertainmentRequirement = Citizen.GetEntertainmentRequirement(agePhase);
    if (entertainmentRequirement != 0)
    {
      int local2;
      int total2;
      Singleton<ImmaterialResourceManager>.instance.CheckResource(ImmaterialResourceManager.Resource.Entertainment, position1, out local2, out total2);
      if (local2 != 0)
        num2 += ImmaterialResourceManager.CalculateResourceEffect(local2, entertainmentRequirement, 500, 30, 60);
      if (total2 != 0)
        num2 += ImmaterialResourceManager.CalculateResourceEffect(total2, entertainmentRequirement >> 1, 250, 10, 40);
    }
    int transportRequirement = Citizen.GetTransportRequirement(agePhase);
    if (transportRequirement != 0)
    {
      int local2;
      int total2;
      Singleton<ImmaterialResourceManager>.instance.CheckResource(ImmaterialResourceManager.Resource.PublicTransport, position1, out local2, out total2);
      if (local2 != 0)
        num2 += ImmaterialResourceManager.CalculateResourceEffect(local2, transportRequirement, 500, 20, 40);
      if (total2 != 0)
        num2 += ImmaterialResourceManager.CalculateResourceEffect(total2, transportRequirement >> 1, 250, 5, 20);
    }
    int deathCareRequirement = Citizen.GetDeathCareRequirement(agePhase);
    int local3;
    int total3;
    Singleton<ImmaterialResourceManager>.instance.CheckResource(ImmaterialResourceManager.Resource.DeathCare, position1, out local3, out total3);
    if (deathCareRequirement != 0)
    {
      if (local3 != 0)
        num2 += ImmaterialResourceManager.CalculateResourceEffect(local3, deathCareRequirement, 500, 10, 20);
      if (total3 != 0)
        num2 += ImmaterialResourceManager.CalculateResourceEffect(total3, deathCareRequirement >> 1, 250, 3, 10);
    }
    int local4;
    Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(ImmaterialResourceManager.Resource.RadioCoverage, position1, out local4);
    if (local4 != 0)
      num2 += ImmaterialResourceManager.CalculateResourceEffect(local4, 50, 100, 2, 3);
    int local5;
    Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(ImmaterialResourceManager.Resource.DisasterCoverage, position1, out local5);
    if (local5 != 0)
      num2 += ImmaterialResourceManager.CalculateResourceEffect(local5, 50, 100, 3, 4);
    int local6;
    Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(ImmaterialResourceManager.Resource.FirewatchCoverage, position1, out local6);
    if (local6 != 0)
      num2 += ImmaterialResourceManager.CalculateResourceEffect(local6, 100, 1000, 0, 3);
    bool electricity;
    Singleton<ElectricityManager>.instance.CheckElectricity(position1, out electricity);
    if (electricity)
    {
      num2 += 12;
      data.NoElectricity = 0;
    }
    else
    {
      int noElectricity = data.NoElectricity;
      if (noElectricity < 2)
        data.NoElectricity = noElectricity + 1;
      else
        num2 -= 5;
    }
    bool heating;
    Singleton<WaterManager>.instance.CheckHeating(position1, out heating);
    if (heating)
      num2 += 5;
    else if ((servicePolicies & DistrictPolicies.Services.NoElectricity) != DistrictPolicies.Services.None)
      num2 -= 10;
    if ((planningPolicies & DistrictPolicies.CityPlanning.ElectricCars) != DistrictPolicies.CityPlanning.None)
    {
      int carProbability = this.GetCarProbability(Citizen.GetAgeGroup(data.Age));
      if (new Randomizer(citizenID).Int32(100U) < carProbability)
        Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.PolicyCost, 200, itemClass);
    }
    bool flag = Singleton<UnlockManager>.instance.Unlocked(ItemClass.Service.PoliceDepartment);
    int workRequirement = Citizen.GetWorkRequirement(agePhase);
    if (workRequirement != 0)
    {
      if (data.m_workBuilding == (ushort) 0)
      {
        int unemployed = data.Unemployed;
        num2 -= unemployed * workRequirement / 100;
        data.Unemployed = !flag ? Mathf.Min(1, unemployed + 1) : unemployed + 1;
      }
      else
        data.Unemployed = 0;
    }
    else
      data.Unemployed = 0;
    if (Singleton<LoadingManager>.instance.SupportsExpansion(Expansion.Industry) && Singleton<UnlockManager>.instance.Unlocked(ItemClass.SubService.PublicTransportPost))
    {
      PrivateBuildingAI buildingAi = info1.m_buildingAI as PrivateBuildingAI;
      if (buildingAi != null)
      {
        int homeCount = buildingAi.CalculateHomeCount(itemClass.m_level, new Randomizer((int) data.m_homeBuilding), instance1.m_buildings.m_buffer[(int) data.m_homeBuilding].Width, instance1.m_buildings.m_buffer[(int) data.m_homeBuilding].Length);
        if (homeCount != 0)
        {
          int resourceRate = 50 - (int) instance1.m_buildings.m_buffer[(int) data.m_homeBuilding].m_mailBuffer / (homeCount * 5);
          num2 += ImmaterialResourceManager.CalculateResourceEffect(resourceRate, 38, 50, 8, 15);
        }
      }
    }
    int wellbeing = Mathf.Clamp(num2, 0, 100);
    data.m_wellbeing = (byte) wellbeing;
    if (flag)
    {
      Randomizer randomizer = new Randomizer((uint) ((int) citizenID * 7931 + 123));
      int num5 = Mathf.Min(Citizen.GetMaxCrimeRate(Citizen.GetWellbeingLevel(data.EducationLevel, wellbeing)), Citizen.GetCrimeRate(data.Unemployed));
      data.Criminal = randomizer.Int32(500U) < num5;
    }
    else
      data.Criminal = false;
  }

  private void FinishSchoolOrWork(uint citizenID, ref Citizen data)
  {
    if (data.m_workBuilding == (ushort) 0)
      return;
    if (data.CurrentLocation == Citizen.Location.Work && data.m_homeBuilding != (ushort) 0)
    {
      data.m_flags &= Citizen.Flags.Unemployed | Citizen.Flags.Wealth | Citizen.Flags.Location | Citizen.Flags.NoElectricity | Citizen.Flags.NoWater | Citizen.Flags.NoSewage | Citizen.Flags.BadHealth | Citizen.Flags.Created | Citizen.Flags.Tourist | Citizen.Flags.Sick | Citizen.Flags.Dead | Citizen.Flags.Student | Citizen.Flags.MovingIn | Citizen.Flags.DummyTraffic | Citizen.Flags.Criminal | Citizen.Flags.Arrested | Citizen.Flags.Collapsed | Citizen.Flags.Education1 | Citizen.Flags.Education2 | Citizen.Flags.Education3 | Citizen.Flags.NeedGoods | Citizen.Flags.Original | Citizen.Flags.CustomName;
      this.StartMoving(citizenID, ref data, data.m_workBuilding, data.m_homeBuilding);
    }
    BuildingManager instance1 = Singleton<BuildingManager>.instance;
    CitizenManager instance2 = Singleton<CitizenManager>.instance;
    uint num1 = instance1.m_buildings.m_buffer[(int) data.m_workBuilding].m_citizenUnits;
    int num2 = 0;
    while (num1 != 0U)
    {
      uint nextUnit = instance2.m_units.m_buffer[(IntPtr) num1].m_nextUnit;
      CitizenUnit.Flags flags = instance2.m_units.m_buffer[(IntPtr) num1].m_flags;
      if ((flags & (CitizenUnit.Flags.Work | CitizenUnit.Flags.Student)) != CitizenUnit.Flags.None)
      {
        if ((flags & CitizenUnit.Flags.Student) != CitizenUnit.Flags.None)
        {
          if (data.RemoveFromUnit(citizenID, ref instance2.m_units.m_buffer[(IntPtr) num1]))
          {
            BuildingInfo info = instance1.m_buildings.m_buffer[(int) data.m_workBuilding].Info;
            if (info.m_buildingAI.GetEducationLevel1())
              data.Education1 = true;
            if (info.m_buildingAI.GetEducationLevel2())
            {
              if (!data.Education1)
                data.Education1 = true;
              else
                data.Education2 = true;
            }
            if (info.m_buildingAI.GetEducationLevel3())
            {
              if (!data.Education1)
                data.Education1 = true;
              else if (!data.Education2)
                data.Education2 = true;
              else
                data.Education3 = true;
            }
            data.m_workBuilding = (ushort) 0;
            data.m_flags &= Citizen.Flags.Unemployed | Citizen.Flags.Wealth | Citizen.Flags.Location | Citizen.Flags.NoElectricity | Citizen.Flags.NoWater | Citizen.Flags.NoSewage | Citizen.Flags.BadHealth | Citizen.Flags.Created | Citizen.Flags.Tourist | Citizen.Flags.Sick | Citizen.Flags.Dead | Citizen.Flags.MovingIn | Citizen.Flags.DummyTraffic | Citizen.Flags.Criminal | Citizen.Flags.Arrested | Citizen.Flags.Evacuating | Citizen.Flags.Collapsed | Citizen.Flags.Education1 | Citizen.Flags.Education2 | Citizen.Flags.Education3 | Citizen.Flags.NeedGoods | Citizen.Flags.Original | Citizen.Flags.CustomName;
            if ((data.m_flags & Citizen.Flags.Original) == Citizen.Flags.None || data.EducationLevel != Citizen.Education.ThreeSchools || (instance2.m_fullyEducatedOriginalResidents++ != 0 || Singleton<SimulationManager>.instance.m_metaData.m_disableAchievements == SimulationMetaData.MetaBool.True))
              break;
            ThreadHelper.dispatcher.Dispatch((System.Action) (() =>
            {
              if (PlatformService.achievements["ClimbingTheSocialLadder"].achieved)
                return;
              PlatformService.achievements["ClimbingTheSocialLadder"].Unlock();
            }));
            break;
          }
        }
        else if (data.RemoveFromUnit(citizenID, ref instance2.m_units.m_buffer[(IntPtr) num1]))
        {
          data.m_workBuilding = (ushort) 0;
          data.m_flags &= Citizen.Flags.Unemployed | Citizen.Flags.Wealth | Citizen.Flags.Location | Citizen.Flags.NoElectricity | Citizen.Flags.NoWater | Citizen.Flags.NoSewage | Citizen.Flags.BadHealth | Citizen.Flags.Created | Citizen.Flags.Tourist | Citizen.Flags.Sick | Citizen.Flags.Dead | Citizen.Flags.MovingIn | Citizen.Flags.DummyTraffic | Citizen.Flags.Criminal | Citizen.Flags.Arrested | Citizen.Flags.Evacuating | Citizen.Flags.Collapsed | Citizen.Flags.Education1 | Citizen.Flags.Education2 | Citizen.Flags.Education3 | Citizen.Flags.NeedGoods | Citizen.Flags.Original | Citizen.Flags.CustomName;
          break;
        }
      }
      num1 = nextUnit;
      if (++num2 > 524288)
      {
        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
        break;
      }
    }
  }

  private bool DoRandomMove()
  {
    uint vehicleCount = (uint) Singleton<VehicleManager>.instance.m_vehicleCount;
    uint instanceCount = (uint) Singleton<CitizenManager>.instance.m_instanceCount;
    if (vehicleCount * 65536U > instanceCount * 16384U)
      return Singleton<SimulationManager>.instance.m_randomizer.UInt32(16384U) > vehicleCount;
    return Singleton<SimulationManager>.instance.m_randomizer.UInt32(65536U) > instanceCount;
  }

  private TransferManager.TransferReason GetShoppingReason()
  {
    switch (Singleton<SimulationManager>.instance.m_randomizer.Int32(8U))
    {
      case 0:
        return TransferManager.TransferReason.Shopping;
      case 1:
        return TransferManager.TransferReason.ShoppingB;
      case 2:
        return TransferManager.TransferReason.ShoppingC;
      case 3:
        return TransferManager.TransferReason.ShoppingD;
      case 4:
        return TransferManager.TransferReason.ShoppingE;
      case 5:
        return TransferManager.TransferReason.ShoppingF;
      case 6:
        return TransferManager.TransferReason.ShoppingG;
      case 7:
        return TransferManager.TransferReason.ShoppingH;
      default:
        return TransferManager.TransferReason.Shopping;
    }
  }

  private TransferManager.TransferReason GetEntertainmentReason()
  {
    switch (Singleton<SimulationManager>.instance.m_randomizer.Int32(4U))
    {
      case 0:
        return TransferManager.TransferReason.Entertainment;
      case 1:
        return TransferManager.TransferReason.EntertainmentB;
      case 2:
        return TransferManager.TransferReason.EntertainmentC;
      case 3:
        return TransferManager.TransferReason.EntertainmentD;
      default:
        return TransferManager.TransferReason.Entertainment;
    }
  }

  private TransferManager.TransferReason GetEvacuationReason(ushort sourceBuilding)
  {
    if (sourceBuilding != (ushort) 0)
    {
      BuildingManager instance1 = Singleton<BuildingManager>.instance;
      DistrictManager instance2 = Singleton<DistrictManager>.instance;
      byte district = instance2.GetDistrict(instance1.m_buildings.m_buffer[(int) sourceBuilding].m_position);
      if ((instance2.m_districts.m_buffer[(int) district].m_cityPlanningPolicies & DistrictPolicies.CityPlanning.VIPArea) != DistrictPolicies.CityPlanning.None)
      {
        switch (Singleton<SimulationManager>.instance.m_randomizer.Int32(4U))
        {
          case 0:
            return TransferManager.TransferReason.EvacuateVipA;
          case 1:
            return TransferManager.TransferReason.EvacuateVipB;
          case 2:
            return TransferManager.TransferReason.EvacuateVipC;
          case 3:
            return TransferManager.TransferReason.EvacuateVipD;
          default:
            return TransferManager.TransferReason.EvacuateVipA;
        }
      }
    }
    switch (Singleton<SimulationManager>.instance.m_randomizer.Int32(4U))
    {
      case 0:
        return TransferManager.TransferReason.EvacuateA;
      case 1:
        return TransferManager.TransferReason.EvacuateB;
      case 2:
        return TransferManager.TransferReason.EvacuateC;
      case 3:
        return TransferManager.TransferReason.EvacuateD;
      default:
        return TransferManager.TransferReason.EvacuateA;
    }
  }

    // a long but important routine where citizens get updated (eg start moving) with actions depending on their "location"
    // location is one of : Home Work Visit Moving
    // called sequentially in sim step for all the citizens in the Manager
  private void UpdateLocation(uint citizenID, ref Citizen data)
  {
        // quick exit do nothing except release null citiizens
    if (data.m_homeBuilding == (ushort) 0 && data.m_workBuilding == (ushort) 0 && (data.m_visitBuilding == (ushort) 0 && data.m_instance == (ushort) 0) && data.m_vehicle == (ushort) 0)
    {
      Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
    }
    else
    {
            switch (data.CurrentLocation)         // the big switch
            {
                case Citizen.Location.Home:
                    if ((data.m_flags & Citizen.Flags.MovingIn) != Citizen.Flags.None)
                    {
                        Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
                        return;
                    }
                    if (data.Dead)
                    {
                        if (data.m_homeBuilding == (ushort)0)
                        {
                            Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
                            return;
                        }
                        if (data.m_workBuilding != (ushort)0)
                            data.SetWorkplace(citizenID, (ushort)0, 0U);
                        if (data.m_visitBuilding != (ushort)0)
                            data.SetVisitplace(citizenID, (ushort)0, 0U);
                        if (data.m_vehicle == (ushort)0 && !this.FindHospital(citizenID, data.m_homeBuilding, TransferManager.TransferReason.Dead))
                            return;
                        break;
                    }
                    if (data.Arrested)
                    {
                        data.Arrested = false;
                        break;
                    }
                    if (data.m_homeBuilding != (ushort)0 && data.m_vehicle == (ushort)0)
                    {
                        if (data.Sick)
                        {
                            if (!this.FindHospital(citizenID, data.m_homeBuilding, TransferManager.TransferReason.Sick))
                                return;
                            break;
                        }
                        if ((Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)data.m_homeBuilding].m_flags & Building.Flags.Evacuating) != Building.Flags.None)
                        {
                            this.FindEvacuationPlace(citizenID, data.m_homeBuilding, this.GetEvacuationReason(data.m_homeBuilding));
                            break;
                        }
                        if ((data.m_flags & Citizen.Flags.NeedGoods) != Citizen.Flags.None)
                        {
                            // FindVisitPlace does not set a target per se. It just creates an IncomingOffer with priority R(8) position Home
                            this.FindVisitPlace(citizenID, data.m_homeBuilding, this.GetShoppingReason());
                            break;
                        }
                        if (data.m_instance != 0 || this.DoRandomMove())    // random moves are less likely with increasing number of vehicles and citizens
                        {
                            int dayTimeFrame = (int)Singleton<SimulationManager>.instance.m_dayTimeFrame;
                            int daytimeFrames = (int)SimulationManager.DAYTIME_FRAMES;
                            int num1 = daytimeFrames / 40;
                            int num2 = (int)SimulationManager.DAYTIME_FRAMES * 8 / 24;
                            int num3 = Mathf.Abs((dayTimeFrame - num2 & daytimeFrames - 1) - (daytimeFrames >> 1));
                            int num4 = num3 * num3 / (daytimeFrames >> 1);
                            int num5 = Singleton<SimulationManager>.instance.m_randomizer.Int32((uint)daytimeFrames);
                            // at random depending on time of day - either find a suggested entertainment visit place or go to work (or do nothing)
                            if (num5 < num1)
                            {
                                this.FindVisitPlace(citizenID, data.m_homeBuilding, this.GetEntertainmentReason());     // set up an offer to find entertainments (does not actually itself find a visit place)
                                break;
                            }
                            if (num5 < num1 + num4 && data.m_workBuilding != (ushort)0)
                            {
                                data.m_flags &= Citizen.Flags.Unemployed | Citizen.Flags.Wealth | Citizen.Flags.Location | Citizen.Flags.NoElectricity | Citizen.Flags.NoWater | Citizen.Flags.NoSewage | Citizen.Flags.BadHealth | Citizen.Flags.Created | Citizen.Flags.Tourist | Citizen.Flags.Sick | Citizen.Flags.Dead | Citizen.Flags.Student | Citizen.Flags.MovingIn | Citizen.Flags.DummyTraffic | Citizen.Flags.Criminal | Citizen.Flags.Arrested | Citizen.Flags.Collapsed | Citizen.Flags.Education1 | Citizen.Flags.Education2 | Citizen.Flags.Education3 | Citizen.Flags.NeedGoods | Citizen.Flags.Original | Citizen.Flags.CustomName;
                                // StartMoving is stronger than an Offer - it flags the citizen as Moving and sets targetBuilding and sourceBuilding (as Buildings, not Nodes)
                                this.StartMoving(citizenID, ref data, data.m_homeBuilding, data.m_workBuilding);
                                break;
                            }
                            break;
                        }
                        break;
                    }
                    break;
                case Citizen.Location.Work:
                    if (data.Dead)
                    {
                        if (data.m_workBuilding == (ushort)0)
                        {
                            Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
                            return;
                        }
                        if (data.m_homeBuilding != (ushort)0)
                            data.SetHome(citizenID, (ushort)0, 0U);
                        if (data.m_visitBuilding != (ushort)0)
                            data.SetVisitplace(citizenID, (ushort)0, 0U);
                        if (data.m_vehicle == (ushort)0 && !this.FindHospital(citizenID, data.m_workBuilding, TransferManager.TransferReason.Dead))
                            return;
                        break;
                    }
                    if (data.Arrested)
                    {
                        data.Arrested = false;
                        break;
                    }
                    if (data.Sick)
                    {
                        if (data.m_workBuilding == (ushort)0)
                        {
                            data.CurrentLocation = Citizen.Location.Home;
                            break;
                        }
                        if (data.m_vehicle == (ushort)0 && !this.FindHospital(citizenID, data.m_workBuilding, TransferManager.TransferReason.Sick))
                            return;
                        break;
                    }
                    if (data.m_workBuilding == (ushort)0)
                    {
                        data.CurrentLocation = Citizen.Location.Home;
                        break;
                    }
                    BuildingManager instance1 = Singleton<BuildingManager>.instance;
                    ushort eventIndex1 = instance1.m_buildings.m_buffer[(int)data.m_workBuilding].m_eventIndex;
                    if ((instance1.m_buildings.m_buffer[(int)data.m_workBuilding].m_flags & Building.Flags.Evacuating) != Building.Flags.None)
                    {
                        if (data.m_vehicle == (ushort)0)
                        {
                            this.FindEvacuationPlace(citizenID, data.m_workBuilding, this.GetEvacuationReason(data.m_workBuilding));
                            break;
                        }
                        break;
                    }
                    if (eventIndex1 == (ushort)0 || (Singleton<EventManager>.instance.m_events.m_buffer[(int)eventIndex1].m_flags & (EventData.Flags.Preparing | EventData.Flags.Active | EventData.Flags.Ready)) == EventData.Flags.None)
                    {
                        if ((data.m_flags & Citizen.Flags.NeedGoods) != Citizen.Flags.None)
                        {
                            if (data.m_vehicle == (ushort)0)
                            {
                                this.FindVisitPlace(citizenID, data.m_workBuilding, this.GetShoppingReason());
                                break;
                            }
                            break;
                        }
                        if (data.m_instance != (ushort)0 || this.DoRandomMove())
                        {
                            int dayTimeFrame = (int)Singleton<SimulationManager>.instance.m_dayTimeFrame;
                            int daytimeFrames = (int)SimulationManager.DAYTIME_FRAMES;
                            int num1 = daytimeFrames / 40;
                            int num2 = (int)SimulationManager.DAYTIME_FRAMES * 16 / 24;
                            int num3 = Mathf.Abs((dayTimeFrame - num2 & daytimeFrames - 1) - (daytimeFrames >> 1));
                            int num4 = num3 * num3 / (daytimeFrames >> 1);
                            int num5 = Singleton<SimulationManager>.instance.m_randomizer.Int32((uint)daytimeFrames);
                            if (num5 < num1)
                            {
                                if (data.m_vehicle == (ushort)0)
                                {
                                    this.FindVisitPlace(citizenID, data.m_workBuilding, this.GetEntertainmentReason());
                                    break;
                                }
                                break;
                            }
                            if (num5 < num1 + num4 && data.m_homeBuilding != (ushort)0 && data.m_vehicle == (ushort)0)
                            {
                                data.m_flags &= Citizen.Flags.Unemployed | Citizen.Flags.Wealth | Citizen.Flags.Location | Citizen.Flags.NoElectricity | Citizen.Flags.NoWater | Citizen.Flags.NoSewage | Citizen.Flags.BadHealth | Citizen.Flags.Created | Citizen.Flags.Tourist | Citizen.Flags.Sick | Citizen.Flags.Dead | Citizen.Flags.Student | Citizen.Flags.MovingIn | Citizen.Flags.DummyTraffic | Citizen.Flags.Criminal | Citizen.Flags.Arrested | Citizen.Flags.Collapsed | Citizen.Flags.Education1 | Citizen.Flags.Education2 | Citizen.Flags.Education3 | Citizen.Flags.NeedGoods | Citizen.Flags.Original | Citizen.Flags.CustomName;
                                this.StartMoving(citizenID, ref data, data.m_workBuilding, data.m_homeBuilding);
                                break;
                            }
                            break;
                        }
                        break;
                    }
                    break;
                case Citizen.Location.Visit:
                    if (data.Dead)
                    {
                        if (data.m_visitBuilding == (ushort)0)
                        {
                            Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
                            return;
                        }
                        if (data.m_homeBuilding != (ushort)0)
                            data.SetHome(citizenID, (ushort)0, 0U);
                        if (data.m_workBuilding != (ushort)0)
                            data.SetWorkplace(citizenID, (ushort)0, 0U);
                        if (data.m_vehicle == (ushort)0 && Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)data.m_visitBuilding].Info.m_class.m_service != ItemClass.Service.HealthCare && !this.FindHospital(citizenID, data.m_visitBuilding, TransferManager.TransferReason.Dead))
                            return;
                        break;
                    }
                    if (data.Arrested)
                    {
                        if (data.m_visitBuilding == (ushort)0)
                        {
                            data.Arrested = false;
                            break;
                        }
                        break;
                    }
                    if (!data.Collapsed)
                    {
                        if (data.Sick)
                        {
                            if (data.m_visitBuilding == (ushort)0)
                            {
                                data.CurrentLocation = Citizen.Location.Home;
                                break;
                            }
                            if (data.m_vehicle == (ushort)0)
                            {
                                switch (Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)data.m_visitBuilding].Info.m_class.m_service)
                                {
                                    case ItemClass.Service.HealthCare:
                                    case ItemClass.Service.Disaster:
                                        break;
                                    default:
                                        if (!this.FindHospital(citizenID, data.m_visitBuilding, TransferManager.TransferReason.Sick))
                                            return;
                                        break;
                                }
                            }
                            else
                                break;
                        }
                        else
                        {
                            BuildingManager instance2 = Singleton<BuildingManager>.instance;
                            ItemClass.Service currentService = ItemClass.Service.None;
                            if (data.m_visitBuilding != (ushort)0)
                                currentService = instance2.m_buildings.m_buffer[(int)data.m_visitBuilding].Info.m_class.m_service;
                            switch (currentService)
                            {
                                case ItemClass.Service.HealthCare:
                                case ItemClass.Service.PoliceDepartment:
                                    if (data.m_homeBuilding != (ushort)0 && data.m_vehicle == (ushort)0)
                                    {
                                        data.m_flags &= Citizen.Flags.Unemployed | Citizen.Flags.Wealth | Citizen.Flags.Location | Citizen.Flags.NoElectricity | Citizen.Flags.NoWater | Citizen.Flags.NoSewage | Citizen.Flags.BadHealth | Citizen.Flags.Created | Citizen.Flags.Tourist | Citizen.Flags.Sick | Citizen.Flags.Dead | Citizen.Flags.Student | Citizen.Flags.MovingIn | Citizen.Flags.DummyTraffic | Citizen.Flags.Criminal | Citizen.Flags.Arrested | Citizen.Flags.Collapsed | Citizen.Flags.Education1 | Citizen.Flags.Education2 | Citizen.Flags.Education3 | Citizen.Flags.NeedGoods | Citizen.Flags.Original | Citizen.Flags.CustomName;
                                        this.StartMoving(citizenID, ref data, data.m_visitBuilding, data.m_homeBuilding);
                                        data.SetVisitplace(citizenID, (ushort)0, 0U);
                                        break;
                                    }
                                    break;
                                case ItemClass.Service.Disaster:
                                    if ((instance2.m_buildings.m_buffer[(int)data.m_visitBuilding].m_flags & Building.Flags.Downgrading) != Building.Flags.None && data.m_homeBuilding != (ushort)0 && data.m_vehicle == (ushort)0)
                                    {
                                        data.m_flags &= Citizen.Flags.Unemployed | Citizen.Flags.Wealth | Citizen.Flags.Location | Citizen.Flags.NoElectricity | Citizen.Flags.NoWater | Citizen.Flags.NoSewage | Citizen.Flags.BadHealth | Citizen.Flags.Created | Citizen.Flags.Tourist | Citizen.Flags.Sick | Citizen.Flags.Dead | Citizen.Flags.Student | Citizen.Flags.MovingIn | Citizen.Flags.DummyTraffic | Citizen.Flags.Criminal | Citizen.Flags.Arrested | Citizen.Flags.Collapsed | Citizen.Flags.Education1 | Citizen.Flags.Education2 | Citizen.Flags.Education3 | Citizen.Flags.NeedGoods | Citizen.Flags.Original | Citizen.Flags.CustomName;
                                        this.StartMoving(citizenID, ref data, data.m_visitBuilding, data.m_homeBuilding);
                                        data.SetVisitplace(citizenID, (ushort)0, 0U);
                                        break;
                                    }
                                    break;
                                default:
                                    if (data.m_visitBuilding == (ushort)0)
                                    {
                                        data.CurrentLocation = Citizen.Location.Home;
                                        break;
                                    }
                                    if ((instance2.m_buildings.m_buffer[(int)data.m_visitBuilding].m_flags & Building.Flags.Evacuating) != Building.Flags.None)
                                    {
                                        if (data.m_vehicle == (ushort)0)
                                        {
                                            this.FindEvacuationPlace(citizenID, data.m_visitBuilding, this.GetEvacuationReason(data.m_visitBuilding));
                                            break;
                                        }
                                        break;
                                    }
                                    if ((data.m_flags & Citizen.Flags.NeedGoods) != Citizen.Flags.None)
                                    {
                                        BuildingInfo info = instance2.m_buildings.m_buffer[(int)data.m_visitBuilding].Info;
                                        int amountDelta = -100;
                                        info.m_buildingAI.ModifyMaterialBuffer(data.m_visitBuilding, ref instance2.m_buildings.m_buffer[(int)data.m_visitBuilding], TransferManager.TransferReason.Shopping, ref amountDelta);
                                        break;
                                    }
                                    ushort eventIndex2 = instance2.m_buildings.m_buffer[(int)data.m_visitBuilding].m_eventIndex;
                                    if (eventIndex2 != (ushort)0)
                                    {
                                        if ((Singleton<EventManager>.instance.m_events.m_buffer[(int)eventIndex2].m_flags & (EventData.Flags.Preparing | EventData.Flags.Active | EventData.Flags.Ready)) == EventData.Flags.None && data.m_homeBuilding != (ushort)0 && data.m_vehicle == (ushort)0)
                                        {
                                            data.m_flags &= Citizen.Flags.Unemployed | Citizen.Flags.Wealth | Citizen.Flags.Location | Citizen.Flags.NoElectricity | Citizen.Flags.NoWater | Citizen.Flags.NoSewage | Citizen.Flags.BadHealth | Citizen.Flags.Created | Citizen.Flags.Tourist | Citizen.Flags.Sick | Citizen.Flags.Dead | Citizen.Flags.Student | Citizen.Flags.MovingIn | Citizen.Flags.DummyTraffic | Citizen.Flags.Criminal | Citizen.Flags.Arrested | Citizen.Flags.Collapsed | Citizen.Flags.Education1 | Citizen.Flags.Education2 | Citizen.Flags.Education3 | Citizen.Flags.NeedGoods | Citizen.Flags.Original | Citizen.Flags.CustomName;
                                            this.StartMoving(citizenID, ref data, data.m_visitBuilding, data.m_homeBuilding);
                                            data.SetVisitplace(citizenID, (ushort)0, 0U);
                                            break;
                                        }
                                        break;
                                    }
                                    if ((data.m_instance != (ushort)0 || this.DoRandomMove()) && (Singleton<SimulationManager>.instance.m_randomizer.Int32(40U) < 10 && data.m_homeBuilding != (ushort)0) && data.m_vehicle == (ushort)0)
                                    {
                                        data.m_flags &= Citizen.Flags.Unemployed | Citizen.Flags.Wealth | Citizen.Flags.Location | Citizen.Flags.NoElectricity | Citizen.Flags.NoWater | Citizen.Flags.NoSewage | Citizen.Flags.BadHealth | Citizen.Flags.Created | Citizen.Flags.Tourist | Citizen.Flags.Sick | Citizen.Flags.Dead | Citizen.Flags.Student | Citizen.Flags.MovingIn | Citizen.Flags.DummyTraffic | Citizen.Flags.Criminal | Citizen.Flags.Arrested | Citizen.Flags.Collapsed | Citizen.Flags.Education1 | Citizen.Flags.Education2 | Citizen.Flags.Education3 | Citizen.Flags.NeedGoods | Citizen.Flags.Original | Citizen.Flags.CustomName;
                                        this.AttemptAutodidact(ref data, currentService);
                                        this.StartMoving(citizenID, ref data, data.m_visitBuilding, data.m_homeBuilding);
                                        data.SetVisitplace(citizenID, (ushort)0, 0U);
                                        break;
                                    }
                                    break;
                            }
                        }
                    }
                    else
                        break;
                case Citizen.Location.Moving:
                    if (data.Dead)
                    {
                        if (data.m_vehicle == (ushort)0)
                        {
                            Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
                            return;
                        }
                        if (data.m_homeBuilding != (ushort)0)
                            data.SetHome(citizenID, (ushort)0, 0U);
                        if (data.m_workBuilding != (ushort)0)
                            data.SetWorkplace(citizenID, (ushort)0, 0U);
                        if (data.m_visitBuilding != (ushort)0)
                        {
                            data.SetVisitplace(citizenID, (ushort)0, 0U);
                            break;
                        }
                        break;
                    }
                    if (data.m_vehicle == (ushort)0 && data.m_instance == (ushort)0)
                    {
                        if (data.m_visitBuilding != (ushort)0)
                            data.SetVisitplace(citizenID, (ushort)0, 0U);
                        data.CurrentLocation = Citizen.Location.Home;
                        data.Arrested = false;
                        break;
                    }
                    if (data.m_instance != (ushort)0 && (Singleton<CitizenManager>.instance.m_instances.m_buffer[(int)data.m_instance].m_flags & (CitizenInstance.Flags.TargetIsNode | CitizenInstance.Flags.OnTour)) == (CitizenInstance.Flags.TargetIsNode | CitizenInstance.Flags.OnTour) && (Singleton<SimulationManager>.instance.m_randomizer.Int32(40U) < 10 && data.m_homeBuilding != (ushort)0))
                    {
                        data.m_flags &= Citizen.Flags.Unemployed | Citizen.Flags.Wealth | Citizen.Flags.Location | Citizen.Flags.NoElectricity | Citizen.Flags.NoWater | Citizen.Flags.NoSewage | Citizen.Flags.BadHealth | Citizen.Flags.Created | Citizen.Flags.Tourist | Citizen.Flags.Sick | Citizen.Flags.Dead | Citizen.Flags.Student | Citizen.Flags.MovingIn | Citizen.Flags.DummyTraffic | Citizen.Flags.Criminal | Citizen.Flags.Arrested | Citizen.Flags.Collapsed | Citizen.Flags.Education1 | Citizen.Flags.Education2 | Citizen.Flags.Education3 | Citizen.Flags.NeedGoods | Citizen.Flags.Original | Citizen.Flags.CustomName;
                        this.StartMoving(citizenID, ref data, (ushort)0, data.m_homeBuilding);
                        break;
                    }
                    break;
                default:
                    break;
            }
            data.m_flags &= Citizen.Flags.Unemployed | Citizen.Flags.Wealth | Citizen.Flags.Location | Citizen.Flags.NoElectricity | Citizen.Flags.NoWater | Citizen.Flags.NoSewage | Citizen.Flags.BadHealth | Citizen.Flags.Created | Citizen.Flags.Tourist | Citizen.Flags.Sick | Citizen.Flags.Dead | Citizen.Flags.Student | Citizen.Flags.MovingIn | Citizen.Flags.DummyTraffic | Citizen.Flags.Criminal | Citizen.Flags.Arrested | Citizen.Flags.Evacuating | Citizen.Flags.Collapsed | Citizen.Flags.Education1 | Citizen.Flags.Education2 | Citizen.Flags.Education3 | Citizen.Flags.Original | Citizen.Flags.CustomName;
        }
    }

    private void AttemptAutodidact(ref Citizen data, ItemClass.Service currentService)
  {
    if (currentService != ItemClass.Service.Education)
      return;
    LibraryAI buildingAi = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int) data.m_visitBuilding].Info.m_buildingAI as LibraryAI;
    if (buildingAi == null)
      return;
    Citizen.AgeGroup ageGroup = Citizen.GetAgeGroup(data.Age);
    int num = Singleton<SimulationManager>.instance.m_randomizer.Int32(1000U);
    switch (ageGroup)
    {
      case Citizen.AgeGroup.Child:
        if (data.Education1 || (double) num > (double) buildingAi.m_percentageChanceElementaryEducation * 10.0)
          break;
        data.Education1 = true;
        break;
      case Citizen.AgeGroup.Teen:
        if (!data.Education1)
        {
          if ((double) num > (double) buildingAi.m_percentageChanceElementaryEducation * 10.0)
            break;
          data.Education1 = true;
          break;
        }
        if (!data.Education1 || data.Education2 || (double) num > (double) buildingAi.m_percentageChanceHighschoolEducation * 10.0)
          break;
        data.Education2 = true;
        break;
      case Citizen.AgeGroup.Young:
      case Citizen.AgeGroup.Adult:
      case Citizen.AgeGroup.Senior:
        if (!data.Education1)
        {
          if ((double) num > (double) buildingAi.m_percentageChanceElementaryEducation * 10.0)
            break;
          data.Education1 = true;
          break;
        }
        if (data.Education1 && !data.Education2)
        {
          if ((double) num > (double) buildingAi.m_percentageChanceHighschoolEducation * 10.0)
            break;
          data.Education2 = true;
          break;
        }
        if (!data.Education1 || !data.Education2 || (data.Education3 || (double) num > (double) buildingAi.m_percentageChanceUniversityEducation * 10.0))
          break;
        data.Education3 = true;
        break;
    }
  }

  public bool CanMakeBabies(uint citizenID, ref Citizen data)
  {
    return !data.Dead && Citizen.GetAgeGroup(data.Age) == Citizen.AgeGroup.Adult && (data.m_flags & Citizen.Flags.MovingIn) == Citizen.Flags.None;
  }

  public void TryMoveAwayFromHome(uint citizenID, ref Citizen data)
  {
    if (data.Dead || data.m_homeBuilding == (ushort) 0)
      return;
    Citizen.AgeGroup ageGroup = Citizen.GetAgeGroup(data.Age);
    switch (ageGroup)
    {
      case Citizen.AgeGroup.Young:
      case Citizen.AgeGroup.Adult:
        TransferManager.TransferOffer offer = new TransferManager.TransferOffer();
        offer.Priority = ageGroup != Citizen.AgeGroup.Young ? Singleton<SimulationManager>.instance.m_randomizer.Int32(2, 4) : 1;
        offer.Citizen = citizenID;
        offer.Position = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int) data.m_homeBuilding].m_position;
        offer.Amount = 1;
        offer.Active = true;
        if (Singleton<SimulationManager>.instance.m_randomizer.Int32(2U) == 0)
        {
          switch (data.EducationLevel)
          {
            case Citizen.Education.Uneducated:
              Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single0, offer);
              return;
            case Citizen.Education.OneSchool:
              Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single1, offer);
              return;
            case Citizen.Education.TwoSchools:
              Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single2, offer);
              return;
            case Citizen.Education.ThreeSchools:
              Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single3, offer);
              return;
            default:
              return;
          }
        }
        else
        {
          switch (data.EducationLevel)
          {
            case Citizen.Education.Uneducated:
              Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single0B, offer);
              return;
            case Citizen.Education.OneSchool:
              Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single1B, offer);
              return;
            case Citizen.Education.TwoSchools:
              Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single2B, offer);
              return;
            case Citizen.Education.ThreeSchools:
              Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single3B, offer);
              return;
            default:
              return;
          }
        }
    }
  }

  public void TryMoveFamily(uint citizenID, ref Citizen data, int familySize)
  {
    if (data.Dead || data.m_homeBuilding == (ushort) 0)
      return;
    TransferManager.TransferOffer offer = new TransferManager.TransferOffer();
    offer.Priority = Singleton<SimulationManager>.instance.m_randomizer.Int32(1, 7);
    offer.Citizen = citizenID;
    offer.Position = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int) data.m_homeBuilding].m_position;
    offer.Amount = 1;
    offer.Active = true;
    if (familySize == 1)
    {
      if (Singleton<SimulationManager>.instance.m_randomizer.Int32(2U) == 0)
      {
        switch (data.EducationLevel)
        {
          case Citizen.Education.Uneducated:
            Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single0, offer);
            break;
          case Citizen.Education.OneSchool:
            Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single1, offer);
            break;
          case Citizen.Education.TwoSchools:
            Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single2, offer);
            break;
          case Citizen.Education.ThreeSchools:
            Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single3, offer);
            break;
        }
      }
      else
      {
        switch (data.EducationLevel)
        {
          case Citizen.Education.Uneducated:
            Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single0B, offer);
            break;
          case Citizen.Education.OneSchool:
            Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single1B, offer);
            break;
          case Citizen.Education.TwoSchools:
            Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single2B, offer);
            break;
          case Citizen.Education.ThreeSchools:
            Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single3B, offer);
            break;
        }
      }
    }
    else
    {
      switch (data.EducationLevel)
      {
        case Citizen.Education.Uneducated:
          Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Family0, offer);
          break;
        case Citizen.Education.OneSchool:
          Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Family1, offer);
          break;
        case Citizen.Education.TwoSchools:
          Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Family2, offer);
          break;
        case Citizen.Education.ThreeSchools:
          Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Family3, offer);
          break;
      }
    }
  }

  public void TryFindPartner(uint citizenID, ref Citizen data)
  {
    if (data.Dead || data.m_homeBuilding == (ushort) 0)
      return;
    Citizen.AgeGroup ageGroup = Citizen.GetAgeGroup(data.Age);
    TransferManager.TransferReason material = TransferManager.TransferReason.None;
    switch (ageGroup)
    {
      case Citizen.AgeGroup.Young:
        material = TransferManager.TransferReason.PartnerYoung;
        break;
      case Citizen.AgeGroup.Adult:
        material = TransferManager.TransferReason.PartnerAdult;
        break;
    }
    if (ageGroup != Citizen.AgeGroup.Young && ageGroup != Citizen.AgeGroup.Adult)
      return;
    Vector3 position = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int) data.m_homeBuilding].m_position;
    TransferManager.TransferOffer offer = new TransferManager.TransferOffer();
    offer.Priority = Singleton<SimulationManager>.instance.m_randomizer.Int32(8U);
    offer.Citizen = citizenID;
    offer.Position = position;
    offer.Amount = 1;
    offer.Active = Singleton<SimulationManager>.instance.m_randomizer.Int32(2U) == 0;
    bool flag = Singleton<SimulationManager>.instance.m_randomizer.Int32(100U) < 5;
    if (Citizen.GetGender(citizenID) == Citizen.Gender.Female != flag)
      Singleton<TransferManager>.instance.AddIncomingOffer(material, offer);
    else
      Singleton<TransferManager>.instance.AddOutgoingOffer(material, offer);
  }

  private bool FindHospital(
    uint citizenID,
    ushort sourceBuilding,
    TransferManager.TransferReason reason)
  {
    if (reason == TransferManager.TransferReason.Dead)
    {
      if (Singleton<UnlockManager>.instance.Unlocked(UnlockManager.Feature.DeathCare))
        return true;
      Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
      return false;
    }
    if (Singleton<UnlockManager>.instance.Unlocked(ItemClass.Service.HealthCare))
    {
      BuildingManager instance1 = Singleton<BuildingManager>.instance;
      DistrictManager instance2 = Singleton<DistrictManager>.instance;
      Vector3 position = instance1.m_buildings.m_buffer[(int) sourceBuilding].m_position;
      byte district = instance2.GetDistrict(position);
      DistrictPolicies.Services servicePolicies = instance2.m_districts.m_buffer[(int) district].m_servicePolicies;
      TransferManager.TransferOffer offer = new TransferManager.TransferOffer();
      offer.Priority = 6;
      offer.Citizen = citizenID;
      offer.Position = position;
      offer.Amount = 1;
      if ((servicePolicies & DistrictPolicies.Services.HelicopterPriority) != DistrictPolicies.Services.None)
      {
        instance2.m_districts.m_buffer[(int) district].m_servicePoliciesEffect |= DistrictPolicies.Services.HelicopterPriority;
        offer.Active = false;
        Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Sick2, offer);
      }
      else if ((instance1.m_buildings.m_buffer[(int) sourceBuilding].m_flags & Building.Flags.RoadAccessFailed) != Building.Flags.None || Singleton<SimulationManager>.instance.m_randomizer.Int32(20U) == 0)
      {
        offer.Active = false;
        Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Sick2, offer);
      }
      else
      {
        offer.Active = Singleton<SimulationManager>.instance.m_randomizer.Int32(2U) == 0;
        Singleton<TransferManager>.instance.AddOutgoingOffer(reason, offer);
      }
      return true;
    }
    Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
    return false;
  }

  public override void StartTransfer(
    uint citizenID,
    ref Citizen data,
    TransferManager.TransferReason reason,
    TransferManager.TransferOffer offer)
  {
    if (data.m_flags == Citizen.Flags.None || data.Dead && reason != TransferManager.TransferReason.Dead)
      return;
    switch (reason)
    {
      case TransferManager.TransferReason.Single0B:
      case TransferManager.TransferReason.Single1B:
      case TransferManager.TransferReason.Single2B:
      case TransferManager.TransferReason.Single3B:
label_34:
        data.SetHome(citizenID, offer.Building, 0U);
        if (data.m_homeBuilding != (ushort) 0 || data.CurrentLocation == Citizen.Location.Visit && (data.m_flags & Citizen.Flags.Evacuating) != Citizen.Flags.None)
          break;
        Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
        break;
      case TransferManager.TransferReason.ShoppingB:
      case TransferManager.TransferReason.ShoppingC:
      case TransferManager.TransferReason.ShoppingD:
      case TransferManager.TransferReason.ShoppingE:
      case TransferManager.TransferReason.ShoppingF:
      case TransferManager.TransferReason.ShoppingG:
      case TransferManager.TransferReason.ShoppingH:
label_25:
        if (data.m_homeBuilding == (ushort) 0 || data.Sick)
          break;
        data.m_flags &= Citizen.Flags.Unemployed | Citizen.Flags.Wealth | Citizen.Flags.Location | Citizen.Flags.NoElectricity | Citizen.Flags.NoWater | Citizen.Flags.NoSewage | Citizen.Flags.BadHealth | Citizen.Flags.Created | Citizen.Flags.Tourist | Citizen.Flags.Sick | Citizen.Flags.Dead | Citizen.Flags.Student | Citizen.Flags.MovingIn | Citizen.Flags.DummyTraffic | Citizen.Flags.Criminal | Citizen.Flags.Arrested | Citizen.Flags.Collapsed | Citizen.Flags.Education1 | Citizen.Flags.Education2 | Citizen.Flags.Education3 | Citizen.Flags.NeedGoods | Citizen.Flags.Original | Citizen.Flags.CustomName;
        if (!this.StartMoving(citizenID, ref data, (ushort) 0, offer))
          break;
        data.SetVisitplace(citizenID, offer.Building, 0U);
        CitizenManager instance1 = Singleton<CitizenManager>.instance;
        BuildingManager instance2 = Singleton<BuildingManager>.instance;
        uint containingUnit = data.GetContainingUnit(citizenID, instance2.m_buildings.m_buffer[(int) data.m_homeBuilding].m_citizenUnits, CitizenUnit.Flags.Home);
        if (containingUnit == 0U)
          break;
        instance1.m_units.m_buffer[(IntPtr) containingUnit].m_goods += (ushort) 100;
        break;
      case TransferManager.TransferReason.EntertainmentB:
      case TransferManager.TransferReason.EntertainmentC:
      case TransferManager.TransferReason.EntertainmentD:
label_30:
        if (data.m_homeBuilding == (ushort) 0 || data.Sick)
          break;
        data.m_flags &= Citizen.Flags.Unemployed | Citizen.Flags.Wealth | Citizen.Flags.Location | Citizen.Flags.NoElectricity | Citizen.Flags.NoWater | Citizen.Flags.NoSewage | Citizen.Flags.BadHealth | Citizen.Flags.Created | Citizen.Flags.Tourist | Citizen.Flags.Sick | Citizen.Flags.Dead | Citizen.Flags.Student | Citizen.Flags.MovingIn | Citizen.Flags.DummyTraffic | Citizen.Flags.Criminal | Citizen.Flags.Arrested | Citizen.Flags.Collapsed | Citizen.Flags.Education1 | Citizen.Flags.Education2 | Citizen.Flags.Education3 | Citizen.Flags.NeedGoods | Citizen.Flags.Original | Citizen.Flags.CustomName;
        if (!this.StartMoving(citizenID, ref data, (ushort) 0, offer))
          break;
        data.SetVisitplace(citizenID, offer.Building, 0U);
        break;
      case TransferManager.TransferReason.EvacuateA:
      case TransferManager.TransferReason.EvacuateB:
      case TransferManager.TransferReason.EvacuateC:
      case TransferManager.TransferReason.EvacuateD:
      case TransferManager.TransferReason.EvacuateVipA:
      case TransferManager.TransferReason.EvacuateVipB:
      case TransferManager.TransferReason.EvacuateVipC:
      case TransferManager.TransferReason.EvacuateVipD:
        data.m_flags |= Citizen.Flags.Evacuating;
        if (this.StartMoving(citizenID, ref data, (ushort) 0, offer))
        {
          data.SetVisitplace(citizenID, offer.Building, 0U);
          break;
        }
        data.SetVisitplace(citizenID, offer.Building, 0U);
        if (data.m_visitBuilding == (ushort) 0 || (int) data.m_visitBuilding != (int) offer.Building)
          break;
        data.CurrentLocation = Citizen.Location.Visit;
        break;
      default:
        switch (reason - 2)
        {
          case TransferManager.TransferReason.Garbage:
            if (!data.Sick)
              return;
            data.m_flags &= Citizen.Flags.Unemployed | Citizen.Flags.Wealth | Citizen.Flags.Location | Citizen.Flags.NoElectricity | Citizen.Flags.NoWater | Citizen.Flags.NoSewage | Citizen.Flags.BadHealth | Citizen.Flags.Created | Citizen.Flags.Tourist | Citizen.Flags.Sick | Citizen.Flags.Dead | Citizen.Flags.Student | Citizen.Flags.MovingIn | Citizen.Flags.DummyTraffic | Citizen.Flags.Criminal | Citizen.Flags.Arrested | Citizen.Flags.Collapsed | Citizen.Flags.Education1 | Citizen.Flags.Education2 | Citizen.Flags.Education3 | Citizen.Flags.NeedGoods | Citizen.Flags.Original | Citizen.Flags.CustomName;
            if (!this.StartMoving(citizenID, ref data, (ushort) 0, offer))
              return;
            data.SetVisitplace(citizenID, offer.Building, 0U);
            return;
          case TransferManager.TransferReason.Crime:
            if (!data.Dead)
              return;
            data.SetVisitplace(citizenID, offer.Building, 0U);
            if (data.m_visitBuilding == (ushort) 0)
              return;
            data.CurrentLocation = Citizen.Location.Visit;
            return;
          case TransferManager.TransferReason.Sick:
          case TransferManager.TransferReason.Dead:
          case TransferManager.TransferReason.Worker0:
          case TransferManager.TransferReason.Worker1:
            if (data.m_workBuilding != (ushort) 0)
              return;
            data.SetWorkplace(citizenID, offer.Building, 0U);
            return;
          case TransferManager.TransferReason.Worker2:
            if (data.m_workBuilding != (ushort) 0 || data.EducationLevel != Citizen.Education.Uneducated)
              return;
            data.SetStudentplace(citizenID, offer.Building, 0U);
            return;
          case TransferManager.TransferReason.Worker3:
            if (data.m_workBuilding != (ushort) 0 || data.EducationLevel != Citizen.Education.OneSchool)
              return;
            data.SetStudentplace(citizenID, offer.Building, 0U);
            return;
          case TransferManager.TransferReason.Student1:
            if (data.m_workBuilding != (ushort) 0 || data.EducationLevel != Citizen.Education.TwoSchools)
              return;
            data.SetStudentplace(citizenID, offer.Building, 0U);
            return;
          case TransferManager.TransferReason.Student2:
            return;
          case TransferManager.TransferReason.Student3:
            return;
          case TransferManager.TransferReason.Fire:
            return;
          case TransferManager.TransferReason.Bus:
            return;
          case TransferManager.TransferReason.Oil:
            return;
          case TransferManager.TransferReason.Ore:
            return;
          case TransferManager.TransferReason.Logs:
            return;
          case TransferManager.TransferReason.Grain:
            return;
          case TransferManager.TransferReason.Goods:
            return;
          case TransferManager.TransferReason.PassengerTrain:
          case TransferManager.TransferReason.Coal:
          case TransferManager.TransferReason.Family0:
          case TransferManager.TransferReason.Family1:
            if (data.m_homeBuilding == (ushort) 0 || offer.Building == (ushort) 0)
              return;
            uint citizenUnit1 = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int) data.m_homeBuilding].FindCitizenUnit(CitizenUnit.Flags.Home, citizenID);
            if (citizenUnit1 == 0U)
              return;
            this.MoveFamily(citizenUnit1, ref Singleton<CitizenManager>.instance.m_units.m_buffer[(IntPtr) citizenUnit1], offer.Building);
            return;
          case TransferManager.TransferReason.Family2:
          case TransferManager.TransferReason.Family3:
          case TransferManager.TransferReason.Single0:
          case TransferManager.TransferReason.Single1:
            goto label_34;
          case TransferManager.TransferReason.Single2:
          case TransferManager.TransferReason.Single3:
            uint citizen = offer.Citizen;
            if (citizen == 0U)
              return;
            CitizenManager instance3 = Singleton<CitizenManager>.instance;
            BuildingManager instance4 = Singleton<BuildingManager>.instance;
            ushort homeBuilding = instance3.m_citizens.m_buffer[(IntPtr) citizen].m_homeBuilding;
            if (homeBuilding == (ushort) 0 || instance3.m_citizens.m_buffer[(IntPtr) citizen].Dead)
              return;
            uint citizenUnit2 = instance4.m_buildings.m_buffer[(int) homeBuilding].FindCitizenUnit(CitizenUnit.Flags.Home, citizen);
            if (citizenUnit2 == 0U)
              return;
            data.SetHome(citizenID, (ushort) 0, citizenUnit2);
            data.m_family = instance3.m_citizens.m_buffer[(IntPtr) citizen].m_family;
            return;
          case TransferManager.TransferReason.PartnerYoung:
            goto label_25;
          case TransferManager.TransferReason.PartnerAdult:
            return;
          case TransferManager.TransferReason.Shopping:
            return;
          case TransferManager.TransferReason.Petrol:
            return;
          case TransferManager.TransferReason.Food:
            return;
          case TransferManager.TransferReason.LeaveCity0:
            return;
          case TransferManager.TransferReason.LeaveCity1:
            goto label_30;
          default:
            return;
        }
    }
  }

  private void MoveFamily(uint homeID, ref CitizenUnit data, ushort targetBuilding)
  {
    BuildingManager instance1 = Singleton<BuildingManager>.instance;
    CitizenManager instance2 = Singleton<CitizenManager>.instance;
    uint unitID = 0;
    if (targetBuilding != (ushort) 0)
      unitID = instance1.m_buildings.m_buffer[(int) targetBuilding].GetEmptyCitizenUnit(CitizenUnit.Flags.Home);
    for (int index = 0; index < 5; ++index)
    {
      uint citizen = data.GetCitizen(index);
      if (citizen != 0U && !instance2.m_citizens.m_buffer[(IntPtr) citizen].Dead)
      {
        instance2.m_citizens.m_buffer[(IntPtr) citizen].SetHome(citizen, (ushort) 0, unitID);
        if (instance2.m_citizens.m_buffer[(IntPtr) citizen].m_homeBuilding == (ushort) 0)
          instance2.ReleaseCitizen(citizen);
      }
    }
  }

  public override void SetSource(
    ushort instanceID,
    ref CitizenInstance data,
    ushort sourceBuilding)
  {
    if ((int) sourceBuilding != (int) data.m_sourceBuilding)
    {
      if (data.m_sourceBuilding != (ushort) 0)
        Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int) data.m_sourceBuilding].RemoveSourceCitizen(instanceID, ref data);
      data.m_sourceBuilding = sourceBuilding;
      if (data.m_sourceBuilding != (ushort) 0)
        Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int) data.m_sourceBuilding].AddSourceCitizen(instanceID, ref data);
    }
    if (sourceBuilding == (ushort) 0)
      return;
    BuildingManager instance = Singleton<BuildingManager>.instance;
    BuildingInfo info = instance.m_buildings.m_buffer[(int) sourceBuilding].Info;
    data.Unspawn(instanceID);
    Randomizer randomizer = new Randomizer((int) instanceID);
    Vector3 position;
    Vector3 target;
    info.m_buildingAI.CalculateSpawnPosition(sourceBuilding, ref instance.m_buildings.m_buffer[(int) sourceBuilding], ref randomizer, this.m_info, out position, out target);
    Quaternion quaternion = Quaternion.identity;
    Vector3 forward = target - position;
    if ((double) forward.sqrMagnitude > 0.00999999977648258)
      quaternion = Quaternion.LookRotation(forward);
    data.m_frame0.m_velocity = Vector3.zero;
    data.m_frame0.m_position = position;
    data.m_frame0.m_rotation = quaternion;
    data.m_frame1 = data.m_frame0;
    data.m_frame2 = data.m_frame0;
    data.m_frame3 = data.m_frame0;
    data.m_targetPos = new Vector4(target.x, target.y, target.z, 1f);
    ushort eventIndex = 0;
    if (data.m_citizen != 0U && (int) Singleton<CitizenManager>.instance.m_citizens.m_buffer[(IntPtr) data.m_citizen].m_workBuilding != (int) sourceBuilding)
      eventIndex = instance.m_buildings.m_buffer[(int) sourceBuilding].m_eventIndex;
    Color32 eventCitizenColor = Singleton<EventManager>.instance.GetEventCitizenColor(eventIndex, data.m_citizen);
    if (eventCitizenColor.a != byte.MaxValue)
      return;
    data.m_color = eventCitizenColor;
    data.m_flags |= CitizenInstance.Flags.CustomColor;
  }


    // this does more than just set the targetBuilding. It also when appropriate calls StartPathFind
    // there is also a 3-parameter version that calls this with targetIsNode set to false
  public override void SetTarget(
    ushort instanceID,
    ref CitizenInstance data,
    ushort targetIndex,
    bool targetIsNode)
  {
        // at random depending in some way on time of day set or clear CannotUseTaxi (not used in the rest of this method)
    int dayTimeFrame = (int) Singleton<SimulationManager>.instance.m_dayTimeFrame;
    int daytimeFrames = (int) SimulationManager.DAYTIME_FRAMES;
    int num1 = Mathf.Max(daytimeFrames >> 2, Mathf.Abs(dayTimeFrame - (daytimeFrames >> 1)));
    if (Singleton<SimulationManager>.instance.m_randomizer.Int32((uint) daytimeFrames >> 1) < num1)
      data.m_flags &= ~CitizenInstance.Flags.CannotUseTaxi;
    else
      data.m_flags |= CitizenInstance.Flags.CannotUseTaxi;
    // in all cases clear CannotUseTransport (for later, not used again here)
    data.m_flags &= ~CitizenInstance.Flags.CannotUseTransport;
    // check for a new (changed) target or changed targetIsNode switch and housekeep for that
    if ((int) targetIndex != (int) data.m_targetBuilding || targetIsNode != ((data.m_flags & CitizenInstance.Flags.TargetIsNode) != CitizenInstance.Flags.None))
    {
            // if came here with an existing targetBuilding ... clear expected visitor from Building (or Node)
      if (data.m_targetBuilding != (ushort) 0)
      {
                // and if the CI has TargetIsNode set when came here
        if ((data.m_flags & CitizenInstance.Flags.TargetIsNode) != CitizenInstance.Flags.None)
        {
                    // remove the CI from the Node's target list (ie tell the node that CI is not coming)
          Singleton<NetManager>.instance.m_nodes.m_buffer[(int) data.m_targetBuilding].RemoveTargetCitizen(instanceID, ref data);
                    // set up num2 as either zero or - if call param targetIsNode is true - the Node's transport line for testing below
          ushort num2 = 0;
          if (targetIsNode)
            num2 = Singleton<NetManager>.instance.m_nodes.m_buffer[(int) data.m_targetBuilding].m_transportLine;
            // if the CI is OnTour (ie if target is not home, work or outside)
            // clear OnTour IF called with targetIsNode true, target Node has a transport line but it is vehicleType.None
          if ((data.m_flags & CitizenInstance.Flags.OnTour) != CitizenInstance.Flags.None)
          {
            ushort transportLine = Singleton<NetManager>.instance.m_nodes.m_buffer[(int) data.m_targetBuilding].m_transportLine;
            uint citizen = data.m_citizen;
                        // if the Node's transportLine exists nonzero, and if SetTarget was called with targetIsMode true, and if CI has a valid Citizen
            if (transportLine != (ushort) 0 && (int) transportLine != (int) num2 && citizen != 0U)
            {
              TransportInfo info = Singleton<TransportManager>.instance.m_lines.m_buffer[(int) transportLine].Info;
              if (info != null && info.m_vehicleType == VehicleInfo.VehicleType.None)
                data.m_flags &= ~CitizenInstance.Flags.OnTour;
            }
          }
          // if called with targetIsNode parameter set false, reset the CI flag to false to match
          if (!targetIsNode)
            data.m_flags &= ~CitizenInstance.Flags.TargetIsNode;
        }
        else
                    // if the CI's target on call was a Building (not a Node), remove them from the Building's list of expected visitors
          Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int) data.m_targetBuilding].RemoveTargetCitizen(instanceID, ref data);
      }
      // then always here
      // reset CI's target per calling parameter
      data.m_targetBuilding = targetIndex;
            // if routine called with targetIsNode true, reflect it in the CI flags
      if (targetIsNode)
        data.m_flags |= CitizenInstance.Flags.TargetIsNode;
      // and tell the target building or node to expect this guest
      // also set CI.m_targetSeed (0 to 255) - the purpose of which I do not know
      if (data.m_targetBuilding != (ushort) 0)
      {
        if ((data.m_flags & CitizenInstance.Flags.TargetIsNode) != CitizenInstance.Flags.None)
          Singleton<NetManager>.instance.m_nodes.m_buffer[(int) data.m_targetBuilding].AddTargetCitizen(instanceID, ref data);
        else
          Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int) data.m_targetBuilding].AddTargetCitizen(instanceID, ref data);
        data.m_targetSeed = (byte) Singleton<SimulationManager>.instance.m_randomizer.Int32(256U);
      }
    }

    // then in all cases
    if ((data.m_flags & CitizenInstance.Flags.TargetIsNode) == CitizenInstance.Flags.None && this.IsRoadConnection(targetIndex) || this.IsRoadConnection(data.m_sourceBuilding))
      data.m_flags |= CitizenInstance.Flags.BorrowCar;
    else
      data.m_flags &= ~CitizenInstance.Flags.BorrowCar;
    // then always check if citizenInstance needs a CustomColor (in m_color) according to event (whatever that is)
    if (targetIndex != (ushort) 0 && (data.m_flags & (CitizenInstance.Flags.Character | CitizenInstance.Flags.TargetIsNode)) == CitizenInstance.Flags.None)
    {
      ushort eventIndex = 0;
      if (data.m_citizen != 0U && (int) Singleton<CitizenManager>.instance.m_citizens.m_buffer[(IntPtr) data.m_citizen].m_workBuilding != (int) targetIndex)
        eventIndex = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int) targetIndex].m_eventIndex;
      Color32 eventCitizenColor = Singleton<EventManager>.instance.GetEventCitizenColor(eventIndex, data.m_citizen);
      if (eventCitizenColor.a == byte.MaxValue)
      {
        data.m_color = eventCitizenColor;
        data.m_flags |= CitizenInstance.Flags.CustomColor;
      }
    }
    //
    // then always call StartPathFind
    //
    if (this.StartPathFind(instanceID, ref data))
      return;
    data.Unspawn(instanceID);       // Unspawn the citizenInstance if no path is found
  }

  public override void BuildingRelocated(
    ushort instanceID,
    ref CitizenInstance data,
    ushort building)
  {
    base.BuildingRelocated(instanceID, ref data, building);
    if ((int) building != (int) data.m_targetBuilding || (data.m_flags & CitizenInstance.Flags.TargetIsNode) != CitizenInstance.Flags.None)
      return;
    this.InvalidPath(instanceID, ref data);
  }

  public override void JoinTarget(
    ushort instanceID,
    ref CitizenInstance data,
    ushort otherInstance)
  {
    ushort num = 0;
    bool flag1 = false;
    bool flag2 = false;
    if (otherInstance != (ushort) 0)
    {
      num = Singleton<CitizenManager>.instance.m_instances.m_buffer[(int) otherInstance].m_targetBuilding;
      flag1 = (Singleton<CitizenManager>.instance.m_instances.m_buffer[(int) otherInstance].m_flags & CitizenInstance.Flags.TargetIsNode) != CitizenInstance.Flags.None;
      flag2 = (Singleton<CitizenManager>.instance.m_instances.m_buffer[(int) otherInstance].m_flags & CitizenInstance.Flags.OnTour) != CitizenInstance.Flags.None;
    }
    if ((int) num != (int) data.m_targetBuilding || flag1 != ((data.m_flags & CitizenInstance.Flags.TargetIsNode) != CitizenInstance.Flags.None))
    {
      if (data.m_targetBuilding != (ushort) 0)
      {
        if ((data.m_flags & CitizenInstance.Flags.TargetIsNode) != CitizenInstance.Flags.None)
        {
          Singleton<NetManager>.instance.m_nodes.m_buffer[(int) data.m_targetBuilding].RemoveTargetCitizen(instanceID, ref data);
          data.m_flags &= ~(CitizenInstance.Flags.TargetIsNode | CitizenInstance.Flags.OnTour);
        }
        else
          Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int) data.m_targetBuilding].RemoveTargetCitizen(instanceID, ref data);
      }
      data.m_targetBuilding = num;
      if (flag1)
        data.m_flags |= CitizenInstance.Flags.TargetIsNode;
      if (flag2)
        data.m_flags |= CitizenInstance.Flags.OnTour;
      if (data.m_targetBuilding != (ushort) 0)
      {
        if ((data.m_flags & CitizenInstance.Flags.TargetIsNode) != CitizenInstance.Flags.None)
          Singleton<NetManager>.instance.m_nodes.m_buffer[(int) data.m_targetBuilding].AddTargetCitizen(instanceID, ref data);
        else
          Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int) data.m_targetBuilding].AddTargetCitizen(instanceID, ref data);
      }
    }
    if (otherInstance == (ushort) 0)
      return;
    PathManager instance1 = Singleton<PathManager>.instance;
    CitizenManager instance2 = Singleton<CitizenManager>.instance;
    data.Unspawn(instanceID);
    CitizenInstance.Frame lastFrameData = instance2.m_instances.m_buffer[(int) otherInstance].GetLastFrameData();
    data.m_frame0 = lastFrameData;
    data.m_frame1 = lastFrameData;
    data.m_frame2 = lastFrameData;
    data.m_frame3 = lastFrameData;
    data.m_targetPos = instance2.m_instances.m_buffer[(int) otherInstance].m_targetPos;
    uint path = instance2.m_instances.m_buffer[(int) otherInstance].m_path;
    if (!instance1.AddPathReference(path))
      return;
    if (data.m_path != 0U)
      instance1.ReleasePath(data.m_path);
    data.m_path = path;
    if ((instance2.m_instances.m_buffer[(int) otherInstance].m_flags & CitizenInstance.Flags.WaitingPath) != CitizenInstance.Flags.None)
      data.m_flags |= CitizenInstance.Flags.WaitingPath;
    else
      data.Spawn(instanceID);
  }

  private bool IsRoadConnection(ushort building)
  {
    if (building != (ushort) 0)
    {
      BuildingManager instance = Singleton<BuildingManager>.instance;
      if ((instance.m_buildings.m_buffer[(int) building].m_flags & Building.Flags.IncomingOutgoing) != Building.Flags.None && instance.m_buildings.m_buffer[(int) building].Info.m_class.m_service == ItemClass.Service.Road)
        return true;
    }
    return false;
  }

  protected override bool SpawnVehicle(
    ushort instanceID,
    ref CitizenInstance citizenData,
    PathUnit.Position pathPos)
  {
    VehicleManager instance1 = Singleton<VehicleManager>.instance;
    float num1 = 20f;
    int num2 = Mathf.Max((int) (((double) citizenData.m_targetPos.x - (double) num1) / 32.0 + 270.0), 0);
    int num3 = Mathf.Max((int) (((double) citizenData.m_targetPos.z - (double) num1) / 32.0 + 270.0), 0);
    int num4 = Mathf.Min((int) (((double) citizenData.m_targetPos.x + (double) num1) / 32.0 + 270.0), 539);
    int num5 = Mathf.Min((int) (((double) citizenData.m_targetPos.z + (double) num1) / 32.0 + 270.0), 539);
    for (int index1 = num3; index1 <= num5; ++index1)
    {
      for (int index2 = num2; index2 <= num4; ++index2)
      {
        ushort nextGridVehicle = instance1.m_vehicleGrid[index1 * 540 + index2];
        int num6 = 0;
        while (nextGridVehicle != (ushort) 0)
        {
          if (this.TryJoinVehicle(instanceID, ref citizenData, nextGridVehicle, ref instance1.m_vehicles.m_buffer[(int) nextGridVehicle]))
          {
            citizenData.m_flags |= CitizenInstance.Flags.EnteringVehicle;
            citizenData.m_flags &= ~CitizenInstance.Flags.TryingSpawnVehicle;
            citizenData.m_flags &= ~CitizenInstance.Flags.BoredOfWaiting;
            citizenData.m_waitCounter = (byte) 0;
            return true;
          }
          nextGridVehicle = instance1.m_vehicles.m_buffer[(int) nextGridVehicle].m_nextGridVehicle;
          if (++num6 > 16384)
          {
            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
            break;
          }
        }
      }
    }
    NetManager instance2 = Singleton<NetManager>.instance;
    CitizenManager instance3 = Singleton<CitizenManager>.instance;
    Vector3 vector3_1 = Vector3.zero;
    Quaternion quaternion = Quaternion.identity;
    ushort num7 = instance3.m_citizens.m_buffer[(IntPtr) citizenData.m_citizen].m_parkedVehicle;
    if (num7 != (ushort) 0)
    {
      vector3_1 = instance1.m_parkedVehicles.m_buffer[(int) num7].m_position;
      quaternion = instance1.m_parkedVehicles.m_buffer[(int) num7].m_rotation;
    }
    VehicleInfo trailer1;
    VehicleInfo vehicleInfo = this.GetVehicleInfo(instanceID, ref citizenData, false, out trailer1);
    if (vehicleInfo == null || vehicleInfo.m_vehicleType == VehicleInfo.VehicleType.Bicycle)
    {
      instance3.m_citizens.m_buffer[(IntPtr) citizenData.m_citizen].SetParkedVehicle(citizenData.m_citizen, (ushort) 0);
      if ((citizenData.m_flags & CitizenInstance.Flags.TryingSpawnVehicle) == CitizenInstance.Flags.None)
      {
        citizenData.m_flags |= CitizenInstance.Flags.TryingSpawnVehicle;
        citizenData.m_flags &= ~CitizenInstance.Flags.BoredOfWaiting;
        citizenData.m_waitCounter = (byte) 0;
      }
      return true;
    }
    if (vehicleInfo.m_class.m_subService == ItemClass.SubService.PublicTransportTaxi)
    {
      instance3.m_citizens.m_buffer[(IntPtr) citizenData.m_citizen].SetParkedVehicle(citizenData.m_citizen, (ushort) 0);
      if ((citizenData.m_flags & CitizenInstance.Flags.WaitingTaxi) == CitizenInstance.Flags.None && instance2.m_segments.m_buffer[(int) pathPos.m_segment].Info.m_hasPedestrianLanes)
      {
        citizenData.m_flags |= CitizenInstance.Flags.WaitingTaxi;
        citizenData.m_flags &= ~CitizenInstance.Flags.BoredOfWaiting;
        citizenData.m_waitCounter = (byte) 0;
      }
      return true;
    }
    uint laneId = PathManager.GetLaneID(pathPos);
    Vector3 vector3_2 = (Vector3) citizenData.m_targetPos;
    if (num7 != (ushort) 0 && (double) Vector3.SqrMagnitude(vector3_1 - vector3_2) < 1024.0)
      vector3_2 = vector3_1;
    else
      num7 = (ushort) 0;
    Vector3 position;
    float laneOffset;
    instance2.m_lanes.m_buffer[(IntPtr) laneId].GetClosestPosition(vector3_2, out position, out laneOffset);
    byte num8 = (byte) Mathf.Clamp(Mathf.RoundToInt(laneOffset * (float) byte.MaxValue), 0, (int) byte.MaxValue);
    Vector3 vector3_3 = vector3_2 + Vector3.ClampMagnitude(position - vector3_2, 5f);
    ushort vehicle;
    if (instance1.CreateVehicle(out vehicle, ref Singleton<SimulationManager>.instance.m_randomizer, vehicleInfo, vector3_2, TransferManager.TransferReason.None, false, false))
    {
      Vehicle.Frame frame0 = instance1.m_vehicles.m_buffer[(int) vehicle].m_frame0;
      if (num7 != (ushort) 0)
      {
        frame0.m_rotation = quaternion;
      }
      else
      {
        Vector3 forward = vector3_3 - citizenData.GetLastFrameData().m_position;
        if ((double) forward.sqrMagnitude > 0.00999999977648258)
          frame0.m_rotation = Quaternion.LookRotation(forward);
      }
      instance1.m_vehicles.m_buffer[(int) vehicle].m_frame0 = frame0;
      instance1.m_vehicles.m_buffer[(int) vehicle].m_frame1 = frame0;
      instance1.m_vehicles.m_buffer[(int) vehicle].m_frame2 = frame0;
      instance1.m_vehicles.m_buffer[(int) vehicle].m_frame3 = frame0;
      vehicleInfo.m_vehicleAI.FrameDataUpdated(vehicle, ref instance1.m_vehicles.m_buffer[(int) vehicle], ref frame0);
      instance1.m_vehicles.m_buffer[(int) vehicle].m_targetPos0 = new Vector4(vector3_3.x, vector3_3.y, vector3_3.z, 2f);
      instance1.m_vehicles.m_buffer[(int) vehicle].m_flags |= Vehicle.Flags.Stopped;
      instance1.m_vehicles.m_buffer[(int) vehicle].m_path = citizenData.m_path;
      instance1.m_vehicles.m_buffer[(int) vehicle].m_pathPositionIndex = citizenData.m_pathPositionIndex;
      instance1.m_vehicles.m_buffer[(int) vehicle].m_lastPathOffset = num8;
      instance1.m_vehicles.m_buffer[(int) vehicle].m_transferSize = (ushort) (citizenData.m_citizen & (uint) ushort.MaxValue);
      if (trailer1 != null)
      {
        int trailer2 = (int) instance1.m_vehicles.m_buffer[(int) vehicle].CreateTrailer(vehicle, trailer1, false);
      }
      vehicleInfo.m_vehicleAI.TrySpawn(vehicle, ref instance1.m_vehicles.m_buffer[(int) vehicle]);
      if (num7 != (ushort) 0)
      {
        InstanceID empty1 = InstanceID.Empty;
        empty1.ParkedVehicle = num7;
        InstanceID empty2 = InstanceID.Empty;
        empty2.Vehicle = vehicle;
        Singleton<InstanceManager>.instance.ChangeInstance(empty1, empty2);
      }
      citizenData.m_path = 0U;
      instance3.m_citizens.m_buffer[(IntPtr) citizenData.m_citizen].SetParkedVehicle(citizenData.m_citizen, (ushort) 0);
      instance3.m_citizens.m_buffer[(IntPtr) citizenData.m_citizen].SetVehicle(citizenData.m_citizen, vehicle, 0U);
      citizenData.m_flags |= CitizenInstance.Flags.EnteringVehicle;
      citizenData.m_flags &= ~CitizenInstance.Flags.TryingSpawnVehicle;
      citizenData.m_flags &= ~CitizenInstance.Flags.BoredOfWaiting;
      citizenData.m_waitCounter = (byte) 0;
      return true;
    }
    instance3.m_citizens.m_buffer[(IntPtr) citizenData.m_citizen].SetParkedVehicle(citizenData.m_citizen, (ushort) 0);
    if ((citizenData.m_flags & CitizenInstance.Flags.TryingSpawnVehicle) == CitizenInstance.Flags.None)
    {
      citizenData.m_flags |= CitizenInstance.Flags.TryingSpawnVehicle;
      citizenData.m_flags &= ~CitizenInstance.Flags.BoredOfWaiting;
      citizenData.m_waitCounter = (byte) 0;
    }
    return true;
  }

  protected override bool SpawnBicycle(
    ushort instanceID,
    ref CitizenInstance citizenData,
    PathUnit.Position pathPos)
  {
    VehicleInfo trailer1;
    VehicleInfo vehicleInfo = this.GetVehicleInfo(instanceID, ref citizenData, false, out trailer1);
    if (vehicleInfo != null && vehicleInfo.m_vehicleType == VehicleInfo.VehicleType.Bicycle)
    {
      CitizenManager instance1 = Singleton<CitizenManager>.instance;
      VehicleManager instance2 = Singleton<VehicleManager>.instance;
      CitizenInstance.Frame lastFrameData = citizenData.GetLastFrameData();
      ushort vehicle;
      if (instance2.CreateVehicle(out vehicle, ref Singleton<SimulationManager>.instance.m_randomizer, vehicleInfo, lastFrameData.m_position, TransferManager.TransferReason.None, false, false))
      {
        Vehicle.Frame frame0 = instance2.m_vehicles.m_buffer[(int) vehicle].m_frame0;
        frame0.m_rotation = lastFrameData.m_rotation;
        instance2.m_vehicles.m_buffer[(int) vehicle].m_frame0 = frame0;
        instance2.m_vehicles.m_buffer[(int) vehicle].m_frame1 = frame0;
        instance2.m_vehicles.m_buffer[(int) vehicle].m_frame2 = frame0;
        instance2.m_vehicles.m_buffer[(int) vehicle].m_frame3 = frame0;
        vehicleInfo.m_vehicleAI.FrameDataUpdated(vehicle, ref instance2.m_vehicles.m_buffer[(int) vehicle], ref frame0);
        if (trailer1 != null)
        {
          int trailer2 = (int) instance2.m_vehicles.m_buffer[(int) vehicle].CreateTrailer(vehicle, trailer1, false);
        }
        vehicleInfo.m_vehicleAI.TrySpawn(vehicle, ref instance2.m_vehicles.m_buffer[(int) vehicle]);
        instance1.m_citizens.m_buffer[(IntPtr) citizenData.m_citizen].SetParkedVehicle(citizenData.m_citizen, (ushort) 0);
        instance1.m_citizens.m_buffer[(IntPtr) citizenData.m_citizen].SetVehicle(citizenData.m_citizen, vehicle, 0U);
        citizenData.m_flags |= CitizenInstance.Flags.RidingBicycle;
        return true;
      }
    }
    return false;
  }

  private bool TryJoinVehicle(
    ushort instanceID,
    ref CitizenInstance citizenData,
    ushort vehicleID,
    ref Vehicle vehicleData)
  {
    if ((vehicleData.m_flags & Vehicle.Flags.Stopped) == ~(Vehicle.Flags.Created | Vehicle.Flags.Deleted | Vehicle.Flags.Spawned | Vehicle.Flags.Inverted | Vehicle.Flags.TransferToTarget | Vehicle.Flags.TransferToSource | Vehicle.Flags.Emergency1 | Vehicle.Flags.Emergency2 | Vehicle.Flags.WaitingPath | Vehicle.Flags.Stopped | Vehicle.Flags.Leaving | Vehicle.Flags.Arriving | Vehicle.Flags.Reversed | Vehicle.Flags.TakingOff | Vehicle.Flags.Flying | Vehicle.Flags.Landing | Vehicle.Flags.WaitingSpace | Vehicle.Flags.WaitingCargo | Vehicle.Flags.GoingBack | Vehicle.Flags.WaitingTarget | Vehicle.Flags.Importing | Vehicle.Flags.Exporting | Vehicle.Flags.Parking | Vehicle.Flags.CustomName | Vehicle.Flags.OnGravel | Vehicle.Flags.WaitingLoading | Vehicle.Flags.Congestion | Vehicle.Flags.DummyTraffic | Vehicle.Flags.Underground | Vehicle.Flags.Transition | Vehicle.Flags.InsideBuilding | Vehicle.Flags.LeftHandDrive))
      return false;
    CitizenManager instance1 = Singleton<CitizenManager>.instance;
    uint num1 = vehicleData.m_citizenUnits;
    int num2 = 0;
    while (num1 != 0U)
    {
      uint nextUnit = instance1.m_units.m_buffer[(IntPtr) num1].m_nextUnit;
      for (int index = 0; index < 5; ++index)
      {
        uint citizen = instance1.m_units.m_buffer[(IntPtr) num1].GetCitizen(index);
        if (citizen != 0U)
        {
          ushort instance2 = instance1.m_citizens.m_buffer[(IntPtr) citizen].m_instance;
          if (instance2 != (ushort) 0 && (int) instance1.m_instances.m_buffer[(int) instance2].m_targetBuilding == (int) citizenData.m_targetBuilding && (instance1.m_instances.m_buffer[(int) instance2].m_flags & CitizenInstance.Flags.TargetIsNode) == (citizenData.m_flags & CitizenInstance.Flags.TargetIsNode))
          {
            instance1.m_citizens.m_buffer[(IntPtr) citizenData.m_citizen].SetVehicle(citizenData.m_citizen, vehicleID, 0U);
            if ((int) instance1.m_citizens.m_buffer[(IntPtr) citizenData.m_citizen].m_vehicle == (int) vehicleID)
            {
              if (citizenData.m_path != 0U)
              {
                Singleton<PathManager>.instance.ReleasePath(citizenData.m_path);
                citizenData.m_path = 0U;
              }
              return true;
            }
            break;
          }
          break;
        }
      }
      num1 = nextUnit;
      if (++num2 > 524288)
      {
        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
        break;
      }
    }
    return false;
  }

  protected override void SwitchBuildingTargetPos(
    ushort instanceID,
    ref CitizenInstance citizenData)
  {
    if (citizenData.m_path != 0U || citizenData.m_targetBuilding == (ushort) 0 || (citizenData.m_flags & CitizenInstance.Flags.TargetIsNode) != CitizenInstance.Flags.None)
      return;
    BuildingManager instance = Singleton<BuildingManager>.instance;
    BuildingInfo info = instance.m_buildings.m_buffer[(int) citizenData.m_targetBuilding].Info;
    if (!info.m_hasPedestrianPaths)
      return;
    Randomizer randomizer = new Randomizer((int) instanceID << 8 | (int) citizenData.m_targetSeed);
    Vector3 position;
    Vector3 target;
    Vector2 direction;
    CitizenInstance.Flags specialFlags;
    info.m_buildingAI.CalculateUnspawnPosition(citizenData.m_targetBuilding, ref instance.m_buildings.m_buffer[(int) citizenData.m_targetBuilding], ref randomizer, this.m_info, instanceID, out position, out target, out direction, out specialFlags);
    if ((double) Vector3.Distance((Vector3) citizenData.m_targetPos, target) <= 10.0)
      return;
    this.StartPathFind(instanceID, ref citizenData, (Vector3) citizenData.m_targetPos, target, (VehicleInfo) null, true, false);
  }

  public override void EnterParkArea(
    ushort instanceID,
    ref CitizenInstance citizenData,
    byte park,
    ushort gateID)
  {
    if (gateID != (ushort) 0)
      ++Singleton<DistrictManager>.instance.m_parks.m_buffer[(int) park].m_tempResidentCount;
    base.EnterParkArea(instanceID, ref citizenData, park, gateID);
  }

    // called by SetTarget (from sim step)
    // returns FALSE in these circs
    //  - CI already had a vehicle (on return they are separated from it)
    // returns per value of CitizenAI.StartPathFind (if no early exit) in these circs (TBD)
  protected override bool StartPathFind(ushort instanceID, ref CitizenInstance citizenData)
  {
    if (citizenData.m_citizen != 0U)
    {
      CitizenManager instance1 = Singleton<CitizenManager>.instance;
      VehicleManager instance2 = Singleton<VehicleManager>.instance;
      ushort vehicle = instance1.m_citizens.m_buffer[(IntPtr) citizenData.m_citizen].m_vehicle;
            // if the m_citizen has an m_vehicle already ... in a nutshell, separate the CI from the vehicle
      if (vehicle != (ushort) 0)
      {
        VehicleInfo info = instance2.m_vehicles.m_buffer[(int) vehicle].Info;
                // if the CI owns their m_vehicle then exit here after doing SetTarget (to zero) *for the vehicle* (not followed all but I think will end vehicle.unspawn)
        if (info != null && (int) info.m_vehicleAI.GetOwnerID(vehicle, ref instance2.m_vehicles.m_buffer[(int) vehicle]).Citizen == (int) citizenData.m_citizen)
        {
          info.m_vehicleAI.SetTarget(vehicle, ref instance2.m_vehicles.m_buffer[(int) vehicle], (ushort) 0);
          return false;
        }
        bool dontSetVehicle = false;
                // if the vehicle is on a transport line ...
        if (instance2.m_vehicles.m_buffer[(int) vehicle].m_transportLine != (ushort) 0)
        {
          NetManager instance3 = Singleton<NetManager>.instance;
          ushort targetBuilding = instance2.m_vehicles.m_buffer[(int) vehicle].m_targetBuilding;    // set LOCAL var targetBuilding to vehicle target
                    // do nothing else if the vehicle has a null target (will fall through to set vehicle below)
          if (targetBuilding != (ushort) 0)
          {
            uint laneID = instance3.m_nodes.m_buffer[(int) targetBuilding].m_lane;
            int laneOffset = (int) instance3.m_nodes.m_buffer[(int) targetBuilding].m_laneOffset;
            if (laneID != 0U)
            {
              ushort segment = instance3.m_lanes.m_buffer[laneID].m_segment;
              NetInfo.Lane laneInfo;
              if (instance3.m_segments.m_buffer[(int) segment].GetClosestLane(laneID, NetInfo.LaneType.Pedestrian, VehicleInfo.VehicleType.None, out laneID, out laneInfo))
              {
                citizenData.m_targetPos = (Vector4) instance3.m_lanes.m_buffer[(IntPtr) laneID].CalculatePosition((float) laneOffset * 0.003921569f); //* this multiplier is 1/255
                dontSetVehicle = true;
              }
            }
          }
        }
        if (!dontSetVehicle)
        {
                    // calling SetVehicle with these args causes the citizen to be removed from the vehicles's units, then CI.m_vehicle is set to zero
          instance1.m_citizens.m_buffer[(IntPtr) citizenData.m_citizen].SetVehicle(citizenData.m_citizen, (ushort) 0, 0U);
          return false;
        }
      }
    }
        // failsafe check, there would be no point in calling this method if the CI did not have a target set already
    if (citizenData.m_targetBuilding == (ushort) 0)
      return false;

    // GetVehicleInfo does not get the info of an existing vehicle, it returns the info of random new vehicle (bike, car, taxi)
    // depending on circs like age
    VehicleInfo trailer;
    VehicleInfo vehicleInfo = GetVehicleInfo(instanceID, ref citizenData, false, out trailer);

    // if CI.TargetIsNode ... calculate target position from Node-building info
    if ((citizenData.m_flags & CitizenInstance.Flags.TargetIsNode) != CitizenInstance.Flags.None)
    {
      NetManager instance = Singleton<NetManager>.instance;
      Vector3 targetPosition = instance.m_nodes.m_buffer[(int) citizenData.m_targetBuilding].m_position;
      uint laneID = instance.m_nodes.m_buffer[(int) citizenData.m_targetBuilding].m_lane;
      if (laneID != 0U)
      {
        ushort segment = instance.m_lanes.m_buffer[(IntPtr) laneID].m_segment;
        NetInfo.Lane laneInfo;
        if (instance.m_segments.m_buffer[(int) segment].GetClosestLane(laneID, NetInfo.LaneType.Pedestrian, VehicleInfo.VehicleType.None, out laneID, out laneInfo))
        {
          int laneOffset = (int) instance.m_nodes.m_buffer[(int) citizenData.m_targetBuilding].m_laneOffset;
          targetPosition = instance.m_lanes.m_buffer[(IntPtr) laneID].CalculatePosition((float) laneOffset * 0.003921569f);
        }
      }
      // I am confused that CI.m_targetPos in the following call is positional for parameter StartPos - *starting* position
      // I have not seen any indication anywhere that m_targetPos would ever be set to current position???  The single case above where I see it set, it is definitely set to target-node position
      return StartPathFind(instanceID: instanceID, citizenData: ref citizenData, startPos: (Vector3) citizenData.m_targetPos, endPos: targetPosition, vehicleInfo: vehicleInfo, enableTransport: true, ignoreCost: false);
    }
    // else if reach here, CI target is Building, not Node...
    // call to StartPathFind with CI.m_targetPos as *start* (why???) and the unspawn position for the target building as argument targetPosition
    BuildingManager instance4 = Singleton<BuildingManager>.instance;
    BuildingInfo info1 = instance4.m_buildings.m_buffer[(int) citizenData.m_targetBuilding].Info;
    Randomizer randomizer = new Randomizer((int) instanceID << 8 | (int) citizenData.m_targetSeed);
    Vector3 position1;
    Vector3 targetPostion;
    Vector2 direction;
    CitizenInstance.Flags specialFlags;
    info1.m_buildingAI.CalculateUnspawnPosition(buildingID: citizenData.m_targetBuilding, data: ref instance4.m_buildings.m_buffer[(int) citizenData.m_targetBuilding], randomizer: ref randomizer, info: this.m_info, ignoreInstance: instanceID, position: out position1, target: out targetPostion, direction: out direction, specialFlags: out specialFlags);
    return this.StartPathFind(instanceID: instanceID, citizenData: ref citizenData, startPos: (Vector3) citizenData.m_targetPos, endPos: targetPostion, vehicleInfo: vehicleInfo, enableTransport: true, ignoreCost: false);
  }

  protected override VehicleInfo GetVehicleInfo(
    ushort instanceID,
    ref CitizenInstance citizenData,
    bool forceProbability,
    out VehicleInfo trailer)
  {
    trailer = (VehicleInfo) null;
    if (citizenData.m_citizen == 0U)
      return (VehicleInfo) null;
    Citizen.AgeGroup ageGroup;
    switch (this.m_info.m_agePhase)
    {
      case Citizen.AgePhase.Child:
        ageGroup = Citizen.AgeGroup.Child;
        break;
      case Citizen.AgePhase.Teen0:
      case Citizen.AgePhase.Teen1:
        ageGroup = Citizen.AgeGroup.Teen;
        break;
      case Citizen.AgePhase.Young0:
      case Citizen.AgePhase.Young1:
      case Citizen.AgePhase.Young2:
        ageGroup = Citizen.AgeGroup.Young;
        break;
      case Citizen.AgePhase.Adult0:
      case Citizen.AgePhase.Adult1:
      case Citizen.AgePhase.Adult2:
      case Citizen.AgePhase.Adult3:
        ageGroup = Citizen.AgeGroup.Adult;
        break;
      case Citizen.AgePhase.Senior0:
      case Citizen.AgePhase.Senior1:
      case Citizen.AgePhase.Senior2:
      case Citizen.AgePhase.Senior3:
        ageGroup = Citizen.AgeGroup.Senior;
        break;
      default:
        ageGroup = Citizen.AgeGroup.Adult;
        break;
    }
    int num1;
    int num2;
    if (forceProbability || (citizenData.m_flags & CitizenInstance.Flags.BorrowCar) != CitizenInstance.Flags.None)
    {
      num1 = 100;
      num2 = 0;
    }
    else
    {
      num1 = this.GetCarProbability(instanceID, ref citizenData, ageGroup);
      num2 = this.GetBikeProbability(instanceID, ref citizenData, ageGroup);
    }
    Randomizer r = new Randomizer(citizenData.m_citizen);
    bool flag1 = r.Int32(100U) < num1;
    bool flag2 = r.Int32(100U) < num2;
    bool flag3;
    bool flag4;
    if (flag1)
    {
      int electricCarProbability = this.GetElectricCarProbability(instanceID, ref citizenData, this.m_info.m_agePhase);
      flag3 = false;
      flag4 = r.Int32(100U) < electricCarProbability;
    }
    else
    {
      int taxiProbability = this.GetTaxiProbability(instanceID, ref citizenData, ageGroup);
      flag3 = r.Int32(100U) < taxiProbability;
      flag4 = false;
    }
    ItemClass.Service service = ItemClass.Service.Residential;
    ItemClass.SubService subService = !flag4 ? ItemClass.SubService.ResidentialLow : ItemClass.SubService.ResidentialLowEco;
    if (!flag1 && flag3)
    {
      service = ItemClass.Service.PublicTransport;
      subService = ItemClass.SubService.PublicTransportTaxi;
    }
    VehicleInfo randomVehicleInfo1 = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref r, service, subService, ItemClass.Level.Level1);
    VehicleInfo randomVehicleInfo2 = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref r, ItemClass.Service.Residential, ItemClass.SubService.ResidentialHigh, ageGroup != Citizen.AgeGroup.Child ? ItemClass.Level.Level2 : ItemClass.Level.Level1);
    if (flag2 && randomVehicleInfo2 != null)
      return randomVehicleInfo2;
    if ((flag1 || flag3) && randomVehicleInfo1 != null)
      return randomVehicleInfo1;
    return (VehicleInfo) null;
  }

  private int GetCarProbability(
    ushort instanceID,
    ref CitizenInstance citizenData,
    Citizen.AgeGroup ageGroup)
  {
    return this.GetCarProbability(ageGroup);
  }

  private int GetCarProbability(Citizen.AgeGroup ageGroup)
  {
    switch (ageGroup)
    {
      case Citizen.AgeGroup.Child:
        return 0;
      case Citizen.AgeGroup.Teen:
        return 5;
      case Citizen.AgeGroup.Young:
        return 15;
      case Citizen.AgeGroup.Adult:
        return 20;
      case Citizen.AgeGroup.Senior:
        return 10;
      default:
        return 0;
    }
  }

  private int GetBikeProbability(
    ushort instanceID,
    ref CitizenInstance citizenData,
    Citizen.AgeGroup ageGroup)
  {
    ushort homeBuilding = Singleton<CitizenManager>.instance.m_citizens.m_buffer[(IntPtr) citizenData.m_citizen].m_homeBuilding;
    int num = 0;
    if (homeBuilding != (ushort) 0)
    {
      Vector3 position = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int) homeBuilding].m_position;
      DistrictManager instance = Singleton<DistrictManager>.instance;
      byte district = instance.GetDistrict(position);
      if ((instance.m_districts.m_buffer[(int) district].m_cityPlanningPolicies & DistrictPolicies.CityPlanning.EncourageBiking) != DistrictPolicies.CityPlanning.None)
        num = 10;
    }
    switch (ageGroup)
    {
      case Citizen.AgeGroup.Child:
        return 40 + num;
      case Citizen.AgeGroup.Teen:
        return 30 + num;
      case Citizen.AgeGroup.Young:
        return 20 + num;
      case Citizen.AgeGroup.Adult:
        return 10 + num;
      case Citizen.AgeGroup.Senior:
        return num;
      default:
        return 0;
    }
  }

  private int GetTaxiProbability(
    ushort instanceID,
    ref CitizenInstance citizenData,
    Citizen.AgeGroup ageGroup)
  {
    switch (ageGroup)
    {
      case Citizen.AgeGroup.Child:
        return 0;
      case Citizen.AgeGroup.Teen:
        return 2;
      case Citizen.AgeGroup.Young:
        return 2;
      case Citizen.AgeGroup.Adult:
        return 4;
      case Citizen.AgeGroup.Senior:
        return 6;
      default:
        return 0;
    }
  }

  private int GetElectricCarProbability(
    ushort instanceID,
    ref CitizenInstance citizenData,
    Citizen.AgePhase agePhase)
  {
    ushort homeBuilding = Singleton<CitizenManager>.instance.m_citizens.m_buffer[(IntPtr) citizenData.m_citizen].m_homeBuilding;
    if (homeBuilding != (ushort) 0)
    {
      Vector3 position = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int) homeBuilding].m_position;
      DistrictManager instance = Singleton<DistrictManager>.instance;
      byte district = instance.GetDistrict(position);
      if ((instance.m_districts.m_buffer[(int) district].m_cityPlanningPolicies & DistrictPolicies.CityPlanning.ElectricCars) != DistrictPolicies.CityPlanning.None)
        return 100;
    }
    switch (agePhase)
    {
      case Citizen.AgePhase.Child:
      case Citizen.AgePhase.Teen0:
      case Citizen.AgePhase.Young0:
      case Citizen.AgePhase.Adult0:
      case Citizen.AgePhase.Senior0:
        return 5;
      case Citizen.AgePhase.Teen1:
      case Citizen.AgePhase.Young1:
      case Citizen.AgePhase.Adult1:
      case Citizen.AgePhase.Senior1:
        return 10;
      case Citizen.AgePhase.Young2:
      case Citizen.AgePhase.Adult2:
      case Citizen.AgePhase.Senior2:
        return 15;
      case Citizen.AgePhase.Adult3:
      case Citizen.AgePhase.Senior3:
        return 20;
      default:
        return 0;
    }
  }
}
