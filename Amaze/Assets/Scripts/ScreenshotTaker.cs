using UnityEngine;

public class ScreenshotTaker : MonoBehaviour
{
    public string screenshotFileName = "iPadScreenshot.png";
    public int resolutionMultiplier = 1; // 1 = native resolution, 2 = 2x resolution

    void Update()
    {
        // Press the 'K' key to take a screenshot
        if (Input.GetKeyDown(KeyCode.K))
        {
            // This captures a screenshot at the resolution set in the Game View
            // multiplied by the 'resolutionMultiplier'.
            ScreenCapture.CaptureScreenshot(screenshotFileName, resolutionMultiplier);
            Debug.Log("Screenshot taken and saved as: " + screenshotFileName);
        }
    }
}