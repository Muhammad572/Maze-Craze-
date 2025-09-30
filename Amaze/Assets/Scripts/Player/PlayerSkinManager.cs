using UnityEngine;

public class PlayerSkinManager : MonoBehaviour
{
    public static PlayerSkinManager instance;

    [Header("Player Skins")]
    public GameObject[] playerPrefabs;

    private int selectedSkinIndex = 0;
    private GameObject currentPlayer;

    [Header("Assign Lock Overlays (Same Order)")]
    public GameObject[] skinLockOverlays;

    private const string PREF_KEY = "UnlockedSkin_";

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // ClearAllSkinUnlocks();
        RefreshSkinUI();
        LoadSkin();
    }

    // ✅ UNLOCK SYSTEM
    public bool IsUnlocked(int index)
    {
        if (index == 0) return true; // Skin 0 always unlocked
        return PlayerPrefs.GetInt(PREF_KEY + index, 0) == 1;
    }

    public void UnlockSkin(int index)
    {
        PlayerPrefs.SetInt(PREF_KEY + index, 1);
        PlayerPrefs.Save();
        Debug.Log($"✅ Skin {index} unlocked!");
    }

    public void ClearAllSkinUnlocks()
    {
        for (int i = 0; i < playerPrefabs.Length; i++)
        {
            if (i == 0) continue;
            PlayerPrefs.DeleteKey(PREF_KEY + i);
        }
        PlayerPrefs.Save();
        Debug.Log("♻️ All skins reset. Only Skin 0 unlocked.");
    }

    public void SelectSkin(int index)
    {
        if (!IsUnlocked(index))
        {
            Debug.Log($"❌ Skin {index} is locked!");

            if (RewardedAdManager.Instance != null)
            {
                RewardedAdManager.Instance.ShowRewardedAd(() =>
                {
                    Debug.Log("🎥 Ad finished callback fired!");
                    RewardPanelManager.Instance.ShowReward(
                        playerPrefabs[index].GetComponentInChildren<SpriteRenderer>().sprite,
                        () =>
                        {
                            Debug.Log("🎁 Claim pressed, unlocking skin " + index);
                            UnlockSkin(index);
                            ApplySkin(index);
                            RefreshSkinUI();
                        });
                });
            }
            else
            {
                Debug.Log("🎁 Rewarded ads not set — auto-unlock fallback.");
                UnlockSkin(index);
                ApplySkin(index);
                RefreshSkinUI();
            }
            return;
        }

        ApplySkin(index);
    }
    
    private void RefreshSkinUI()
    {
        for (int i = 0; i < playerPrefabs.Length; i++)
        {
            bool unlocked = IsUnlocked(i);

            if (skinLockOverlays != null && i < skinLockOverlays.Length && skinLockOverlays[i] != null)
                skinLockOverlays[i].SetActive(!unlocked);
        }
    }


    private void ApplySkin(int index)
    {
        selectedSkinIndex = index;
        PlayerPrefs.SetInt("SelectedSkin", index);
        PlayerPrefs.Save();

        if (currentPlayer != null)
        {
            ChangeSkin(index);
        }

        Debug.Log($"🎨 Skin {index} selected!");
    }

    public void LoadSkin()
    {
        selectedSkinIndex = PlayerPrefs.GetInt("SelectedSkin", 0);

        if (currentPlayer != null)
        {
            ChangeSkin(selectedSkinIndex);
        }
    }

    public GameObject SpawnSelectedSkinAt(Vector3 position)
    {
        selectedSkinIndex = PlayerPrefs.GetInt("SelectedSkin", 0);

        if (currentPlayer != null)
        {
            currentPlayer.transform.position = position;
            Debug.Log("♻️ Reusing existing player instead of spawning a new one.");
            return currentPlayer;
        }

        GameObject prefab = playerPrefabs[selectedSkinIndex];
        currentPlayer = Instantiate(prefab, position, Quaternion.identity);
        return currentPlayer;
    }

    public void ChangeSkin(int index)
    {
        if (currentPlayer == null) return;

        Vector3 pos = currentPlayer.transform.position;
        Quaternion rot = currentPlayer.transform.rotation;

        Destroy(currentPlayer);

        GameObject prefab = playerPrefabs[index];
        currentPlayer = Instantiate(prefab, pos, rot);

        LevelManager.instance.NotifyPlayerReplaced(currentPlayer);

        Debug.Log("🎨 Skin changed to index " + index);
    }

    public GameObject GetCurrentPlayer()
    {
        return currentPlayer;
    }

}
