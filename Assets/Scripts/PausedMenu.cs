using UnityEngine;

enum Step
{
    Init,
    MenuActivated,
    FirstMenuIn,
    FirstMenuOut
}

public class PausedMenu : MonoBehaviour
{

    Step menuState;

    public bool isGamePaused;
    public GameObject pauseMenuUI;
    public KeyCode pauseKey;
    public KeyCode exitAppKey;


   private void Update()
   {
        if (Input.GetKeyUp(exitAppKey))
        {
            if (Application.isPlaying)
            {
                Application.Quit();
            }
        }

        if (Input.GetKeyDown(pauseKey))
        {
            if (isGamePaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }


    public void Resume()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        isGamePaused = false;
        pauseMenuUI.SetActive(false);
    }


    public void Pause()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        isGamePaused = true;
        pauseMenuUI.SetActive(true);
    }

}