using UnityEngine;
using System.Collections.Generic;

public class CamerasActive : MonoBehaviour
{

    public List<GameObject> targetCameras = new List<GameObject>();

    public static CamerasActive instance;


    private void Awake()
    {
        instance = this;
    }


    public void ActivateCameraDisableOthers(GameObject parentCameraObject)
    {
        foreach (GameObject targetCamera in targetCameras)
        {
            targetCamera.SetActive(false);
        }

        if (parentCameraObject != null)
        {
            if (parentCameraObject.transform.childCount != 0)
            {
                parentCameraObject.transform.GetChild(0).gameObject.SetActive(true);
            }
        }
    }
}