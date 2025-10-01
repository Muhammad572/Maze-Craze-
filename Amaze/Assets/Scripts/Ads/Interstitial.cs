using System;
using UnityEngine;
using GoogleMobileAds;
using GoogleMobileAds.Api;

public class Interstitial : MonoBehaviour
{
    public static Interstitial instance;

#if UNITY_ANDROID
    private string _adUnitId = "ca-app-pub-3940256099942544/1033173712"; // test
#elif UNITY_IPHONE
    private string _adUnitId = "ca-app-pub-3940256099942544/4411468910"; // test
    // private string _adUnitId = "ca-app-pub-8653293678103388/9567963590"; // Orignal
#else
    private string _adUnitId = "unused";
#endif

    private InterstitialAd _interstitialAd;

    private Action _onAdClosed; // 🔑 store callback

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // ... (AdMob initialization code remains the same)
        RequestConfiguration requestConfiguration = new RequestConfiguration
        {
            TagForChildDirectedTreatment = TagForChildDirectedTreatment.True
        };
        MobileAds.SetRequestConfiguration(requestConfiguration);

        MobileAds.Initialize(initStatus =>
        {
            // Initialization is safe to log directly
            Debug.Log("AdMob initialized.");
            
            // Loading the ad should be done on the main thread if possible, 
            // but the callback itself is often thread-safe. Keep LoadInterstitialAd
            // as is, since its internal logic is already safe.
            LoadInterstitialAd(); 
        });
    }

    // ... (LoadInterstitialAd and IsAdAvailable methods remain the same)
    public void LoadInterstitialAd()
    {
        if (_interstitialAd != null)
        {
            _interstitialAd.Destroy();
            _interstitialAd = null;
        }

        Debug.Log("Loading Interstitial Ad...");

        var adRequest = new AdRequest();
        InterstitialAd.Load(_adUnitId, adRequest, (InterstitialAd ad, LoadAdError error) =>
        {
            // The Load callback itself is generally safe on the background thread,
            // but we wrap the assignment and logging to be absolutely sure.
            UnityMainThread.Run(() =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError($"Interstitial failed to load: {error?.GetMessage()}");
                    return;
                }

                _interstitialAd = ad;
                RegisterEventHandlers(ad);
                Debug.Log("Interstitial ad loaded successfully.");
            });
        });
    }

    /// <summary>
    /// Returns true if ad is loaded and ready to show.
    /// </summary>
    public bool IsAdAvailable()
    {
        return _interstitialAd != null && _interstitialAd.CanShowAd();
    }

    /// <summary>
    /// Show the ad. If you pass a callback, it will fire after close/fail.
    /// </summary>
    public void ShowInterstitialAd(Action onClosed = null)
    {
        if (IsAdAvailable())
        {
            _onAdClosed = onClosed; // save callback
            Debug.Log("Showing interstitial ad...");
            _interstitialAd.Show();
        }
        else
        {
            Debug.LogWarning("Interstitial ad not ready. Skipping...");
            onClosed?.Invoke(); // fail-safe
        }
    }


    private void RegisterEventHandlers(InterstitialAd ad)
    {
        ad.OnAdFullScreenContentClosed += () =>
        {
            // 🚨 CRITICAL FIX: Wrap all Unity API calls in UnityMainThread.Run()
            UnityMainThread.Run(() =>
            {
                Debug.Log("Interstitial closed.");
                // PanelManager interaction must be on main thread
                PanelManager.TriggerPauseState(false); 
                _onAdClosed?.Invoke();
                _onAdClosed = null;
                // Ad loading also involves Unity objects/logging
                LoadInterstitialAd(); 
            });
        };

        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            // 🚨 CRITICAL FIX: Wrap all Unity API calls in UnityMainThread.Run()
            UnityMainThread.Run(() =>
            {
                Debug.LogError($"Interstitial failed: {error.GetMessage()}");
                // PanelManager interaction must be on main thread
                PanelManager.TriggerPauseState(false); 
                _onAdClosed?.Invoke();
                _onAdClosed = null;
                // Ad loading also involves Unity objects/logging
                LoadInterstitialAd(); 
            });
        };

        ad.OnAdFullScreenContentOpened += () =>
        {
            // 🚨 CRITICAL FIX: Wrap all Unity API calls in UnityMainThread.Run()
            UnityMainThread.Run(() =>
            {
                Debug.Log("Interstitial opened. Pausing game.");
                // PanelManager interaction must be on main thread
                PanelManager.TriggerPauseState(true);
            });
        };
    }

    // ... (OnDestroy and IsInternetAvailable methods remain the same)
    private void OnDestroy()
    {
        if (_interstitialAd != null)
        {
            _interstitialAd.Destroy();
            _interstitialAd = null;
        }
        if (instance == this) instance = null;
    }

    public bool IsInternetAvailable()
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }
}