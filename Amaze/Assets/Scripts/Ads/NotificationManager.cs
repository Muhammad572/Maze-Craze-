using UnityEngine;
using TMPro;
using System.Collections;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager instance;

    [Tooltip("The TextMeshPro object used to display messages.")]
    public TextMeshProUGUI notificationText;

    [Header("Notification Settings")]
    public float displayDuration = 3.0f;
    public float fadeDuration = 0.5f;

    private Coroutine displayRoutine;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            // Ensure the text is hidden at start
            if (notificationText != null)
            {
                notificationText.gameObject.SetActive(false);
                notificationText.alpha = 0f;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowToast(string message)
    {
        if (notificationText == null)
        {
            Debug.LogError("NotificationManager requires a TextMeshProUGUI component assigned to notificationText.");
            return;
        }

        if (displayRoutine != null)
        {
            StopCoroutine(displayRoutine);
        }

        notificationText.text = message;
        displayRoutine = StartCoroutine(DisplayToastRoutine());
    }

    private IEnumerator DisplayToastRoutine()
    {
        notificationText.gameObject.SetActive(true);

        // Fade In
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            notificationText.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null;
        }
        notificationText.alpha = 1f;

        // Display hold time
        yield return new WaitForSeconds(displayDuration);

        // Fade Out
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            notificationText.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null;
        }
        notificationText.alpha = 0f;
        notificationText.gameObject.SetActive(false);

        displayRoutine = null;
    }
}