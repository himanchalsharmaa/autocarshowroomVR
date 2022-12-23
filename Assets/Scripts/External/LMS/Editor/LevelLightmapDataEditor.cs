using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections;
using Unity.EditorCoroutines.Editor;

namespace PixelWizards.LightmapSwitcher
{

    [CustomEditor(typeof(LevelLightmapData))]
    public class LevelLightmapDataEditor : Editor
    {
        static int selectedIndex;
        static int selectedLgtIndex;
        public bool toggleSettingGroup = false;
        public bool toggleBuildGroup = false;

        public SerializedProperty lightingScenariosScenes;
        public SerializedProperty lightingScenesNames;
        public SerializedProperty allowLoadingLightingScenes;
        public SerializedProperty applyLightmapScaleAndOffset;

        public SerializedProperty themes;
        public SerializedProperty vehicleSpawnRoot;

        GUIContent allowLoading = new GUIContent("Allow loading Lighting Scenes", "Allow the Level Lightmap Data script to load a lighting scene additively at runtime if the lighting scenario contains realtime lights.");


        public void OnEnable()
        {
            lightingScenariosScenes = serializedObject.FindProperty("lightingScenariosScenes");
            lightingScenesNames = serializedObject.FindProperty("lightingScenesNames");
            allowLoadingLightingScenes = serializedObject.FindProperty("allowLoadingLightingScenes");
            applyLightmapScaleAndOffset = serializedObject.FindProperty("applyLightmapScaleAndOffset");

            themes = serializedObject.FindProperty("themes");
            vehicleSpawnRoot = serializedObject.FindProperty("vehicleSpawnRoot");
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            LevelLightmapData lightmapData = (LevelLightmapData)target;

            if (lightmapData.currentLightingScenario != -1)
                selectedLgtIndex = lightmapData.currentLightingScenario;


            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);

            {

                EditorGUI.indentLevel++;

                GUILayout.BeginVertical("Box");

                EditorGUILayout.ObjectField(vehicleSpawnRoot, typeof(GameObject), new GUIContent("Target Root"));
                

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(themes, new GUIContent("Themes"), includeChildren: true);
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(lightingScenariosScenes, new GUIContent("Lighting Scenarios Scenes"), includeChildren: true);
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    lightingScenesNames.arraySize = lightingScenariosScenes.arraySize;

                    for (int i = 0; i < lightingScenariosScenes.arraySize; i++)
                    {
                        lightingScenesNames.GetArrayElementAtIndex(i).stringValue = lightingScenariosScenes.GetArrayElementAtIndex(i).objectReferenceValue == null ? "" : lightingScenariosScenes.GetArrayElementAtIndex(i).objectReferenceValue.name;
                    }
                    serializedObject.ApplyModifiedProperties();
                }

                serializedObject.ApplyModifiedProperties();

                GUILayout.EndVertical();

                EditorGUI.indentLevel--;
            }


            EditorGUILayout.Space();

            
            toggleBuildGroup = EditorGUILayout.BeginFoldoutHeaderGroup(toggleBuildGroup, "LightSet Builds");

            GUILayout.BeginVertical("Box");

            if (toggleBuildGroup)
            {

                EditorGUI.indentLevel++;

                for (int i = 0; i < lightmapData.lightingScenariosScenes.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();

                    if (lightmapData.lightingScenariosScenes[i] != null)
                    {
                        EditorGUILayout.LabelField(lightmapData.lightingScenariosScenes[i].name.ToString(), EditorStyles.boldLabel);

                        if (GUILayout.Button("Build "))
                        {
                            if (UnityEditor.Lightmapping.giWorkflowMode != UnityEditor.Lightmapping.GIWorkflowMode.OnDemand)
                            {
                                Debug.LogError("ExtractLightmapData requires that you have baked you lightmaps and Auto mode is disabled.");
                            }
                            else
                                BuildLightingScenario(i, lightmapData);
                        }
                        if (GUILayout.Button("Store "))
                        {
                            lightmapData.StoreLightmapInfos(i);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUI.indentLevel++;
            }

            GUILayout.EndVertical();

            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Separator();



            GUILayout.BeginVertical("Box");
            
            GUILayout.Label("Controls");

            EditorGUILayout.Separator();

            EditorGUI.BeginChangeCheck();

            GUILayout.BeginHorizontal();

            GUILayout.Label("Apply LightSet", GUILayout.Width(130));

            GUI.SetNextControlName("SelectedLightSetPopup");
            selectedLgtIndex = EditorGUILayout.Popup(selectedLgtIndex, GetLightSet(), GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                if (lightmapData.lightingScenariosScenes[selectedLgtIndex] != null)
                    lightmapData.LoadLightingScenario(selectedLgtIndex);
            }

            EditorGUILayout.Separator();
            EditorGUILayout.Separator();

            GUILayout.EndVertical();


        }


        string[] GetThemeNames()
        {
            string[] outString = new string[themes.arraySize];

            for (int i = 0; i < themes.arraySize; i++)
            {
                if (themes.GetArrayElementAtIndex(i) != null)
                {
                    if (themes.GetArrayElementAtIndex(i).objectReferenceValue != null)
                        outString[i] = themes.GetArrayElementAtIndex(i).objectReferenceValue.name;
                }
            }

            return outString;
        }


        string[] GetLightSet()
        {
            LevelLightmapData lightmapData = (LevelLightmapData)target;

            string[] outString = new string[lightmapData.lightingScenariosScenes.Count];

            for (int i = 0; i < lightmapData.lightingScenariosScenes.Count; i++)
            {
                if (lightmapData.lightingScenariosScenes[i] != null)
                    outString[i] = lightmapData.lightingScenariosScenes[i].name;
            }

            return outString;
        }


        public void BuildLightingScenario(int ScenarioID, LevelLightmapData levelLightmapData)
        {
            Lightmapping.lightingDataAsset = null;

            string currentBuildScenename = lightingScenariosScenes.GetArrayElementAtIndex(ScenarioID).objectReferenceValue.name;

            string lightingSceneGUID = AssetDatabase.FindAssets(currentBuildScenename)[0];
            string lightingScenePath = AssetDatabase.GUIDToAssetPath(lightingSceneGUID);
            if (!lightingScenePath.EndsWith(".unity"))
                lightingScenePath = lightingScenePath + ".unity";

            EditorSceneManager.OpenScene(lightingScenePath, OpenSceneMode.Additive);

            Scene lightingScene = SceneManager.GetSceneByName(currentBuildScenename);
            EditorSceneManager.SetActiveScene(lightingScene);

            SearchLightsNeededRealtime(levelLightmapData);

            EditorCoroutineUtility.StartCoroutine(BuildLightingAsync(lightingScene), this);
        }


        private IEnumerator BuildLightingAsync(Scene lightingScene)
        {
            var newLightmapMode = new LightmapsMode();
            newLightmapMode = LightmapSettings.lightmapsMode;
            Lightmapping.BakeAsync();
            while (Lightmapping.isRunning) { yield return null; }
            EditorSceneManager.SaveScene(lightingScene);
            EditorSceneManager.CloseScene(lightingScene, true);
            LightmapSettings.lightmapsMode = newLightmapMode;
        }


        public void SearchLightsNeededRealtime(LevelLightmapData levelLightmapData)
        {
            bool latestBuildHasRealtimeLights = false;

            var lights = FindObjectsOfType<Light>();
            var reflectionProbes = FindObjectsOfType<ReflectionProbe>();

            foreach (Light light in lights)
            {
                if (light.lightmapBakeType == LightmapBakeType.Mixed || light.lightmapBakeType == LightmapBakeType.Realtime)
                    latestBuildHasRealtimeLights = true;
            }

            if (reflectionProbes.Length > 0)
                latestBuildHasRealtimeLights = true;

            levelLightmapData.latestBuildHasReltimeLights = latestBuildHasRealtimeLights;
        }
    }
}