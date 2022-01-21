using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.PlatformServices;
using System;
using System.Collections.Generic;
using System.Threading;
using ColossalFramework.UI;
using UnityEngine;
using Journeys.RedirectionFramework;
using Journeys.RedirectionFramework.Attributes;
using Journeys.RedirectionFramework.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;


namespace Journeys.Visualizer
{

    [TargetType(typeof(PathVisualizer))]

    public class JourneyVisualizer : MonoBehaviour
    {

        public static JourneyVisualizer instance;

        private void Awake()
        {
            instance = this;
            Debug.Log("JV Awake calling in as run");    // this won't happen unless I create an instance - have to do that
        } 

        public void Init()
        {
            Debug.Log("reached call to JV.instance.Init");
        }

        [RedirectMethod]
        public void SimulationStep(int subStep)
        {
            Debug.Log("PV sim step redirected to JV");
        }

    }
}
