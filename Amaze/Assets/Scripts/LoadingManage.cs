using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class LoadingManager : MonoBehaviour
{
    public GameObject loadingPanel;
    public Image loadingFrontImage;
    public TMP_Text loadingText;
    public string sceneToLoad;
    
    // You can adjust this speed to control how fast the bar fills up.
    public float fillSpeed = 0.5f;

    private void Start()
    {
        // Start the loading process
        StartCoroutine(LoadAsyncScene());
    }

    private IEnumerator LoadAsyncScene()
    {
        // Set the loading panel to active
        loadingPanel.SetActive(true);

        // Load the scene asynchronously
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);

        // Don't activate the scene until the loading is complete
        asyncLoad.allowSceneActivation = false;

        // The target progress for the loading bar
        float targetProgress = 0;

        // Loop until the loading is complete
        while (!asyncLoad.isDone)
        {
            // The actual progress of the async load
            float actualProgress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            
            // Gradually move the target progress towards the actual progress
            targetProgress = Mathf.MoveTowards(targetProgress, actualProgress, Time.deltaTime * fillSpeed);

            // Update the loading bar and text based on the smoothed progress
            loadingFrontImage.fillAmount = targetProgress;
            loadingText.text = "Loading: " + (int)(targetProgress * 100) + "%";

            // If the loading is complete and the smoothed progress is also at max, activate the scene
            if (asyncLoad.progress >= 0.9f && targetProgress >= 0.99f)
            {
                // Optionally, you can add a fixed wait or a user input here
                // before setting allowSceneActivation to true.
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }

        // Deactivate the loading panel after the scene is loaded
        loadingPanel.SetActive(false);
    }
}