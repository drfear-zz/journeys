// Decompiled with JetBrains decompiler
// Type: CitizenManager
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 9724B8F4-19DD-48C3-AE02-CDA150D75CEC
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Cities_Skylines\Cities_Data\Managed\Assembly-CSharp.dll

using ColossalFramework;
using ColossalFramework.IO;
using ColossalFramework.Math;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class CitizenManager : SimulationManagerBase<CitizenManager, CitizenProperties>, ISimulationManager, IRenderableManager, IAudibleManager
{
  public const float CITIZENGRID_CELL_SIZE = 8f;
  public const int CITIZENGRID_RESOLUTION = 2160;
  public const int MAX_CITIZEN_COUNT = 1048576;
  public const int MAX_UNIT_COUNT = 524288;
  public const int MAX_INSTANCE_COUNT = 65536;
  public int m_citizenCount;
  public int m_unitCount;
  public int m_instanceCount;
  public int m_infoCount;
  [NonSerialized]
  public Array32<Citizen> m_citizens;
  [NonSerialized]
  public Array32<CitizenUnit> m_units;
  [NonSerialized]
  public Array16<CitizenInstance> m_instances;
  [NonSerialized]
  public ushort[] m_citizenGrid;
  [NonSerialized]
  public MaterialPropertyBlock m_materialBlock;
  [NonSerialized]
  public int ID_Color;
  [NonSerialized]
  public int ID_Speed;
  [NonSerialized]
  public int ID_State;
  [NonSerialized]
  public int ID_CitizenLocation;
  [NonSerialized]
  public int ID_CitizenColor;
  [NonSerialized]
  public AudioGroup m_audioGroup;
  [NonSerialized]
  public int m_citizenLayer;
  [NonSerialized]
  public int m_undergroundLayer;
  [NonSerialized]
  public int m_tempOldestOriginalResident;
  [NonSerialized]
  public int m_finalOldestOriginalResident;
  [NonSerialized]
  public int m_fullyEducatedOriginalResidents;
  private FastList<ushort>[] m_groupCitizens;
  private FastList<ushort>[] m_groupAnimals;
  private bool m_citizensRefreshed;
  private ulong[] m_renderBuffer;

  protected override void Awake()
  {
    base.Awake();
    this.m_citizens = new Array32<Citizen>(1048576U);
    this.m_units = new Array32<CitizenUnit>(524288U);
    this.m_instances = new Array16<CitizenInstance>(65536U);
    this.m_citizenGrid = new ushort[4665600];
    this.m_renderBuffer = new ulong[1024];
    this.m_groupCitizens = new FastList<ushort>[812];
    this.m_groupAnimals = new FastList<ushort>[59];
    this.m_materialBlock = new MaterialPropertyBlock();
    this.ID_Color = Shader.PropertyToID("_Color");
    this.ID_Speed = Animator.StringToHash("Speed");
    this.ID_State = Animator.StringToHash("State");
    this.ID_CitizenLocation = Shader.PropertyToID("_CitizenLocation");
    this.ID_CitizenColor = Shader.PropertyToID("_CitizenColor");
    this.m_citizenLayer = LayerMask.NameToLayer("Citizens");
    this.m_undergroundLayer = LayerMask.NameToLayer("MetroTunnels");
    this.m_audioGroup = new AudioGroup(5, new SavedFloat(Settings.effectAudioVolume, Settings.gameSettingsFile, DefaultSettings.effectAudioVolume, true));
    uint num1;
    this.m_citizens.CreateItem(out num1);
    this.m_units.CreateItem(out num1);
    ushort num2;
    this.m_instances.CreateItem(out num2);
  }

  public override void InitializeProperties(CitizenProperties properties)
  {
    base.InitializeProperties(properties);
  }

  public override void DestroyProperties(CitizenProperties properties)
  {
    if ((UnityEngine.Object) this.m_properties == (UnityEngine.Object) properties && this.m_audioGroup != null)
      this.m_audioGroup.Reset();
    base.DestroyProperties(properties);
  }

  protected override void EndRenderingImpl(RenderManager.CameraInfo cameraInfo)
  {
    float levelOfDetailFactor = RenderManager.LevelOfDetailFactor;
    float near = cameraInfo.m_near;
    float num1 = Mathf.Min(Mathf.Min(levelOfDetailFactor * 800f, (float) ((double) levelOfDetailFactor * 400.0 + (double) cameraInfo.m_height * 0.5)), cameraInfo.m_far);
    Vector3 lhs1 = cameraInfo.m_position + cameraInfo.m_directionA * near;
    Vector3 rhs1 = cameraInfo.m_position + cameraInfo.m_directionB * near;
    Vector3 lhs2 = cameraInfo.m_position + cameraInfo.m_directionC * near;
    Vector3 rhs2 = cameraInfo.m_position + cameraInfo.m_directionD * near;
    Vector3 lhs3 = cameraInfo.m_position + cameraInfo.m_directionA * num1;
    Vector3 rhs3 = cameraInfo.m_position + cameraInfo.m_directionB * num1;
    Vector3 lhs4 = cameraInfo.m_position + cameraInfo.m_directionC * num1;
    Vector3 rhs4 = cameraInfo.m_position + cameraInfo.m_directionD * num1;
    Vector3 vector3_1 = Vector3.Min(Vector3.Min(Vector3.Min(lhs1, rhs1), Vector3.Min(lhs2, rhs2)), Vector3.Min(Vector3.Min(lhs3, rhs3), Vector3.Min(lhs4, rhs4)));
    Vector3 vector3_2 = Vector3.Max(Vector3.Max(Vector3.Max(lhs1, rhs1), Vector3.Max(lhs2, rhs2)), Vector3.Max(Vector3.Max(lhs3, rhs3), Vector3.Max(lhs4, rhs4)));
    int num2 = Mathf.Max((int) (((double) vector3_1.x - 1.0) / 8.0 + 1080.0), 0);
    int num3 = Mathf.Max((int) (((double) vector3_1.z - 1.0) / 8.0 + 1080.0), 0);
    int num4 = Mathf.Min((int) (((double) vector3_2.x + 1.0) / 8.0 + 1080.0), 2159);
    int num5 = Mathf.Min((int) (((double) vector3_2.z + 1.0) / 8.0 + 1080.0), 2159);
    for (int index1 = num3; index1 <= num5; ++index1)
    {
      for (int index2 = num2; index2 <= num4; ++index2)
      {
        ushort num6 = this.m_citizenGrid[index1 * 2160 + index2];
        if (num6 != (ushort) 0)
          this.m_renderBuffer[(int) num6 >> 6] |= (ulong) (1L << (int) num6);
      }
    }
    int length = this.m_renderBuffer.Length;
    for (int index1 = 0; index1 < length; ++index1)
    {
      ulong num6 = this.m_renderBuffer[index1];
      if (num6 != 0UL)
      {
        for (int index2 = 0; index2 < 64; ++index2)
        {
          ulong num7 = 1UL << index2;
          if (((long) num6 & (long) num7) != 0L)
          {
            ushort instanceID = (ushort) (index1 << 6 | index2);
            if (!this.m_instances.m_buffer[(int) instanceID].RenderInstance(cameraInfo, instanceID))
              num6 &= ~num7;
            ushort nextGridInstance = this.m_instances.m_buffer[(int) instanceID].m_nextGridInstance;
            int num8 = 0;
            while (nextGridInstance != (ushort) 0)
            {
              int index3 = (int) nextGridInstance >> 6;
              ulong num9 = 1UL << (int) nextGridInstance;
              if (index3 == index1)
              {
                if (((long) num6 & (long) num9) == 0L)
                  num6 |= num9;
                else
                  break;
              }
              else
              {
                ulong num10 = this.m_renderBuffer[index3];
                if (((long) num10 & (long) num9) == 0L)
                  this.m_renderBuffer[index3] = num10 | num9;
                else
                  break;
              }
              if ((int) nextGridInstance <= (int) instanceID)
              {
                nextGridInstance = this.m_instances.m_buffer[(int) nextGridInstance].m_nextGridInstance;
                if (++num8 > 65536)
                {
                  CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                  break;
                }
              }
              else
                break;
            }
          }
        }
        this.m_renderBuffer[index1] = num6;
      }
    }
    int num11 = PrefabCollection<CitizenInfo>.PrefabCount();
    for (int index = 0; index < num11; ++index)
    {
      CitizenInfo prefab = PrefabCollection<CitizenInfo>.GetPrefab((uint) index);
      if (prefab != null)
      {
        prefab.UpdatePrefabInstances();
        if (prefab.m_lodCount != 0)
          CitizenInstance.RenderLod(cameraInfo, prefab);
        if (prefab.m_undergroundLodCount != 0)
          CitizenInstance.RenderUndergroundLod(cameraInfo, prefab);
      }
    }
  }

  protected override void PlayAudioImpl(AudioManager.ListenerInfo listenerInfo)
  {
    if (!((UnityEngine.Object) this.m_properties != (UnityEngine.Object) null))
      return;
    LoadingManager instance1 = Singleton<LoadingManager>.instance;
    SimulationManager instance2 = Singleton<SimulationManager>.instance;
    AudioManager instance3 = Singleton<AudioManager>.instance;
    float masterVolume = instance1.m_currentlyLoading || instance2.SimulationPaused || instance3.MuteAll ? 0.0f : instance3.MasterVolume;
    this.m_audioGroup.UpdatePlayers(listenerInfo, masterVolume);
  }

  public bool CreateUnits(
    out uint firstUnit,
    ref Randomizer randomizer,
    ushort building,
    ushort vehicle,
    int homeCount,
    int workCount,
    int visitCount,
    int passengerCount,
    int studentCount)
  {
    firstUnit = 0U;
    workCount = (workCount + 4) / 5;
    visitCount = (visitCount + 4) / 5;
    passengerCount = (passengerCount + 4) / 5;
    studentCount = (studentCount + 4) / 5;
    int num1 = homeCount + workCount + visitCount + passengerCount + studentCount;
    if (num1 == 0)
      return true;
    CitizenUnit citizenUnit = new CitizenUnit();
    uint num2 = 0;
    for (int index = 0; index < num1; ++index)
    {
      uint num3;
      if (this.m_units.CreateItem(out num3, ref randomizer))
      {
        if (index == 0)
        {
          firstUnit = num3;
        }
        else
        {
          citizenUnit.m_nextUnit = num3;
          this.m_units.m_buffer[(IntPtr) num2] = citizenUnit;
        }
        citizenUnit = new CitizenUnit();
        citizenUnit.m_flags = CitizenUnit.Flags.Created;
        if (index < homeCount)
        {
          citizenUnit.m_flags |= CitizenUnit.Flags.Home;
          citizenUnit.m_goods = (ushort) 200;
        }
        else if (index < homeCount + workCount)
          citizenUnit.m_flags |= CitizenUnit.Flags.Work;
        else if (index < homeCount + workCount + visitCount)
          citizenUnit.m_flags |= CitizenUnit.Flags.Visit;
        else if (index < homeCount + workCount + visitCount + passengerCount)
          citizenUnit.m_flags |= CitizenUnit.Flags.Vehicle;
        else if (index < homeCount + workCount + visitCount + passengerCount + studentCount)
          citizenUnit.m_flags |= CitizenUnit.Flags.Student;
        citizenUnit.m_building = building;
        citizenUnit.m_vehicle = vehicle;
        num2 = num3;
      }
      else
      {
        this.ReleaseUnits(firstUnit);
        firstUnit = 0U;
        return false;
      }
    }
    this.m_units.m_buffer[(IntPtr) num2] = citizenUnit;
    this.m_unitCount = (int) this.m_units.ItemCount() - 1;
    return true;
  }

  public void ReleaseUnits(uint firstUnit)
  {
    int num = 0;
    while (firstUnit != 0U)
    {
      uint nextUnit = this.m_units.m_buffer[(IntPtr) firstUnit].m_nextUnit;
      this.ReleaseUnitImplementation(firstUnit, ref this.m_units.m_buffer[(IntPtr) firstUnit]);
      firstUnit = nextUnit;
      if (++num > 524288)
      {
        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
        break;
      }
    }
    this.m_unitCount = (int) this.m_units.ItemCount() - 1;
  }

  private void ReleaseUnitImplementation(uint unit, ref CitizenUnit data)
  {
    this.ReleaseUnitCitizen(unit, ref data, data.m_citizen0);
    this.ReleaseUnitCitizen(unit, ref data, data.m_citizen1);
    this.ReleaseUnitCitizen(unit, ref data, data.m_citizen2);
    this.ReleaseUnitCitizen(unit, ref data, data.m_citizen3);
    this.ReleaseUnitCitizen(unit, ref data, data.m_citizen4);
    data = new CitizenUnit();
    this.m_units.ReleaseItem(unit);
  }

  private void ReleaseUnitCitizen(uint unit, ref CitizenUnit data, uint citizen)
  {
    if (citizen == 0U)
      return;
    if ((data.m_flags & CitizenUnit.Flags.Home) != CitizenUnit.Flags.None)
      this.m_citizens.m_buffer[(IntPtr) citizen].m_homeBuilding = (ushort) 0;
    if ((data.m_flags & (CitizenUnit.Flags.Work | CitizenUnit.Flags.Student)) != CitizenUnit.Flags.None)
      this.m_citizens.m_buffer[(IntPtr) citizen].m_workBuilding = (ushort) 0;
    if ((data.m_flags & CitizenUnit.Flags.Visit) != CitizenUnit.Flags.None)
      this.m_citizens.m_buffer[(IntPtr) citizen].m_visitBuilding = (ushort) 0;
    if ((data.m_flags & CitizenUnit.Flags.Vehicle) == CitizenUnit.Flags.None)
      return;
    this.m_citizens.m_buffer[(IntPtr) citizen].m_vehicle = (ushort) 0;
  }

  public bool CreateCitizen(out uint citizen, int age, int family, ref Randomizer r)
  {
    uint num;
    if (this.m_citizens.CreateItem(out num, ref r))
    {
      citizen = num;
      this.m_citizens.m_buffer[(IntPtr) citizen] = new Citizen()
      {
        m_flags = Citizen.Flags.Created,
        Age = age,
        m_health = (byte) 50,
        m_wellbeing = (byte) 50,
        m_family = (byte) family
      };
      this.m_citizenCount = (int) this.m_citizens.ItemCount() - 1;
      return true;
    }
    citizen = 0U;
    return false;
  }

  public bool CreateCitizen(
    out uint citizen,
    int age,
    int family,
    ref Randomizer r,
    Citizen.Gender gender)
  {
    for (int index = 0; index < 1000; ++index)
    {
      Randomizer r1 = r;
      uint citizenID = this.m_citizens.NextFreeItem(ref r);
      if (citizenID != 0U)
      {
        uint num;
        if (Citizen.GetGender(citizenID) == gender && this.m_citizens.CreateItem(out num, ref r1))
        {
          citizen = num;
          this.m_citizens.m_buffer[(IntPtr) citizen] = new Citizen()
          {
            m_flags = Citizen.Flags.Created,
            Age = age,
            m_health = (byte) 50,
            m_wellbeing = (byte) 50,
            m_family = (byte) family
          };
          this.m_citizenCount = (int) this.m_citizens.ItemCount() - 1;
          return true;
        }
      }
      else
        break;
    }
    citizen = 0U;
    return false;
  }

  public void ReleaseCitizen(uint citizen)
  {
    this.ReleaseCitizenImplementation(citizen, ref this.m_citizens.m_buffer[(IntPtr) citizen]);
  }

  private void ReleaseCitizenImplementation(uint citizen, ref Citizen data)
  {
    Singleton<InstanceManager>.instance.ReleaseInstance(new InstanceID()
    {
      Citizen = citizen
    });
    if (data.m_instance != (ushort) 0)
    {
      this.ReleaseCitizenInstance(data.m_instance);
      data.m_instance = (ushort) 0;
    }
    data.SetHome(citizen, (ushort) 0, 0U);
    data.SetWorkplace(citizen, (ushort) 0, 0U);
    data.SetVisitplace(citizen, (ushort) 0, 0U);
    data.SetVehicle(citizen, (ushort) 0, 0U);
    data.SetParkedVehicle(citizen, (ushort) 0);
    data = new Citizen();
    this.m_citizens.ReleaseItem(citizen);
    this.m_citizenCount = (int) this.m_citizens.ItemCount() - 1;
  }

  public bool CreateCitizenInstance(
    out ushort instance,
    ref Randomizer randomizer,
    CitizenInfo info,
    uint citizen)
  {
    ushort num;
    if (this.m_instances.CreateItem(out num, ref randomizer))
    {
      instance = num;
      CitizenInstance.Frame frame;
      frame.m_velocity = Vector3.zero;
      frame.m_position = Vector3.zero;
      frame.m_rotation = Quaternion.identity;
      frame.m_underground = false;
      frame.m_insideBuilding = false;
      frame.m_transition = false;
      this.m_instances.m_buffer[(int) instance].m_flags = CitizenInstance.Flags.Created;
      this.m_instances.m_buffer[(int) instance].Info = info;
      this.m_instances.m_buffer[(int) instance].m_citizen = citizen;
      this.m_instances.m_buffer[(int) instance].m_frame0 = frame;
      this.m_instances.m_buffer[(int) instance].m_frame1 = frame;
      this.m_instances.m_buffer[(int) instance].m_frame2 = frame;
      this.m_instances.m_buffer[(int) instance].m_frame3 = frame;
      this.m_instances.m_buffer[(int) instance].m_targetPos = (Vector4) Vector3.zero;
      this.m_instances.m_buffer[(int) instance].m_targetDir = Vector2.zero;
      this.m_instances.m_buffer[(int) instance].m_color = new Color32();
      this.m_instances.m_buffer[(int) instance].m_sourceBuilding = (ushort) 0;
      this.m_instances.m_buffer[(int) instance].m_targetBuilding = (ushort) 0;
      this.m_instances.m_buffer[(int) instance].m_nextGridInstance = (ushort) 0;
      this.m_instances.m_buffer[(int) instance].m_nextSourceInstance = (ushort) 0;
      this.m_instances.m_buffer[(int) instance].m_nextTargetInstance = (ushort) 0;
      this.m_instances.m_buffer[(int) instance].m_lastFrame = (byte) 0;
      this.m_instances.m_buffer[(int) instance].m_pathPositionIndex = (byte) 0;
      this.m_instances.m_buffer[(int) instance].m_lastPathOffset = (byte) 0;
      this.m_instances.m_buffer[(int) instance].m_waitCounter = (byte) 0;
      this.m_instances.m_buffer[(int) instance].m_targetSeed = (byte) 0;
      if (citizen != 0U)
        this.m_citizens.m_buffer[(IntPtr) citizen].m_instance = instance;
      info.m_citizenAI.CreateInstance(instance, ref this.m_instances.m_buffer[(int) instance]);
      this.m_instanceCount = (int) this.m_instances.ItemCount() - 1;
      return true;
    }
    instance = (ushort) 0;
    return false;
  }

  private void InitializeInstance(ushort instance, ref CitizenInstance data)
  {
    if ((data.m_flags & CitizenInstance.Flags.Character) == CitizenInstance.Flags.None)
      return;
    this.AddToGrid(instance, ref data);
  }

  public void ReleaseCitizenInstance(ushort instance)
  {
    this.ReleaseCitizenInstanceImplementation(instance, ref this.m_instances.m_buffer[(int) instance]);
  }

  private void ReleaseCitizenInstanceImplementation(ushort instance, ref CitizenInstance data)
  {
    data.Info?.m_citizenAI.ReleaseInstance(instance, ref this.m_instances.m_buffer[(int) instance]);
    data.Unspawn(instance);
    Singleton<InstanceManager>.instance.ReleaseInstance(new InstanceID()
    {
      CitizenInstance = instance
    });
    if (data.m_path != 0U)
    {
      Singleton<PathManager>.instance.ReleasePath(data.m_path);
      data.m_path = 0U;
    }
    if (data.m_citizen != 0U)
    {
      this.m_citizens.m_buffer[(IntPtr) data.m_citizen].SetVehicle(data.m_citizen, (ushort) 0, 0U);
      this.m_citizens.m_buffer[(IntPtr) data.m_citizen].m_instance = (ushort) 0;
      data.m_citizen = 0U;
    }
    data.m_flags = CitizenInstance.Flags.None;
    this.m_instances.ReleaseItem(instance);
    this.m_instanceCount = (int) this.m_instances.ItemCount() - 1;
  }

  public void AddToGrid(ushort instance, ref CitizenInstance data)
  {
    CitizenInstance.Frame lastFrameData = data.GetLastFrameData();
    int gridX = Mathf.Clamp((int) ((double) lastFrameData.m_position.x / 8.0 + 1080.0), 0, 2159);
    int gridZ = Mathf.Clamp((int) ((double) lastFrameData.m_position.z / 8.0 + 1080.0), 0, 2159);
    this.AddToGrid(instance, ref data, gridX, gridZ);
  }

  public void AddToGrid(ushort instance, ref CitizenInstance data, int gridX, int gridZ)
  {
    int index = gridZ * 2160 + gridX;
    data.m_nextGridInstance = this.m_citizenGrid[index];
    this.m_citizenGrid[index] = instance;
  }

  public void RemoveFromGrid(ushort instance, ref CitizenInstance data)
  {
    CitizenInstance.Frame lastFrameData = data.GetLastFrameData();
    int gridX = Mathf.Clamp((int) ((double) lastFrameData.m_position.x / 8.0 + 1080.0), 0, 2159);
    int gridZ = Mathf.Clamp((int) ((double) lastFrameData.m_position.z / 8.0 + 1080.0), 0, 2159);
    this.RemoveFromGrid(instance, ref data, gridX, gridZ);
  }

  public void RemoveFromGrid(ushort instance, ref CitizenInstance data, int gridX, int gridZ)
  {
    int index = gridZ * 2160 + gridX;
    ushort num1 = 0;
    ushort nextGridInstance = this.m_citizenGrid[index];
    int num2 = 0;
    while (nextGridInstance != (ushort) 0)
    {
      if ((int) nextGridInstance == (int) instance)
      {
        if (num1 == (ushort) 0)
        {
          this.m_citizenGrid[index] = data.m_nextGridInstance;
          break;
        }
        this.m_instances.m_buffer[(int) num1].m_nextGridInstance = data.m_nextGridInstance;
        break;
      }
      num1 = nextGridInstance;
      nextGridInstance = this.m_instances.m_buffer[(int) nextGridInstance].m_nextGridInstance;
      if (++num2 > 65536)
      {
        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
        break;
      }
    }
    data.m_nextGridInstance = (ushort) 0;
  }

  private int GetGroupIndex(
    ItemClass.Service service,
    Citizen.Gender gender,
    Citizen.SubCulture subCulture,
    Citizen.AgePhase agePhase)
  {
    return (int) ((int) ((subCulture == Citizen.SubCulture.Generic ? (int) (service - 1) : (int) (subCulture + 24 - 1)) * 2 + gender) * 14 + agePhase);
  }

  private int GetGroupIndex(ItemClass.Service service, ItemClass.SubService subService)
  {
    return subService == ItemClass.SubService.None ? (int) (service - 1) : (int) (subService + 24 - 1);
  }

  public CitizenInfo GetGroupCitizenInfo(
    ref Randomizer r,
    ItemClass.Service service,
    Citizen.Gender gender,
    Citizen.SubCulture subCulture,
    Citizen.AgePhase agePhase)
  {
    if (!this.m_citizensRefreshed)
    {
      CODebugBase<LogChannel>.Error(LogChannel.Core, "Random citizens not refreshed yet!");
      return (CitizenInfo) null;
    }
    FastList<ushort> groupCitizen = this.m_groupCitizens[this.GetGroupIndex(service, gender, subCulture, agePhase)];
    if (groupCitizen == null)
      return (CitizenInfo) null;
    if (groupCitizen.m_size == 0)
      return (CitizenInfo) null;
    int index = r.Int32((uint) groupCitizen.m_size);
    return PrefabCollection<CitizenInfo>.GetPrefab((uint) groupCitizen.m_buffer[index]);
  }

  public CitizenInfo GetGroupAnimalInfo(
    ref Randomizer r,
    ItemClass.Service service,
    ItemClass.SubService subService)
  {
    if (!this.m_citizensRefreshed)
    {
      CODebugBase<LogChannel>.Error(LogChannel.Core, "Random citizens not refreshed yet!");
      return (CitizenInfo) null;
    }
    FastList<ushort> groupAnimal = this.m_groupAnimals[this.GetGroupIndex(service, subService)];
    if (groupAnimal == null)
      return (CitizenInfo) null;
    if (groupAnimal.m_size == 0)
      return (CitizenInfo) null;
    int index = r.Int32((uint) groupAnimal.m_size);
    return PrefabCollection<CitizenInfo>.GetPrefab((uint) groupAnimal.m_buffer[index]);
  }

  private void RefreshGroupCitizens()
  {
    int length1 = this.m_groupCitizens.Length;
    int length2 = this.m_groupAnimals.Length;
    for (int index = 0; index < length1; ++index)
      this.m_groupCitizens[index] = (FastList<ushort>) null;
    for (int index = 0; index < length2; ++index)
      this.m_groupAnimals[index] = (FastList<ushort>) null;
    int num1 = PrefabCollection<CitizenInfo>.PrefabCount();
    for (int index = 0; index < num1; ++index)
    {
      CitizenInfo prefab = PrefabCollection<CitizenInfo>.GetPrefab((uint) index);
      if (prefab != null && prefab.m_placementStyle == ItemClass.Placement.Automatic)
      {
        if (prefab.m_citizenAI.IsAnimal())
        {
          int groupIndex = this.GetGroupIndex(prefab.m_class.m_service, prefab.m_class.m_subService);
          if (this.m_groupAnimals[groupIndex] == null)
            this.m_groupAnimals[groupIndex] = new FastList<ushort>();
          this.m_groupAnimals[groupIndex].Add((ushort) index);
        }
        else
        {
          int groupIndex = this.GetGroupIndex(prefab.m_class.m_service, prefab.m_gender, prefab.m_subCulture, prefab.m_agePhase);
          if (this.m_groupCitizens[groupIndex] == null)
            this.m_groupCitizens[groupIndex] = new FastList<ushort>();
          this.m_groupCitizens[groupIndex].Add((ushort) index);
        }
      }
    }
    int num2 = 29;
    for (int index1 = 0; index1 < num2; ++index1)
    {
      for (int index2 = 0; index2 < 2; ++index2)
      {
        for (int index3 = 1; index3 < 14; ++index3)
        {
          int index4 = (index1 * 2 + index2) * 14 + index3;
          FastList<ushort> groupCitizen1 = this.m_groupCitizens[index4];
          FastList<ushort> groupCitizen2 = this.m_groupCitizens[index4 - 1];
          if (groupCitizen1 == null && groupCitizen2 != null)
            this.m_groupCitizens[index4] = groupCitizen2;
        }
      }
    }
    this.m_citizensRefreshed = true;
  }

  public bool RayCast(
    Segment3 ray,
    CitizenInstance.Flags ignoreFlags,
    out Vector3 hit,
    out ushort instanceIndex)
  {
    Bounds bounds = new Bounds(new Vector3(0.0f, 512f, 0.0f), new Vector3(17280f, 1152f, 17280f));
    if (ray.Clip(bounds))
    {
      Vector3 vector3_1 = ray.b - ray.a;
      Vector3 normalized = vector3_1.normalized;
      float num1 = 2f;
      instanceIndex = (ushort) 0;
      Vector3 vector3_2 = ray.a - normalized * 5f;
      Vector3 vector3_3 = ray.a + Vector3.ClampMagnitude(ray.b - ray.a, 400f) + normalized * 5f;
      int a1 = (int) ((double) vector3_2.x / 8.0 + 1080.0);
      int a2 = (int) ((double) vector3_2.z / 8.0 + 1080.0);
      int num2 = (int) ((double) vector3_3.x / 8.0 + 1080.0);
      int num3 = (int) ((double) vector3_3.z / 8.0 + 1080.0);
      float num4 = Mathf.Abs(vector3_1.x);
      float num5 = Mathf.Abs(vector3_1.z);
      int num6;
      int num7;
      if ((double) num4 >= (double) num5)
      {
        num6 = (double) vector3_1.x <= 0.0 ? -1 : 1;
        num7 = 0;
        if ((double) num4 != 0.0)
          vector3_1 *= 8f / num4;
      }
      else
      {
        num6 = 0;
        num7 = (double) vector3_1.z <= 0.0 ? -1 : 1;
        if ((double) num5 != 0.0)
          vector3_1 *= 8f / num5;
      }
      Vector3 vector3_4 = vector3_2;
      Vector3 vector3_5 = vector3_2;
      do
      {
        Vector3 vector3_6 = vector3_5 + vector3_1;
        int num8;
        int num9;
        int num10;
        int num11;
        if (num6 != 0)
        {
          num8 = Mathf.Max(a1, 0);
          num9 = Mathf.Min(a1, 2159);
          num10 = Mathf.Max((int) (((double) Mathf.Min(vector3_4.z, vector3_6.z) - 5.0) / 8.0 + 1080.0), 0);
          num11 = Mathf.Min((int) (((double) Mathf.Max(vector3_4.z, vector3_6.z) + 5.0) / 8.0 + 1080.0), 2159);
        }
        else
        {
          num10 = Mathf.Max(a2, 0);
          num11 = Mathf.Min(a2, 2159);
          num8 = Mathf.Max((int) (((double) Mathf.Min(vector3_4.x, vector3_6.x) - 5.0) / 8.0 + 1080.0), 0);
          num9 = Mathf.Min((int) (((double) Mathf.Max(vector3_4.x, vector3_6.x) + 5.0) / 8.0 + 1080.0), 2159);
        }
        for (int index1 = num10; index1 <= num11; ++index1)
        {
          for (int index2 = num8; index2 <= num9; ++index2)
          {
            ushort nextGridInstance = this.m_citizenGrid[index1 * 2160 + index2];
            int num12 = 0;
            while (nextGridInstance != (ushort) 0)
            {
              float t;
              if (this.m_instances.m_buffer[(int) nextGridInstance].RayCast(nextGridInstance, ray, ignoreFlags, out t) && (double) t < (double) num1)
              {
                num1 = t;
                instanceIndex = nextGridInstance;
              }
              nextGridInstance = this.m_instances.m_buffer[(int) nextGridInstance].m_nextGridInstance;
              if (++num12 > 1048576)
              {
                CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                break;
              }
            }
          }
        }
        vector3_4 = vector3_5;
        vector3_5 = vector3_6;
        a1 += num6;
        a2 += num7;
      }
      while ((a1 <= num2 || num6 <= 0) && (a1 >= num2 || num6 >= 0) && ((a2 <= num3 || num7 <= 0) && (a2 >= num3 || num7 >= 0)));
      if ((double) num1 != 2.0)
      {
        hit = ray.a + (ray.b - ray.a) * num1;
        return true;
      }
    }
    hit = Vector3.zero;
    instanceIndex = (ushort) 0;
    return false;
  }

  protected override void SimulationStepImpl(int subStep)
  {
    if (subStep != 0)
    {
        // I am not 100% certain what m_currentFrameIndex means, but I think...
        // it marks a "timeframe" during which new objects are created with the timeframe as "signature"
        // eg I think if timeframe is 0x123, then its citizens are numbered 0x12300, 0x12301, etc up to 0x123ff
        // I suppose also that by the time you reach timeframe 0xfff and wrap around to 0x1000 then the original 0x000 cohort are long gone so re-usable numbers
        // so I think this first loop is over "new citizens"
      int partFrameIndex = (int) Singleton<SimulationManager>.instance.m_currentFrameIndex & 4095;    // AND 0x00000fff, so this must be some kind of "subframing" I suppose
      int startIndex = partFrameIndex * 256;      // so startIndex is masked 0x000fff00  (256 is 0x100)
      int endIndex = (partFrameIndex + 1) * 256 - 1;    // so endIndex is startIndex + 255  (0x00000000ff)
      for (int index = startIndex; index <= endIndex; ++index)
      {
        if ((this.m_citizens.m_buffer[index].m_flags & Citizen.Flags.Created) != Citizen.Flags.None)
        {
          CitizenInfo citizenInfo = this.m_citizens.m_buffer[index].GetCitizenInfo((uint) index);
          if (citizenInfo == null)
            this.ReleaseCitizen((uint) index);
          else
            citizenInfo.m_citizenAI.SimulationStep((uint) index, ref this.m_citizens.m_buffer[index]);  // CitizenAI(overridden ResidentAI or TouristAI).SimulationStep(citizenID, citizen) is called
                                                                                                        // if citizen is flagged as Created and their CitzenInfo is not null
        }
      }
      if (partFrameIndex == 4095)   // corroborates my theory that FrameIndex is a timeframe thing.  This step tests "if at the end of a millenium" in effect
      {
        this.m_finalOldestOriginalResident = this.m_tempOldestOriginalResident;
        this.m_tempOldestOriginalResident = 0;
      }
    }
    if (subStep != 0)
    {
            // loop indices like above except we only look at the first 0x7f (total 0x80) units in the buffer (presume because always less units than citizens by def)
      int partFrameIndex = (int) Singleton<SimulationManager>.instance.m_currentFrameIndex & 4095;
      int startIndex = partFrameIndex * 128;
      int endIndex = (partFrameIndex + 1) * 128 - 1;
      for (int index = startIndex; index <= endIndex; ++index)
      {
        if ((this.m_units.m_buffer[index].m_flags & CitizenUnit.Flags.Created) != CitizenUnit.Flags.None)
          this.m_units.m_buffer[index].SimulationStep((uint) index);
            // this calls CitizenUnit.SimulationStep which does nothing if the unit is not at home;
            // else if there is any nonnull citizen in the unit with nonnull CitizenInfo, call SimulationsStep(homeID, CitzenUnit) for "home actions"
            // note that the CitizenAI for this is overridden by ResidentAI (for everyone else including TouristAI, just quick return no action)
      }
    }
    if (subStep == 0)
      return;

    // in the following I have not the faintest idea what a physicsLodRefPos might be
    // (I did Google to find out that LOD is level of detail)
    // why would LOD affect the simulation step?  Perhaps it is about showing on the GUI (eg if zoomed out enough, citizens do not show)
    SimulationManager instance = Singleton<SimulationManager>.instance;
    Vector3 physicsLodRefPos = instance.m_simulationView.m_position + instance.m_simulationView.m_direction * 200f;
    int partFrameIndex = (int) Singleton<SimulationManager>.instance.m_currentFrameIndex & 15;      // this time masked 0x000000f
    int startIndex = partFrameIndex * 4096;     // shift it to 0x0000f000
    int endIndex = (partFrameIndex + 1) * 4096 - 1;     // loop through 4096 items this time (I rather imagine this is the total size of the citizenInstances buffer)
    for (int index = startIndex; index <= endIndex; ++index)
    {
      if ((this.m_instances.m_buffer[index].m_flags & CitizenInstance.Flags.Created) != CitizenInstance.Flags.None)
        this.m_instances.m_buffer[index].Info.m_citizenAI.SimulationStep((ushort) index, ref this.m_instances.m_buffer[index], physicsLodRefPos);
    }
  }

  [DebuggerHidden]
  public IEnumerator<bool> SetCitizenName(uint citizenID, string name)
  {
    // ISSUE: object of a compiler-generated type is created
    return (IEnumerator<bool>) new CitizenManager.\u003CSetCitizenName\u003Ec__Iterator0()
    {
      citizenID = citizenID,
      name = name,
      \u0024this = this
    };
  }

  [DebuggerHidden]
  public IEnumerator<bool> SetInstanceName(ushort instanceID, string name)
  {
    // ISSUE: object of a compiler-generated type is created
    return (IEnumerator<bool>) new CitizenManager.\u003CSetInstanceName\u003Ec__Iterator1()
    {
      instanceID = instanceID,
      name = name,
      \u0024this = this
    };
  }

  public override void UpdateData(SimulationManager.UpdateMode mode)
  {
    Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.BeginLoading("CitizenManager.UpdateData");
    base.UpdateData(mode);
    for (int index = 1; index < 65536; ++index)
    {
      if (this.m_instances.m_buffer[index].m_flags != CitizenInstance.Flags.None && this.m_instances.m_buffer[index].Info == null)
        this.ReleaseCitizenInstance((ushort) index);
    }
    this.m_infoCount = PrefabCollection<CitizenInfo>.PrefabCount();
    Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.EndLoading();
  }

  public override void GetData(FastList<IDataContainer> data)
  {
    base.GetData(data);
    data.Add((IDataContainer) new CitizenManager.Data());
  }

  public string GetDefaultCitizenName(uint citizenID)
  {
    return CitizenAI.GenerateCitizenName(citizenID, (byte) citizenID);
  }

  public string GetCitizenName(uint citizenID)
  {
    if (this.m_citizens.m_buffer[(IntPtr) citizenID].m_flags == Citizen.Flags.None)
      return (string) null;
    string str = (string) null;
    if ((this.m_citizens.m_buffer[(IntPtr) citizenID].m_flags & Citizen.Flags.CustomName) != Citizen.Flags.None)
      str = Singleton<InstanceManager>.instance.GetName(new InstanceID()
      {
        Citizen = citizenID
      });
    if (str == null)
      str = CitizenAI.GenerateCitizenName(citizenID, this.m_citizens.m_buffer[(IntPtr) citizenID].m_family);
    return str;
  }

  public string GetDefaultInstanceName(ushort instanceID)
  {
    return this.GenerateInstanceName(instanceID, false);
  }

  public string GetInstanceName(ushort instanceID)
  {
    if (this.m_instances.m_buffer[(int) instanceID].m_flags == CitizenInstance.Flags.None)
      return (string) null;
    string str = (string) null;
    if ((this.m_instances.m_buffer[(int) instanceID].m_flags & CitizenInstance.Flags.CustomName) != CitizenInstance.Flags.None)
      str = Singleton<InstanceManager>.instance.GetName(new InstanceID()
      {
        CitizenInstance = instanceID
      });
    if (str == null)
      str = this.GenerateInstanceName(instanceID, true);
    return str;
  }

  private string GenerateInstanceName(ushort instanceID, bool useCitizen)
  {
    string str = (string) null;
    CitizenInfo info = this.m_instances.m_buffer[(int) instanceID].Info;
    if (info != null)
      str = info.m_citizenAI.GenerateName(instanceID, useCitizen);
    if (str == null)
      str = "Invalid";
    return str;
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

  string IRenderableManager.GetName()
  {
    return this.GetName();
  }

  DrawCallData IRenderableManager.GetDrawCallData()
  {
    return this.GetDrawCallData();
  }

  void IRenderableManager.BeginRendering(RenderManager.CameraInfo cameraInfo)
  {
    this.BeginRendering(cameraInfo);
  }

  void IRenderableManager.EndRendering(RenderManager.CameraInfo cameraInfo)
  {
    this.EndRendering(cameraInfo);
  }

  void IRenderableManager.BeginOverlay(RenderManager.CameraInfo cameraInfo)
  {
    this.BeginOverlay(cameraInfo);
  }

  void IRenderableManager.EndOverlay(RenderManager.CameraInfo cameraInfo)
  {
    this.EndOverlay(cameraInfo);
  }

  void IRenderableManager.UndergroundOverlay(RenderManager.CameraInfo cameraInfo)
  {
    this.UndergroundOverlay(cameraInfo);
  }

  void IAudibleManager.PlayAudio(AudioManager.ListenerInfo listenerInfo)
  {
    this.PlayAudio(listenerInfo);
  }

  public class Data : IDataContainer
  {
    public void Serialize(DataSerializer s)
    {
      Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.BeginSerialize(s, nameof (CitizenManager));
      CitizenManager instance = Singleton<CitizenManager>.instance;
      Citizen[] buffer1 = instance.m_citizens.m_buffer;
      CitizenUnit[] buffer2 = instance.m_units.m_buffer;
      CitizenInstance[] buffer3 = instance.m_instances.m_buffer;
      int length1 = buffer1.Length;
      int length2 = buffer2.Length;
      int length3 = buffer3.Length;
      EncodedArray.UInt uint1 = EncodedArray.UInt.BeginWrite(s);
      for (int index = 1; index < length1; ++index)
        uint1.Write((uint) buffer1[index].m_flags);
      uint1.EndWrite();
      EncodedArray.Byte byte1 = EncodedArray.Byte.BeginWrite(s);
      for (int index = 1; index < length1; ++index)
      {
        if (buffer1[index].m_flags != Citizen.Flags.None)
          byte1.Write(buffer1[index].m_health);
      }
      byte1.EndWrite();
      EncodedArray.Byte byte2 = EncodedArray.Byte.BeginWrite(s);
      for (int index = 1; index < length1; ++index)
      {
        if (buffer1[index].m_flags != Citizen.Flags.None)
          byte2.Write(buffer1[index].m_wellbeing);
      }
      byte2.EndWrite();
      EncodedArray.Byte byte3 = EncodedArray.Byte.BeginWrite(s);
      for (int index = 1; index < length1; ++index)
      {
        if (buffer1[index].m_flags != Citizen.Flags.None)
          byte3.Write(buffer1[index].m_age);
      }
      byte3.EndWrite();
      EncodedArray.Byte byte4 = EncodedArray.Byte.BeginWrite(s);
      for (int index = 1; index < length1; ++index)
      {
        if (buffer1[index].m_flags != Citizen.Flags.None)
          byte4.Write(buffer1[index].m_family);
      }
      byte4.EndWrite();
      EncodedArray.Byte byte5 = EncodedArray.Byte.BeginWrite(s);
      for (int index = 1; index < length1; ++index)
      {
        if (buffer1[index].m_flags != Citizen.Flags.None)
          byte5.Write(buffer1[index].m_jobTitleIndex);
      }
      byte5.EndWrite();
      EncodedArray.UShort ushort1 = EncodedArray.UShort.BeginWrite(s);
      for (int index = 1; index < length2; ++index)
        ushort1.Write((ushort) buffer2[index].m_flags);
      ushort1.EndWrite();
      for (int index = 1; index < length2; ++index)
      {
        if (buffer2[index].m_flags != CitizenUnit.Flags.None)
        {
          uint num = 0;
          if (buffer2[index].m_citizen0 != 0U)
            num |= 1U;
          if (buffer2[index].m_citizen1 != 0U)
            num |= 2U;
          if (buffer2[index].m_citizen2 != 0U)
            num |= 4U;
          if (buffer2[index].m_citizen3 != 0U)
            num |= 8U;
          if (buffer2[index].m_citizen4 != 0U)
            num |= 16U;
          if (buffer2[index].m_nextUnit != 0U)
            num |= 32U;
          s.WriteUInt8(num);
          if (buffer2[index].m_citizen0 != 0U)
            s.WriteUInt24(buffer2[index].m_citizen0);
          if (buffer2[index].m_citizen1 != 0U)
            s.WriteUInt24(buffer2[index].m_citizen1);
          if (buffer2[index].m_citizen2 != 0U)
            s.WriteUInt24(buffer2[index].m_citizen2);
          if (buffer2[index].m_citizen3 != 0U)
            s.WriteUInt24(buffer2[index].m_citizen3);
          if (buffer2[index].m_citizen4 != 0U)
            s.WriteUInt24(buffer2[index].m_citizen4);
          if (buffer2[index].m_nextUnit != 0U)
            s.WriteUInt24(buffer2[index].m_nextUnit);
        }
      }
      EncodedArray.UShort ushort2 = EncodedArray.UShort.BeginWrite(s);
      for (int index = 1; index < length2; ++index)
      {
        if (buffer2[index].m_flags != CitizenUnit.Flags.None)
          ushort2.Write(buffer2[index].m_goods);
      }
      ushort2.EndWrite();
      EncodedArray.UInt uint2 = EncodedArray.UInt.BeginWrite(s);
      for (int index = 1; index < length3; ++index)
        uint2.Write((uint) buffer3[index].m_flags);
      uint2.EndWrite();
      try
      {
        PrefabCollection<CitizenInfo>.BeginSerialize(s);
        for (int index = 1; index < length3; ++index)
        {
          if (buffer3[index].m_flags != CitizenInstance.Flags.None)
            PrefabCollection<CitizenInfo>.Serialize((uint) buffer3[index].m_infoIndex);
        }
      }
      finally
      {
        PrefabCollection<CitizenInfo>.EndSerialize(s);
      }
      for (int index = 1; index < length3; ++index)
      {
        if (buffer3[index].m_flags != CitizenInstance.Flags.None)
        {
          CitizenInstance.Frame lastFrameData = buffer3[index].GetLastFrameData();
          s.WriteVector3(lastFrameData.m_velocity);
          s.WriteVector3(lastFrameData.m_position);
          s.WriteVector4(buffer3[index].m_targetPos);
          s.WriteVector2(buffer3[index].m_targetDir);
          if ((buffer3[index].m_flags & CitizenInstance.Flags.CustomColor) != CitizenInstance.Flags.None)
          {
            s.WriteUInt8((uint) buffer3[index].m_color.r);
            s.WriteUInt8((uint) buffer3[index].m_color.g);
            s.WriteUInt8((uint) buffer3[index].m_color.b);
          }
          s.WriteUInt24(buffer3[index].m_citizen);
          s.WriteUInt24(buffer3[index].m_path);
          s.WriteUInt8((uint) buffer3[index].m_pathPositionIndex);
          s.WriteUInt8((uint) buffer3[index].m_lastPathOffset);
          s.WriteUInt8((uint) buffer3[index].m_waitCounter);
          s.WriteUInt8((uint) buffer3[index].m_targetSeed);
          s.WriteUInt16((uint) buffer3[index].m_sourceBuilding);
          s.WriteUInt16((uint) buffer3[index].m_targetBuilding);
        }
      }
      s.WriteInt16(instance.m_tempOldestOriginalResident);
      s.WriteInt16(instance.m_finalOldestOriginalResident);
      s.WriteInt32(instance.m_fullyEducatedOriginalResidents);
      Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.EndSerialize(s, nameof (CitizenManager));
    }

    public void Deserialize(DataSerializer s)
    {
      Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.BeginDeserialize(s, nameof (CitizenManager));
      CitizenManager instance = Singleton<CitizenManager>.instance;
      Citizen[] buffer1 = instance.m_citizens.m_buffer;
      CitizenUnit[] buffer2 = instance.m_units.m_buffer;
      CitizenInstance[] buffer3 = instance.m_instances.m_buffer;
      ushort[] citizenGrid = instance.m_citizenGrid;
      int length1 = buffer1.Length;
      int length2 = buffer2.Length;
      int length3 = buffer3.Length;
      int length4 = citizenGrid.Length;
      instance.m_citizens.ClearUnused();
      instance.m_units.ClearUnused();
      instance.m_instances.ClearUnused();
      for (int index = 0; index < length4; ++index)
        citizenGrid[index] = (ushort) 0;
      if (s.version < 30U)
      {
        EncodedArray.Byte @byte = EncodedArray.Byte.BeginRead(s);
        for (int index = 1; index < length1; ++index)
        {
          buffer1[index].m_flags = (Citizen.Flags) @byte.Read();
          buffer1[index].m_homeBuilding = (ushort) 0;
          buffer1[index].m_workBuilding = (ushort) 0;
          buffer1[index].m_visitBuilding = (ushort) 0;
          buffer1[index].m_vehicle = (ushort) 0;
          buffer1[index].m_parkedVehicle = (ushort) 0;
          buffer1[index].m_instance = (ushort) 0;
          buffer1[index].m_health = (byte) 0;
          buffer1[index].m_wellbeing = (byte) 0;
          buffer1[index].m_age = (byte) 0;
          buffer1[index].m_family = (byte) 0;
        }
        @byte.EndRead();
      }
      else
      {
        EncodedArray.UInt @uint = EncodedArray.UInt.BeginRead(s);
        for (int index = 1; index < length1; ++index)
        {
          buffer1[index].m_flags = (Citizen.Flags) @uint.Read();
          buffer1[index].m_homeBuilding = (ushort) 0;
          buffer1[index].m_workBuilding = (ushort) 0;
          buffer1[index].m_visitBuilding = (ushort) 0;
          buffer1[index].m_vehicle = (ushort) 0;
          buffer1[index].m_parkedVehicle = (ushort) 0;
          buffer1[index].m_instance = (ushort) 0;
          buffer1[index].m_health = (byte) 0;
          buffer1[index].m_wellbeing = (byte) 0;
          buffer1[index].m_age = (byte) 0;
          buffer1[index].m_family = (byte) 0;
          if (buffer1[index].m_flags != Citizen.Flags.None)
          {
            if (s.version < 157U)
            {
              buffer1[index].m_age = (byte) (((uint) (buffer1[index].m_flags & (Citizen.Flags.Student | Citizen.Flags.MovingIn | Citizen.Flags.DummyTraffic | Citizen.Flags.Criminal | Citizen.Flags.Arrested | Citizen.Flags.Evacuating | Citizen.Flags.Collapsed)) >> 4) * 3U);
              buffer1[index].m_flags &= Citizen.Flags.Unemployed | Citizen.Flags.Wealth | Citizen.Flags.Location | Citizen.Flags.NoElectricity | Citizen.Flags.NoWater | Citizen.Flags.NoSewage | Citizen.Flags.BadHealth | Citizen.Flags.Created | Citizen.Flags.Tourist | Citizen.Flags.Sick | Citizen.Flags.Dead | Citizen.Flags.Education1 | Citizen.Flags.Education2 | Citizen.Flags.Education3 | Citizen.Flags.NeedGoods | Citizen.Flags.Original | Citizen.Flags.CustomName;
            }
          }
          else
            instance.m_citizens.ReleaseItem((uint) index);
        }
        @uint.EndRead();
      }
      if (s.version >= 65U)
      {
        EncodedArray.Byte @byte = EncodedArray.Byte.BeginRead(s);
        for (int index = 1; index < length1; ++index)
        {
          if (buffer1[index].m_flags != Citizen.Flags.None)
            buffer1[index].m_health = @byte.Read();
        }
        @byte.EndRead();
      }
      if (s.version >= 65U)
      {
        EncodedArray.Byte @byte = EncodedArray.Byte.BeginRead(s);
        for (int index = 1; index < length1; ++index)
        {
          if (buffer1[index].m_flags != Citizen.Flags.None)
            buffer1[index].m_wellbeing = @byte.Read();
        }
        @byte.EndRead();
      }
      if (s.version >= 157U)
      {
        EncodedArray.Byte @byte = EncodedArray.Byte.BeginRead(s);
        for (int index = 1; index < length1; ++index)
        {
          if (buffer1[index].m_flags != Citizen.Flags.None)
            buffer1[index].m_age = @byte.Read();
        }
        @byte.EndRead();
      }
      if (s.version >= 190U)
      {
        EncodedArray.Byte @byte = EncodedArray.Byte.BeginRead(s);
        for (int index = 1; index < length1; ++index)
        {
          if (buffer1[index].m_flags != Citizen.Flags.None)
            buffer1[index].m_family = @byte.Read();
        }
        @byte.EndRead();
      }
      if (s.version >= 112007U)
      {
        EncodedArray.Byte @byte = EncodedArray.Byte.BeginRead(s);
        for (int index = 1; index < length1; ++index)
        {
          if (buffer1[index].m_flags != Citizen.Flags.None)
            buffer1[index].m_jobTitleIndex = @byte.Read();
        }
        @byte.EndRead();
      }
      if (s.version < 30U)
      {
        for (int index = 1; index < length1; ++index)
        {
          if (buffer1[index].m_flags != Citizen.Flags.None)
          {
            if (s.version < 23U)
            {
              int num = (int) s.ReadUInt8();
            }
          }
          else
            instance.m_citizens.ReleaseItem((uint) index);
        }
      }
      if (s.version >= 23U && s.version < 30U)
      {
        EncodedArray.Byte @byte = EncodedArray.Byte.BeginRead(s);
        for (int index = 1; index < length1; ++index)
        {
          if (buffer1[index].m_flags != Citizen.Flags.None)
          {
            int num = (int) @byte.Read();
          }
        }
        @byte.EndRead();
      }
      if (s.version >= 23U && s.version < 30U)
      {
        EncodedArray.Byte @byte = EncodedArray.Byte.BeginRead(s);
        for (int index = 1; index < length1; ++index)
        {
          if (buffer1[index].m_flags != Citizen.Flags.None)
          {
            int num = (int) @byte.Read();
          }
        }
        @byte.EndRead();
      }
      if (s.version >= 23U && s.version < 30U)
      {
        EncodedArray.Byte @byte = EncodedArray.Byte.BeginRead(s);
        for (int index = 1; index < length1; ++index)
        {
          if (buffer1[index].m_flags != Citizen.Flags.None)
          {
            int num = (int) @byte.Read();
          }
        }
        @byte.EndRead();
      }
      if (s.version >= 23U && s.version < 30U)
      {
        EncodedArray.Byte @byte = EncodedArray.Byte.BeginRead(s);
        for (int index = 1; index < length1; ++index)
        {
          if (buffer1[index].m_flags != Citizen.Flags.None)
          {
            int num = (int) @byte.Read();
          }
        }
        @byte.EndRead();
      }
      EncodedArray.UShort ushort1 = EncodedArray.UShort.BeginRead(s);
      for (int index = 1; index < length2; ++index)
        buffer2[index].m_flags = (CitizenUnit.Flags) ushort1.Read();
      ushort1.EndRead();
      for (int index = 1; index < length2; ++index)
      {
        buffer2[index].m_citizen0 = 0U;
        buffer2[index].m_citizen1 = 0U;
        buffer2[index].m_citizen2 = 0U;
        buffer2[index].m_citizen3 = 0U;
        buffer2[index].m_citizen4 = 0U;
        buffer2[index].m_nextUnit = 0U;
        buffer2[index].m_building = (ushort) 0;
        if (buffer2[index].m_flags != CitizenUnit.Flags.None)
        {
          if (s.version >= 69U)
          {
            uint num = s.ReadUInt8();
            if (((int) num & 1) != 0)
              buffer2[index].m_citizen0 = s.ReadUInt24();
            if (((int) num & 2) != 0)
              buffer2[index].m_citizen1 = s.ReadUInt24();
            if (((int) num & 4) != 0)
              buffer2[index].m_citizen2 = s.ReadUInt24();
            if (((int) num & 8) != 0)
              buffer2[index].m_citizen3 = s.ReadUInt24();
            if (((int) num & 16) != 0)
              buffer2[index].m_citizen4 = s.ReadUInt24();
            if (((int) num & 32) != 0)
              buffer2[index].m_nextUnit = s.ReadUInt24();
          }
          else
          {
            if ((buffer2[index].m_flags & (CitizenUnit.Flags) 2048) != CitizenUnit.Flags.None)
              buffer2[index].m_citizen0 = s.ReadUInt24();
            if ((buffer2[index].m_flags & (CitizenUnit.Flags) 4096) != CitizenUnit.Flags.None)
              buffer2[index].m_citizen1 = s.ReadUInt24();
            if ((buffer2[index].m_flags & (CitizenUnit.Flags) 8192) != CitizenUnit.Flags.None)
              buffer2[index].m_citizen2 = s.ReadUInt24();
            if ((buffer2[index].m_flags & (CitizenUnit.Flags) 16384) != CitizenUnit.Flags.None)
              buffer2[index].m_citizen3 = s.ReadUInt24();
            if ((buffer2[index].m_flags & (CitizenUnit.Flags) 32768) != CitizenUnit.Flags.None)
              buffer2[index].m_citizen4 = s.ReadUInt24();
            if ((buffer2[index].m_flags & (CitizenUnit.Flags) 2) == CitizenUnit.Flags.None)
              buffer2[index].m_nextUnit = s.ReadUInt24();
            buffer2[index].m_flags &= (CitizenUnit.Flags) 2045;
          }
        }
        else
          instance.m_units.ReleaseItem((uint) index);
      }
      if (s.version >= 69U)
      {
        EncodedArray.UShort ushort2 = EncodedArray.UShort.BeginRead(s);
        for (int index = 1; index < length2; ++index)
          buffer2[index].m_goods = buffer2[index].m_flags == CitizenUnit.Flags.None ? (ushort) 0 : ushort2.Read();
        ushort2.EndRead();
      }
      else
      {
        for (int index = 1; index < length2; ++index)
          buffer2[index].m_goods = (ushort) 0;
      }
      if (s.version >= 204U)
      {
        EncodedArray.UInt @uint = EncodedArray.UInt.BeginRead(s);
        for (int index = 1; index < length3; ++index)
          buffer3[index].m_flags = (CitizenInstance.Flags) @uint.Read();
        @uint.EndRead();
      }
      else if (s.version >= 44U)
      {
        EncodedArray.UInt @uint = EncodedArray.UInt.BeginRead(s);
        for (int index = 1; index < 32768; ++index)
          buffer3[index].m_flags = (CitizenInstance.Flags) @uint.Read();
        for (int index = 32768; index < length3; ++index)
          buffer3[index].m_flags = CitizenInstance.Flags.None;
        @uint.EndRead();
      }
      else
      {
        for (int index = 1; index < length3; ++index)
        {
          buffer3[index] = new CitizenInstance();
          instance.m_instances.ReleaseItem((ushort) index);
        }
      }
      if (s.version >= 44U)
      {
        PrefabCollection<CitizenInfo>.BeginDeserialize(s);
        for (int index = 1; index < length3; ++index)
        {
          if (buffer3[index].m_flags != CitizenInstance.Flags.None)
            buffer3[index].m_infoIndex = (ushort) PrefabCollection<CitizenInfo>.Deserialize(true);
        }
        PrefabCollection<CitizenInfo>.EndDeserialize(s);
      }
      if (s.version >= 44U)
      {
        for (int index = 1; index < length3; ++index)
        {
          buffer3[index].m_nextGridInstance = (ushort) 0;
          buffer3[index].m_nextSourceInstance = (ushort) 0;
          buffer3[index].m_nextTargetInstance = (ushort) 0;
          buffer3[index].m_lastFrame = (byte) 0;
          if (buffer3[index].m_flags != CitizenInstance.Flags.None)
          {
            buffer3[index].m_frame0.m_velocity = s.version < 46U ? Vector3.zero : s.ReadVector3();
            buffer3[index].m_frame0.m_position = s.ReadVector3();
            buffer3[index].m_frame0.m_rotation = Quaternion.identity;
            buffer3[index].m_frame0.m_underground = (buffer3[index].m_flags & CitizenInstance.Flags.Underground) != CitizenInstance.Flags.None;
            buffer3[index].m_frame0.m_transition = (buffer3[index].m_flags & CitizenInstance.Flags.Transition) != CitizenInstance.Flags.None;
            buffer3[index].m_frame0.m_insideBuilding = (buffer3[index].m_flags & CitizenInstance.Flags.InsideBuilding) != CitizenInstance.Flags.None;
            buffer3[index].m_frame1 = buffer3[index].m_frame0;
            buffer3[index].m_frame2 = buffer3[index].m_frame0;
            buffer3[index].m_frame3 = buffer3[index].m_frame0;
            buffer3[index].m_targetPos = s.version < 195U ? (s.version < 47U ? (Vector4) buffer3[index].m_frame0.m_position : (Vector4) s.ReadVector3()) : s.ReadVector4();
            buffer3[index].m_targetDir = s.version < 200U ? Vector2.zero : s.ReadVector2();
            if (s.version >= 247U && (buffer3[index].m_flags & CitizenInstance.Flags.CustomColor) != CitizenInstance.Flags.None)
            {
              buffer3[index].m_color.r = (byte) s.ReadUInt8();
              buffer3[index].m_color.g = (byte) s.ReadUInt8();
              buffer3[index].m_color.b = (byte) s.ReadUInt8();
              buffer3[index].m_color.a = byte.MaxValue;
            }
            else
              buffer3[index].m_color = new Color32();
            buffer3[index].m_citizen = s.ReadUInt24();
            if (s.version >= 46U)
            {
              buffer3[index].m_path = s.ReadUInt24();
              buffer3[index].m_pathPositionIndex = (byte) s.ReadUInt8();
              buffer3[index].m_lastPathOffset = (byte) s.ReadUInt8();
            }
            else
            {
              buffer3[index].m_path = 0U;
              buffer3[index].m_pathPositionIndex = (byte) 0;
              buffer3[index].m_lastPathOffset = (byte) 0;
            }
            buffer3[index].m_waitCounter = s.version < 86U ? (byte) 0 : (byte) s.ReadUInt8();
            buffer3[index].m_targetSeed = s.version < 201U ? (byte) 0 : (byte) s.ReadUInt8();
            buffer3[index].m_sourceBuilding = s.version < 180U ? (ushort) 0 : (ushort) s.ReadUInt16();
            buffer3[index].m_targetBuilding = (ushort) s.ReadUInt16();
            instance.InitializeInstance((ushort) index, ref buffer3[index]);
          }
          else
          {
            buffer3[index].m_frame0 = new CitizenInstance.Frame();
            buffer3[index].m_frame1 = new CitizenInstance.Frame();
            buffer3[index].m_frame2 = new CitizenInstance.Frame();
            buffer3[index].m_frame3 = new CitizenInstance.Frame();
            buffer3[index].m_targetPos = (Vector4) Vector3.zero;
            buffer3[index].m_targetDir = Vector2.zero;
            buffer3[index].m_color = new Color32();
            buffer3[index].m_citizen = 0U;
            buffer3[index].m_path = 0U;
            buffer3[index].m_pathPositionIndex = (byte) 0;
            buffer3[index].m_lastPathOffset = (byte) 0;
            buffer3[index].m_waitCounter = (byte) 0;
            buffer3[index].m_targetSeed = (byte) 0;
            buffer3[index].m_sourceBuilding = (ushort) 0;
            buffer3[index].m_targetBuilding = (ushort) 0;
            instance.m_instances.ReleaseItem((ushort) index);
          }
        }
      }
      if (s.version >= 169U)
      {
        instance.m_tempOldestOriginalResident = s.ReadInt16();
        instance.m_finalOldestOriginalResident = s.ReadInt16();
      }
      else
      {
        instance.m_tempOldestOriginalResident = 0;
        instance.m_finalOldestOriginalResident = 0;
      }
      instance.m_fullyEducatedOriginalResidents = s.version < 179U ? 0 : s.ReadInt32();
      Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.EndDeserialize(s, nameof (CitizenManager));
    }

    public void AfterDeserialize(DataSerializer s)
    {
      Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.BeginAfterDeserialize(s, nameof (CitizenManager));
      Singleton<LoadingManager>.instance.WaitUntilEssentialScenesLoaded();
      PrefabCollection<CitizenInfo>.BindPrefabs();
      CitizenManager instance = Singleton<CitizenManager>.instance;
      instance.RefreshGroupCitizens();
      Citizen[] buffer1 = instance.m_citizens.m_buffer;
      CitizenInstance[] buffer2 = instance.m_instances.m_buffer;
      int length = buffer2.Length;
      for (int index = 1; index < length; ++index)
      {
        if (buffer2[index].m_flags != CitizenInstance.Flags.None)
        {
          CitizenInfo info = buffer2[index].Info;
          if (info != null)
          {
            buffer2[index].m_infoIndex = (ushort) info.m_prefabDataIndex;
            info.m_citizenAI.LoadInstance((ushort) index, ref buffer2[index]);
          }
          uint citizen = buffer2[index].m_citizen;
          if (citizen != 0U)
          {
            if (buffer1[(IntPtr) citizen].m_instance != (ushort) 0)
            {
              CODebugBase<LogChannel>.Warn(LogChannel.Core, "Citizen has a clone: " + (object) citizen);
              buffer2[index].m_citizen = 0U;
            }
            else
              buffer1[(IntPtr) citizen].m_instance = (ushort) index;
          }
          if (buffer2[index].m_path != 0U)
            ++Singleton<PathManager>.instance.m_pathUnits.m_buffer[(IntPtr) buffer2[index].m_path].m_referenceCount;
        }
      }
      instance.m_citizenCount = (int) instance.m_citizens.ItemCount() - 1;
      instance.m_unitCount = (int) instance.m_units.ItemCount() - 1;
      instance.m_instanceCount = (int) instance.m_instances.ItemCount() - 1;
      Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.EndAfterDeserialize(s, nameof (CitizenManager));
    }
  }
}
