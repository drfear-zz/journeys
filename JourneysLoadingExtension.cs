using ColossalFramework.UI;
using ICities;
using UnityEngine;


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
                    journeysGameObject.AddComponent<JourneyVisualizer>();
                    journeysGameObject.AddComponent<JourneysPanel>(); // this calls Awake
                    journeysGameObject.AddComponent<JourneysButton>(); // this calls Awake
                }
                catch
                {
                    Debug.LogError("JV: journeys loading failed");
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

