using UnityEngine;
using System;

public class RewardedAdManagerTest : MonoBehaviour
{
    public static RewardedAdManagerTest Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Stub method: replace with real ad SDK integration
    public void ShowRewardedAd(Action onReward)
    {
        Debug.Log("[Stub] RewardedAd shown. Immediately granting reward...");
        onReward?.Invoke();
    }
}
