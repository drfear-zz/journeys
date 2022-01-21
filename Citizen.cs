// Decompiled with JetBrains decompiler
// Type: Citizen
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 9724B8F4-19DD-48C3-AE02-CDA150D75CEC
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Cities_Skylines\Cities_Data\Managed\Assembly-CSharp.dll

using ColossalFramework;
using ColossalFramework.Math;
using System;
using UnityEngine;

public struct Citizen
{
  public const int GENDER_COUNT = 2;
  public const int SUBCULTURE_COUNT = 5;
  public const int WEALTH_COUNT = 3;
  public const int AGEPHASE_COUNT = 14;
  public const int AGE_LIMIT_CHILD = 15;
  public const int AGE_LIMIT_TEEN = 45;
  public const int AGE_LIMIT_YOUNG = 90;
  public const int AGE_LIMIT_ADULT = 180;
  public const int AGE_LIMIT_SENIOR = 240;
  public const int AGE_LIMIT_FINAL = 255;
  public Citizen.Flags m_flags;
  public ushort m_homeBuilding;
  public ushort m_workBuilding;
  public ushort m_visitBuilding;
  public ushort m_vehicle;
  public ushort m_parkedVehicle;
  public ushort m_instance;
  public byte m_health;
  public byte m_wellbeing;
  public byte m_age;
  public byte m_family;
  public byte m_jobTitleIndex;

  public ItemClass.Level GetCurrentSchoolLevel(uint citizenID)
  {
    ushort workBuilding = this.m_workBuilding;
    if (workBuilding == (ushort) 0 || (this.m_flags & Citizen.Flags.Student) == Citizen.Flags.None)
      return ItemClass.Level.None;
    return Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int) workBuilding].Info.m_class.m_level;
  }

  public bool Sick
  {
    get
    {
      return (this.m_flags & Citizen.Flags.Sick) != Citizen.Flags.None;
    }
    set
    {
      if (value)
        this.m_flags |= Citizen.Flags.Sick;
      else
        this.m_flags &= Citizen.Flags.Unemployed | Citizen.Flags.Wealth | Citizen.Flags.Location | Citizen.Flags.NoElectricity | Citizen.Flags.NoWater | Citizen.Flags.NoSewage | Citizen.Flags.BadHealth | Citizen.Flags.Created | Citizen.Flags.Tourist | Citizen.Flags.Dead | Citizen.Flags.Student | Citizen.Flags.MovingIn | Citizen.Flags.DummyTraffic | Citizen.Flags.Criminal | Citizen.Flags.Arrested | Citizen.Flags.Evacuating | Citizen.Flags.Collapsed | Citizen.Flags.Education1 | Citizen.Flags.Education2 | Citizen.Flags.Education3 | Citizen.Flags.NeedGoods | Citizen.Flags.Original | Citizen.Flags.CustomName;
    }
  }

  public bool Dead
  {
    get
    {
      return (this.m_flags & Citizen.Flags.Dead) != Citizen.Flags.None;
    }
    set
    {
      if (value)
        this.m_flags |= Citizen.Flags.Dead;
      else
        this.m_flags &= Citizen.Flags.Unemployed | Citizen.Flags.Wealth | Citizen.Flags.Location | Citizen.Flags.NoElectricity | Citizen.Flags.NoWater | Citizen.Flags.NoSewage | Citizen.Flags.BadHealth | Citizen.Flags.Created | Citizen.Flags.Tourist | Citizen.Flags.Sick | Citizen.Flags.Student | Citizen.Flags.MovingIn | Citizen.Flags.DummyTraffic | Citizen.Flags.Criminal | Citizen.Flags.Arrested | Citizen.Flags.Evacuating | Citizen.Flags.Collapsed | Citizen.Flags.Education1 | Citizen.Flags.Education2 | Citizen.Flags.Education3 | Citizen.Flags.NeedGoods | Citizen.Flags.Original | Citizen.Flags.CustomName;
    }
  }

  public int Age
  {
    get
    {
      return (int) this.m_age;
    }
    set
    {
      this.m_age = (byte) Mathf.Clamp(value, 0, (int) byte.MaxValue);
    }
  }

  public bool Education1
  {
    get
    {
      return (this.m_flags & Citizen.Flags.Education1) != Citizen.Flags.None;
    }
    set
    {
      if (value)
        this.m_flags |= Citizen.Flags.Education1;
      else
        this.m_flags &= Citizen.Flags.Unemployed | Citizen.Flags.Wealth | Citizen.Flags.Location | Citizen.Flags.NoElectricity | Citizen.Flags.NoWater | Citizen.Flags.NoSewage | Citizen.Flags.BadHealth | Citizen.Flags.Created | Citizen.Flags.Tourist | Citizen.Flags.Sick | Citizen.Flags.Dead | Citizen.Flags.Student | Citizen.Flags.MovingIn | Citizen.Flags.DummyTraffic | Citizen.Flags.Criminal | Citizen.Flags.Arrested | Citizen.Flags.Evacuating | Citizen.Flags.Collapsed | Citizen.Flags.Education2 | Citizen.Flags.Education3 | Citizen.Flags.NeedGoods | Citizen.Flags.Original | Citizen.Flags.CustomName;
    }
  }

  public bool Education2
  {
    get
    {
      return (this.m_flags & Citizen.Flags.Education2) != Citizen.Flags.None;
    }
    set
    {
      if (value)
        this.m_flags |= Citizen.Flags.Education2;
      else
        this.m_flags &= Citizen.Flags.Unemployed | Citizen.Flags.Wealth | Citizen.Flags.Location | Citizen.Flags.NoElectricity | Citizen.Flags.NoWater | Citizen.Flags.NoSewage | Citizen.Flags.BadHealth | Citizen.Flags.Created | Citizen.Flags.Tourist | Citizen.Flags.Sick | Citizen.Flags.Dead | Citizen.Flags.Student | Citizen.Flags.MovingIn | Citizen.Flags.DummyTraffic | Citizen.Flags.Criminal | Citizen.Flags.Arrested | Citizen.Flags.Evacuating | Citizen.Flags.Collapsed | Citizen.Flags.Education1 | Citizen.Flags.Education3 | Citizen.Flags.NeedGoods | Citizen.Flags.Original | Citizen.Flags.CustomName;
    }
  }

  public bool Education3
  {
    get
    {
      return (this.m_flags & Citizen.Flags.Education3) != Citizen.Flags.None;
    }
    set
    {
      if (value)
        this.m_flags |= Citizen.Flags.Education3;
      else
        this.m_flags &= Citizen.Flags.Unemployed | Citizen.Flags.Wealth | Citizen.Flags.Location | Citizen.Flags.NoElectricity | Citizen.Flags.NoWater | Citizen.Flags.NoSewage | Citizen.Flags.BadHealth | Citizen.Flags.Created | Citizen.Flags.Tourist | Citizen.Flags.Sick | Citizen.Flags.Dead | Citizen.Flags.Student | Citizen.Flags.MovingIn | Citizen.Flags.DummyTraffic | Citizen.Flags.Criminal | Citizen.Flags.Arrested | Citizen.Flags.Evacuating | Citizen.Flags.Collapsed | Citizen.Flags.Education1 | Citizen.Flags.Education2 | Citizen.Flags.NeedGoods | Citizen.Flags.Original | Citizen.Flags.CustomName;
    }
  }

  public bool Criminal
  {
    get
    {
      return (this.m_flags & Citizen.Flags.Criminal) != Citizen.Flags.None;
    }
    set
    {
      if (value)
        this.m_flags |= Citizen.Flags.Criminal;
      else
        this.m_flags &= Citizen.Flags.Unemployed | Citizen.Flags.Wealth | Citizen.Flags.Location | Citizen.Flags.NoElectricity | Citizen.Flags.NoWater | Citizen.Flags.NoSewage | Citizen.Flags.BadHealth | Citizen.Flags.Created | Citizen.Flags.Tourist | Citizen.Flags.Sick | Citizen.Flags.Dead | Citizen.Flags.Student | Citizen.Flags.MovingIn | Citizen.Flags.DummyTraffic | Citizen.Flags.Arrested | Citizen.Flags.Evacuating | Citizen.Flags.Collapsed | Citizen.Flags.Education1 | Citizen.Flags.Education2 | Citizen.Flags.Education3 | Citizen.Flags.NeedGoods | Citizen.Flags.Original | Citizen.Flags.CustomName;
    }
  }

  public bool Arrested
  {
    get
    {
      return (this.m_flags & Citizen.Flags.Arrested) != Citizen.Flags.None;
    }
    set
    {
      if (value)
        this.m_flags |= Citizen.Flags.Arrested;
      else
        this.m_flags &= Citizen.Flags.Unemployed | Citizen.Flags.Wealth | Citizen.Flags.Location | Citizen.Flags.NoElectricity | Citizen.Flags.NoWater | Citizen.Flags.NoSewage | Citizen.Flags.BadHealth | Citizen.Flags.Created | Citizen.Flags.Tourist | Citizen.Flags.Sick | Citizen.Flags.Dead | Citizen.Flags.Student | Citizen.Flags.MovingIn | Citizen.Flags.DummyTraffic | Citizen.Flags.Criminal | Citizen.Flags.Evacuating | Citizen.Flags.Collapsed | Citizen.Flags.Education1 | Citizen.Flags.Education2 | Citizen.Flags.Education3 | Citizen.Flags.NeedGoods | Citizen.Flags.Original | Citizen.Flags.CustomName;
    }
  }

  public bool Collapsed
  {
    get
    {
      return (this.m_flags & Citizen.Flags.Collapsed) != Citizen.Flags.None;
    }
    set
    {
      if (value)
        this.m_flags |= Citizen.Flags.Collapsed;
      else
        this.m_flags &= Citizen.Flags.Unemployed | Citizen.Flags.Wealth | Citizen.Flags.Location | Citizen.Flags.NoElectricity | Citizen.Flags.NoWater | Citizen.Flags.NoSewage | Citizen.Flags.BadHealth | Citizen.Flags.Created | Citizen.Flags.Tourist | Citizen.Flags.Sick | Citizen.Flags.Dead | Citizen.Flags.Student | Citizen.Flags.MovingIn | Citizen.Flags.DummyTraffic | Citizen.Flags.Criminal | Citizen.Flags.Arrested | Citizen.Flags.Evacuating | Citizen.Flags.Education1 | Citizen.Flags.Education2 | Citizen.Flags.Education3 | Citizen.Flags.NeedGoods | Citizen.Flags.Original | Citizen.Flags.CustomName;
    }
  }

  public Citizen.Education EducationLevel
  {
    get
    {
      int num = 0;
      if ((this.m_flags & Citizen.Flags.Education1) != Citizen.Flags.None)
        ++num;
      if ((this.m_flags & Citizen.Flags.Education2) != Citizen.Flags.None)
        ++num;
      if ((this.m_flags & Citizen.Flags.Education3) != Citizen.Flags.None)
        ++num;
      return (Citizen.Education) num;
    }
  }

  public Citizen.Location CurrentLocation
  {
    get
    {
      return (Citizen.Location) ((uint) (this.m_flags & Citizen.Flags.Location) >> 19);
    }
    set
    {
      this.m_flags = this.m_flags & (Citizen.Flags.Unemployed | Citizen.Flags.Wealth | Citizen.Flags.NoElectricity | Citizen.Flags.NoWater | Citizen.Flags.NoSewage | Citizen.Flags.BadHealth | Citizen.Flags.Created | Citizen.Flags.Tourist | Citizen.Flags.Sick | Citizen.Flags.Dead | Citizen.Flags.Student | Citizen.Flags.MovingIn | Citizen.Flags.DummyTraffic | Citizen.Flags.Criminal | Citizen.Flags.Arrested | Citizen.Flags.Evacuating | Citizen.Flags.Collapsed | Citizen.Flags.Education1 | Citizen.Flags.Education2 | Citizen.Flags.Education3 | Citizen.Flags.NeedGoods | Citizen.Flags.Original | Citizen.Flags.CustomName) | (Citizen.Flags) (Mathf.Clamp((int) value, 0, 3) << 19);
    }
  }

  public Citizen.Wealth WealthLevel
  {
    get
    {
      return (Citizen.Wealth) ((uint) (this.m_flags & Citizen.Flags.Wealth) >> 17);
    }
    set
    {
      this.m_flags = this.m_flags & (Citizen.Flags.Unemployed | Citizen.Flags.Location | Citizen.Flags.NoElectricity | Citizen.Flags.NoWater | Citizen.Flags.NoSewage | Citizen.Flags.BadHealth | Citizen.Flags.Created | Citizen.Flags.Tourist | Citizen.Flags.Sick | Citizen.Flags.Dead | Citizen.Flags.Student | Citizen.Flags.MovingIn | Citizen.Flags.DummyTraffic | Citizen.Flags.Criminal | Citizen.Flags.Arrested | Citizen.Flags.Evacuating | Citizen.Flags.Collapsed | Citizen.Flags.Education1 | Citizen.Flags.Education2 | Citizen.Flags.Education3 | Citizen.Flags.NeedGoods | Citizen.Flags.Original | Citizen.Flags.CustomName) | (Citizen.Flags) (Mathf.Clamp((int) value, 0, 2) << 17);
    }
  }

  public int NoElectricity
  {
    get
    {
      return (int) ((uint) (this.m_flags & Citizen.Flags.NoElectricity) >> 30);
    }
    set
    {
      this.m_flags = this.m_flags & (Citizen.Flags.Unemployed | Citizen.Flags.Wealth | Citizen.Flags.Location | Citizen.Flags.NoWater | Citizen.Flags.NoSewage | Citizen.Flags.BadHealth | Citizen.Flags.Created | Citizen.Flags.Tourist | Citizen.Flags.Sick | Citizen.Flags.Dead | Citizen.Flags.Student | Citizen.Flags.MovingIn | Citizen.Flags.DummyTraffic | Citizen.Flags.Criminal | Citizen.Flags.Arrested | Citizen.Flags.Evacuating | Citizen.Flags.Collapsed | Citizen.Flags.Education1 | Citizen.Flags.Education2 | Citizen.Flags.Education3 | Citizen.Flags.NeedGoods | Citizen.Flags.Original | Citizen.Flags.CustomName) | (Citizen.Flags) (Mathf.Clamp(value, 0, 3) << 30);
    }
  }

  public int NoWater
  {
    get
    {
      return (int) ((uint) (this.m_flags & Citizen.Flags.NoWater) >> 28);
    }
    set
    {
      this.m_flags = this.m_flags & (Citizen.Flags.Unemployed | Citizen.Flags.Wealth | Citizen.Flags.Location | Citizen.Flags.NoElectricity | Citizen.Flags.NoSewage | Citizen.Flags.BadHealth | Citizen.Flags.Created | Citizen.Flags.Tourist | Citizen.Flags.Sick | Citizen.Flags.Dead | Citizen.Flags.Student | Citizen.Flags.MovingIn | Citizen.Flags.DummyTraffic | Citizen.Flags.Criminal | Citizen.Flags.Arrested | Citizen.Flags.Evacuating | Citizen.Flags.Collapsed | Citizen.Flags.Education1 | Citizen.Flags.Education2 | Citizen.Flags.Education3 | Citizen.Flags.NeedGoods | Citizen.Flags.Original | Citizen.Flags.CustomName) | (Citizen.Flags) (Mathf.Clamp(value, 0, 3) << 28);
    }
  }

  public int NoSewage
  {
    get
    {
      return (int) ((uint) (this.m_flags & Citizen.Flags.NoSewage) >> 26);
    }
    set
    {
      this.m_flags = this.m_flags & (Citizen.Flags.Unemployed | Citizen.Flags.Wealth | Citizen.Flags.Location | Citizen.Flags.NoElectricity | Citizen.Flags.NoWater | Citizen.Flags.BadHealth | Citizen.Flags.Created | Citizen.Flags.Tourist | Citizen.Flags.Sick | Citizen.Flags.Dead | Citizen.Flags.Student | Citizen.Flags.MovingIn | Citizen.Flags.DummyTraffic | Citizen.Flags.Criminal | Citizen.Flags.Arrested | Citizen.Flags.Evacuating | Citizen.Flags.Collapsed | Citizen.Flags.Education1 | Citizen.Flags.Education2 | Citizen.Flags.Education3 | Citizen.Flags.NeedGoods | Citizen.Flags.Original | Citizen.Flags.CustomName) | (Citizen.Flags) (Mathf.Clamp(value, 0, 3) << 26);
    }
  }

  public int BadHealth
  {
    get
    {
      return (int) ((uint) (this.m_flags & Citizen.Flags.BadHealth) >> 24);
    }
    set
    {
      this.m_flags = this.m_flags & (Citizen.Flags.Unemployed | Citizen.Flags.Wealth | Citizen.Flags.Location | Citizen.Flags.NoElectricity | Citizen.Flags.NoWater | Citizen.Flags.NoSewage | Citizen.Flags.Created | Citizen.Flags.Tourist | Citizen.Flags.Sick | Citizen.Flags.Dead | Citizen.Flags.Student | Citizen.Flags.MovingIn | Citizen.Flags.DummyTraffic | Citizen.Flags.Criminal | Citizen.Flags.Arrested | Citizen.Flags.Evacuating | Citizen.Flags.Collapsed | Citizen.Flags.Education1 | Citizen.Flags.Education2 | Citizen.Flags.Education3 | Citizen.Flags.NeedGoods | Citizen.Flags.Original | Citizen.Flags.CustomName) | (Citizen.Flags) (Mathf.Clamp(value, 0, 3) << 24);
    }
  }

  public int Unemployed
  {
    get
    {
      return (int) ((uint) (this.m_flags & Citizen.Flags.Unemployed) >> 21);
    }
    set
    {
      this.m_flags = this.m_flags & (Citizen.Flags.Wealth | Citizen.Flags.Location | Citizen.Flags.NoElectricity | Citizen.Flags.NoWater | Citizen.Flags.NoSewage | Citizen.Flags.BadHealth | Citizen.Flags.Created | Citizen.Flags.Tourist | Citizen.Flags.Sick | Citizen.Flags.Dead | Citizen.Flags.Student | Citizen.Flags.MovingIn | Citizen.Flags.DummyTraffic | Citizen.Flags.Criminal | Citizen.Flags.Arrested | Citizen.Flags.Evacuating | Citizen.Flags.Collapsed | Citizen.Flags.Education1 | Citizen.Flags.Education2 | Citizen.Flags.Education3 | Citizen.Flags.NeedGoods | Citizen.Flags.Original | Citizen.Flags.CustomName) | (Citizen.Flags) (Mathf.Clamp(value, 0, 7) << 21);
    }
  }

  public static Citizen.Gender GetGender(uint citizenID)
  {
    return (Citizen.Gender) ((int) citizenID & 1);
  }

  public CitizenInfo GetCitizenInfo(uint citizenID)
  {
    if (this.m_instance != (ushort) 0)
      return Singleton<CitizenManager>.instance.m_instances.m_buffer[(int) this.m_instance].Info;
    ItemClass.Service service = (this.m_flags & Citizen.Flags.Tourist) == Citizen.Flags.None ? ItemClass.Service.Residential : ItemClass.Service.Tourism;
    Citizen.Gender gender = Citizen.GetGender(citizenID);
    Citizen.AgePhase agePhase = Citizen.GetAgePhase(this.EducationLevel, this.Age);
    Randomizer r = new Randomizer(citizenID);
    return Singleton<CitizenManager>.instance.GetGroupCitizenInfo(ref r, service, gender, Citizen.SubCulture.Generic, agePhase);
  }

  public void SetLocationByBuilding(uint citizenID, ushort buildingID)
  {
    if ((int) buildingID == (int) this.m_workBuilding)
      this.CurrentLocation = Citizen.Location.Work;
    else if ((int) buildingID == (int) this.m_visitBuilding)
      this.CurrentLocation = Citizen.Location.Visit;
    else
      this.CurrentLocation = Citizen.Location.Home;
  }

  public ushort GetBuildingByLocation()
  {
    switch (this.CurrentLocation)
    {
      case Citizen.Location.Home:
        return this.m_homeBuilding;
      case Citizen.Location.Work:
        return this.m_workBuilding;
      case Citizen.Location.Visit:
        return this.m_visitBuilding;
      default:
        return 0;
    }
  }

  public void SetHome(uint citizenID, ushort buildingID, uint unitID)
  {
    if (this.m_homeBuilding != (ushort) 0)
    {
      BuildingManager instance = Singleton<BuildingManager>.instance;
      this.RemoveFromUnits(citizenID, instance.m_buildings.m_buffer[(int) this.m_homeBuilding].m_citizenUnits, CitizenUnit.Flags.Home);
      this.m_homeBuilding = (ushort) 0;
    }
    if (unitID != 0U)
    {
      BuildingManager instance1 = Singleton<BuildingManager>.instance;
      CitizenManager instance2 = Singleton<CitizenManager>.instance;
      if (!this.AddToUnit(citizenID, ref instance2.m_units.m_buffer[(IntPtr) unitID]))
        return;
      this.m_homeBuilding = instance2.m_units.m_buffer[(IntPtr) unitID].m_building;
      this.WealthLevel = Citizen.GetWealthLevel(instance1.m_buildings.m_buffer[(int) this.m_homeBuilding].Info.m_class.m_level);
    }
    else
    {
      if (buildingID == (ushort) 0)
        return;
      BuildingManager instance = Singleton<BuildingManager>.instance;
      if (!this.AddToUnits(citizenID, instance.m_buildings.m_buffer[(int) buildingID].m_citizenUnits, CitizenUnit.Flags.Home))
        return;
      this.m_homeBuilding = buildingID;
      this.WealthLevel = Citizen.GetWealthLevel(instance.m_buildings.m_buffer[(int) this.m_homeBuilding].Info.m_class.m_level);
    }
  }

  public void SetWorkplace(uint citizenID, ushort buildingID, uint unitID)
  {
    if (this.m_workBuilding != (ushort) 0)
    {
      BuildingManager instance = Singleton<BuildingManager>.instance;
      this.RemoveFromUnits(citizenID, instance.m_buildings.m_buffer[(int) this.m_workBuilding].m_citizenUnits, CitizenUnit.Flags.Work | CitizenUnit.Flags.Student);
      this.m_workBuilding = (ushort) 0;
      this.m_flags &= Citizen.Flags.Unemployed | Citizen.Flags.Wealth | Citizen.Flags.Location | Citizen.Flags.NoElectricity | Citizen.Flags.NoWater | Citizen.Flags.NoSewage | Citizen.Flags.BadHealth | Citizen.Flags.Created | Citizen.Flags.Tourist | Citizen.Flags.Sick | Citizen.Flags.Dead | Citizen.Flags.MovingIn | Citizen.Flags.DummyTraffic | Citizen.Flags.Criminal | Citizen.Flags.Arrested | Citizen.Flags.Evacuating | Citizen.Flags.Collapsed | Citizen.Flags.Education1 | Citizen.Flags.Education2 | Citizen.Flags.Education3 | Citizen.Flags.NeedGoods | Citizen.Flags.Original | Citizen.Flags.CustomName;
    }
    if (unitID != 0U)
    {
      CitizenManager instance = Singleton<CitizenManager>.instance;
      if (!this.AddToUnit(citizenID, ref instance.m_units.m_buffer[(IntPtr) unitID]))
        return;
      this.m_workBuilding = instance.m_units.m_buffer[(IntPtr) unitID].m_building;
    }
    else
    {
      if (buildingID == (ushort) 0)
        return;
      BuildingManager instance = Singleton<BuildingManager>.instance;
      if (!this.AddToUnits(citizenID, instance.m_buildings.m_buffer[(int) buildingID].m_citizenUnits, CitizenUnit.Flags.Work))
        return;
      this.m_workBuilding = buildingID;
    }
  }

  public void SetStudentplace(uint citizenID, ushort buildingID, uint unitID)
  {
    if (this.m_workBuilding != (ushort) 0)
    {
      BuildingManager instance = Singleton<BuildingManager>.instance;
      this.RemoveFromUnits(citizenID, instance.m_buildings.m_buffer[(int) this.m_workBuilding].m_citizenUnits, CitizenUnit.Flags.Work | CitizenUnit.Flags.Student);
      this.m_workBuilding = (ushort) 0;
      this.m_flags &= Citizen.Flags.Unemployed | Citizen.Flags.Wealth | Citizen.Flags.Location | Citizen.Flags.NoElectricity | Citizen.Flags.NoWater | Citizen.Flags.NoSewage | Citizen.Flags.BadHealth | Citizen.Flags.Created | Citizen.Flags.Tourist | Citizen.Flags.Sick | Citizen.Flags.Dead | Citizen.Flags.MovingIn | Citizen.Flags.DummyTraffic | Citizen.Flags.Criminal | Citizen.Flags.Arrested | Citizen.Flags.Evacuating | Citizen.Flags.Collapsed | Citizen.Flags.Education1 | Citizen.Flags.Education2 | Citizen.Flags.Education3 | Citizen.Flags.NeedGoods | Citizen.Flags.Original | Citizen.Flags.CustomName;
    }
    if (unitID != 0U)
    {
      CitizenManager instance = Singleton<CitizenManager>.instance;
      if (!this.AddToUnit(citizenID, ref instance.m_units.m_buffer[(IntPtr) unitID]))
        return;
      this.m_workBuilding = instance.m_units.m_buffer[(IntPtr) unitID].m_building;
      this.m_flags |= Citizen.Flags.Student;
    }
    else
    {
      if (buildingID == (ushort) 0)
        return;
      BuildingManager instance = Singleton<BuildingManager>.instance;
      if (!this.AddToUnits(citizenID, instance.m_buildings.m_buffer[(int) buildingID].m_citizenUnits, CitizenUnit.Flags.Student))
        return;
      this.m_workBuilding = buildingID;
      this.m_flags |= Citizen.Flags.Student;
    }
  }

  public void SetVisitplace(uint citizenID, ushort buildingID, uint unitID)
  {
    if (this.m_visitBuilding != (ushort) 0)
    {
      BuildingManager instance = Singleton<BuildingManager>.instance;
      this.RemoveFromUnits(citizenID, instance.m_buildings.m_buffer[(int) this.m_visitBuilding].m_citizenUnits, CitizenUnit.Flags.Visit);
      this.m_visitBuilding = (ushort) 0;
    }
    if (unitID != 0U)
    {
      CitizenManager instance = Singleton<CitizenManager>.instance;
      if (!this.AddToUnit(citizenID, ref instance.m_units.m_buffer[(IntPtr) unitID]))
        return;
      this.m_visitBuilding = instance.m_units.m_buffer[(IntPtr) unitID].m_building;
    }
    else
    {
      if (buildingID == (ushort) 0)
        return;
      BuildingManager instance = Singleton<BuildingManager>.instance;
      if (!this.AddToUnits(citizenID, instance.m_buildings.m_buffer[(int) buildingID].m_citizenUnits, CitizenUnit.Flags.Visit))
        return;
      this.m_visitBuilding = buildingID;
    }
  }

  public void SetVehicle(uint citizenID, ushort vehicleID, uint unitID)
  {
    if (this.m_vehicle != (ushort) 0)
    {
      VehicleManager instance = Singleton<VehicleManager>.instance;
      if (this.RemoveFromUnits(citizenID, instance.m_vehicles.m_buffer[(int) this.m_vehicle].m_citizenUnits, CitizenUnit.Flags.Vehicle) && (this.m_flags & Citizen.Flags.Tourist) != Citizen.Flags.None)
        instance.m_vehicles.m_buffer[m_vehicle].m_touristCount = (ushort) Mathf.Max(0, (int) instance.m_vehicles.m_buffer[(int) this.m_vehicle].m_touristCount - 1);
      this.m_vehicle = (ushort) 0;
    }
    if (unitID != 0U)
    {
      CitizenManager instance1 = Singleton<CitizenManager>.instance;
      if (!this.AddToUnit(citizenID, ref instance1.m_units.m_buffer[(IntPtr) unitID]))
        return;
      this.m_vehicle = instance1.m_units.m_buffer[unitID].m_vehicle;
      if ((this.m_flags & Citizen.Flags.Tourist) == Citizen.Flags.None)
        return;
      VehicleManager instance2 = Singleton<VehicleManager>.instance;
      instance2.m_vehicles.m_buffer[(int) this.m_vehicle].m_touristCount = (ushort) Mathf.Min((int) ushort.MaxValue, (int) instance2.m_vehicles.m_buffer[(int) this.m_vehicle].m_touristCount + 1);
    }
    else
    {
      if (vehicleID == (ushort) 0)
        return;
      VehicleManager instance = Singleton<VehicleManager>.instance;
      if (!this.AddToUnits(citizenID, instance.m_vehicles.m_buffer[(int) vehicleID].m_citizenUnits, CitizenUnit.Flags.Vehicle))
        return;
      this.m_vehicle = vehicleID;
      if ((this.m_flags & Citizen.Flags.Tourist) == Citizen.Flags.None)
        return;
      instance.m_vehicles.m_buffer[(int) this.m_vehicle].m_touristCount = (ushort) Mathf.Min((int) ushort.MaxValue, (int) instance.m_vehicles.m_buffer[(int) this.m_vehicle].m_touristCount + 1);
    }
  }

  public void SetParkedVehicle(uint citizenID, ushort parkedVehicleID)
  {
    if (this.m_parkedVehicle != (ushort) 0)
    {
      Singleton<VehicleManager>.instance.m_parkedVehicles.m_buffer[(int) this.m_parkedVehicle].m_ownerCitizen = 0U;
      Singleton<VehicleManager>.instance.ReleaseParkedVehicle(this.m_parkedVehicle);
      this.m_parkedVehicle = (ushort) 0;
    }
    if (parkedVehicleID == (ushort) 0)
      return;
    Singleton<VehicleManager>.instance.m_parkedVehicles.m_buffer[(int) parkedVehicleID].m_ownerCitizen = citizenID;
    this.m_parkedVehicle = parkedVehicleID;
  }

  public uint GetContainingUnit(uint citizenID, uint units, CitizenUnit.Flags flag)
  {
    CitizenManager instance = Singleton<CitizenManager>.instance;
    int num = 0;
    while (units != 0U)
    {
      uint nextUnit = instance.m_units.m_buffer[(IntPtr) units].m_nextUnit;
      if ((instance.m_units.m_buffer[(IntPtr) units].m_flags & flag) != CitizenUnit.Flags.None && instance.m_units.m_buffer[(IntPtr) units].ContainsCitizen(citizenID))
        return units;
      units = nextUnit;
      if (++num > 524288)
      {
        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
        break;
      }
    }
    return 0;
  }

  public bool AddToUnits(uint citizenID, uint units, CitizenUnit.Flags flag)
  {
    CitizenManager instance = Singleton<CitizenManager>.instance;
    int num = 0;
    while (units != 0U)
    {
      uint nextUnit = instance.m_units.m_buffer[(IntPtr) units].m_nextUnit;
      if ((instance.m_units.m_buffer[(IntPtr) units].m_flags & flag) != CitizenUnit.Flags.None && this.AddToUnit(citizenID, ref instance.m_units.m_buffer[(IntPtr) units]))
        return true;
      units = nextUnit;
      if (++num > 524288)
      {
        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
        break;
      }
    }
    return false;
  }

  public bool AddToUnit(uint citizenID, ref CitizenUnit data)
  {
    if (data.m_citizen0 == 0U)
    {
      data.m_citizen0 = citizenID;
      return true;
    }
    if (data.m_citizen1 == 0U)
    {
      data.m_citizen1 = citizenID;
      return true;
    }
    if (data.m_citizen2 == 0U)
    {
      data.m_citizen2 = citizenID;
      return true;
    }
    if (data.m_citizen3 == 0U)
    {
      data.m_citizen3 = citizenID;
      return true;
    }
    if (data.m_citizen4 != 0U)
      return false;
    data.m_citizen4 = citizenID;
    return true;
  }

  public bool RemoveFromUnits(uint citizenID, uint units, CitizenUnit.Flags flag)
  {
    CitizenManager instance = Singleton<CitizenManager>.instance;
    int num = 0;
    while (units != 0U)
    {
      uint nextUnit = instance.m_units.m_buffer[(IntPtr) units].m_nextUnit;
      if ((instance.m_units.m_buffer[(IntPtr) units].m_flags & flag) != CitizenUnit.Flags.None && this.RemoveFromUnit(citizenID, ref instance.m_units.m_buffer[(IntPtr) units]))
        return true;
      units = nextUnit;
      if (++num > 524288)
      {
        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
        break;
      }
    }
    return false;
  }

  public bool RemoveFromUnit(uint citizenID, ref CitizenUnit data)
  {
    if ((int) data.m_citizen0 == (int) citizenID)
    {
      data.m_citizen0 = 0U;
      return true;
    }
    if ((int) data.m_citizen1 == (int) citizenID)
    {
      data.m_citizen1 = 0U;
      return true;
    }
    if ((int) data.m_citizen2 == (int) citizenID)
    {
      data.m_citizen2 = 0U;
      return true;
    }
    if ((int) data.m_citizen3 == (int) citizenID)
    {
      data.m_citizen3 = 0U;
      return true;
    }
    if ((int) data.m_citizen4 != (int) citizenID)
      return false;
    data.m_citizen4 = 0U;
    return true;
  }

  public static Citizen.Health GetHealthLevel(int health)
  {
    if (health <= 10)
      return Citizen.Health.VerySick;
    if (health <= 25)
      return Citizen.Health.Sick;
    if (health <= 50)
      return Citizen.Health.PoorHealth;
    if (health <= 60)
      return Citizen.Health.Healthy;
    return health <= 80 ? Citizen.Health.VeryHealthy : Citizen.Health.ExcellentHealth;
  }

  public static Citizen.Wellbeing GetWellbeingLevel(
    Citizen.Education education,
    int wellbeing)
  {
    switch (education)
    {
      case Citizen.Education.Uneducated:
        if (wellbeing >= 60)
          return Citizen.Wellbeing.VeryHappy;
        if (wellbeing >= 35)
          return Citizen.Wellbeing.Happy;
        if (wellbeing >= 25)
          return Citizen.Wellbeing.Satisfied;
        return wellbeing >= 10 ? Citizen.Wellbeing.Unhappy : Citizen.Wellbeing.VeryUnhappy;
      case Citizen.Education.OneSchool:
        if (wellbeing >= 70)
          return Citizen.Wellbeing.VeryHappy;
        if (wellbeing >= 45)
          return Citizen.Wellbeing.Happy;
        if (wellbeing >= 31)
          return Citizen.Wellbeing.Satisfied;
        return wellbeing >= 16 ? Citizen.Wellbeing.Unhappy : Citizen.Wellbeing.VeryUnhappy;
      case Citizen.Education.TwoSchools:
        if (wellbeing >= 80)
          return Citizen.Wellbeing.VeryHappy;
        if (wellbeing >= 60)
          return Citizen.Wellbeing.Happy;
        if (wellbeing >= 40)
          return Citizen.Wellbeing.Satisfied;
        return wellbeing >= 20 ? Citizen.Wellbeing.Unhappy : Citizen.Wellbeing.VeryUnhappy;
      default:
        if (wellbeing >= 85)
          return Citizen.Wellbeing.VeryHappy;
        if (wellbeing >= 63)
          return Citizen.Wellbeing.Happy;
        if (wellbeing >= 50)
          return Citizen.Wellbeing.Satisfied;
        return wellbeing >= 26 ? Citizen.Wellbeing.Unhappy : Citizen.Wellbeing.VeryUnhappy;
    }
  }

  public static int GetHappiness(int health, int wellbeing)
  {
    return health + wellbeing + 1 >> 1;
  }

  public static Citizen.Happiness GetHappinessLevel(int happiness)
  {
    if (happiness <= 15)
      return Citizen.Happiness.Bad;
    if (happiness <= 30)
      return Citizen.Happiness.Poor;
    if (happiness <= 44)
      return Citizen.Happiness.Good;
    return happiness <= 69 ? Citizen.Happiness.Excellent : Citizen.Happiness.Suberb;
  }

  public static Citizen.AgeGroup GetAgeGroup(int age)
  {
    if (age < 15)
      return Citizen.AgeGroup.Child;
    if (age < 45)
      return Citizen.AgeGroup.Teen;
    if (age < 90)
      return Citizen.AgeGroup.Young;
    return age < 180 ? Citizen.AgeGroup.Adult : Citizen.AgeGroup.Senior;
  }

  public static Citizen.AgePhase GetAgePhase(Citizen.Education education, int age)
  {
    if (age < 15)
      return Citizen.AgePhase.Child;
    if (age < 45)
      return (Citizen.AgePhase) (1 + education);
    if (age < 90)
      return (Citizen.AgePhase) (3 + education);
    if (age < 180)
      return (Citizen.AgePhase) (6 + education);
    return (Citizen.AgePhase) (10 + education);
  }

  public static Citizen.Wealth GetWealthLevel(ItemClass.Level homeLevel)
  {
    switch (homeLevel)
    {
      case ItemClass.Level.Level1:
        return Citizen.Wealth.Low;
      case ItemClass.Level.Level2:
      case ItemClass.Level.Level3:
        return Citizen.Wealth.Medium;
      default:
        return Citizen.Wealth.High;
    }
  }

  public static int GetWorkProbability(Citizen.Wellbeing wellbeingLevel, Citizen.Wealth wealthLevel)
  {
    switch (wealthLevel)
    {
      case Citizen.Wealth.Low:
        switch (wellbeingLevel)
        {
          case Citizen.Wellbeing.VeryUnhappy:
            return 50;
          case Citizen.Wellbeing.Unhappy:
            return 70;
          case Citizen.Wellbeing.Satisfied:
            return 80;
          case Citizen.Wellbeing.Happy:
            return 90;
          case Citizen.Wellbeing.VeryHappy:
            return 100;
        }
      case Citizen.Wealth.Medium:
        switch (wellbeingLevel)
        {
          case Citizen.Wellbeing.VeryUnhappy:
            return 40;
          case Citizen.Wellbeing.Unhappy:
            return 55;
          case Citizen.Wellbeing.Satisfied:
            return 70;
          case Citizen.Wellbeing.Happy:
            return 85;
          case Citizen.Wellbeing.VeryHappy:
            return 100;
        }
      default:
        switch (wellbeingLevel)
        {
          case Citizen.Wellbeing.VeryUnhappy:
            return 20;
          case Citizen.Wellbeing.Unhappy:
            return 40;
          case Citizen.Wellbeing.Satisfied:
            return 60;
          case Citizen.Wellbeing.Happy:
            return 80;
          case Citizen.Wellbeing.VeryHappy:
            return 100;
        }
    }
    return 0;
  }

  public static int GetWorkEfficiency(Citizen.Health healthLevel)
  {
    switch (healthLevel)
    {
      case Citizen.Health.VerySick:
        return 10;
      case Citizen.Health.Sick:
        return 30;
      case Citizen.Health.PoorHealth:
        return 50;
      case Citizen.Health.Healthy:
        return 80;
      case Citizen.Health.VeryHealthy:
        return 90;
      case Citizen.Health.ExcellentHealth:
        return 100;
      default:
        return 0;
    }
  }

  public static int GetCrimeRate(int unemploymentLength)
  {
    switch (unemploymentLength)
    {
      case 0:
        return 10;
      case 1:
        return 15;
      case 2:
        return 20;
      case 3:
        return 25;
      case 4:
        return 35;
      case 5:
        return 50;
      default:
        return 50;
    }
  }

  public static int GetMaxCrimeRate(Citizen.Wellbeing happinessLevel)
  {
    switch (happinessLevel)
    {
      case Citizen.Wellbeing.VeryUnhappy:
        return 100;
      case Citizen.Wellbeing.Unhappy:
        return 85;
      case Citizen.Wellbeing.Satisfied:
        return 70;
      case Citizen.Wellbeing.Happy:
        return 55;
      case Citizen.Wellbeing.VeryHappy:
        return 40;
      default:
        return 0;
    }
  }

  public static int GetCrimeRate(
    Citizen.Wellbeing happinessLevel,
    int unemploymentLength,
    bool isCriminal)
  {
    int maxCrimeRate = Citizen.GetMaxCrimeRate(happinessLevel);
    int crimeRate = Citizen.GetCrimeRate(unemploymentLength);
    if (isCriminal)
      return Mathf.Min(crimeRate << 1, maxCrimeRate) << 1;
    return Mathf.Min(crimeRate, maxCrimeRate) >> 1;
  }

  public static int GetIncomeRate(Citizen.AgePhase agePhase, int unemploymentLength)
  {
    switch (agePhase)
    {
      case Citizen.AgePhase.Child:
        return 25;
      case Citizen.AgePhase.Teen0:
        return 25;
      case Citizen.AgePhase.Teen1:
        return 35;
      case Citizen.AgePhase.Young0:
        return 50;
      case Citizen.AgePhase.Young1:
        return 60;
      case Citizen.AgePhase.Young2:
        return 70;
      case Citizen.AgePhase.Adult0:
        return unemploymentLength != 0 ? 75 : 100;
      case Citizen.AgePhase.Adult1:
        return unemploymentLength != 0 ? 75 : 110;
      case Citizen.AgePhase.Adult2:
        return unemploymentLength != 0 ? 75 : 120;
      case Citizen.AgePhase.Adult3:
        return unemploymentLength != 0 ? 75 : 130;
      case Citizen.AgePhase.Senior0:
        return 100;
      case Citizen.AgePhase.Senior1:
        return 110;
      case Citizen.AgePhase.Senior2:
        return 120;
      case Citizen.AgePhase.Senior3:
        return 130;
      default:
        return 0;
    }
  }

  public static int GetHealthCareRequirement(Citizen.AgePhase agePhase)
  {
    switch (agePhase)
    {
      case Citizen.AgePhase.Child:
        return 150;
      case Citizen.AgePhase.Teen0:
        return 30;
      case Citizen.AgePhase.Teen1:
        return 30;
      case Citizen.AgePhase.Young0:
        return 60;
      case Citizen.AgePhase.Young1:
        return 60;
      case Citizen.AgePhase.Young2:
        return 60;
      case Citizen.AgePhase.Adult0:
        return 100;
      case Citizen.AgePhase.Adult1:
        return 100;
      case Citizen.AgePhase.Adult2:
        return 100;
      case Citizen.AgePhase.Adult3:
        return 125;
      case Citizen.AgePhase.Senior0:
        return 200;
      case Citizen.AgePhase.Senior1:
        return 200;
      case Citizen.AgePhase.Senior2:
        return 200;
      case Citizen.AgePhase.Senior3:
        return 200;
      default:
        return 0;
    }
  }

  public static int GetDeathCareRequirement(Citizen.AgePhase agePhase)
  {
    switch (agePhase)
    {
      case Citizen.AgePhase.Child:
        return 0;
      case Citizen.AgePhase.Teen0:
        return 0;
      case Citizen.AgePhase.Teen1:
        return 0;
      case Citizen.AgePhase.Young0:
        return 10;
      case Citizen.AgePhase.Young1:
        return 15;
      case Citizen.AgePhase.Young2:
        return 20;
      case Citizen.AgePhase.Adult0:
        return 50;
      case Citizen.AgePhase.Adult1:
        return 60;
      case Citizen.AgePhase.Adult2:
        return 70;
      case Citizen.AgePhase.Adult3:
        return 80;
      case Citizen.AgePhase.Senior0:
        return 100;
      case Citizen.AgePhase.Senior1:
        return 120;
      case Citizen.AgePhase.Senior2:
        return 140;
      case Citizen.AgePhase.Senior3:
        return 160;
      default:
        return 0;
    }
  }

  public static int GetPoliceDepartmentRequirement(Citizen.AgePhase agePhase)
  {
    switch (agePhase)
    {
      case Citizen.AgePhase.Child:
        return 25;
      case Citizen.AgePhase.Teen0:
        return 60;
      case Citizen.AgePhase.Teen1:
        return 60;
      case Citizen.AgePhase.Young0:
        return 60;
      case Citizen.AgePhase.Young1:
        return 60;
      case Citizen.AgePhase.Young2:
        return 60;
      case Citizen.AgePhase.Adult0:
        return 100;
      case Citizen.AgePhase.Adult1:
        return 100;
      case Citizen.AgePhase.Adult2:
        return 150;
      case Citizen.AgePhase.Adult3:
        return 150;
      case Citizen.AgePhase.Senior0:
        return 150;
      case Citizen.AgePhase.Senior1:
        return 150;
      case Citizen.AgePhase.Senior2:
        return 200;
      case Citizen.AgePhase.Senior3:
        return 200;
      default:
        return 0;
    }
  }

  public static int GetFireDepartmentRequirement(Citizen.AgePhase agePhase)
  {
    switch (agePhase)
    {
      case Citizen.AgePhase.Child:
        return 25;
      case Citizen.AgePhase.Teen0:
        return 60;
      case Citizen.AgePhase.Teen1:
        return 60;
      case Citizen.AgePhase.Young0:
        return 60;
      case Citizen.AgePhase.Young1:
        return 60;
      case Citizen.AgePhase.Young2:
        return 60;
      case Citizen.AgePhase.Adult0:
        return 100;
      case Citizen.AgePhase.Adult1:
        return 100;
      case Citizen.AgePhase.Adult2:
        return 150;
      case Citizen.AgePhase.Adult3:
        return 150;
      case Citizen.AgePhase.Senior0:
        return 150;
      case Citizen.AgePhase.Senior1:
        return 150;
      case Citizen.AgePhase.Senior2:
        return 200;
      case Citizen.AgePhase.Senior3:
        return 200;
      default:
        return 0;
    }
  }

  public static int GetEducationRequirement(Citizen.AgePhase agePhase)
  {
    switch (agePhase)
    {
      case Citizen.AgePhase.Child:
        return 200;
      case Citizen.AgePhase.Teen0:
        return 200;
      case Citizen.AgePhase.Teen1:
        return 200;
      case Citizen.AgePhase.Young0:
        return 150;
      case Citizen.AgePhase.Young1:
        return 150;
      case Citizen.AgePhase.Young2:
        return 150;
      case Citizen.AgePhase.Adult0:
        return 100;
      case Citizen.AgePhase.Adult1:
        return 60;
      case Citizen.AgePhase.Adult2:
        return 30;
      case Citizen.AgePhase.Adult3:
        return 0;
      case Citizen.AgePhase.Senior0:
        return 0;
      case Citizen.AgePhase.Senior1:
        return 0;
      case Citizen.AgePhase.Senior2:
        return 0;
      case Citizen.AgePhase.Senior3:
        return 0;
      default:
        return 0;
    }
  }

  public static int GetEntertainmentRequirement(Citizen.AgePhase agePhase)
  {
    switch (agePhase)
    {
      case Citizen.AgePhase.Child:
        return 120;
      case Citizen.AgePhase.Teen0:
        return 130;
      case Citizen.AgePhase.Teen1:
        return 140;
      case Citizen.AgePhase.Young0:
        return 150;
      case Citizen.AgePhase.Young1:
        return 160;
      case Citizen.AgePhase.Young2:
        return 170;
      case Citizen.AgePhase.Adult0:
        return 50;
      case Citizen.AgePhase.Adult1:
        return 60;
      case Citizen.AgePhase.Adult2:
        return 70;
      case Citizen.AgePhase.Adult3:
        return 80;
      case Citizen.AgePhase.Senior0:
        return 100;
      case Citizen.AgePhase.Senior1:
        return 110;
      case Citizen.AgePhase.Senior2:
        return 120;
      case Citizen.AgePhase.Senior3:
        return 130;
      default:
        return 0;
    }
  }

  public static int GetTransportRequirement(Citizen.AgePhase agePhase)
  {
    switch (agePhase)
    {
      case Citizen.AgePhase.Child:
        return 200;
      case Citizen.AgePhase.Teen0:
        return 100;
      case Citizen.AgePhase.Teen1:
        return 110;
      case Citizen.AgePhase.Young0:
        return 60;
      case Citizen.AgePhase.Young1:
        return 70;
      case Citizen.AgePhase.Young2:
        return 80;
      case Citizen.AgePhase.Adult0:
        return 50;
      case Citizen.AgePhase.Adult1:
        return 60;
      case Citizen.AgePhase.Adult2:
        return 70;
      case Citizen.AgePhase.Adult3:
        return 80;
      case Citizen.AgePhase.Senior0:
        return 100;
      case Citizen.AgePhase.Senior1:
        return 110;
      case Citizen.AgePhase.Senior2:
        return 120;
      case Citizen.AgePhase.Senior3:
        return 130;
      default:
        return 0;
    }
  }

  public static int GetWorkRequirement(Citizen.AgePhase agePhase)
  {
    switch (agePhase)
    {
      case Citizen.AgePhase.Child:
        return 0;
      case Citizen.AgePhase.Teen0:
        return 0;
      case Citizen.AgePhase.Teen1:
        return 0;
      case Citizen.AgePhase.Young0:
        return 60;
      case Citizen.AgePhase.Young1:
        return 60;
      case Citizen.AgePhase.Young2:
        return 60;
      case Citizen.AgePhase.Adult0:
        return 150;
      case Citizen.AgePhase.Adult1:
        return 150;
      case Citizen.AgePhase.Adult2:
        return 150;
      case Citizen.AgePhase.Adult3:
        return 150;
      case Citizen.AgePhase.Senior0:
        return 0;
      case Citizen.AgePhase.Senior1:
        return 0;
      case Citizen.AgePhase.Senior2:
        return 0;
      case Citizen.AgePhase.Senior3:
        return 0;
      default:
        return 0;
    }
  }

  public static int GetElectricityConsumption(Citizen.Education educationLevel)
  {
    switch (educationLevel)
    {
      case Citizen.Education.Uneducated:
        return 100;
      case Citizen.Education.OneSchool:
        return 90;
      case Citizen.Education.TwoSchools:
        return 80;
      case Citizen.Education.ThreeSchools:
        return 70;
      default:
        return 0;
    }
  }

  public static int GetWaterConsumption(Citizen.Education educationLevel)
  {
    switch (educationLevel)
    {
      case Citizen.Education.Uneducated:
        return 100;
      case Citizen.Education.OneSchool:
        return 90;
      case Citizen.Education.TwoSchools:
        return 80;
      case Citizen.Education.ThreeSchools:
        return 70;
      default:
        return 0;
    }
  }

  public static int GetSewageAccumulation(Citizen.Education educationLevel)
  {
    switch (educationLevel)
    {
      case Citizen.Education.Uneducated:
        return 100;
      case Citizen.Education.OneSchool:
        return 90;
      case Citizen.Education.TwoSchools:
        return 80;
      case Citizen.Education.ThreeSchools:
        return 70;
      default:
        return 0;
    }
  }

  public static int GetGarbageAccumulation(Citizen.Education educationLevel)
  {
    switch (educationLevel)
    {
      case Citizen.Education.Uneducated:
        return 100;
      case Citizen.Education.OneSchool:
        return 90;
      case Citizen.Education.TwoSchools:
        return 80;
      case Citizen.Education.ThreeSchools:
        return 70;
      default:
        return 0;
    }
  }

  public static int GetMailAccumulation(Citizen.Education educationLevel)
  {
    switch (educationLevel)
    {
      case Citizen.Education.Uneducated:
        return 70;
      case Citizen.Education.OneSchool:
        return 80;
      case Citizen.Education.TwoSchools:
        return 90;
      case Citizen.Education.ThreeSchools:
        return 100;
      default:
        return 0;
    }
  }

  public void GetCitizenHomeBehaviour(
    ref Citizen.BehaviourData behaviour,
    ref int aliveCount,
    ref int totalCount)
  {
    if ((this.m_flags & Citizen.Flags.MovingIn) == Citizen.Flags.None)
    {
      if (this.Dead)
      {
        if (this.CurrentLocation == Citizen.Location.Home)
          ++behaviour.m_deadCount;
      }
      else
      {
        if (this.Sick)
        {
          ++behaviour.m_totalSickCount;
          if (this.CurrentLocation == Citizen.Location.Home)
            ++behaviour.m_sickCount;
        }
        int age = this.Age;
        Citizen.Education educationLevel = this.EducationLevel;
        Citizen.AgePhase agePhase = Citizen.GetAgePhase(educationLevel, age);
        Citizen.AgeGroup ageGroup = Citizen.GetAgeGroup(age);
        int unemployed = this.Unemployed;
        int crimeRate = Citizen.GetCrimeRate(Citizen.GetWellbeingLevel(educationLevel, (int) this.m_wellbeing), unemployed, this.Criminal);
        behaviour.m_electricityConsumption += Citizen.GetElectricityConsumption(educationLevel);
        behaviour.m_waterConsumption += Citizen.GetWaterConsumption(educationLevel);
        behaviour.m_sewageAccumulation += Citizen.GetSewageAccumulation(educationLevel);
        behaviour.m_garbageAccumulation += Citizen.GetGarbageAccumulation(educationLevel);
        behaviour.m_mailAccumulation += Citizen.GetMailAccumulation(educationLevel);
        behaviour.m_incomeAccumulation += Citizen.GetIncomeRate(agePhase, unemployed);
        if (!this.Arrested)
          behaviour.m_crimeAccumulation += crimeRate;
        behaviour.m_healthAccumulation += (int) this.m_health;
        behaviour.m_wellbeingAccumulation += (int) this.m_wellbeing;
        switch (ageGroup)
        {
          case Citizen.AgeGroup.Child:
            if (!this.Education1)
              ++behaviour.m_elementaryEligibleCount;
            ++behaviour.m_childCount;
            break;
          case Citizen.AgeGroup.Teen:
            if (this.Education1 && !this.Education2)
              ++behaviour.m_highschoolEligibleCount;
            ++behaviour.m_teenCount;
            break;
          case Citizen.AgeGroup.Young:
            if (this.Education1 && this.Education2 && !this.Education3)
              ++behaviour.m_universityEligibleCount;
            ++behaviour.m_youngCount;
            break;
          case Citizen.AgeGroup.Adult:
            if (this.Education1 && this.Education2 && !this.Education3)
              ++behaviour.m_universityEligibleCount;
            ++behaviour.m_adultCount;
            break;
          case Citizen.AgeGroup.Senior:
            ++behaviour.m_seniorCount;
            break;
        }
        if (this.Education1)
          ++behaviour.m_education1Count;
        if (this.Education2)
          ++behaviour.m_education2Count;
        if (this.Education3)
          ++behaviour.m_education3Count;
        switch (educationLevel)
        {
          case Citizen.Education.Uneducated:
            ++behaviour.m_educated0Count;
            break;
          case Citizen.Education.OneSchool:
            ++behaviour.m_educated1Count;
            break;
          case Citizen.Education.TwoSchools:
            ++behaviour.m_educated2Count;
            break;
          case Citizen.Education.ThreeSchools:
            ++behaviour.m_educated3Count;
            break;
        }
        if (unemployed != 0)
        {
          switch (educationLevel)
          {
            case Citizen.Education.Uneducated:
              ++behaviour.m_educated0Unemployed;
              break;
            case Citizen.Education.OneSchool:
              ++behaviour.m_educated1Unemployed;
              break;
            case Citizen.Education.TwoSchools:
              ++behaviour.m_educated2Unemployed;
              break;
            case Citizen.Education.ThreeSchools:
              ++behaviour.m_educated3Unemployed;
              break;
          }
        }
        if (ageGroup == Citizen.AgeGroup.Young || ageGroup == Citizen.AgeGroup.Adult)
        {
          switch (educationLevel)
          {
            case Citizen.Education.Uneducated:
              ++behaviour.m_educated0EligibleWorkers;
              break;
            case Citizen.Education.OneSchool:
              ++behaviour.m_educated1EligibleWorkers;
              break;
            case Citizen.Education.TwoSchools:
              ++behaviour.m_educated2EligibleWorkers;
              break;
            case Citizen.Education.ThreeSchools:
              ++behaviour.m_educated3EligibleWorkers;
              break;
          }
        }
        ++aliveCount;
      }
    }
    ++totalCount;
  }

  public void GetCitizenWorkBehaviour(
    ref Citizen.BehaviourData behaviour,
    ref int aliveCount,
    ref int totalCount)
  {
    if ((this.m_flags & Citizen.Flags.MovingIn) == Citizen.Flags.None)
    {
      if (this.Dead)
      {
        if (this.CurrentLocation == Citizen.Location.Work)
          ++behaviour.m_deadCount;
      }
      else
      {
        if (this.CurrentLocation == Citizen.Location.Work && this.Sick)
          ++behaviour.m_sickCount;
        Citizen.Wealth wealthLevel = this.WealthLevel;
        Citizen.Education educationLevel = this.EducationLevel;
        int crimeRate = Citizen.GetCrimeRate(Citizen.GetWellbeingLevel(educationLevel, (int) this.m_wellbeing), 0, this.Criminal);
        int workProbability = Citizen.GetWorkProbability(Citizen.GetWellbeingLevel(educationLevel, (int) this.m_wellbeing), wealthLevel);
        if (workProbability != 0 && Singleton<SimulationManager>.instance.m_randomizer.Int32(100U) < workProbability)
          behaviour.m_efficiencyAccumulation += Citizen.GetWorkEfficiency(Citizen.GetHealthLevel((int) this.m_health));
        switch (educationLevel)
        {
          case Citizen.Education.Uneducated:
            ++behaviour.m_educated0Count;
            break;
          case Citizen.Education.OneSchool:
            ++behaviour.m_educated1Count;
            break;
          case Citizen.Education.TwoSchools:
            ++behaviour.m_educated2Count;
            break;
          case Citizen.Education.ThreeSchools:
            ++behaviour.m_educated3Count;
            break;
        }
        if (!this.Arrested)
          behaviour.m_crimeAccumulation += crimeRate;
        behaviour.m_healthAccumulation += (int) this.m_health;
        behaviour.m_wellbeingAccumulation += (int) this.m_wellbeing;
        ++aliveCount;
      }
    }
    ++totalCount;
  }

  public void GetCitizenStudentBehaviour(
    ref Citizen.BehaviourData behaviour,
    ref int aliveCount,
    ref int totalCount)
  {
    if ((this.m_flags & Citizen.Flags.MovingIn) == Citizen.Flags.None)
    {
      if (this.Dead)
      {
        if (this.CurrentLocation == Citizen.Location.Work)
          ++behaviour.m_deadCount;
      }
      else
      {
        if (this.CurrentLocation == Citizen.Location.Work && this.Sick)
          ++behaviour.m_sickCount;
        int crimeRate = Citizen.GetCrimeRate(Citizen.GetWellbeingLevel(this.EducationLevel, (int) this.m_wellbeing), 0, this.Criminal);
        if (!this.Arrested)
          behaviour.m_crimeAccumulation += crimeRate;
        behaviour.m_healthAccumulation += (int) this.m_health;
        behaviour.m_wellbeingAccumulation += (int) this.m_wellbeing;
        ++aliveCount;
      }
    }
    ++totalCount;
  }

  public void GetCitizenVisitBehaviour(
    ref Citizen.BehaviourData behaviour,
    ref int aliveCount,
    ref int totalCount)
  {
    if ((this.m_flags & Citizen.Flags.MovingIn) == Citizen.Flags.None)
    {
      if (this.Dead)
      {
        if (this.CurrentLocation == Citizen.Location.Visit)
          ++behaviour.m_deadCount;
      }
      else if (this.CurrentLocation == Citizen.Location.Visit)
      {
        if (this.CurrentLocation == Citizen.Location.Visit && this.Sick)
          ++behaviour.m_sickCount;
        Citizen.Education educationLevel = this.EducationLevel;
        Citizen.Wealth wealthLevel = this.WealthLevel;
        int unemployed = this.Unemployed;
        int crimeRate = Citizen.GetCrimeRate(Citizen.GetWellbeingLevel(educationLevel, (int) this.m_wellbeing), unemployed, this.Criminal);
        switch (wealthLevel)
        {
          case Citizen.Wealth.Low:
            ++behaviour.m_wealth1Count;
            break;
          case Citizen.Wealth.Medium:
            ++behaviour.m_wealth2Count;
            break;
          case Citizen.Wealth.High:
            ++behaviour.m_wealth3Count;
            break;
        }
        behaviour.m_crimeAccumulation += crimeRate;
        behaviour.m_healthAccumulation += (int) this.m_health;
        behaviour.m_wellbeingAccumulation += (int) this.m_wellbeing;
        if ((this.m_flags & Citizen.Flags.Tourist) != Citizen.Flags.None)
          ++behaviour.m_touristCount;
        ++aliveCount;
      }
    }
    ++totalCount;
  }

  [System.Flags]
  public enum Flags
  {
    None = 0,
    Created = 1,
    Tourist = 2,
    Sick = 4,
    Dead = 8,
    Student = 16, // 0x00000010
    MovingIn = 32, // 0x00000020
    DummyTraffic = 64, // 0x00000040
    Criminal = 128, // 0x00000080
    Arrested = 256, // 0x00000100
    Evacuating = 512, // 0x00000200
    Collapsed = 1024, // 0x00000400
    Education1 = 2048, // 0x00000800
    Education2 = 4096, // 0x00001000
    Education3 = 8192, // 0x00002000
    NeedGoods = 16384, // 0x00004000
    Original = 32768, // 0x00008000
    CustomName = 65536, // 0x00010000
    Wealth = 393216, // 0x00060000
    Location = 1572864, // 0x00180000
    NoElectricity = -1073741824, // -0x40000000
    NoWater = 805306368, // 0x30000000
    NoSewage = 201326592, // 0x0C000000
    BadHealth = 50331648, // 0x03000000
    Unemployed = 14680064, // 0x00E00000
    All = Unemployed | BadHealth | NoSewage | NoWater | NoElectricity | Location | Wealth | CustomName | Original | NeedGoods | Education3 | Education2 | Education1 | Collapsed | Evacuating | Arrested | Criminal | DummyTraffic | MovingIn | Student | Dead | Sick | Tourist | Created, // -0x00000001
  }

  public enum Gender
  {
    Male,
    Female,
  }

  public enum SubCulture
  {
    Generic,
    Hippie,
    Hipster,
    Redneck,
    Gangsta,
  }

  public enum Education
  {
    Uneducated,
    OneSchool,
    TwoSchools,
    ThreeSchools,
  }

  public enum Health
  {
    VerySick,
    Sick,
    PoorHealth,
    Healthy,
    VeryHealthy,
    ExcellentHealth,
  }

  public enum Wellbeing
  {
    VeryUnhappy,
    Unhappy,
    Satisfied,
    Happy,
    VeryHappy,
  }

  public enum Happiness
  {
    Bad,
    Poor,
    Good,
    Excellent,
    Suberb,
  }

  public enum Location
  {
    Home,
    Work,
    Visit,
    Moving,
  }

  public enum Wealth
  {
    Low,
    Medium,
    High,
  }

  public enum AgeGroup
  {
    Child,
    Teen,
    Young,
    Adult,
    Senior,
  }

  public enum AgePhase
  {
    Child,
    Teen0,
    Teen1,
    Young0,
    Young1,
    Young2,
    Adult0,
    Adult1,
    Adult2,
    Adult3,
    Senior0,
    Senior1,
    Senior2,
    Senior3,
  }

  public struct BehaviourData
  {
    public int m_electricityConsumption;
    public int m_waterConsumption;
    public int m_sewageAccumulation;
    public int m_garbageAccumulation;
    public int m_mailAccumulation;
    public int m_incomeAccumulation;
    public int m_crimeAccumulation;
    public int m_healthAccumulation;
    public int m_wellbeingAccumulation;
    public int m_efficiencyAccumulation;
    public int m_sickCount;
    public int m_totalSickCount;
    public int m_deadCount;
    public int m_childCount;
    public int m_teenCount;
    public int m_youngCount;
    public int m_adultCount;
    public int m_seniorCount;
    public int m_touristCount;
    public int m_educated0Count;
    public int m_educated0Unemployed;
    public int m_educated0EligibleWorkers;
    public int m_educated1Count;
    public int m_educated1Unemployed;
    public int m_educated1EligibleWorkers;
    public int m_educated2Count;
    public int m_educated2Unemployed;
    public int m_educated2EligibleWorkers;
    public int m_educated3Count;
    public int m_educated3Unemployed;
    public int m_educated3EligibleWorkers;
    public int m_education1Count;
    public int m_education2Count;
    public int m_education3Count;
    public int m_elementaryEligibleCount;
    public int m_highschoolEligibleCount;
    public int m_universityEligibleCount;
    public int m_wealth1Count;
    public int m_wealth2Count;
    public int m_wealth3Count;
  }
}
