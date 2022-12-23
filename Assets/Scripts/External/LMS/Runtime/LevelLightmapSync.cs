using UnityEngine;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace PixelWizards.LightmapSwitcher
{

    [ExecuteInEditMode, RequireComponent(typeof(LevelLightmapData))]

    public class LevelLightmapSync : MonoBehaviour
    {

        private void Start()
        {

#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                return;
            }

            LevelLightmapData curLightmapData = this.GetComponent<LevelLightmapData>();

            if (curLightmapData != null)
            {
                if (EditorSceneManager.sceneCount == 1)
                {
                    if (curLightmapData.lightingScenariosCount > 0)
                    {
                        curLightmapData.LoadInitialLightSceneOnStartup();
                    }
                }
                else if (EditorSceneManager.sceneCount > 1)
                {
                    if (curLightmapData.lightingScenariosCount > 0)
                    {
                        curLightmapData.RefreshLightMapOnly();
                    }
                }
            }
#endif
        }
    }
}