using System.Collections;
using UnityEngine;
using System;


public class ScreenCapturer : MonoBehaviour
{
    public void DoSingleCapture()
    {
        Canvas[] targetCanvases = this.gameObject.transform.root.gameObject.GetComponentsInChildren<Canvas>();

        if (targetCanvases.Length != 0)
            StartCoroutine(DoCapture(targetCanvases));
    }

    IEnumerator DoCapture(Canvas[] targetCanvases)
    {
        yield return null;

        foreach(Canvas targetCanvas in targetCanvases)
            targetCanvas.enabled = false;

        yield return new WaitForEndOfFrame();

        string currentScreenshot = string.Format("Capture_{0}_{1}.png",Camera.main.name, DateTime.Now.ToLongTimeString().Replace(" ", "_").Replace(":", "_"));
        ScreenCapture.CaptureScreenshot(currentScreenshot);

        foreach (Canvas targetCanvas in targetCanvases)
            targetCanvas.enabled = true;

    }
}
