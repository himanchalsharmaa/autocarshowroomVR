﻿using UnityEngine;
using System.Collections.Generic;

public class CarColor : MonoBehaviour
{

    public List<GameObject> colouredBodyWorkParts = new List<GameObject>();

    public List<Material> mColourSet = new List<Material>();

    public List<ReflectionProbe> reflectionProbes = new List<ReflectionProbe>();

    private int currentColourIndex = 0;



    public void SetColourByIndex(int index)
    {
        if (mColourSet.Count == 0)
        {
            Debug.LogWarning("Empty body Colour sets.");
            return;
        }

        if (index >= mColourSet.Count)
        {
            Debug.LogError("Out of index.");
            return;
        }

        SetPartMaterial(mColourSet[index]);
    }


    public void SetNextColour()
    {
        if (mColourSet.Count == 0)
            return;

        if (currentColourIndex < mColourSet.Count - 1)
        {
            currentColourIndex++;
        }
        else if (currentColourIndex == mColourSet.Count - 1)
        {
            currentColourIndex = 0;
        }

        SetPartMaterial(mColourSet[currentColourIndex]);
    }


    bool IsCarPaint(Material mat)
    {
        if (mColourSet.Contains(mat))
        {
            return true;
        }
        return false;
    }


    private void SetPartColour(Color lColour)
    {
        if (colouredBodyWorkParts.Count == 0)
            return;

        foreach (GameObject bodyPart in colouredBodyWorkParts)
        {
            MeshRenderer[] lMeshRenderers = bodyPart.GetComponentsInChildren<MeshRenderer>();

            foreach (MeshRenderer lMeshRenderer in lMeshRenderers)
            {
                if (lMeshRenderer != null)
                {
                    foreach (Material mat in lMeshRenderer.sharedMaterials)
                    {
                        if (IsCarPaint(mat))
                            mat.SetColor("_BaseColor", lColour);
                    }
                }
            }
        }
    }


    private void UpdateReflectionProbes()
    {
        foreach (ReflectionProbe refProbe in reflectionProbes)
        {
            refProbe.gameObject.SetActive(false);
            refProbe.gameObject.SetActive(true);
        }
    }


    private void SetPartMaterial(Material lMaterial)
    {
        if (colouredBodyWorkParts.Count == 0)
            return;

        foreach (GameObject bodyPart in colouredBodyWorkParts)
        {
            MeshRenderer[] lMeshRenderers = bodyPart.GetComponentsInChildren<MeshRenderer>();

            foreach (MeshRenderer lMeshRenderer in lMeshRenderers)
            {
                if (lMeshRenderer != null)
                {
                    Material[] lSharedMaterials = lMeshRenderer.sharedMaterials;
                    bool update = false;
                    for (int i = 0; i < lSharedMaterials.Length; i++)
                    {
                        if (IsCarPaint(lSharedMaterials[i]))
                        {
                            lSharedMaterials[i] = lMaterial;
                            update = true;
                        }
                    }
                    if (update) lMeshRenderer.sharedMaterials = lSharedMaterials;
                }
            }
        }
        UpdateReflectionProbes();
    }
}