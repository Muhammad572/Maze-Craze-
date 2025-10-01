using System;
using UnityEngine;
using GoogleMobileAds.Api;

public class RewardedAdManager : MonoBehaviour
{
    public static RewardedAdManager Instance;

    private RewardedAd _rewardedAd;
    private string _adUnitId;

    private bool _showWhenLoaded = false;

    // 🔹 Store pending reward callback (theme OR skin)
    private Action _pendingRewardAction;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

#if UNITY_ANDROID
                _adUnitId = "ca-app-pub-3940256099942544/5224354917"; // ✅ Test ID
        #elif UNITY_IPHONE
                _adUnitId = "ca-app-pub-3940256099942544/1712485313"; // ✅ Test ID
                // _adUnitId = "ca-app-pub-8653293678103388/8251012495"; // ✅ Orignal ID
#else
                _adUnitId = null;
#endif

        var requestConfiguration = new RequestConfiguration
        {
            TagForChildDirectedTreatment = TagForChildDirectedTreatment.True
        };
        MobileAds.SetRequestConfiguration(requestConfiguration);

        MobileAds.Initialize(initStatus => { LoadRewardedAd(); });
    }

    private void LoadRewardedAd()
    {
        // ✅ COMBINED AND CORRECTED CHECK
        if (!IsInternetAvailable() || string.IsNullOrEmpty(_adUnitId))
        {
            Debug.LogWarning("Rewarded Ad: No internet or invalid Ad Unit ID. Cannot load.");
            // Optional: Show a user-facing popup here
            // UIManager.Instance.ShowMessagePopup("No internet connection. Please try again.");
            return;
        }

        Debug.Log("Loading Rewarded Ad...");
        var adRequest = new AdRequest();

        RewardedAd.Load(_adUnitId, adRequest, (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("Rewarded Ad failed to load: " + error);
                return;
            }

            Debug.Log("Rewarded Ad loaded successfully.");
            _rewardedAd = ad;
            RegisterEventHandlers(ad);

            if (_showWhenLoaded)
            {
                _showWhenLoaded = false;
                ShowRewardedAd();
            }
        });
    }
    // 🔹 New: queue one unlock action at a time
    public void ShowRewardedAd(Action onRewardEarned = null)
    {
        if (onRewardEarned != null)
            _pendingRewardAction = onRewardEarned; // store action

        if (_rewardedAd != null && _rewardedAd.CanShowAd())
        {
            Debug.Log("Showing Rewarded Ad...");
            _rewardedAd.Show(reward =>
            {
                Debug.Log($"User earned reward: {reward.Type} - {reward.Amount}");

                // 🔹 Only ONE unlock per ad
                _pendingRewardAction?.Invoke();
                _pendingRewardAction = null;
            });
        }
        else
        {
            Debug.LogWarning("Rewarded Ad not ready, will show once loaded.");
            _showWhenLoaded = true;
            LoadRewardedAd();
        }
    }

    private void RegisterEventHandlers(RewardedAd ad)
    {
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Rewarded Ad opened.");
        };

        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Rewarded Ad closed. Reloading...");
            LoadRewardedAd();
        };

        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Rewarded Ad impression recorded.");
        };

        ad.OnAdClicked += () =>
        {
            Debug.Log("Rewarded Ad clicked.");
        };

        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Rewarded Ad failed to show: " + error);
            LoadRewardedAd();
        };
    }

    private bool IsInternetAvailable()
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }

    private void OnDestroy()
    {
        if (_rewardedAd != null)
        {
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }
    }
}
