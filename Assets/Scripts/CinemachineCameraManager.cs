using System.Collections.Generic;
using UnityEngine;

using System;


[Serializable]
public class CinemachineCameraSets
{
    public GameObject CinemachineCamera;
    public GameObject ParentTransform;
}


public class CinemachineCameraManager : MonoBehaviour
{
    public List<CinemachineCameraSets> cinemachineCameraSets = new List<CinemachineCameraSets>();

    public int InitCamIndex = 0;

    public static bool IsPlayerCam; 


    public void CameraPlayer()
    {
        IsPlayerCam = true;
        ChangeCamera(cinemachineCameraSets[0].CinemachineCamera);
    }


    public void CameraFreeMode()
    {
        IsPlayerCam = false;
        ChangeCamera(cinemachineCameraSets[1].CinemachineCamera);
    }


    public void CameraOne()
    {
        IsPlayerCam = false;
        ChangeCamera(cinemachineCameraSets[2].CinemachineCamera);
    }


    public void CameraTwo()
    {
        IsPlayerCam = false;
        ChangeCamera(cinemachineCameraSets[3].CinemachineCamera);
    }


    public void CameraThree()
    {
        IsPlayerCam = false;
        ChangeCamera(cinemachineCameraSets[4].CinemachineCamera);
    }


    void SetCamParents()
    {
        foreach (CinemachineCameraSets curCamSet in cinemachineCameraSets)
        {
            if (curCamSet.ParentTransform != null)
                curCamSet.CinemachineCamera.transform.SetParent(curCamSet.ParentTransform.transform);
        }
    }


    private void Start()
    {
        ChangeCameraByIndex(InitCamIndex);

        SetCamParents();
    }


    public void ChangeCameraByIndex(int index)
    {
        if (index < cinemachineCameraSets.Count)
        {
            ChangeCamera(cinemachineCameraSets[index].CinemachineCamera);
        }
    }


    private void ChangeCamera(GameObject lCamera)
    {
        if (lCamera == null)
            return;

        foreach (CinemachineCameraSets curCamSet in cinemachineCameraSets)
        {
            if (curCamSet.CinemachineCamera == cinemachineCameraSets[0].CinemachineCamera)
            {
                continue;
            }

            curCamSet.CinemachineCamera.SetActive(false);
            
        }

        lCamera.SetActive(true);
    }
}
