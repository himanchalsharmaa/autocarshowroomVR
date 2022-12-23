using PixelWizards.LightmapSwitcher;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Unity.Autoshowroom.Tutorials
{
    class TutorialCallbacks : ScriptableObject
    {
        const string initialScenePath = "Assets/Scenes/Showroom.unity";

        public void UnFoldLevelLightmapDataSettings()
        {
            if (Resources.FindObjectsOfTypeAll<LevelLightmapDataEditor>().Length > 0)
            {
                var editor = Resources.FindObjectsOfTypeAll<LevelLightmapDataEditor>()[0];
                if (editor != null)
                    editor.toggleSettingGroup = true;
            }
        }

        public void UnFoldLevelLightmapDataBuildSets()
        {
            if (Resources.FindObjectsOfTypeAll<LevelLightmapDataEditor>().Length > 0)
            {
                var editor = Resources.FindObjectsOfTypeAll<LevelLightmapDataEditor>()[0];
                if (editor != null)
                    editor.toggleBuildGroup = true;
            }
        }

        public void SetFreeAspectRatioWithDisabledLowResAR()
        {
            var gvWndType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
            
            var gvWnd = EditorWindow.GetWindow(gvWndType);

            var lowResolutionForAspectRatios = gvWndType.GetProperty("lowResolutionForAspectRatios", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var selectedSizeIndexProp = gvWndType.GetProperty("selectedSizeIndex", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            lowResolutionForAspectRatios.SetValue(gvWnd, false);
            selectedSizeIndexProp.SetValue(gvWnd, 0, null);

            gvWnd.Repaint();
        }

        public void UnloadLightSceneAfterCompletion()
        {
            LightControl.RegisterActionToUnloadLightScene();
        }
    }
}