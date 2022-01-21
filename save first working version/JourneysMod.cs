using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using System;
//using Journeys.RedirectionFramework;
using UnityEngine;

namespace Journeys
{
    public class JourneysMod : IUserMod
    {
        public string Name => "Journeys";

        public string Description => "Show citizen journeys to their final destination including their public transport rides";

    } 
}

/*
 * PV (the class) is loaded by NetManager.Awake()
 * as this.m_pathVisualizer = this.gameObject.AddComponent<PathVisualizer>();
 * and NetManager is public class NetManager : SimulationManagerBase<NetManager, NetProperties>, ISimulationManager, IRenderableManager, ITerrainManager
 * and NM's method  protected override void Awake()
 * overrides  protected virtual void Awake() in
 * public abstract class SimulationManagerBase<Manager, Properties> : Singleton<Manager>
 * 
 * PV itself is public class PathVisualizer : MonoBehaviour
 * 
 * so one thing I don't know is: what calls PV.Awake()?  Perhaps it is called by AddComponent or some deep control
 * of new game objects, because PV-Awake is private, I presume only some kind of constructor could call it
 * 
 * PV.DestroyPaths is called by NM.DestroyProperties (if PV is active)
 * 
 * PV.RenderPath, although public, is only called by PV.RenderPaths()
 * PV.UpdateMesh is private (and only called by PV.RenderPath)
 * 
 * PV.RenderPaths is called by NM.BeginOverlayImpl (which ALSO then calls NetAdjust, the purpose of which I do not understand as yet)
 * (I think also NM.EndOverlyImpl may be important - it runs through a list of name instances (which are not explicit) )
 *  - both of these are overrides from the abstract SimulationManagerBase class so perhaps I can put mine in a JourneysManager?
 *  - BeginOverlayImpl is called by BeginOverlay in SMBase
 *    specifically for NM this is called as IRenderableManager.BeginOverlay
 *  
 * 
 * PV.SimulationStep is called in only one place, and explicitly, by NM.SimulationStepImpl (immediately followed by netAdjust.SimulationStep, whatever that does)
 * 
 * PV.UpdateData is called in only one place, and explicitly, by NM.UpdateData
 *  (where NM.UpdateData implements a method from interface ISimulationManager overriding the SimulationManagerBase method
 *  
 * PV.IsPathVisible is accessed by all the VehicleAI classes (where if in PV mode, colours the vehicle as public transport)
 * NOTE - I would not be able to implement this without redirecting all the vehicle methods!  But I am not sure it is critical.
 * 
 * 
 */