using UnityEngine;
using UnityEngine.UI;

public class ThemeManager : MonoBehaviour
{
    public static ThemeManager Instance;

    [Header("Assign Theme Buttons (Index Order)")]
    public Button[] themeButtons;

    [Header("Assign Lock Overlays (Same Order)")]
    public GameObject[] lockOverlays;   // lock images on top of each button

    private int selectedThemeIndex = 0;
    private const string PREF_KEY = "UnlockedTheme_";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SetupButtons();
        RefreshUI();
    }

    void Start()
    {
        // ClearAllUnlocks();
    }

    private void SetupButtons()
    {
        for (int i = 0; i < themeButtons.Length; i++)
        {
            int index = i; // local copy for closure
            themeButtons[i].onClick.AddListener(() => OnThemeButtonClicked(index));
        }
    }

    private void OnThemeButtonClicked(int index)
    {
        if (IsUnlocked(index))
        {
            SelectTheme(index);
        }
        else
        {
            Debug.Log($"Theme {index} is locked. Attempting unlock...");
            TryUnlockTheme(index);
        }
    }

    private void TryUnlockTheme(int index)
    {
        if (RewardedAdManager.Instance != null)
        {
            RewardedAdManager.Instance.ShowRewardedAd(() =>
            {
                UnlockTheme(index); // ✅ this already refreshes UI
                SelectTheme(index);
            });
        }
        else
        {
    #if UNITY_EDITOR
            Debug.Log($"[Editor] Auto-unlocking theme {index}.");
            UnlockTheme(index);
            SelectTheme(index);
    #else
            Debug.Log("Rewarded ads not set up yet! Auto-unlocking for now.");
            UnlockTheme(index);
            SelectTheme(index);
    #endif
        }
    }

    private void RefreshButtonUI(int index)
    {
        bool unlocked = IsUnlocked(index);
        if (lockOverlays != null && index < lockOverlays.Length && lockOverlays[index] != null)
        {
            lockOverlays[index].SetActive(!unlocked);
        }
    }

    private void SelectTheme(int index)
    {
        selectedThemeIndex = index;

        // Example: fetch button color
        Color selectedColor = themeButtons[index].GetComponent<Image>().color;

        // Apply where needed (example: background or player skin)
        Camera.main.backgroundColor = selectedColor;

        Debug.Log($"Theme {index} selected! Color = {selectedColor}");
    }

    private bool IsUnlocked(int index)
    {
        if (index == 0) return true; // Theme 0 always unlocked
        return PlayerPrefs.GetInt(PREF_KEY + index, 0) == 1;
    }

    private void UnlockTheme(int index)
    {
        PlayerPrefs.SetInt(PREF_KEY + index, 1);
        PlayerPrefs.Save();
        RefreshUI();
    }

    private void RefreshUI()
    {
        for (int i = 0; i < themeButtons.Length; i++)
        {
            bool unlocked = IsUnlocked(i);

            if (lockOverlays != null && i < lockOverlays.Length && lockOverlays[i] != null)
                lockOverlays[i].SetActive(!unlocked);
        }
    }

    [ContextMenu("Clear All Theme Unlocks")]
    public void ClearAllUnlocks()
    {
        for (int i = 0; i < themeButtons.Length; i++)
        {
            if (i == 0) continue; // Keep theme 0 always unlocked
            PlayerPrefs.DeleteKey(PREF_KEY + i);
        }
        PlayerPrefs.Save();

        Debug.Log("All theme unlocks cleared! Only Theme 0 is unlocked now.");
        RefreshUI();
    }
}
