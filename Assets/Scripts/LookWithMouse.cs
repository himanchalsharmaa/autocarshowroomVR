using UnityEngine;


public class LookWithMouse : MonoBehaviour
{
    public bool lockCursor;
    public float mouseSensitivity = 100f;
    public Transform playerBody;
    public GameObject pauseMenuUI;

    private PausedMenu pauseMenu;

    private float xRotation = 0f;

    public float speedH = 2.0f;
    public float speedV = 2.0f;

    private void Start()
    {
        if (pauseMenuUI != null)
        {
            pauseMenu = pauseMenuUI.GetComponent<PausedMenu>();
        }
    }


    void Update()
    {
        if (pauseMenu != null)
        {
            MoveCamera();
        }
    }


    private void MoveCamera()
    {
        if (!CinemachineCameraManager.IsPlayerCam)
            return;

        if (pauseMenu.isGamePaused)
            return;

        this.transform.localPosition = Vector3.zero;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        playerBody.Rotate(Vector3.up * mouseX);
    }

}
