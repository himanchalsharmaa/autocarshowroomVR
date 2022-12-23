using System.Collections.Generic;
using UnityEngine;


public class CarLight : MonoBehaviour
{

    private bool lightOn = false;

    public Color LightEmissiveColor = Color.white;
    public float HdrIntensity = 1000f;

    public List<GameObject> PartsForLight = new List<GameObject>();


    private List<Light> CarLights = new List<Light>();
    private List<Renderer> CarLightRenderers = new List<Renderer>();


    void Start()
    {
        lightOn = false;

        RefreshObjects();

        SetLightComponents();
        SetMaterialsForLight();
    }


    public void RefreshObjects()
    {
        CarLights.Clear();

        foreach(GameObject go in PartsForLight)
        {
            if (go == null)
                continue;

            Light[] lgts = go.GetComponentsInChildren<Light>();
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>();

            foreach(Light lgt in lgts)
            {
                if (!CarLights.Contains(lgt))
                    CarLights.Add(lgt);
            }

            foreach(Renderer render in renderers)
            {
                if (!CarLightRenderers.Contains(render))
                    CarLightRenderers.Add(render);
            }
        }

    }


    private void SetMaterialsForLight()
    {
        foreach(Renderer rend in CarLightRenderers)
        {
            if (rend == null)
                continue;

            MaterialPropertyBlock mBlock = new MaterialPropertyBlock();

            if (lightOn)
            {
                mBlock.SetColor("_EmissiveColor", LightEmissiveColor * HdrIntensity);
                rend.SetPropertyBlock(mBlock);
            }
            else
            {
                rend.SetPropertyBlock(null);
            }
        }
    }


    private void SetLightComponents()
    {
        foreach(Light carLight in CarLights)
        {
            if (carLight != null)
                carLight.enabled = lightOn;
        }
    }


    public void ToogleLights()
    {
        lightOn = !lightOn;

        SetLightComponents();
        SetMaterialsForLight();

    }

}
