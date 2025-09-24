using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds;
using GoogleMobileAds.Api;
using UnityEngine.SceneManagement;

public class Interstitial : MonoBehaviour
{
    // The static reference to our single instance, allowing other scripts to access it.
    public static Interstitial instance; 

    /// <summary>
    /// Ad unit IDs for testing and production.
    /// Use test IDs for development and original credentials for release.
    /// </summary>
#if UNITY_ANDROID
    // Your Android test ad unit ID. Use this for testing.
    private string _adUnitId = "ca-app-pub-3940256099942544/1033173712";
#elif UNITY_IPHONE
    // Your iOS test ad unit ID. Use this for testing.
    private string _adUnitId = "ca-app-pub-3940256099942544/4411468910";
#else
    // Fallback for other platforms (e.g., Unity Editor)
    private string _adUnitId = "unused";
#endif


    // // / <summary>

    // // / orignal credentails

    // // / </summary>

    // #if UNITY_ANDROID

    // // Your Android ad unit ID (test ID by default)

    // private string _adUnitId = "ca-app-pub-8653293678103388/3646744070";

    // #elif UNITY_IPHONE

    // // Your iOS ad unit ID (test ID by default)

    // private string _adUnitId = "ca-app-pub-8653293678103388/9567963590";

    // #else

    // Fallback for other platforms (e.g., Unity Editor)

    // private string _adUnitId = "unused";

    // #endif



    // The interstitial ad object
    private InterstitialAd _interstitialAd; 

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// This is where the Singleton pattern is implemented to ensure only one instance of this script exists.
    /// </summary>
    void Awake()
    {
        // Check if an instance already exists.
        if (instance != null && instance != this)
        {
            // If another instance exists, destroy this new (duplicate) GameObject.
            Debug.LogWarning("Interstitial: Duplicate instance found, destroying new GameObject.");
            Destroy(this.gameObject);
        }
        else
        {
            // If no instance exists, this is the first one, so set it as the instance.
            instance = this;
            // Mark this GameObject to not be destroyed when loading new scenes.
            // This is essential for the ad manager to persist across scene changes.
            DontDestroyOnLoad(this.gameObject);
            Debug.Log("Interstitial: First instance created and marked DontDestroyOnLoad.");
        }
    }

    /// <summary>
    /// Start is called before the first frame update.
    /// Used here for AdMob initialization once the Singleton is surely set up.
    /// </summary>
    void Start()
    {
        // IMPORTANT: Set the child-directed treatment flag using RequestConfiguration.
        // This is the most modern and reliable way to ensure all ad requests are
        // compliant with Google Play's Families Policy. This method will work
        // with your updated Google Mobile Ads Unity Plugin.
        RequestConfiguration requestConfiguration = new RequestConfiguration
        {
            TagForChildDirectedTreatment = TagForChildDirectedTreatment.True
        };
        MobileAds.SetRequestConfiguration(requestConfiguration);
        
        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize((InitializationStatus initStatus) =>
        {
            // This callback is called once the MobileAds SDK is initialized.
            // You can log the adapter statuses for debugging.
            Debug.Log("AdMob initialization complete.");

            // Load the interstitial ad after SDK is initialized.
            LoadInterstitialAd();
        });
    }

    /// <summary>
    /// Loads the interstitial ad.
    /// </summary>
    public void LoadInterstitialAd()
    {
        // Clean up the old ad before loading a new one to prevent memory leaks.
        if (_interstitialAd != null)
        {
            Debug.Log("Destroying old interstitial ad instance.");
            _interstitialAd.Destroy();
            _interstitialAd = null;
        }

        Debug.Log("Loading a new interstitial ad.");

        // Create a simple ad request now that the global configuration is set.
        var adRequest = new AdRequest();

        // Send the request to load the ad.
        InterstitialAd.Load(_adUnitId, adRequest,
            (InterstitialAd ad, LoadAdError error) =>
            {
                // If error is not null or ad is null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError($"Interstitial ad failed to load an ad with error: {error?.GetMessage() ?? "Unknown error"}");
                    return;
                }

                // Ad loaded successfully.
                Debug.Log("Interstitial ad loaded successfully with response info: " + ad.GetResponseInfo());
                _interstitialAd = ad;
                RegisterEventHandlers(ad); // Register event handlers for this new ad
            });
    }

    /// <summary>
    /// Shows the interstitial ad if it's ready.
    /// </summary>
    public void ShowInterstitialAd()
    {
        if (_interstitialAd != null && _interstitialAd.CanShowAd())
        {
            Debug.Log("Attempting to show interstitial ad.");
            _interstitialAd.Show();
        }
        else
        {
            Debug.LogError("Interstitial ad is not ready yet. Please ensure it was loaded successfully.");
        }
    }

    /// <summary>
    /// Registers all the event handlers for the interstitial ad.
    /// </summary>
    /// <param name="interstitialAd">The interstitial ad instance.</param>
    public void RegisterEventHandlers(InterstitialAd interstitialAd)
    {
        // Unregister any existing handlers to prevent duplicate calls if an ad is reloaded
        interstitialAd.OnAdPaid -= HandleAdPaid;
        interstitialAd.OnAdImpressionRecorded -= HandleAdImpressionRecorded;
        interstitialAd.OnAdClicked -= HandleAdClicked;
        interstitialAd.OnAdFullScreenContentOpened -= HandleAdFullScreenContentOpened;
        interstitialAd.OnAdFullScreenContentClosed -= HandleAdFullScreenContentClosed;
        interstitialAd.OnAdFullScreenContentFailed -= HandleAdFullScreenContentFailed;

        // Register new handlers
        interstitialAd.OnAdPaid += HandleAdPaid;
        interstitialAd.OnAdImpressionRecorded += HandleAdImpressionRecorded;
        interstitialAd.OnAdClicked += HandleAdClicked;
        interstitialAd.OnAdFullScreenContentOpened += HandleAdFullScreenContentOpened;
        interstitialAd.OnAdFullScreenContentClosed += HandleAdFullScreenContentClosed;
        interstitialAd.OnAdFullScreenContentFailed += HandleAdFullScreenContentFailed;
    }

    // --- Private Event Handler Methods ---
    private void HandleAdPaid(AdValue adValue)
    {
        Debug.Log($"Interstitial ad paid: Value {adValue.Value} {adValue.CurrencyCode}");
    }

    private void HandleAdImpressionRecorded()
    {
        Debug.Log("Interstitial ad recorded an impression.");
    }

    private void HandleAdClicked()
    {
        Debug.Log("Interstitial ad was clicked.");
    }

    private void HandleAdFullScreenContentOpened()
    {
        Debug.Log("Interstitial ad full screen content opened.");
        // ✅ Pause game when ad opens
        PanelManager.TriggerPauseState(true);  // Pause
    }

    private void HandleAdFullScreenContentClosed()
    {
        Debug.Log("Interstitial ad full screen content closed.");
        // ✅ Resume game when ad closes
        PanelManager.TriggerPauseState(false); // Resume
        LoadInterstitialAd();
    }

    private void HandleAdFullScreenContentFailed(AdError error)
    {
        Debug.LogError($"Interstitial ad failed to open full screen content with error: {error?.GetMessage() ?? "Unknown error"}");
        // ✅ Resume game if ad fails
        PanelManager.TriggerPauseState(false); // Resume
        LoadInterstitialAd();
    }




    /// <summary>
    /// Called when the MonoBehaviour is destroyed.
    /// This is where you should clean up any ad instances to prevent memory leaks.
    /// </summary>
    void OnDestroy()
    {
        if (_interstitialAd != null)
        {
            Debug.Log("Interstitial: Destroying ad instance on GameObject destruction.");
            _interstitialAd.Destroy();
        }
        // If this is the instance that was actually managing things, clear the static reference.
        if (instance == this)
        {
            instance = null;
        }
    }
    
    /// <summary>
    /// A simple check for internet connectivity.
    /// </summary>
    public bool IsInternetAvailable()
    {
        bool isConnected = Application.internetReachability != NetworkReachability.NotReachable;
        if (isConnected)
        {
            Debug.Log("Internet is available!");
            return true;
        }
        else
        {
            Debug.LogWarning("Internet is NOT available!");
            return false;
        }
    }
    
}

