using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR;
using static UnityEngine.XR.XRDisplaySubsystem;

public class GetDisplay : MonoBehaviour
{
    List<XRDisplaySubsystem> displayso;
    float elapsed = 0;
    Camera cam;
    public TMP_Text textinfo;
    public RenderTexture colorTex;

    // Start is called before the first frame update
    void Start()
    {


        cam = GetComponent<Camera>();
        displayso = new List<XRDisplaySubsystem>();
        SubsystemManager.GetInstances(displayso);
        if (displayso.Count > 0)
        {
            displayso[0].singlePassRenderingDisabled = false;
        }

        colorTex = new RenderTexture(Screen.width * 2, Screen.height, 0, RenderTextureFormat.ARGB32);
        colorTex.autoGenerateMips = false;
        colorTex.anisoLevel = 0;
        colorTex.name = "ColorTexture";
        //colorTex.vrUsage = VRTextureUsage.TwoEyes;
        colorTex.Create();

        StartCoroutine(createtexture());

    }
    IEnumerator createtexture()
    {
        yield return new WaitForEndOfFrame();
        elapsed += Time.deltaTime;
        if (elapsed > 1)
        {
            elapsed = elapsed % 1f;
               XRRenderPass xrp1 = new XRRenderPass();
               displayso[0].GetRenderPass(0, out xrp1);
               XRRenderParameter xrpr1;
              xrp1.GetRenderParameter(cam, 0, out xrpr1);
              Debug.Log(xrpr1.projection);
              XRRenderPass xrp2 = new XRRenderPass();
             displayso[0].GetRenderPass(1, out xrp2);
            XRRenderParameter xrpr2;
            xrp2.GetRenderParameter(cam, 0, out xrpr2);
            Debug.Log(xrpr2.projection);
            //  xrpr1.projection = Matrix4x4.zero;
            //Debug.Log(xrpr2.projection);
            //elapsed = elapsed % 1f;
            //count += 1;
            colorTex = displayso[0].GetRenderTextureForRenderPass(0);
            //colorTex1 = displays[0].GetRenderTextureForRenderPass(1);
            // RenderTexture lastActive = RenderTexture.active;

            //RenderTexture.active= displays[0].GetRenderTextureForRenderPass(0);
            //Texture2D texy = new Texture2D(RenderTexture.active.width, RenderTexture.active.height);
            //texy.ReadPixels(new Rect(0, 0, RenderTexture.active.width, RenderTexture.active.height), 0, 0);
            //texy.Apply();
            //byte[] dat = texy.EncodeToPNG();
            //textinfo.text=""+ displays[0].GetRenderPassCount();
            //File.WriteAllBytes(UnityEngine.Application.persistentDataPath + "/sourceTexture0_" + count + ".png", dat);
            //Destroy(texy);

            //RenderTexture.active = displays[0].GetRenderTextureForRenderPass(1);
            //Texture2D texy1 = new Texture2D(RenderTexture.active.width, RenderTexture.active.height);
            //texy.ReadPixels(new Rect(0, 0, RenderTexture.active.width, RenderTexture.active.height), 0, 0);
            //texy.Apply();
            //byte[] dat1 = texy1.EncodeToPNG();
            //textinfo.text = "" + displays[0].GetRenderPassCount();
            //File.WriteAllBytes(UnityEngine.Application.persistentDataPath + "/sourceTexture1_" + count + ".png", dat);

            ////  Graphics.Blit(texy, colorTex1);
            //Destroy(texy);
            //RenderTexture.active = lastActive;
        }
        StartCoroutine(createtexture());
    }
    // Update is called once per frame
    void Update()
    {
       /* 
        if (displayso[0].GetRenderPassCount() > 0)
        {
            Debug.Log(displayso[0].GetRenderPassCount());
                XRRenderPass xrp;
                displayso[0].GetRenderPass(0, out xrp);
                XRRenderParameter xrpr;
                xrp.GetRenderParameter(cam, 0, out xrpr);
                Debug.Log(xrpr.projection);
                XRRenderPass xrp1;
                displayso[0].GetRenderPass(1, out xrp1);
                XRRenderParameter xrpr1;
                xrp1.GetRenderParameter(cam, 0, out xrpr1);
                Debug.Log(xrpr1.projection);      
            } */
        }
    }
