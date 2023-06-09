using UnityEngine;
using Journeys.RedirectionFramework.Attributes;



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
            JourneyVisualizer.instance.RenderJourneys(cameraInfo, layerMask);
        }

        [RedirectMethod]
        public void DestroyPaths()
        {
            JourneyVisualizer.instance.DestroyJourneys();
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
    }
}
