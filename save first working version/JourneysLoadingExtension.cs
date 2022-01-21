using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using System;
using UnityEngine;
using Journeys.Visualizer;
using Journeys.RedirectionFramework;
using Journeys.RedirectionFramework.Attributes;
using Journeys.RedirectionFramework.Extensions;


namespace Journeys
{
    public class JourneysLoadingExtension : LoadingExtensionBase
    {
        private GameObject journeysGameObject;

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            // this seems like overkill but it works so leave it
            UIView objectOfType = UnityEngine.Object.FindObjectOfType<UIView>();
            if (objectOfType != null)
            {
                try
                {
                journeysGameObject = new GameObject("JourneysGameObject");
                journeysGameObject.transform.parent = objectOfType.transform;
                journeysGameObject.AddComponent<JourneysToggle>();
                journeysGameObject.AddComponent<JourneyVisualizer>();
                JourneyVisualizer.instance.Init();
                Debug.Log("done loading journeys!");
                }
                catch
                {
                    Debug.LogError("journeys loading failed");
                }
            }
        }

        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();
            if (journeysGameObject != null)
                UnityEngine.Object.Destroy(journeysGameObject);
        }
    }
}

