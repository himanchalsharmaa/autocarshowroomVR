using System;
using Unity.InteractiveTutorials;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Autoshowroom.Tutorials
{
    public class LightControl
    {
        static Button targetButton;

        public static void RegisterActionToUnloadLightScene()
        {
            Action actionForunloadingLight = () =>
            {
                UnityEditor.SceneManagement.EditorSceneManager.CloseScene(UnityEditor.SceneManagement.EditorSceneManager.GetSceneAt(1), true);
            };

            try
            {
                if (UnityEditor.SceneManagement.EditorSceneManager.sceneCount > 1)
                {
                    targetButton = EditorWindow.GetWindow<TutorialWindow>().rootVisualElement.Q<Button>("NextButton");

                    if (targetButton != null)
                    {
                        targetButton.clicked -= actionForunloadingLight;
                        targetButton.clicked += actionForunloadingLight;
                    }
                }
            }
            catch
            {
                Debug.LogError("Faile to register an action for unloading Scene.");
                return;
            }
        }

    }
}
