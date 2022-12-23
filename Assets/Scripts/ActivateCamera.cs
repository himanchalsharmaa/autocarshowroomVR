using UnityEngine;

public class ActivateCamera : MonoBehaviour
{
    void OnClick()
    {
        CamerasActive.instance.ActivateCameraDisableOthers(this.gameObject);
    }
}