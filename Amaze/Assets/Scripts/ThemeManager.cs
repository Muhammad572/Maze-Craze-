using UnityEngine;
using UnityEngine.UI;

public class ThemeManager : MonoBehaviour
{
    public static ThemeManager Instance;

    [Header("Assign Theme Buttons (Index Order)")]
    public Button[] themeButtons;

    [Header("Assign Lock Overlays (Same Order)")]
    public GameObject[] lockOverlays;

    [Header("Assign Theme Preview Images (Same Order)")]
    public Sprite[] themeSprites; // ✅ NEW: for Reward Panel preview

    private int selectedThemeIndex = 0;

    private const string PREF_KEY = "UnlockedTheme_";
    private const string SELECTED_THEME_KEY = "SelectedTheme"; // ✅ new key

    [Header("Theme Preview")]
    public Sprite themePreviewSprite; // assign white placeholder image

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
        int savedTheme = PlayerPrefs.GetInt(SELECTED_THEME_KEY, 0); // default to 0
        if (IsUnlocked(savedTheme))
        {
            SelectTheme(savedTheme);
        }
        else
        {
            SelectTheme(0); // fallback
        }
    }

    private void SetupButtons()
    {
        for (int i = 0; i < themeButtons.Length; i++)
        {
            int index = i;
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
            Debug.Log($"🎨 Theme {index} is locked! Showing ad...");
            TryUnlockTheme(index);
        }
    }

    private void TryUnlockTheme(int index)
    {
        if (RewardedAdManager.Instance != null)
        {
            RewardedAdManager.Instance.ShowRewardedAd(() =>
            {
                // 🎨 Use white sprite + button color
                Sprite preview = themePreviewSprite;
                Color themeColor = themeButtons[index].GetComponent<Image>().color;

                RewardPanelManager.Instance.ShowReward(preview, () =>
                {
                    UnlockTheme(index);
                    SelectTheme(index);
                }, themeColor); // pass color too
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


    private void SelectTheme(int index)
    {
        selectedThemeIndex = index;

        // Save selected theme
        PlayerPrefs.SetInt(SELECTED_THEME_KEY, selectedThemeIndex);
        PlayerPrefs.Save();

        // Example: use button color as background
        Color selectedColor = themeButtons[index].GetComponent<Image>().color;
        Camera.main.backgroundColor = selectedColor;

        Debug.Log($"✅ Theme {index} selected! Color = {selectedColor}");
    }

    private bool IsUnlocked(int index)
    {
        if (index == 0) return true;
        return PlayerPrefs.GetInt(PREF_KEY + index, 0) == 1;
    }

    private void UnlockTheme(int index)
    {
        PlayerPrefs.SetInt(PREF_KEY + index, 1);
        PlayerPrefs.Save();
        RefreshUI();
        Debug.Log($"🎉 Theme {index} unlocked!");
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
            if (i == 0) continue;
            PlayerPrefs.DeleteKey(PREF_KEY + i);
        }
        PlayerPrefs.Save();
        RefreshUI();
        Debug.Log("♻️ All themes reset. Only Theme 0 unlocked.");
    }
}
