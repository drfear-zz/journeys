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
using System.Linq;
using System.Runtime.InteropServices;



namespace Journeys
{

    [TargetType(typeof(PathVisualizer))]
    public class JourneyDetourer : MonoBehaviour
    {

        [RedirectMethod]
        public void SimulationStep(int subStep)
        {
            JourneyVisualizer.instance.SimulationStep(subStep);
        }

        [RedirectMethod]
        public void RenderPaths(RenderManager.CameraInfo cameraInfo, int layerMask)
        {
            JourneyVisualizer.instance.RenderPaths(cameraInfo, layerMask);
        }

        [RedirectMethod]
        public void DestroyPaths()
        {
            JourneyVisualizer.instance.DestroyPaths();
        }

        [RedirectMethod]
        public void UpdateData()
        {
            JourneyVisualizer.instance.UpdateData();
        }

        [RedirectMethod]
        public bool IsPathVisible(InstanceID id)
        {
            return JourneyVisualizer.instance.IsPathVisible(id);
        }

        //[RedirectReverse]
        //private void DestroyPaths()
        //{
        //    Debug.LogError("JD destroy paths not redirected!!");
        //}


    }
}
