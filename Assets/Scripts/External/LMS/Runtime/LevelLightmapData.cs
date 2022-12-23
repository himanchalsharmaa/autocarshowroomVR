using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using UnityEngine.Rendering;
using System.Collections;

using System.IO;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEditor;
#endif

using UnityEngine.Rendering.HighDefinition;



namespace PixelWizards.LightmapSwitcher
{

#if UNITY_EDITOR
    [InitializeOnLoad]
    public class LightSceneValidationOnStart
    {

        static LevelLightmapData instanceLS;

        static LightSceneValidationOnStart()
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += LogPlayModeState;
#endif
        }

        static void OnCheckLightSceneLoaded()
        {
            instanceLS = GameObject.FindObjectOfType<LevelLightmapData>();

#if UNITY_EDITOR
            EditorApplication.update -= OnCheckLightSceneLoaded;

            if (instanceLS != null)
            {
                instanceLS.LoadInitialLightSceneOnStartup();
            }
#endif
        }

        private static void LogPlayModeState(PlayModeStateChange state)
        {
            instanceLS = GameObject.FindObjectOfType<LevelLightmapData>();

            if (state == PlayModeStateChange.EnteredEditMode)
            {
                if (instanceLS != null)
                    instanceLS.RefreshLightMapOnly();
            }
        }
    }
#endif


    public enum FadeInOutType
    {
        FADEIN,
        FADEOUT
    }


    public class LevelLightmapData : MonoBehaviour
    {
        static List<string> logs = new List<string>();

        private Rect screenRect;
        private Texture2D fadeInTexture;

        public Color fadeInOutColor = Color.black;


        static void Log(string text)
        {
            logs.Add(text);
        }

        public static bool SaveLog()
        {
            string targetLog = Path.GetFullPath(Application.dataPath + "/../Log.txt");

            StreamWriter sw = new StreamWriter(targetLog);

            foreach(string l in logs)
            {
                sw.WriteLine(l);
            }

            sw.Close();

            return true;
        }


        [System.Serializable]
        public class SphericalHarmonics
        {
            public float[] coefficients = new float[27];
        }

        [System.Serializable]
        public class RendererInfo
        {
            public Renderer renderer;
            public int lightmapIndex;
            public Vector4 lightmapOffsetScale;
        }

        [System.Serializable]
        public class LightingScenarioData
        {
            public RendererInfo[] rendererInfos;
            public Texture2D[] lightmaps;
            public Texture2D[] lightmapsDir;
            public Texture2D[] shadowMasks;
            public LightmapsMode lightmapsMode;
            public SphericalHarmonics[] lightProbes;
            public bool hasRealtimeLights;
        }

        public bool latestBuildHasReltimeLights;
        public bool allowLoadingLightingScenes = true;
        [Tooltip("Enable this if you want to use different lightmap resolutions in your different lighting scenarios. In that case you'll have to disable Static Batching in the Player Settings. When disabled, Static Batching can be used but all your lighting scenarios need to use the same lightmap resolution.")]
        public bool applyLightmapScaleAndOffset = true;

        public bool enableAutoLightScene = true;

        [SerializeField]
        List<LightingScenarioData> lightingScenariosData = new List<LightingScenarioData>();

        public GameObject vehicleSpawnRoot;

        [SerializeField]
        List<ReflectionProbe> vehicleReflectionProbe = new List<ReflectionProbe>();

        public List<GameObject> themes = new List<GameObject>();

#if UNITY_EDITOR
        public List<SceneAsset> lightingScenariosScenes = new List<SceneAsset>();
#endif
        public String[] lightingScenesNames = new string[1];

        public int currentLightingScenario = -1;

        public int previousLightingScenario = -1;

        private Coroutine m_SwitchSceneCoroutine;

        public int lightingScenariosCount;

        public bool verbose = false;

        private List<SphericalHarmonicsL2[]> lightProbesRuntime = new List<SphericalHarmonicsL2[]>();
        
        
        [ContextMenu("Validate LightingScenariosData")]
        public void OnValidateLightSetting()
        {
            if (lightingScenariosData.Count == 0)
                return;

            int dataIndex = 0;

            foreach(LightingScenarioData sData in lightingScenariosData)
            {
                int renderInfoIndex = 0;

                foreach(RendererInfo rInfo in sData.rendererInfos)
                {
                    if (rInfo.renderer == null)
                    {
                        Debug.LogError(string.Format("Missing Renderer : LightDataIndex = {0}, RenderInfoIndex = {1}", dataIndex, renderInfoIndex));
                    }
                    
                    renderInfoIndex++;
                }

                dataIndex++;
            }
        }


        [ContextMenu("Fix LightingScenariosData")]
        public void OnFixLightSetting()
        {
            if (lightingScenariosData.Count == 0)
                return;

            foreach (LightingScenarioData sData in lightingScenariosData)
            {
                List<RendererInfo> newInfo = new List<RendererInfo>();

                foreach (RendererInfo rInfo in sData.rendererInfos)
                {
                    if (rInfo.renderer != null)
                    {
                        newInfo.Add(rInfo);
                    }

                }

                sData.rendererInfos = newInfo.ToArray();

#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
        }



        public int GetLightingScenarioByName(string name)
        {
            for (var i = 0; i < lightingScenesNames.Length; i++)
            {
                if (lightingScenesNames[i] == name)
                    return i;
            }
            return -1;
        }


        public void RefreshVehicle()
        {
            if (vehicleSpawnRoot != null)
            {
                vehicleSpawnRoot.SetActive(false);
                vehicleSpawnRoot.SetActive(true);
            }
        }


        private void LoadTheme(int index)
        {
            for (int i = 0; i < themes.Count; i++)
            {
                themes[i].SetActive(false);
            }

            if (index < themes.Count)
            {
                if (themes[index] != null)
                    themes[index].SetActive(true);
            }
            
        }


        Queue<Coroutine> CoroutineGroup = new Queue<Coroutine>();
        Queue<Coroutine> CoroutineReflectionGroup = new Queue<Coroutine>();


        public void LoadLightingScenario(int index)
        {

            if (index != currentLightingScenario)
            {
                if (themes.Count != lightingScenariosData.Count)
                {
#if UNITY_EDITOR
                    Log("Number of \"Theme\" and \"LightScenarioScene\" are different. should be same");
#endif
                }

                if (Application.isPlaying)
                {
                    if (CoroutineGroup.Count != 0)
                    {
                        StopCoroutine(CoroutineGroup.Dequeue());
                        isFadeIn = false;
                        isFadeOut = false;
                    }

                    Coroutine currentRoutine = StartCoroutine(PlayFadeInAndOut(5f, FadeInOutType.FADEOUT, 1f));
                    CoroutineGroup.Enqueue(currentRoutine);
                }

                LoadTheme(index);

                previousLightingScenario = currentLightingScenario == -1 ? index : currentLightingScenario;

                currentLightingScenario = index;

                LightmapSettings.lightmapsMode = lightingScenariosData[index].lightmapsMode;

                if (allowLoadingLightingScenes)
                    m_SwitchSceneCoroutine = StartCoroutine(SwitchSceneCoroutine(lightingScenesNames[previousLightingScenario], lightingScenesNames[currentLightingScenario]));


                var newLightmaps = LoadLightmaps(index);

                if (applyLightmapScaleAndOffset)
                {
                    ApplyRendererInfo(lightingScenariosData[index].rendererInfos);
                }

                LightmapSettings.lightmaps = newLightmaps;

                LoadLightProbes(currentLightingScenario);

                if (CoroutineReflectionGroup.Count != 0)
                {
                    StopCoroutine(CoroutineReflectionGroup.Dequeue());
                }

                Coroutine currentRefRoutine = StartCoroutine(RefreshVehicleReflectionProbes());
                CoroutineReflectionGroup.Enqueue(currentRefRoutine);


#if UNITY_EDITOR
                EditorUtility.SetDirty(this.gameObject);
                Selection.activeGameObject = this.gameObject;

                if (!Application.isPlaying)
                    EditorSceneManager.SaveScene(EditorSceneManager.GetSceneAt(0));

#endif

            }
        }


        void GetVehicleReflectionProbes()
        {
            vehicleReflectionProbe.Clear();

            if (vehicleSpawnRoot != null)
            {
                ReflectionProbe[] refComps = vehicleSpawnRoot.gameObject.GetComponentsInChildren<ReflectionProbe>(true);

                foreach (ReflectionProbe probe in refComps)
                {
                    vehicleReflectionProbe.Add(probe);
                }
            }
        }


        IEnumerator RefreshVehicleReflectionProbes()
        {
            foreach (ReflectionProbe probe in vehicleReflectionProbe)
            {

                HDAdditionalReflectionData comp = probe.gameObject.GetComponent<HDAdditionalReflectionData>();

                if (comp.realtimeMode != ProbeSettings.RealtimeMode.OnDemand)
                    comp.realtimeMode = ProbeSettings.RealtimeMode.OnDemand;

                yield return new WaitForSeconds(0.15f);

                comp.RequestRenderNextUpdate();
            }

            yield return new WaitForEndOfFrame();

        }


        public int ActuallyLoadedLightScene()
        {
            int id = -1;

            for (int i=0;i<SceneManager.sceneCount;i++)
            {
                Scene curScene = SceneManager.GetSceneAt(i);

                for (int j=0;j< lightingScenesNames.Length;j++)
                {
                    if (curScene.name == lightingScenesNames[j])
                    {
                        id = j;
                        return id;
                    }
                }
            }

            return id;
        }


        public void RefreshLightMapOnly()
        {
#if UNITY_EDITOR
            if (lightingScenariosData.Count == 0)
                return;

            if (currentLightingScenario == -1)
                return;

            int checkId = ActuallyLoadedLightScene();

            if (checkId == -1)
            {
                return;
            }

            if (checkId != currentLightingScenario)
            {
                currentLightingScenario = checkId;

                if (checkId + 1 < lightingScenesNames.Length)
                    previousLightingScenario = checkId + 1;
                else
                    previousLightingScenario = 0;

            }

            PrepareLightProbeArrays();

            LightmapSettings.lightmapsMode = lightingScenariosData[currentLightingScenario].lightmapsMode;

            if (allowLoadingLightingScenes)
                LoadLightProbes(currentLightingScenario);

            var newLightmaps = LoadLightmaps(currentLightingScenario);

            if (applyLightmapScaleAndOffset)
            {
                ApplyRendererInfo(lightingScenariosData[currentLightingScenario].rendererInfos);
            }

            LightmapSettings.lightmaps = newLightmaps;

            LoadLightProbes(currentLightingScenario);

            GetVehicleReflectionProbes();


            if (CoroutineReflectionGroup.Count != 0)
            {
                StopCoroutine(CoroutineReflectionGroup.Dequeue());
            }

            Coroutine currentRefRoutine = StartCoroutine(RefreshVehicleReflectionProbes());
            CoroutineReflectionGroup.Enqueue(currentRefRoutine);

#endif
        }


        public void LoadInitialLightSceneOnStartup()
        {

#if UNITY_EDITOR
            if (currentLightingScenario == -1 || lightingScenesNames.Length == 0)
                return;

            if (EditorSceneManager.sceneCount != 1)
            {
                RefreshLightMapOnly();
                return;
            }

            string initLightSceneName = lightingScenesNames[currentLightingScenario];


            string lightingSceneGUID = AssetDatabase.FindAssets(initLightSceneName)[0];
            string lightingScenePath = AssetDatabase.GUIDToAssetPath(lightingSceneGUID);
            if (!lightingScenePath.EndsWith(".unity"))
                lightingScenePath = lightingScenePath + ".unity";


            Scene lightingScene = EditorSceneManager.OpenScene(lightingScenePath, OpenSceneMode.Additive);

            LoadTheme(currentLightingScenario);

            Log("Loaded Initial Lighting Scene : " + initLightSceneName);

            RefreshLightMapOnly();

#endif
        }


        IEnumerator Start()
        {
            Log("Get Started...");
            Log("Current Lighting Scenario : " + currentLightingScenario.ToString());

            screenRect = new Rect(0, 0, Screen.width, Screen.height);
            fadeInTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            fadeInTexture.SetPixels(new Color[] { fadeInOutColor });
            fadeInTexture.Apply();

            StartCoroutine(PlayFadeInAndOut(5f, FadeInOutType.FADEOUT, 1.2f));

            if (currentLightingScenario != -1)
            {
                string initLightSceneName = lightingScenesNames[currentLightingScenario];

                Scene loadedScene = SceneManager.GetSceneByName(initLightSceneName);

                if (!loadedScene.isLoaded)
                {

                    if (SceneManager.sceneCount > 1)
                    {
                        var exScene = SceneManager.GetSceneAt(1);

                        if (exScene != null)
                        {
                            AsyncOperation initUnload = SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(1));

                            while (!initUnload.isDone)
                            {
                                yield return new WaitForEndOfFrame();
                            }
                        }
                    }

                    if (SceneManager.sceneCount == 1)
                    {
                        AsyncOperation initAsync = SceneManager.LoadSceneAsync(initLightSceneName, LoadSceneMode.Additive);

                        while (!initAsync.isDone)
                        {
                            yield return new WaitForEndOfFrame();
                        }

                        SceneManager.SetActiveScene(SceneManager.GetSceneByName(initLightSceneName));

                    }
                }

                LoadTheme(currentLightingScenario);

                Log("Loaded Initial Lighting Scene : " + initLightSceneName);
                PrepareLightProbeArrays();



                LightmapSettings.lightmapsMode = lightingScenariosData[currentLightingScenario].lightmapsMode;

                if (allowLoadingLightingScenes)
                    LoadLightProbes(currentLightingScenario);

                var newLightmaps = LoadLightmaps(currentLightingScenario);

                if (applyLightmapScaleAndOffset)
                {
                    ApplyRendererInfo(lightingScenariosData[currentLightingScenario].rendererInfos);
                }

                LightmapSettings.lightmaps = newLightmaps;

                LoadLightProbes(currentLightingScenario);


                GetVehicleReflectionProbes();


                if (CoroutineReflectionGroup.Count != 0)
                {
                    StopCoroutine(CoroutineReflectionGroup.Dequeue());
                }

                Coroutine currentRefRoutine = StartCoroutine(RefreshVehicleReflectionProbes());
                CoroutineReflectionGroup.Enqueue(currentRefRoutine);

            }

        }


        private void PrepareLightProbeArrays()
        {
            Log("Prepare for LightProbe Arrays");

            for (int x = 0; x < lightingScenariosCount; x++)
            {
                lightProbesRuntime.Add(DeserializeLightProbes(x));
            }

            Log("lightProbesRuntime Count : " + lightProbesRuntime.Count.ToString());
            Log("Ready for preparing LightProbe Arrays");
        }


        private SphericalHarmonicsL2[] DeserializeLightProbes(int index)
        {
            var sphericalHarmonicsArray = new SphericalHarmonicsL2[lightingScenariosData[index].lightProbes.Length];

            for (int i = 0; i < lightingScenariosData[index].lightProbes.Length; i++)
            {
                var sphericalHarmonics = new SphericalHarmonicsL2();

                for (int j = 0; j < 3; j++)
                {
                    for (int k = 0; k < 9; k++)
                    {
                        sphericalHarmonics[j, k] = lightingScenariosData[index].lightProbes[i].coefficients[j * 9 + k];
                    }
                }

                sphericalHarmonicsArray[i] = sphericalHarmonics;
            }
            return sphericalHarmonicsArray;
        }


        IEnumerator SwitchSceneCoroutine(string sceneToUnload, string sceneToLoad)
        {
            AsyncOperation unloadop = null;
            AsyncOperation loadop = null;

            if (sceneToUnload != null && sceneToUnload != string.Empty && sceneToUnload != sceneToLoad)
            {
                if (Application.isPlaying)
                {
                    Log("Try to unload Scene : " + sceneToUnload);

                    Scene sceneToBeUnloaded = SceneManager.GetSceneByName(sceneToUnload);

                    if (sceneToBeUnloaded != null)
                    {
                        if (sceneToBeUnloaded.isLoaded)
                        {
                            unloadop = SceneManager.UnloadSceneAsync(sceneToUnload);
                            
                            while (!unloadop.isDone)
                            {
                                yield return new WaitForEndOfFrame();
                            }

                            Log("Unloaded Scene : " + sceneToUnload);
                        }
                    }
                }
#if UNITY_EDITOR
                else
                {
                    var scene = EditorSceneManager.GetSceneByName(sceneToUnload);

                    if (scene.isLoaded)
                    {
                        var path = scene.path;

                        EditorSceneManager.CloseScene(scene, true);
                    }
                    else
                    {
                        if (EditorSceneManager.sceneCount > 1)
                        {
                            EditorSceneManager.CloseScene(UnityEditor.SceneManagement.EditorSceneManager.GetSceneAt(1), true);
                        }
                    }

                }
#endif
            }

            if (sceneToLoad != null && sceneToLoad != string.Empty && sceneToLoad != "")
            {
                if (Application.isPlaying)
                {
                    Log("Try to Load Scene : " + sceneToLoad);

                    loadop = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);

                    while ((!loadop.isDone))
                    {
                        yield return new WaitForEndOfFrame();
                    }

                    Log("Loaded Scene : " + sceneToLoad);
                }
#if UNITY_EDITOR
                else
                {

                    string lightingSceneGUID = AssetDatabase.FindAssets(sceneToLoad)[0];
                    string lightingScenePath = AssetDatabase.GUIDToAssetPath(lightingSceneGUID);
                    if (!lightingScenePath.EndsWith(".unity"))
                        lightingScenePath = lightingScenePath + ".unity";

                    EditorSceneManager.OpenScene(lightingScenePath, OpenSceneMode.Additive);

                    Scene lightingScene = SceneManager.GetSceneByName(sceneToLoad);
                    EditorSceneManager.SetActiveScene(lightingScene);
                }
#endif

                Log("Try to Set Active Scene : " + sceneToLoad);

                try
                {
                    SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneToLoad));
                }
                catch(Exception ex)
                {
                    Log("[Failure] Set Active Scene : " + sceneToLoad + " " + ex.ToString());
                }

                Log("Set Active Scene : " + sceneToLoad);

            }
            LoadLightProbes(currentLightingScenario);
        }


        LightmapData[] LoadLightmaps(int index)
        {
            var newLightmaps = new LightmapData[lightingScenariosData[index].lightmaps.Length];

            for (int i = 0; i < newLightmaps.Length; i++)
            {
                newLightmaps[i] = new LightmapData();
                newLightmaps[i].lightmapColor = lightingScenariosData[index].lightmaps[i];

                if (lightingScenariosData[index].lightmapsMode != LightmapsMode.NonDirectional)
                {
                    newLightmaps[i].lightmapDir = lightingScenariosData[index].lightmapsDir[i];
                }
                if (lightingScenariosData[index].shadowMasks.Length > 0)
                {
                    newLightmaps[i].shadowMask = lightingScenariosData[index].shadowMasks[i];
                }
            }

            return newLightmaps;
        }


        public void ApplyRendererInfo(RendererInfo[] infos)
        {
            Terrain terrain = FindObjectOfType<Terrain>();

            int i = 0;

            if (terrain != null)
            {
                terrain.lightmapIndex = infos[i].lightmapIndex;
                terrain.lightmapScaleOffset = infos[i].lightmapOffsetScale;
                i++;
            }

            for (int j = i; j < infos.Length; j++)
            {

                RendererInfo info = infos[j];

                if (info.renderer == null)
                    continue;

                info.renderer.lightmapIndex = infos[j].lightmapIndex;

                if (!info.renderer.isPartOfStaticBatch)
                {
                    info.renderer.lightmapScaleOffset = infos[j].lightmapOffsetScale;
                }
                if (info.renderer.isPartOfStaticBatch && verbose == true && Application.isEditor)
                {
                    Log("Object " + info.renderer.gameObject.name + " is part of static batch, skipping lightmap offset and scale.");
                }
            }
        }


        public void LoadLightProbes(int index)
        {
            Log("Load LightProbes");

            if (Application.isEditor && !Application.isPlaying)
            {
                PrepareLightProbeArrays();
            }

            try
            {
                Log("lightProbes.bakedProbes count : " + LightmapSettings.lightProbes.bakedProbes.Length.ToString());

                LightmapSettings.lightProbes.bakedProbes = lightProbesRuntime[index];
            }
            catch (Exception ex)
            {
                Log("Exception : " + ex.ToString());
                Log("[Warning] : error when trying to load lightprobes for scenario " + index.ToString());
                Log("lightProbesRuntime count : " + lightProbesRuntime.Count.ToString());
                Log("Warning, error when trying to load lightprobes for scenario " + index);
            }

            Log("Finished LightProbles Loading.");
        }


        public void StoreLightmapInfos(int index)
        {
            var newLightingScenarioData = new LightingScenarioData();
            var newRendererInfos = new List<RendererInfo>();
            var newLightmapsTextures = new List<Texture2D>();
            var newLightmapsTexturesDir = new List<Texture2D>();
            var newLightmapsMode = new LightmapsMode();
            var newSphericalHarmonicsList = new List<SphericalHarmonics>();
            var newLightmapsShadowMasks = new List<Texture2D>();

            newLightmapsMode = LightmapSettings.lightmapsMode;

            GenerateLightmapInfo(gameObject, newRendererInfos, newLightmapsTextures, newLightmapsTexturesDir, newLightmapsShadowMasks, newLightmapsMode);

            newLightingScenarioData.lightmapsMode = newLightmapsMode;

            newLightingScenarioData.lightmaps = newLightmapsTextures.ToArray();

            if (newLightmapsMode != LightmapsMode.NonDirectional)
            {
                newLightingScenarioData.lightmapsDir = newLightmapsTexturesDir.ToArray();
            }

            newLightingScenarioData.hasRealtimeLights = latestBuildHasReltimeLights;

            newLightingScenarioData.shadowMasks = newLightmapsShadowMasks.ToArray();

            newLightingScenarioData.rendererInfos = newRendererInfos.ToArray();

            var scene_LightProbes = new SphericalHarmonicsL2[LightmapSettings.lightProbes.bakedProbes.Length];
            scene_LightProbes = LightmapSettings.lightProbes.bakedProbes;

            for (int i = 0; i < scene_LightProbes.Length; i++)
            {
                var SHCoeff = new SphericalHarmonics();

                for (int j = 0; j < 3; j++)
                {
                    for (int k = 0; k < 9; k++)
                    {
                        SHCoeff.coefficients[j * 9 + k] = scene_LightProbes[i][j, k];
                    }
                }

                newSphericalHarmonicsList.Add(SHCoeff);
            }

            newLightingScenarioData.lightProbes = newSphericalHarmonicsList.ToArray();

            if (lightingScenariosData.Count < index + 1)
            {
                lightingScenariosData.Insert(index, newLightingScenarioData);
            }
            else
            {
                lightingScenariosData[index] = newLightingScenarioData;
            }

            lightingScenariosCount = lightingScenariosData.Count;

            if (lightingScenesNames == null || lightingScenesNames.Length < lightingScenariosCount)
            {
                lightingScenesNames = new string[lightingScenariosCount];
            }
        }


        static void GenerateLightmapInfo(GameObject root, List<RendererInfo> newRendererInfos, List<Texture2D> newLightmapsLight, List<Texture2D> newLightmapsDir, List<Texture2D> newLightmapsShadow, LightmapsMode newLightmapsMode)
        {
            Terrain terrain = FindObjectOfType<Terrain>();
            if (terrain != null && terrain.lightmapIndex != -1 && terrain.lightmapIndex != 65534)
            {
                RendererInfo terrainRendererInfo = new RendererInfo();
                terrainRendererInfo.lightmapOffsetScale = terrain.lightmapScaleOffset;

                Texture2D lightmaplight = LightmapSettings.lightmaps[terrain.lightmapIndex].lightmapColor;
                terrainRendererInfo.lightmapIndex = newLightmapsLight.IndexOf(lightmaplight);
                if (terrainRendererInfo.lightmapIndex == -1)
                {
                    terrainRendererInfo.lightmapIndex = newLightmapsLight.Count;
                    newLightmapsLight.Add(lightmaplight);
                }

                if (newLightmapsMode != LightmapsMode.NonDirectional)
                {
                    Texture2D lightmapdir = LightmapSettings.lightmaps[terrain.lightmapIndex].lightmapDir;
                    terrainRendererInfo.lightmapIndex = newLightmapsDir.IndexOf(lightmapdir);
                    if (terrainRendererInfo.lightmapIndex == -1)
                    {
                        terrainRendererInfo.lightmapIndex = newLightmapsDir.Count;
                        newLightmapsDir.Add(lightmapdir);
                    }
                }
                if (LightmapSettings.lightmaps[terrain.lightmapIndex].shadowMask != null)
                {
                    Texture2D lightmapShadow = LightmapSettings.lightmaps[terrain.lightmapIndex].shadowMask;
                    terrainRendererInfo.lightmapIndex = newLightmapsShadow.IndexOf(lightmapShadow);
                    if (terrainRendererInfo.lightmapIndex == -1)
                    {
                        terrainRendererInfo.lightmapIndex = newLightmapsShadow.Count;
                        newLightmapsShadow.Add(lightmapShadow);
                    }
                }
                newRendererInfos.Add(terrainRendererInfo);

                if (Application.isEditor)
                {
                    Log("Terrain lightmap stored in" + terrainRendererInfo.lightmapIndex.ToString());
                }
            }

            var renderers = FindObjectsOfType(typeof(Renderer));

            if (Application.isEditor)
            {
                Log("stored info for " + renderers.Length + " meshrenderers");
            }

            foreach (Renderer renderer in renderers)
            {
                if (renderer.lightmapIndex != -1 && renderer.lightmapIndex != 65534)
                {
                    RendererInfo info = new RendererInfo();
                    info.renderer = renderer;
                    info.lightmapOffsetScale = renderer.lightmapScaleOffset;

                    Texture2D lightmaplight = LightmapSettings.lightmaps[renderer.lightmapIndex].lightmapColor;
                    info.lightmapIndex = newLightmapsLight.IndexOf(lightmaplight);
                    if (info.lightmapIndex == -1)
                    {
                        info.lightmapIndex = newLightmapsLight.Count;
                        newLightmapsLight.Add(lightmaplight);
                    }

                    if (newLightmapsMode != LightmapsMode.NonDirectional)
                    {
                        Texture2D lightmapdir = LightmapSettings.lightmaps[renderer.lightmapIndex].lightmapDir;
                        info.lightmapIndex = newLightmapsDir.IndexOf(lightmapdir);
                        if (info.lightmapIndex == -1)
                        {
                            info.lightmapIndex = newLightmapsDir.Count;
                            newLightmapsDir.Add(lightmapdir);
                        }
                    }

                    if (LightmapSettings.lightmaps[renderer.lightmapIndex].shadowMask != null)
                    {
                        Texture2D lightmapShadow = LightmapSettings.lightmaps[renderer.lightmapIndex].shadowMask;
                        info.lightmapIndex = newLightmapsShadow.IndexOf(lightmapShadow);
                        if (info.lightmapIndex == -1)
                        {
                            info.lightmapIndex = newLightmapsShadow.Count;
                            newLightmapsShadow.Add(lightmapShadow);
                        }
                    }

                    newRendererInfos.Add(info);
                }
            }
        }


        private void OnGUI()
        {
            if (isFadeIn || isFadeOut)
            {
                if (Event.current.type.Equals(EventType.Repaint))
                    Graphics.DrawTexture(screenRect, fadeInTexture);
            }

        }


        bool isFadeIn = false;
        bool isFadeOut = false;


        IEnumerator PlayFadeInAndOut(float speed, FadeInOutType typeToPlay, float delaySeconds)
        {

            Color[] fadecolor = fadeInTexture.GetPixels();

            float initAlpha = typeToPlay == FadeInOutType.FADEOUT ? 1f : 0f;
            float targetAlpha = typeToPlay == FadeInOutType.FADEOUT ? 0f : 1f;

            float t = 0f;

            if (typeToPlay == FadeInOutType.FADEOUT)
            {
                fadecolor[0] = fadeInOutColor;
                fadeInTexture.SetPixels(fadecolor);
                fadeInTexture.Apply();

                isFadeOut = true;

                yield return new WaitForSeconds(delaySeconds);

                while (fadecolor[0].a > 0f)
                {
                    t = t + (speed * Time.deltaTime);
                    float alpha = Mathf.Lerp(initAlpha, targetAlpha, t);

                    fadecolor[0].a = Mathf.Clamp(alpha, 0f, 1f);

                    fadeInTexture.SetPixels(fadecolor);
                    fadeInTexture.Apply();

                    yield return new WaitForSeconds(0.1f);

                }

                isFadeOut = false;

            }
            else
            {
                isFadeIn = true;

                while (fadecolor[0].a < 1f)
                {
                    t = t + (speed * Time.deltaTime);
                    float alpha = Mathf.Lerp(initAlpha, targetAlpha, t);

                    fadecolor[0].a = Mathf.Clamp(alpha, 0f, 1f);

                    fadeInTexture.SetPixels(fadecolor);
                    fadeInTexture.Apply();

                    yield return new WaitForSeconds(0.1f);

                }

                isFadeIn = false;

            }

        }


    }

}