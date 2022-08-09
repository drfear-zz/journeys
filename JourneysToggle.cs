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
            //if (Input.GetMouseButtonDown(0) && Input.GetKeyDown(KeyCode.LeftShift))
            //    Debug.Log("Left shift mouse click");
            if (Input.GetKeyDown(KeyCode.J)) {
                FlagShowJourneys = !FlagShowJourneys;
                flagChanged = true;
            }
            if (FlagShowJourneys)
            {
                JourneyVisualizer theJV = Singleton<JourneyVisualizer>.instance;
                if (Input.GetKeyDown(KeyCode.K))
                {
                    theJV.ChangeHeatMap(!theJV.GetHeatMap);
                    Debug.Log("JV: heatmap changed to " + theJV.GetHeatMap);
                }
                if (Input.GetKeyDown(KeyCode.Comma))
                {
                    theJV.ChangeHeatOnlyAsSelected(!theJV.GetHeatOnlyAsSelected);
                    Debug.Log("JV: only-as-selected changed to " + theJV.GetHeatOnlyAsSelected);
                }
                if (Input.GetKeyDown(KeyCode.N))
                {
                    int newvalue = theJV.GetDiscreteHeats;
                    if (++newvalue > 8)
                        newvalue = 0;
                    theJV.ChangeDiscreteHeats(newvalue);
                    Debug.Log("JV: discrete categories changed to " + theJV.GetDiscreteHeats);
                }
                if (Input.GetKeyDown(KeyCode.Period))
                {
                    if (theJV.SelectedSegment != 0)
                    {
                        int numlanes = Singleton<NetManager>.instance.m_segments.m_buffer[theJV.SelectedSegment].Info.m_lanes.Length;
                        if (++theJV.CurrentLane == numlanes)
                            theJV.CurrentLane = 0;
                        theJV.SubSelectByLane(theJV.SelectedSegment, theJV.CurrentLane);
                        Debug.Log("JV: lane selection, currently lane " + theJV.CurrentLane);
                    }
                }
                if (Input.GetKeyDown(KeyCode.L))
                {
                    theJV.m_FromToCode = theJV.m_FromToCode + 1;
                    if (theJV.m_FromToCode == 3)
                        theJV.m_FromToCode = 0;
                    theJV.FromToHere();
                }
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
                Debug.Log("JV redirector deployed");
                InfoManager.instance.SetCurrentMode(InfoManager.InfoMode.TrafficRoutes, InfoManager.SubInfoMode.Default);
                UIView.library.Hide("TrafficRoutesInfoViewPanel");
                //Singleton<JourneysPanel>.instance.Show();
            }
            else if (flagChanged && Redirector<JourneyDetourer>.IsDeployed())
            {
                //DestroyPaths() I might need here I think
                Redirector<JourneyDetourer>.Revert();
                //Singleton<JourneysPanel>.instance.Hide();
                Debug.Log("JV redirect of PV reverted");
                if (InfoManager.instance.CurrentMode == InfoManager.InfoMode.TrafficRoutes)
                {
                    UIView.library.Show("TrafficRoutesInfoViewPanel");
                    Singleton<NetManager>.instance.PathVisualizer.PathsVisible = true;
                }
            }
        }
    }
}
