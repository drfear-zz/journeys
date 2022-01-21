using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using ColossalFramework.UI;
using Journeys.RedirectionFramework;
using Journeys.RedirectionFramework.Attributes;
using Journeys.RedirectionFramework.Extensions;

namespace Journeys
{
    public class JourneysToggle : MonoBehaviour
    {
        public bool FlagShowJourneys { get; set; } = false;

        public void Update()
        {
            bool flagChanged = false;
            if (Input.GetKeyDown(KeyCode.J)) {
                FlagShowJourneys = !FlagShowJourneys;
                flagChanged = true;
            }
            if (FlagShowJourneys && flagChanged)
            {
                //Debug.Log("toggled to show journeys");
                if (InfoManager.instance.CurrentMode == InfoManager.InfoMode.TrafficRoutes)
                {
                    //InfoManager.instance.SetCurrentMode(InfoManager.InfoMode.None, InfoManager.SubInfoMode.None);
                    PathVisualizer thePV = Singleton<NetManager>.instance.PathVisualizer;
                    thePV.PathsVisible = false;     // mark paths not visible stops SimulationStep and RenderPaths from doing anything (Update() might still run but it does nothing on screen)
                    thePV.DestroyPaths();
                }
                Redirector<JourneyDetourer>.Deploy();
                //Debug.Log("JV redirector deployed");
                InfoManager.instance.SetCurrentMode(InfoManager.InfoMode.TrafficRoutes, InfoManager.SubInfoMode.Default);
            }
            else if (flagChanged && Redirector<JourneyDetourer>.IsDeployed())
            {
                //DestroyPaths() I might need here I think
                Redirector<JourneyDetourer>.Revert();
                Debug.Log("JV redirect of PV reverted");
                Singleton<NetManager>.instance.PathVisualizer.PathsVisible = true;      // I assume this is needed if set to false above, and will have no ill effect if never switched off
            }
        }
    }
}
