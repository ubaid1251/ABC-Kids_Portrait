using GoogleMobileAds.Api;
using System;
using System.Collections.Generic;
using GoogleMobileAds.Common;
using GoogleMobileAds.Ump.Api;
using UnityEngine;

[System.Serializable]
public class BannerAdData
{
    [Header("Banner Ad ID")] public string bannerAdUnitIds, secondBannerAdUnitIds;
    [Header("Banner Ad Position")] public AdPosition BadsPosition, secondBadsPosition;

    [Header("Banner Ad Type")]
    public BannerType bannerType = BannerType.Simple_Banner, secondBannerType = BannerType.Simple_Banner;

    [HideInInspector] public BannerView bannerView, secondBannerView;
}

[System.Serializable]
public class InterstialAdData
{
    [Header("Interstitial Ad ID")] public string admobInterstialID;
    [Header("Interstitial Ad ID")] public string admobInterstialStaticID;
}

public class IntitializeAdmob : MonoBehaviour
{
    public bool showLog = true;

    public static IntitializeAdmob instance;

    // public static bool showMeAd;
    [Header("For Test Ads")] public bool AdmobTestIds = false;
    [Header("App ID")] public string admobAppID;
    public BannerAdData BannerData;
    public InterstialAdData InterData;
    [Header("Delay timer for Inter")] public float Delay_Time;
    public float get_touch = 0;
    public bool IsPlay_Count = true;
    public bool play_ad = false;
    public int BannerFailed = 0, secondBannerFailed = 0, InterFailed = 0, staticInterFailed = 0;
    public bool twoBanners = false;
    [HideInInspector] public InterstitialAd interstitialAd, interstitialStaticAd;


    void Update()
    {
        if (IsPlay_Count /*&& PlayerPrefs.GetInt("RemoveAds") == 0*/)
        {
            get_touch += Time.deltaTime;
            if (get_touch >= Delay_Time)
            {
                play_ad = true;
                IsPlay_Count = false;
            }
        }
    }


    private void Start()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
        IsPlay_Count = true;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Debug.unityLogger.logEnabled = showLog;
        // try
        // {
        // {
        //     CallAds();
        // }
        // catch (Exception ex)
        // {
        //     Debug.Log(ex.Message);
        //     Debug.Log("Ads package not Initialise");
        // }
    }

    public void RequestConsent()
    {
        ConsentRequestParameters request = new ConsentRequestParameters
        {
            TagForUnderAgeOfConsent = true
        };

        ConsentInformation.Update(request, OnConsentInfoUpdated);
    }

    private void OnConsentInfoUpdated(FormError consentError)
    {
        if (consentError != null)
        {
            Debug.Log("Consent update failed: " + consentError.Message);
            CallAds();
            return;
        }

        if (ConsentInformation.IsConsentFormAvailable())
        {
            ConsentForm.LoadAndShowConsentFormIfRequired((FormError formError) =>
            {
                if (formError != null)
                {
                    Debug.Log("Consent form error: " + formError.Message);
                }

                CallAds();
            });
        }
        else
        {
            CallAds();
        }
    }

    #region AdsInitialization

    private bool adsInitialized = false;
    [Obsolete]
    private void CallAds()
    {
        print("calling ads");

        if (adsInitialized) return;
        adsInitialized = true;

        twoBanners = InitializeFirebase_CB._Instance.isBanner;

        if (twoBanners)
        {
            BannerData.BadsPosition = AdPosition.TopLeft;
            BannerData.bannerType = BannerType.Simple_Banner;
        }

        List<string> deviceIds = new List<string>()
        {
            AdRequest.TestDeviceSimulator,
            "060A1C49D0434CFC892DA0DE609FC7B6"
        };

        MobileAds.SetRequestConfiguration(new RequestConfiguration
        {
            TestDeviceIds = deviceIds,
            MaxAdContentRating = MaxAdContentRating.G,
            TagForChildDirectedTreatment = TagForChildDirectedTreatment.True
        });

        MobileAds.Initialize(initStatus =>
        {
            if (PlayerPrefs.GetInt("RemoveAds") == 0)
            {
                if (InitializeFirebase_CB._Instance.removeInter)
                {
                    RequestAdmobInterstitial();
                    RequestStaticAdmobInterstitial();
                }

                RequestBannerHigh();

                if (twoBanners)
                {
                    RequestSecondBannerHigh();
                }
            }
        });
    }

    #endregion

    #region StaticInterstitalAd

    [Obsolete]
    public void RequestStaticAdmobInterstitial()
    {
        try
        {
            string interId = null;
            if (AdmobTestIds)
            {
                print("In test ID");
                interId = "ca-app-pub-3940256099942544/1033173712";
                InterData.admobInterstialStaticID = interId;
            }

            // Clean up the old ad before loading a new one.
            if (interstitialStaticAd != null)
            {
                interstitialStaticAd.Destroy();
                interstitialStaticAd = null;
            }

            Debug.Log("Loading the static interstitial ad.");
            // create our request used to load the ad.
            var adRequest = new AdRequest();
            // send the request to load the ad.
            InterstitialAd.Load(InterData.admobInterstialStaticID, adRequest,
                (InterstitialAd ad, LoadAdError error) =>
                {
                    // if error is not null, the load request failed.
                    if (error != null || ad == null)
                    {
                        Debug.Log("static interstitial ad failed to load an ad " + "with error : " + error);
                        //InitializeFirebase_CB._Instance.LogEvent("InterstitialAd_error");
                        return;
                    }
                    else
                    {
                        Debug.Log("static interstitial ad loaded");
                    }

                    adRequest.Extras.Add("npa", "1");
                    //Debug.Log("Interstitial ad loaded with response : " + ad.GetResponseInfo());
                    interstitialStaticAd = ad;
                    RegisterStaticEventHandlers(interstitialStaticAd);
                });
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    private void RegisterStaticEventHandlers(InterstitialAd interstitialAd)
    {
        // Raised when a click is recorded for an ad.
        interstitialAd.OnAdClicked += () =>
        {
            MobileAdsEventExecutor.ExecuteInUpdate(() => { Debug.Log("static Interstitial ad was clicked."); });
        };
        // Raised when an ad opened full screen content.
        interstitialAd.OnAdFullScreenContentOpened += () =>
        {
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                staticInterFailed = 0;
                Debug.Log("static Interstitial ad full screen content opened.");
            });
        };
        // Raised when the ad closed full screen content.
        interstitialAd.OnAdFullScreenContentClosed += () =>
        {
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                Debug.Log("static Interstitial ad full screen content closed.");
                //interstitialAd.Destroy();
                if (Application.internetReachability != NetworkReachability.NotReachable)
                {
                    Invoke(nameof(RequestStaticAdmobInterstitial), 3);
                    //CheckInternetConnection(AdmobAdInterstitialRequest);
                }
            });
        };
        // Raised when the ad failed to open full screen content.
        interstitialAd.OnAdFullScreenContentFailed += (AdError error) =>
        {
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                Debug.Log("static Interstitial ad failed to open full screen content " + "with error : " + error);
                staticInterFailed++;
                if (staticInterFailed < 5)
                {
                    Invoke(nameof(RequestStaticAdmobInterstitial), 3);
                }
            });
        };
    }

    #endregion

    #region InterstitalAd

    [Obsolete]
    public void RequestAdmobInterstitial()
    {
        try
        {
            string interId = null;
            if (AdmobTestIds)
            {
                print("In test ID");
                interId = "ca-app-pub-3940256099942544/1033173712";
                InterData.admobInterstialID = interId;
            }

            // Clean up the old ad before loading a new one.
            if (interstitialAd != null)
            {
                interstitialAd.Destroy();
                interstitialAd = null;
            }

            Debug.Log("Loading the interstitial ad.");
            // create our request used to load the ad.
            var adRequest = new AdRequest();
            // send the request to load the ad.
            InterstitialAd.Load(InterData.admobInterstialID, adRequest,
                (InterstitialAd ad, LoadAdError error) =>
                {
                    // if error is not null, the load request failed.
                    if (error != null || ad == null)
                    {
                        Debug.Log("interstitial ad failed to load an ad " + "with error : " + error);
                        //InitializeFirebase_CB._Instance.LogEvent("InterstitialAd_error");
                        return;
                    }
                    else
                    {
                        Debug.Log("interstitial ad loaded");
                    }

                    adRequest.Extras.Add("npa", "1");
                    //Debug.Log("Interstitial ad loaded with response : " + ad.GetResponseInfo());
                    interstitialAd = ad;
                    RegisterEventHandlers(interstitialAd);
                });
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    public void ShowInterstitial()
    {
        if (InitializeFirebase_CB._Instance.removeInter) return;
        if (interstitialAd != null)
        {
            if (interstitialAd.CanShowAd())
            {
                Debug.Log("<color=green>Showing ad 1</color>");
                resetLoaderTimer();
                HideBanner();
                // Debug.Log("Showing interstitial ad.");
                interstitialAd.Show();
            }
            else if (interstitialStaticAd != null)
            {
                if (interstitialStaticAd.CanShowAd())
                {
                    Debug.Log("<color=yellow>Showing ad 2</color>");
                    resetLoaderTimer();
                    HideBanner();
                    // Debug.Log("Showing interstitial ad.");
                    interstitialStaticAd.Show();
                }

                RequestAdmobInterstitial();
            }
        }
        else if (interstitialStaticAd != null)
        {
            if (interstitialStaticAd.CanShowAd())
            {
                Debug.Log("<color=red>Showing ad 3</color>");
                resetLoaderTimer();
                HideBanner();
                // Debug.Log("Showing interstitial ad.");
                interstitialStaticAd.Show();
            }

            RequestAdmobInterstitial();
        }
        else
        {
            RequestAdmobInterstitial();
            RequestStaticAdmobInterstitial();
            resetLoaderTimer();
        }
    }

    public void ShowStaticInterstitial()
    {
        if (interstitialStaticAd != null)
        {
            if (interstitialStaticAd.CanShowAd())
            {
                Debug.Log("<color=red>Showing static Inter</color>");
                resetLoaderTimer();
                HideBanner();
                interstitialStaticAd.Show();
            }
        }
        else
        {
            RequestStaticAdmobInterstitial();
            resetLoaderTimer();
        }
    }

    public void resetLoaderTimer()
    {
        play_ad = false;
        //showMeAd = false;
        get_touch = 0;
        IsPlay_Count = true;
    }

    private void RegisterEventHandlers(InterstitialAd interstitialAd)
    {
        // Raised when a click is recorded for an ad.
        interstitialAd.OnAdClicked += () =>
        {
            MobileAdsEventExecutor.ExecuteInUpdate(() => { Debug.Log("Interstitial ad was clicked."); });
        };
        // Raised when an ad opened full screen content.
        interstitialAd.OnAdFullScreenContentOpened += () =>
        {
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                InterFailed = 0;
                Debug.Log("Interstitial ad full screen content opened.");
            });
        };
        // Raised when the ad closed full screen content.
        interstitialAd.OnAdFullScreenContentClosed += () =>
        {
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                Debug.Log("Interstitial ad full screen content closed.");
                if (Application.internetReachability != NetworkReachability.NotReachable)
                {
                    Invoke(nameof(RequestAdmobInterstitial), 3);
                }
            });
        };
        // Raised when the ad failed to open full screen content.
        interstitialAd.OnAdFullScreenContentFailed += (AdError error) =>
        {
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                Debug.Log("Interstitial ad failed to open full screen content " + "with error : " + error);
                InterFailed++;
                if (InterFailed < 5)
                {
                    Invoke(nameof(RequestAdmobInterstitial), 3);
                }
            });
        };
    }

    public bool IsInterAvailable()
    {
        if (InitializeFirebase_CB._Instance.removeInter)
        {
            return false;
        }

        if (PlayerPrefs.GetInt("RemoveAds") == 1)
        {
            return false;
        }

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            return false;
        }

        if (interstitialAd == null)
        {
            InterFailed++;
            if (InterFailed < 5)
            {
                Invoke(nameof(RequestAdmobInterstitial), 3);
                print("interstital ad null " + InterFailed);
            }

            return false;
        }

        if (interstitialAd != null)
        {
            if (!interstitialAd.CanShowAd())
            {
                InterFailed++;
                if (InterFailed < 5)
                {
                    Invoke(nameof(RequestAdmobInterstitial), 3);
                    print("interstital ad available but not shown " + InterFailed);
                }

                return false;
            }
        }

        return play_ad;
    }

    public bool IsStaticInterAvailable()
    {
        if (InitializeFirebase_CB._Instance.removeInter)
        {
            return false;
        }

        if (PlayerPrefs.GetInt("RemoveAds") == 1)
        {
            return false;
        }

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            return false;
        }

        if (interstitialStaticAd == null)
        {
            staticInterFailed++;
            if (staticInterFailed < 5)
            {
                Invoke(nameof(RequestStaticAdmobInterstitial), 3);
                print("static interstital ad null " + staticInterFailed);
            }

            return false;
        }

        if (interstitialStaticAd != null)
        {
            if (!interstitialStaticAd.CanShowAd())
            {
                staticInterFailed++;
                if (staticInterFailed < 5)
                {
                    Invoke(nameof(RequestStaticAdmobInterstitial), 3);
                    print("static interstital ad available but not shown " + staticInterFailed);
                }

                return false;
            }
        }

        return play_ad;
    }

    public void ShowInterstitialAd()
    {
        ShowInterstitial();
    }

    #endregion

    #region BannerAd

    public void RequestBannerHigh()
    {
        if (BannerData.bannerView != null)
        {
            DestroyBannerView();
        }

        string bannerId = null;
        if (AdmobTestIds)
        {
            bannerId = "ca-app-pub-3940256099942544/6300978111";
            BannerData.bannerAdUnitIds = bannerId;
        }
        else
        {
            bannerId = BannerData.bannerAdUnitIds;
        }

        if (BannerData.bannerType == BannerType.Simple_Banner)
        {
            BannerData.bannerView = new BannerView(BannerData.bannerAdUnitIds, AdSize.Banner, BannerData.BadsPosition);
        }
        else if (BannerData.bannerType == BannerType.Adaptive_Banner)
        {
            AdSize adaptiveSize = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
            BannerData.bannerView = new BannerView(BannerData.bannerAdUnitIds, adaptiveSize, BannerData.BadsPosition);
        }
        else
        {
            BannerData.bannerView =
                new BannerView(BannerData.bannerAdUnitIds, AdSize.SmartBanner, BannerData.BadsPosition);
        }

        // Create an ad request with the highest priority
        AdRequest bannerAdRequest = new AdRequest();

        // Load the banner ad with the ad request
        BannerData.bannerView.LoadAd(bannerAdRequest);
        // Debug.Log("Hiding banner view.");
        BannerData.bannerView.Hide();
        ListenToAdEvents();
    }

    private void ListenToAdEvents()
    {
        // Raised when an ad is loaded into the banner view.
        BannerData.bannerView.OnBannerAdLoaded += () =>
        {
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                BannerFailed = 0;
                Debug.Log("Banner view loaded an ad with response : " + BannerData.bannerView.GetResponseInfo());
            });
        };
        // Raised when an ad fails to load into the banner view.
        BannerData.bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
        {
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                BannerFailed++;
                if (BannerFailed < 5)
                {
                    RequestBannerHigh();
                }

                Debug.Log("Banner view failed to load an ad with error : " + error);
            });
        };
        BannerData.bannerView.OnAdImpressionRecorded += () =>
        {
            MobileAdsEventExecutor.ExecuteInUpdate(() => { Debug.Log("Banner view recorded an impression."); });
        };
        // Raised when a click is recorded for an ad.
        BannerData.bannerView.OnAdClicked += () =>
        {
            MobileAdsEventExecutor.ExecuteInUpdate(() => { Debug.Log("Banner view was clicked."); });
        };
        // Raised when an ad opened full screen content.
        BannerData.bannerView.OnAdFullScreenContentOpened += () =>
        {
            MobileAdsEventExecutor.ExecuteInUpdate(() => { Debug.Log("Banner view full screen content opened."); });
        };
        // Raised when the ad closed full screen content.
        BannerData.bannerView.OnAdFullScreenContentClosed += () =>
        {
            MobileAdsEventExecutor.ExecuteInUpdate(() => { Debug.Log("Banner view full screen content closed."); });
        };
    }

    public void ShowBanner()
    {
        if (PlayerPrefs.GetInt("RemoveAds") == 0)
        {
            if (BannerData.bannerView != null) // && !BannerExsist())
            {
                Debug.Log("Showing banner view.");
                BannerData.bannerView.Show();
            }
            else
            {
                RequestBannerHigh();
            }

            if (twoBanners)
            {
                if (BannerData.secondBannerView != null) // && !BannerExsist())
                {
                    Debug.Log("Showing second banner view.");
                    BannerData.secondBannerView.Show();
                }
                else
                {
                    RequestSecondBannerHigh();
                }
            }
        }
    }

    public void HideBanner()
    {
        if (PlayerPrefs.GetInt("RemoveAds") == 0)
        {
            if (BannerData.bannerView != null) // && !BannerExsist())
            {
                Debug.Log("Hiding banner view.");
                BannerData.bannerView.Hide();
            }

            if (twoBanners)
            {
                if (BannerData.secondBannerView != null) // && !BannerExsist())
                {
                    Debug.Log("Hiding banner view.");
                    BannerData.secondBannerView.Hide();
                }
            }
        }
    }

    public void DestroyBannerView()
    {
        if (BannerData.bannerView != null)
        {
            Debug.Log("Destroying banner view.");
            BannerData.bannerView.Destroy();
            BannerData.bannerView = null;
        }

        if (twoBanners)
        {
            DestroySecondBannerView();
        }
    }

    #endregion

    #region SecondBannerAd

    public void RequestSecondBannerHigh()
    {
        if (BannerData.secondBannerView != null)
        {
            DestroySecondBannerView();
        }

        string bannerId = null;
        if (AdmobTestIds)
        {
            bannerId = "ca-app-pub-3940256099942544/6300978111";
            BannerData.secondBannerAdUnitIds = bannerId;
        }
        else
        {
            bannerId = BannerData.secondBannerAdUnitIds;
        }

        if (BannerData.secondBannerType == BannerType.Simple_Banner)
        {
            BannerData.secondBannerView = new BannerView(BannerData.secondBannerAdUnitIds, AdSize.Banner,
                BannerData.secondBadsPosition);
        }
        else if (BannerData.secondBannerType == BannerType.Adaptive_Banner)
        {
            AdSize adaptiveSize = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
            BannerData.secondBannerView = new BannerView(BannerData.secondBannerAdUnitIds, adaptiveSize,
                BannerData.secondBadsPosition);
        }
        else
        {
            BannerData.secondBannerView = new BannerView(BannerData.secondBannerAdUnitIds, AdSize.SmartBanner,
                BannerData.secondBadsPosition);
        }

        // Create an ad request with the highest priority
        AdRequest bannerAdRequest = new AdRequest();

        // Load the banner ad with the ad request
        BannerData.secondBannerView.LoadAd(bannerAdRequest);
        // Debug.Log("Hiding banner view.");
        BannerData.secondBannerView.Hide();
        ListenToSecondAdEvents();
    }

    private void ListenToSecondAdEvents()
    {
        // Raised when an ad is loaded into the banner view.
        BannerData.secondBannerView.OnBannerAdLoaded += () =>
        {
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                secondBannerFailed = 0;
                Debug.Log("Banner view loaded an ad with response : " + BannerData.secondBannerView.GetResponseInfo());
            });
        };
        // Raised when an ad fails to load into the banner view.
        BannerData.secondBannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
        {
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                secondBannerFailed++;
                if (secondBannerFailed < 5)
                {
                    RequestSecondBannerHigh();
                }

                Debug.Log("static Banner view failed to load an ad with error : " + error);
            });
        };
        BannerData.secondBannerView.OnAdImpressionRecorded += () =>
        {
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                Debug.Log("static Banner view recorded an impression.");
            });
        };
        // Raised when a click is recorded for an ad.
        BannerData.secondBannerView.OnAdClicked += () =>
        {
            MobileAdsEventExecutor.ExecuteInUpdate(() => { Debug.Log("static Banner view was clicked."); });
        };
        // Raised when an ad opened full screen content.
        BannerData.secondBannerView.OnAdFullScreenContentOpened += () =>
        {
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                Debug.Log("static Banner view full screen content opened.");
            });
        };
        // Raised when the ad closed full screen content.
        BannerData.secondBannerView.OnAdFullScreenContentClosed += () =>
        {
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                Debug.Log("static Banner view full screen content closed.");
            });
        };
    }

    public void DestroySecondBannerView()
    {
        if (BannerData.secondBannerView != null)
        {
            Debug.Log("Destroying banner view.");
            BannerData.secondBannerView.Destroy();
            BannerData.secondBannerView = null;
        }
    }

    #endregion
}

public enum BannerType
{
    Simple_Banner = 1,
    Smart_Banner = 2,
    Adaptive_Banner = 3
}