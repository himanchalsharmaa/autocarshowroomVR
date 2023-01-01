using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using static UnityEngine.XR.XRDisplaySubsystem;


[RequireComponent(typeof(Camera))]
public class RenderToTexture : MonoBehaviour
{
    [HideInInspector]
    public RenderTexture colorTex;
    [HideInInspector]
    public RenderTexture colorTex1;

    [HideInInspector]
    public RenderTexture depthTex;

    // public GameObject forviewmatr;
    
    private bool once=true;
    int count = 0;
    Camera cam;
    Shader PostprocessShader;
    Material PostprocessMaterial;
    float elapsed = 0;
    List<XRDisplaySubsystem> displays = new List<XRDisplaySubsystem>();
   // public RawImage img;
  //  public TMP_Text textinfo;

    void Start()
    {
        Vector3 camPos = gameObject.transform.position;
        Quaternion camRot = gameObject.transform.rotation;
        Debug.Log(gameObject.name);
        cam = GetComponent<Camera>();
        StartSubsystem();
        Debug.Log(XRSettings.enabled);
        colorTex = new RenderTexture(Screen.width*2, Screen.height, 0, RenderTextureFormat.ARGB32);
        colorTex.autoGenerateMips = false;
        colorTex.anisoLevel = 0;
        colorTex.name = "ColorTexture";
       // textinfo.text = "" + XRSettings.enabled;
        //if (XRSettings.enabled)
        //{
        //    colorTex.vrUsage = VRTextureUsage.TwoEyes;
        //  //  colorTex.width *= 2;
        SubsystemManager.GetSubsystems(displays);
       // textinfo.text = "" + displays[0].supportedTextureLayouts;
       displays[0].singlePassRenderingDisabled = true;
       //colorTex.vrUsage = VRTextureUsage.TwoEyes;
        //    if (displays.Count > 0)
        //    {
        //       // displays[0].textureLayout = TextureLayout.Texture2DArray;
        //           Debug.Log(displays[0].singlePassRenderingDisabled);
        //       // textinfo.text = ""+ displays[0].textureLayout;
        //    }

        //}

        colorTex.Create();
        //colorTex1.Create();

        //  cam.depthTextureMode |= DepthTextureMode.Depth;
        //cam.targetTexture = colorTex;
        //cam.SetTargetBuffers(colorTex.colorBuffer, depthTex.depthBuffer);

        //PostprocessShader = Shader.Find("GR/ExtractDepth");

        //if (PostprocessShader == null)
        //    PostprocessShader = Resources.Load<Shader>("ExtractDepth.shader");

        //if (PostprocessShader)
        //{
        //    PostprocessMaterial = new Material(PostprocessShader);
        //    PostprocessMaterial.enableInstancing = true;
        //    Debug.Log("[GR]: Found Shader for Depth Post Processing!");
        //}
        //else
        //{
        //    Debug.LogError("[GR]: Did not find Shader for Depth Post Processing!");
        //}

        gameObject.transform.position=camPos;
        gameObject.transform.rotation=camRot;
        Debug.Log(gameObject.name);

        StartCoroutine(createtexture());
    }

    void Update()
    {


        //cam.Render();


    }
    IEnumerator createtexture()
    {
        yield return new WaitForEndOfFrame();
       // yield return null;
        elapsed += Time.deltaTime;
        //Debug.Log(displays[0].GetRenderPassCount());
        colorTex = displays[0].GetRenderTextureForRenderPass(0);
        if (elapsed > 1)
        {
            if (colorTex == null)
            {
                Debug.Log("NULL TEXTURE");
            }
            else
            {
                Debug.Log(displays[0].GetRenderPassCount());   
            }
            elapsed = elapsed % 1f;
            XRRenderPass xrp1,xrp2;
            displays[0].GetRenderPass(0, out xrp1);
            Debug.Log(xrp1.GetRenderParameterCount());
          //  XRRenderParameter xrpr1, xrpr2;
            //xrp1.GetRenderParameter(cam, 0, out xrpr1);
             //    xrp2.GetRenderParameter(cam, 0, out xrpr2);
          //  Debug.Log(xrpr1.projection);
/*
            RenderTexture lastActive = RenderTexture.active;
            RenderTexture.active = displays[0].GetRenderTextureForRenderPass(0);
            Texture2D texy = new Texture2D(RenderTexture.active.width, RenderTexture.active.height);
            texy.ReadPixels(new Rect(0, 0, RenderTexture.active.width, RenderTexture.active.height), 0, 0);
            texy.Apply();
            byte[] dat = texy.EncodeToPNG();
            File.WriteAllBytes(UnityEngine.Application.persistentDataPath + "/sourceTexture0_" + count + ".png", dat);
            Destroy(texy);
            RenderTexture.active = lastActive;
             Debug.Log("WITTEN");
*/
        }
        StartCoroutine(createtexture());
    }
    void StartSubsystem()
    {
        string match = "Display Sample";
        List<XRDisplaySubsystemDescriptor> displays = new List<XRDisplaySubsystemDescriptor>();
        SubsystemManager.GetSubsystemDescriptors(displays);
        //Unity.XR.SDK.DisplaySampleXRLoader.Start()
        foreach (var d in displays)
        {
            if (d.id.Contains(match))
            {
                XRDisplaySubsystem dispInst = d.Create();

                if (dispInst != null)
                {
                    Debug.Log("Starting display " + d.id);
                    dispInst.Start();
                }
                else
                {
                    Debug.Log("Can' start subsystem: "+d.id);
                }
            }
        }
    }
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        //if (cam.name != Camera.current.name) return;
        // Graphics.Blit(source, depthTex, PostprocessMaterial);
        Graphics.Blit(source, destination);
    }
}