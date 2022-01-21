// Decompiled with JetBrains decompiler
// Type: PathVisualizer
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 9724B8F4-19DD-48C3-AE02-CDA150D75CEC
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Cities_Skylines\Cities_Data\Managed\Assembly-CSharp.dll

using ColossalFramework;
using ColossalFramework.PlatformServices;
using System;
using ImprovedPublicTransport2.RedirectionFramework.Attributes;
using System.Collections.Generic;
using System.Threading;
using Mono.Security;
using ICities;
using UnityEngine;








namespace PathVisualizerMod {


public class PathVisualizerDetour : PathVisualizer

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

    [RedirectMethod]
    public new void SimulationStep(int subStep)
    {
        if (!this.m_pathsVisible)
            return;
        InstanceID instanceId = Singleton<InstanceManager>.instance.GetSelectedInstance();
        if (instanceId.Citizen != 0U)
        {
            CitizenManager instance1 = Singleton<CitizenManager>.instance;
            VehicleManager instance2 = Singleton<VehicleManager>.instance;
            ushort instance3 = instance1.m_citizens.m_buffer[(IntPtr)instanceId.Citizen].m_instance;
            ushort vehicle = instance1.m_citizens.m_buffer[(IntPtr)instanceId.Citizen].m_vehicle;
            if (instance3 != (ushort)0 && instance1.m_instances.m_buffer[(int)instance3].m_path != 0U)
                instanceId.CitizenInstance = instance3;
            else if (vehicle != (ushort)0 && instance2.m_vehicles.m_buffer[(int)vehicle].m_path != 0U)
                instanceId.Vehicle = vehicle;
        }
        if (instanceId.Vehicle != (ushort)0)
        {
            CitizenManager instance1 = Singleton<CitizenManager>.instance;
            VehicleManager instance2 = Singleton<VehicleManager>.instance;
            instanceId.Vehicle = instance2.m_vehicles.m_buffer[(int)instanceId.Vehicle].GetFirstVehicle(instanceId.Vehicle);
            VehicleInfo info = instance2.m_vehicles.m_buffer[(int)instanceId.Vehicle].Info;
            if (info.m_vehicleType == VehicleInfo.VehicleType.Bicycle)
            {
                InstanceID ownerId = info.m_vehicleAI.GetOwnerID(instanceId.Vehicle, ref instance2.m_vehicles.m_buffer[(int)instanceId.Vehicle]);
                if (ownerId.Citizen != 0U)
                    ownerId.CitizenInstance = instance1.m_citizens.m_buffer[(IntPtr)ownerId.Citizen].m_instance;
                if (ownerId.CitizenInstance != (ushort)0)
                    instanceId = ownerId;
            }
        }
        if (instanceId != this.m_lastInstance || this.m_filterModified)
        {
            this.m_filterModified = false;
            this.PreAddInstances();
            if (instanceId.Vehicle != (ushort)0 || instanceId.CitizenInstance != (ushort)0)
            {
                Singleton<GuideManager>.instance.m_routeButton.Disable();
                if (instanceId.CitizenInstance != (ushort)0 && !this.m_citizenPathChecked && Singleton<SimulationManager>.instance.m_metaData.m_disableAchievements != SimulationMetaData.MetaBool.True)
                {
                    this.m_citizenPathChecked = true;
                    ColossalFramework.Threading.ThreadHelper.dispatcher.Dispatch((System.Action)(() =>
                   {
                       if (PlatformService.achievements["Reporting"].achieved)
                           return;
                       PlatformService.achievements["Reporting"].Unlock();
                   }));
                }
                this.AddInstance(instanceId);
            }
            else if (instanceId.NetSegment != (ushort)0 || instanceId.Building != (ushort)0 || (instanceId.District != (byte)0 || instanceId.Park != (byte)0))
                this.AddPaths(instanceId, 0, 256);
            this.PostAddInstances();
            this.m_lastInstance = instanceId;
            this.m_pathRefreshFrame = 0;
        }
        else if (instanceId.NetSegment != (ushort)0 || instanceId.Building != (ushort)0 || (instanceId.District != (byte)0 || instanceId.Park != (byte)0))
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
}
}



