using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class disableCameraOrbital : MonoBehaviour
{
    public CinemachineBrain cinemachinebrain;
    public GameObject camera_orbital;

    private void Awake()
    {
        StartCoroutine(disablecam());
    }
    public void disablecameraorbital()
    {
        StartCoroutine(disablecam());
    }
    IEnumerator disablecam()
    {
        if (!cinemachinebrain.IsBlending)
        {
            yield return new WaitForSeconds(2);
            camera_orbital.SetActive(false);
        }
        else if (cinemachinebrain.IsBlending)
        {
            yield return new WaitForSeconds(1);
            StartCoroutine(disablecam());
        }
    }
}
