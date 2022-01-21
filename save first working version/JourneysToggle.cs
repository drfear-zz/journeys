using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using ColossalFramework.UI;
using Journeys.Visualizer;
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
            if (Input.GetKeyDown(KeyCode.J)) {
                FlagShowJourneys = !FlagShowJourneys;
            }
            if (FlagShowJourneys)
            {
                //InfoManager.instance.SetCurrentMode(InfoManager.InfoMode.TrafficRoutes, InfoManager.SubInfoMode.Default);
                //Debug.Log("toggled to show journeys");
                Redirector<JourneyVisualizer>.Deploy();
                //Debug.Log("JV redirector deployed");
                //JourneyVisualizer.Init();
            }
            else
            {
                //Debug.Log("toggled to stop showing journeys");
                if (Redirector<JourneyVisualizer>.IsDeployed())
                {
                    Redirector<JourneyVisualizer>.Revert();
                    Debug.Log("JV redirect of PV reverted");
                }
            }
        }
    }
}
