using UnityEngine;

public class TurnTableAdhesive : MonoBehaviour
{

    public GameObject PlayerController;

    private Transform OriginalPlayerControllerRoot;

    private Collider attachedCollider;


    void Start()
    {
        attachedCollider = this.GetComponent<Collider>();

        if (attachedCollider != null)
            attachedCollider.isTrigger = true;

        if (PlayerController != null)
            OriginalPlayerControllerRoot = PlayerController.transform.parent;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (CinemachineCameraManager.IsPlayerCam)
            PlayerController.transform.SetParent(this.transform.parent);
    }


    private void OnTriggerExit(Collider other)
    {
        PlayerController.transform.SetParent(OriginalPlayerControllerRoot);
    }

}
