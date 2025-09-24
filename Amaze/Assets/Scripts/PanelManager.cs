using UnityEngine;
using System.Collections.Generic; // Used for a list to easily manage panels

public class PanelManager : MonoBehaviour
{
    public GameObject themePanel;
    public GameObject skinPanel;

    // 🔔 Declare the event
    public static event System.Action<bool> OnPauseStateChanged;

    private List<GameObject> allPanels = new List<GameObject>();

    void Awake()
    {
        if (themePanel != null) allPanels.Add(themePanel);
        if (skinPanel != null) allPanels.Add(skinPanel);

        CloseAllPanels();
    }

    public void OpenPanel(GameObject panelToOpen)
    {
        SetGamePaused(true);

        foreach (GameObject panel in allPanels)
        {
            if (panel == panelToOpen)
                panel.SetActive(true);
            else
                panel.SetActive(false);
        }
    }

    public void TogglePanel(GameObject panelToToggle)
    {
        bool isPanelAlreadyActive = panelToToggle.activeSelf;

        if (!isPanelAlreadyActive)
            SetGamePaused(true);

        CloseAllPanels();
        panelToToggle.SetActive(!isPanelAlreadyActive);
    }

    public void CloseAllPanels()
    {
        foreach (GameObject panel in allPanels)
        {
            if (panel != null)
                panel.SetActive(false);
        }

        bool anyPanelIsActive = false;
        foreach (GameObject panel in allPanels)
        {
            if (panel != null && panel.activeSelf)
            {
                anyPanelIsActive = true;
                break;
            }
        }

        if (!anyPanelIsActive)
            SetGamePaused(false);
    }

    private void SetGamePaused(bool isPaused)
    {
        Time.timeScale = isPaused ? 0f : 1f;

        // 🔔 Notify listeners (like SwipeSlidePlayer)
        OnPauseStateChanged?.Invoke(isPaused);
    }
    
    public static void TriggerPauseState(bool isPaused)
    {
        OnPauseStateChanged?.Invoke(isPaused);
    }
}
