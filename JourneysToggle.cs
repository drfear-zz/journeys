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
                if (InfoManager.instance.CurrentMode == InfoManager.InfoMode.TrafficRoutes)
                {
                    UIView.library.Hide("TrafficRoutesInfoViewPanel");  // this happens when eg user goes to public transport view then back to PV view
                }
                JourneyVisualizer theJV = Singleton<JourneyVisualizer>.instance;
                if (Input.GetKeyDown(KeyCode.K))
                {
                    theJV.ChangeHeatMap();
                    Debug.Log("JV: heatmap changed to " + theJV.HeatMap);
                }
                if (Input.GetKeyDown(KeyCode.O))
                {
                    theJV.ChangeHeatOnlyAsSelected();
                    Debug.Log("JV: only-as-selected changed to " + theJV.HeatOnlyAsSelected);
                }
                if (Input.GetKeyDown(KeyCode.N))
                {
                    theJV.ChangeDiscreteHeats();
                    Debug.Log("JV: discrete categories changed to " + theJV.DiscreteHeats);
                }
                //if (Input.GetKeyDown(KeyCode.Period))
                //{
                //    theJV.ChangeAbsoluteHeats();
                //}
                //if (Input.GetKeyDown(KeyCode.Minus))
                //{
                //    theJV.ChangeAbsoluteHeats(forwards: false);
                //}
                if (Input.GetKeyDown(KeyCode.P))
                {
                    theJV.SubSelectByStep();
                }
                if (Input.GetKeyDown(KeyCode.L))
                {
                    theJV.SubSelectByLane();
                }
                if (Input.GetKeyDown(KeyCode.H))
                {
                    theJV.ToggleFromToHere();
                }
                if (Input.GetKeyDown(KeyCode.Comma))
                {
                    theJV.ToggleTransportSteps();
                }
                if (Input.GetKeyDown(KeyCode.Keypad0))
                {
                    theJV.ByJourney();
                }
                if (Input.GetKeyDown(KeyCode.Keypad1))
                {
                    theJV.ToggleAllCars();
                }
                if (Input.GetKeyDown(KeyCode.Keypad2))
                {
                    theJV.ToggleShowPTstretches();
                }
                if (Input.GetKeyDown(KeyCode.Keypad3))
                {
                    theJV.ToggleShowBlended();
                }
                if (Input.GetKeyDown(KeyCode.Keypad7))
                {
                    theJV.SubselectByLaneLine();
                }
                if (Input.GetKeyDown(KeyCode.Keypad4))
                {
                    theJV.SubselectByLaneLine(forwards: false);
                }
                if (Input.GetKeyDown(KeyCode.Keypad5))
                {
                    theJV.ShowAllJourneys();
                }
                if (Input.GetKeyDown(KeyCode.Keypad6))
                {
                    theJV.ToggleShowPTstops();
                }
                if (Input.GetKeyDown(KeyCode.Keypad8))
                {
                    theJV.ChangeMinWidth();
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
                Singleton<JourneysButton>.instance.Show();
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
