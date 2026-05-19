using UnityEngine;
using TMPro;
using GoogleMobileAds.Api;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;
using GoogleMobileAds.Ump.Api;
using System.Collections.Generic;
using GoogleMobileAds.Common;
using System.Collections;

namespace MobileMonetizationPro
{
    public class MobileMonetizationPro_AdmobAdsInitializer : MonoBehaviour
    {
        public static MobileMonetizationPro_AdmobAdsInitializer instance;

        [Tooltip("Enable or disable GDPR consent support.")]
        public bool UseGDPRConsent = true;

        public bool EnableBannerAds = true;
        public bool EnableInterstitialAds = true;
        public bool EnableRewardedAds = true;
        public bool EnableRewardedInterstitialAds = true;
        public bool EnableAppOpenAds = true; 

        [Header("Ad unit Id's For Android")]
        [Tooltip("Android AdMob Banner Ad Unit ID.")]
        public string AndroidBannerId = "ca-app-pub-3940256099942544/6300978111";

        [Tooltip("Android AdMob Interstitial Ad Unit ID.")]
        public string AndroidInterstitalId = "ca-app-pub-3940256099942544/1033173712";

        [Tooltip("Android AdMob Rewarded Video Ad Unit ID.")]
        public string AndroidRewardedId = "ca-app-pub-3940256099942544/5224354917";

        //[Tooltip("Android AdMob Native Ad Unit ID.")]
        //public string AndroidNativeId = "ca-app-pub-3940256099942544/2247696110";

        [Tooltip("Android AdMob App Open Ad Unit ID.")]
        public string AndroidAppOpenId = "ca-app-pub-3940256099942544/9257395921";

        [Tooltip("Android AdMob Rewarded Interstitial Ad Unit ID.")]
        public string AndroidRewardedInterstitialID = "ca-app-pub-3940256099942544/5354046379";

        //public string AndroidNativeOverlayID = "ca-app-pub-3940256099942544/5354046379";

        [Header("Ad unit Id's For iOS")]
        public string IOSBannerId = "ca-app-pub-3940256099942544/2934735716";

        [Tooltip("iOS AdMob Interstitial Ad Unit ID.")]
        public string IOSInterstitalId = "ca-app-pub-3940256099942544/44114689102";

        [Tooltip("iOS AdMob Rewarded Video Ad Unit ID.")]
        public string IOSRewardedId = "ca-app-pub-3940256099942544/1712485313";

        //[Tooltip("iOS AdMob Native Ad Unit ID.")]
        //public string IOSNativeId = "ca-app-pub-3940256099942544/3986624511";

        [Tooltip("iOS AdMob App Open Ad Unit ID.")]
        public string IOSAppOpenId = "ca-app-pub-3940256099942544/9257395921";

        [Tooltip("iOS AdMob Rewarded Interstitial Ad Unit ID.")]
        public string IOSRewardedInterstitialID = "ca-app-pub-3940256099942544/6978759866";

        //public string IOSNativeOverlayID = "ca-app-pub-3940256099942544/5354046379";

        private NativeOverlayAd _nativeOverlayAd;

        BannerView bannerView;
        InterstitialAd interstitialAd;
        RewardedAd rewardedAd;
      
        string bannerId;
        string interId;
        string rewardedId;
        string nativeId;
        string appopenId;
        string rewardedinterstitalId;
        string nativeoverlayID;
        //string rewardedinterstitalId;

        [Header("Ads Settings")]
        [Tooltip("Automatically show banner ads when the game starts.")]
        public bool ShowBannerAdsInStart = true;

        [Tooltip("Choose where the banner ad should be displayed.")]
        public AdPosition ChooseBannerPosition = AdPosition.Bottom;

        [System.Serializable]
        public enum AdaptiveBannerAdSizeOptionsEnum
        {
            Landscape,
            Portrait,
            LandscapeAndPortrait
        }


        [Tooltip("Enable support for adaptive banners based on screen orientation.")]
        public bool UseAdaptiveBannerSize = true;

        [Tooltip("Enable to manually set the width for adaptive banners.")]
        public bool UseCustomAdaptiveBannerWidth = false;

        [Tooltip("Specify a custom width for adaptive banners (only used if UseCustomAdaptiveBannerWidth is true).")]
        public int CustomAdaptiveBannerWidth;

        [Tooltip("Select which screen orientations to support for adaptive banners.")]
        public AdaptiveBannerAdSizeOptionsEnum AdaptiveBannerAdSizeOptions = AdaptiveBannerAdSizeOptionsEnum.LandscapeAndPortrait;

        [Tooltip("Set the standard banner ad size if adaptive is not used.")]
        public AdSize BannerAdSize = AdSize.Banner;

        [Tooltip("Enable to automatically show interstitial ads after a time interval.")]
        public bool EnableTimedInterstitalAds = true;

        [Tooltip("Time interval (in seconds) to wait before showing an interstitial ad again.")]
        public int InterstitialAdIntervalSeconds = 10;

        [Tooltip("Reset interstitial ad timer after showing a interstitial ad.")]
        public bool ResetInterstitalAdTimerOnRewardedAd = true;

        [Header("AppOpen Ads Settings")]
        [Tooltip("Number of app opens to check before showing an AppOpen Ad.")]
        public int AppOpensToCheckBeforeShowingAppOpenAd = 3;

        [Tooltip("Delay in seconds before showing an AppOpen Ad after the game starts.")]
        public float DelayShowAppOpenAd = 2f;

        [HideInInspector]
        public bool IsInterstitialAdTimerCompleted = false;

        [HideInInspector]
        public float Timer = 0;

        [HideInInspector]
        public bool IsAdSkipped = false;
        [HideInInspector]
        public bool IsAdCompleted = false;
        [HideInInspector]
        public bool IsAdUnknown = false;


        [HideInInspector]
        public bool IsRewardedAdCompleted = false;

        MobileMonetizationPro_AdmobAdsManager AdsManagerAdmobAdsScript;

        [HideInInspector]
        public Image ImageToUseToDisplayNativeAd;

        [HideInInspector]
        public bool IsBannerStartShowing = false;

        [HideInInspector]
        public bool IsInitializationCompleted = false;

        AppOpenAd AppOpenAdV;

        private int openCount = 0;

        private RewardedInterstitialAd _rewardedInterstitialAd;
        AdSize adaptiveSize;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                PlayerPrefs.SetInt("Admob_IsAppOpened", 0);
            }
            else
            {
                // If an instance already exists, destroy this duplicate
                Destroy(gameObject);
            }


#if UNITY_ANDROID
        bannerId = AndroidBannerId;
        interId = AndroidInterstitalId;
        rewardedId = AndroidRewardedId;
        //nativeId = AndroidNativeId;
        appopenId = AndroidAppOpenId;
        rewardedinterstitalId = AndroidRewardedInterstitialID;
        //nativeoverlayID = AndroidNativeOverlayID;
            //rewardedinterstitalId = AndroidRewardedInterstitalId;
#elif UNITY_IPHONE
            bannerId = IOSBannerId;
            interId = IOSInterstitalId;
            rewardedId = IOSRewardedId;
            //nativeId = IOSNativeId;
            appopenId = IOSAppOpenId;
            rewardedinterstitalId = IOSRewardedInterstitialID;
            //nativeoverlayID = IOSNativeOverlayID;
#endif
        }
        void Start()
        {
            if (UseGDPRConsent == false)
            {
                InitializeAndLoadAds();
                SceneManager.sceneLoaded += OnSceneLoaded;
            }

            if (IsInitializationCompleted == true)
            {
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
        }
        public void InitializeAndLoadAds()
        {
            MobileAds.RaiseAdEventsOnUnityMainThread = true;
            MobileAds.Initialize(initStatus =>
            {
                IsInitializationCompleted = true;
                if (ShowBannerAdsInStart == true && EnableBannerAds == true)
                {
                    LoadBanner();
                }

               

                if(EnableInterstitialAds == true)
                {
                    LoadInterstitial();
                }

                if(EnableRewardedAds == true)
                {
                    LoadRewarded();
                }
                
                //RequestNativeAd();

                if(EnableRewardedInterstitialAds == true)
                {
                    LoadRewardedInterstitialAd();
                }

                if (EnableAppOpenAds == true)
                {
                    LoadAppOpenAd();
                    if (PlayerPrefs.GetInt("Admob_IsAppOpened") == 0)
                    {
                        openCount = PlayerPrefs.GetInt("AdmobAd_AppOpenCount", 0);
                        openCount++;
                        PlayerPrefs.SetInt("AdmobAd_AppOpenCount", openCount);
                        PlayerPrefs.Save();
                        PlayerPrefs.SetInt("Admob_IsAppOpened", 1);
                    }
                    if (openCount >= AppOpensToCheckBeforeShowingAppOpenAd)
                    {
                        StartCoroutine(ShowAppOpenAdWithDelay());
                        PlayerPrefs.SetInt("AdmobAd_AppOpenCount", 0);
                        openCount = PlayerPrefs.GetInt("AdmobAd_AppOpenCount", 0);
                    }
                }
              
            });
        }
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (IsInitializationCompleted == true)
            {
                //if (IsBannerStartShowing == true || ShowBannerAdsInStart == true)
                //{
                //    LoadBanner();
                //}
                if (EnableInterstitialAds == true)
                {
                    LoadInterstitial();
                }

                if (EnableRewardedAds == true)
                {
                    LoadRewarded();
                }
                //RequestNativeAd();
                if (EnableRewardedInterstitialAds == true)
                {
                    LoadRewardedInterstitialAd();
                }

                if (EnableAppOpenAds == true)
                {
                    if (PlayerPrefs.GetInt("Admob_IsAppOpened") == 0)
                    {
                        openCount = PlayerPrefs.GetInt("AdmobAd_AppOpenCount", 0);
                        openCount++;
                        PlayerPrefs.SetInt("AdmobAd_AppOpenCount", openCount);
                        PlayerPrefs.Save();
                        PlayerPrefs.SetInt("Admob_IsAppOpened", 1);
                    }

                    if (openCount >= AppOpensToCheckBeforeShowingAppOpenAd)
                    {
                        StartCoroutine(ShowAppOpenAdWithDelay());
                        PlayerPrefs.SetInt("AdmobAd_AppOpenCount", 0);
                        openCount = PlayerPrefs.GetInt("AdmobAd_AppOpenCount", 0);
                    }
                }
            }

        }
        public void LoadAppOpenAd()
        {
            if(PlayerPrefs.GetInt("AdsRemoved") == 0)
            {
                // Clean up the old ad before loading a new one.
                if (AppOpenAdV != null)
                {
                    AppOpenAdV.Destroy();
                    AppOpenAdV = null;
                }

                Debug.Log("Loading the app open ad.");

                // Create our request used to load the ad.
                var adRequest = new AdRequest();

                // send the request to load the ad.
                AppOpenAd.Load(appopenId, adRequest,
                    (AppOpenAd ad, LoadAdError error) =>
                    {
                    // if error is not null, the load request failed.
                    if (error != null || ad == null)
                        {
                            Debug.LogError("app open ad failed to load an ad " +
                                           "with error : " + error);
                            return;
                        }

                        Debug.Log("App open ad loaded with response : "
                                  + ad.GetResponseInfo());

                        AppOpenAdV = ad;
                        RegisterEventHandlers(ad);
                    });
            }
           
        }
        IEnumerator ShowAppOpenAdWithDelay()
        {
            yield return new WaitForSeconds(DelayShowAppOpenAd);
            if (PlayerPrefs.GetInt("AdsRemoved") == 0)
            {
                ShowAppOpenAd();
            } 
        }
        public void ShowAppOpenAd()
        {
            if (AppOpenAdV != null && AppOpenAdV.CanShowAd())
            {
                Debug.Log("Showing app open ad.");
                AppOpenAdV.Show();
            }
            else
            {
                Debug.LogError("App open ad is not ready yet.");
            }
        }
        private void RegisterEventHandlers(AppOpenAd ad)
        {
            // Raised when the ad is estimated to have earned money.
            ad.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log(String.Format("App open ad paid {0} {1}.",
                    adValue.Value,
                    adValue.CurrencyCode));
            };
            // Raised when an impression is recorded for an ad.
            ad.OnAdImpressionRecorded += () =>
            {
                Debug.Log("App open ad recorded an impression.");
            };
            // Raised when a click is recorded for an ad.
            ad.OnAdClicked += () =>
            {
                Debug.Log("App open ad was clicked.");
            };
            // Raised when an ad opened full screen content.
            ad.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("App open ad full screen content opened.");
            };
            // Raised when the ad closed full screen content.
            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("App open ad full screen content closed.");
                LoadAppOpenAd();
            };
            // Raised when the ad failed to open full screen content.
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError("App open ad failed to open full screen content " +
                               "with error : " + error);
                LoadAppOpenAd();
            };
        } 
        private void Update()
        {
            if (PlayerPrefs.GetInt("AdsRemovedSuccessfully") == 0 && EnableInterstitialAds == true)
            {
                if (Timer >= InterstitialAdIntervalSeconds)
                {
                    Timer = 0;
                    IsInterstitialAdTimerCompleted = true;
                }
                else
                {
                    if (EnableTimedInterstitalAds == true)
                    {
                        Timer += Time.deltaTime;
                        if (PlayerPrefs.GetInt("AdsRemoved") == 1)
                        {
                            if (PlayerPrefs.GetInt("AdsRemovedSuccessfully") == 0)
                            {
                                DestroyBannerAd();
                                PlayerPrefs.SetInt("AdsRemovedSuccessfully", 1);
                            }
                        }
                    }
                }
            }
        }
        public void CheckForAdmobManagerScript()
        {
            if (AdsManagerAdmobAdsScript == null)
            {
                if (FindObjectOfType<MobileMonetizationPro_AdmobAdsManager>() != null)
                {
                    AdsManagerAdmobAdsScript = FindObjectOfType<MobileMonetizationPro_AdmobAdsManager>();
                }
                if (AdsManagerAdmobAdsScript != null)
                {
                    AdsManagerAdmobAdsScript.CheckForAdCompletion();
                }
            }
            else
            {
                AdsManagerAdmobAdsScript.CheckForAdCompletion();
            }
        }

        #region Banner

        public void LoadBanner()
        {
            if (PlayerPrefs.GetInt("AdsRemoved") == 0 && IsInitializationCompleted == true)
            {
                if (bannerView == null || IsBannerStartShowing == false)
                {
                    //create a banner
                    CreateBannerView();

                    //listen to banner events
                    ListenToBannerEvents();

                    //load the banner
                    if (bannerView == null)
                    {
                        CreateBannerView();
                    }

                    var adRequest = new AdRequest();
                    adRequest.Keywords.Add("unity-admob-sample");

                    Debug.Log("Loading banner Ad !!");
                    bannerView.LoadAd(adRequest);//show the banner on the screen
                    IsBannerStartShowing = true;
                }
                else
                {
                    bannerView.Show();
                }
            }
        }
        public void ShowBanner()
        {
            if (bannerView != null)
            {
                Debug.Log("Showing banner view.");
                bannerView.Show();
            }
        }
        public void HideBanner()
        {
            if (bannerView != null)
            {
                Debug.Log("Hiding banner view.");
                bannerView.Hide();
            }
        }
        void CreateBannerView()
        {
            if (PlayerPrefs.GetInt("AdsRemoved") == 0)
            {
                if (bannerView != null)
                {
                    DestroyBannerAd();
                }
                if(UseAdaptiveBannerSize == true)
                {
                    if(AdaptiveBannerAdSizeOptions == AdaptiveBannerAdSizeOptionsEnum.LandscapeAndPortrait)
                    {
                        if(UseCustomAdaptiveBannerWidth == false)
                        {
                            adaptiveSize = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
                        }
                        else
                        {
                            adaptiveSize = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(CustomAdaptiveBannerWidth);
                        }
                        bannerView = new BannerView(bannerId, adaptiveSize, ChooseBannerPosition);
                    }
                    else if (AdaptiveBannerAdSizeOptions == AdaptiveBannerAdSizeOptionsEnum.Landscape)
                    {
                        if (UseCustomAdaptiveBannerWidth == false)
                        {
                            adaptiveSize = AdSize.GetLandscapeAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
                        }
                        else
                        {
                            adaptiveSize = AdSize.GetLandscapeAnchoredAdaptiveBannerAdSizeWithWidth(CustomAdaptiveBannerWidth);
                        }
                        bannerView = new BannerView(bannerId, adaptiveSize, ChooseBannerPosition);
                    }
                    else
                    {
                        if (UseCustomAdaptiveBannerWidth == false)
                        {
                            adaptiveSize = AdSize.GetPortraitAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
                        }
                        else
                        {
                            adaptiveSize = AdSize.GetPortraitAnchoredAdaptiveBannerAdSizeWithWidth(CustomAdaptiveBannerWidth);
                        }
                        bannerView = new BannerView(bannerId, adaptiveSize, ChooseBannerPosition);
                    }
                }
                else
                {
                    bannerView = new BannerView(bannerId, BannerAdSize, ChooseBannerPosition);
                }
            }
        }
        void ListenToBannerEvents()
        {
            if (PlayerPrefs.GetInt("AdsRemoved") == 0)
            {
                bannerView.OnBannerAdLoaded += () =>
            {
                Debug.Log("Banner view loaded an ad with response : "
                    + bannerView.GetResponseInfo());
            };
                // Raised when an ad fails to load into the banner view.
                bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
                {
                    Debug.LogError("Banner view failed to load an ad with error : "
                        + error);
                };
                // Raised when the ad is estimated to have earned money.
                bannerView.OnAdPaid += (AdValue adValue) =>
                {
                    Debug.Log("Banner view paid {0} {1}." +
                        adValue.Value +
                        adValue.CurrencyCode);
                };
                // Raised when an impression is recorded for an ad.
                bannerView.OnAdImpressionRecorded += () =>
                {
                    Debug.Log("Banner view recorded an impression.");
                };
                // Raised when a click is recorded for an ad.
                bannerView.OnAdClicked += () =>
                {
                    Debug.Log("Banner view was clicked.");
                };
                // Raised when an ad opened full screen content.
                bannerView.OnAdFullScreenContentOpened += () =>
                {
                    Debug.Log("Banner view full screen content opened.");
                };
                // Raised when the ad closed full screen content.
                bannerView.OnAdFullScreenContentClosed += () =>
                {
                    Debug.Log("Banner view full screen content closed.");
                };
            }
        }
        public void DestroyBannerAd()
        {
            if (bannerView != null)
            {
                print("Destroying banner Ad");
                bannerView.Destroy();
                bannerView = null;
            }
        }
        #endregion

        #region Interstitial

        public void LoadInterstitial()
        {
            if (PlayerPrefs.GetInt("AdsRemoved") == 0 && IsInitializationCompleted == true)
            {
                if (interstitialAd != null)
                {
                    interstitialAd.Destroy();
                    interstitialAd = null;
                }
                var adRequest = new AdRequest();
                adRequest.Keywords.Add("unity-admob-sample");

                InterstitialAd.Load(interId, adRequest, (InterstitialAd ad, LoadAdError error) =>
                {
                    if (error != null || ad == null)
                    {
                        print("Interstitial ad failed to load" + error);
                        return;
                    }

                    print("Interstitial ad loaded !!" + ad.GetResponseInfo());

                    interstitialAd = ad;
                    InterstitialEvent(interstitialAd);
                });
            }
        }
        public void ShowInterstitialAd(bool ShowInterstitialImmediately)
        {
            if (PlayerPrefs.GetInt("AdsRemoved") == 0)
            {
                if(ShowInterstitialImmediately == true)
                {
                    if (interstitialAd != null && interstitialAd.CanShowAd())
                    {
                        interstitialAd.Show();
                    }
                    else
                    {
                        print("Intersititial ad not ready!!");
                    }
                }
                else
                {
                    if (EnableTimedInterstitalAds == false)
                    {
                        if (interstitialAd != null && interstitialAd.CanShowAd())
                        {
                            interstitialAd.Show();
                        }
                        else
                        {
                            print("Intersititial ad not ready!!");
                        }
                    }
                    else
                    {

                        if (IsInterstitialAdTimerCompleted == true)
                        {
                            if (interstitialAd != null && interstitialAd.CanShowAd())
                            {
                                interstitialAd.Show();
                            }
                            else
                            {
                                print("Intersititial ad not ready!!");
                            }
                        }
                    }
                }
            
            }
        }
        public void ResetInterstitialAdTimer()
        {
            Timer = 0;
            IsInterstitialAdTimerCompleted = false;
        }
        public void InterstitialEvent(InterstitialAd ad)
        {
            // Raised when the ad is estimated to have earned money.
            ad.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log("Interstitial ad paid {0} {1}." +
                    adValue.Value +
                    adValue.CurrencyCode);
            };
            // Raised when an impression is recorded for an ad.
            ad.OnAdImpressionRecorded += () =>
            {
                Debug.Log("Interstitial ad recorded an impression.");
                IsAdSkipped = true;
                CheckForAdmobManagerScript();

            };
            // Raised when a click is recorded for an ad.
            ad.OnAdClicked += () =>
            {
                Debug.Log("Interstitial ad was clicked.");
            };
            // Raised when an ad opened full screen content.
            ad.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("Interstitial ad full screen content opened.");
            };
            // Raised when the ad closed full screen content.
            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Interstitial ad full screen content closed.");
                IsAdSkipped = true;
                CheckForAdmobManagerScript();
            };
            // Raised when the ad failed to open full screen content.
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError("Interstitial ad failed to open full screen content " +
                               "with error : " + error);
            };
        }

        #endregion

        #region Rewarded

        public void LoadRewarded()
        {
            if (IsInitializationCompleted == true)
            {
                if (rewardedAd != null)
                {
                    rewardedAd.Destroy();
                    rewardedAd = null;
                }
                var adRequest = new AdRequest();
                adRequest.Keywords.Add("unity-admob-sample");

                RewardedAd.Load(rewardedId, adRequest, (RewardedAd ad, LoadAdError error) =>
                {
                    if (error != null || ad == null)
                    {
                        print("Rewarded failed to load" + error);
                        return;
                    }

                    print("Rewarded ad loaded !!");
                    rewardedAd = ad;
                    RewardedAdEvents(rewardedAd);
                });
            }
        }
        public void ShowRewardedAd()
        {
            if (rewardedAd != null && rewardedAd.CanShowAd())
            {
                IsRewardedAdCompleted = false;
                rewardedAd.Show((Reward reward) =>
                {
                    print("Give reward to player !!");
                });
            }
            else
            {
                print("Rewarded ad not ready");
            }
        }
        public void RewardedAdEvents(RewardedAd ad)
        {
            // Raised when the ad is estimated to have earned money.
            ad.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log("Rewarded ad paid {0} {1}." +
                    adValue.Value +
                    adValue.CurrencyCode);
            };
            // Raised when an impression is recorded for an ad.
            ad.OnAdImpressionRecorded += () =>
            {
                Debug.Log("Rewarded ad recorded an impression.");
            };
            // Raised when a click is recorded for an ad.
            ad.OnAdClicked += () =>
            {
                Debug.Log("Rewarded ad was clicked.");
            };
            // Raised when an ad opened full screen content.
            ad.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("Rewarded ad full screen content opened.");
            };
            // Raised when the ad closed full screen content.
            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Rewarded ad full screen content closed.");
                IsAdCompleted = true;
                IsRewardedAdCompleted = true;
                CheckForAdmobManagerScript();
            };
            // Raised when the ad failed to open full screen content.
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError("Rewarded ad failed to open full screen content " +
                               "with error : " + error);
            };
        }

        #endregion

        //#region Native

        //public void RequestNativeAd()
        //{
        //    if (PlayerPrefs.GetInt("AdsRemoved") == 0)
        //    {
        //        AdLoader adLoader = new AdLoader.Builder(nativeId).ForNativeAd().Build();

        //        adLoader.OnNativeAdLoaded += this.HandleNativeAdLoaded;

        //        adLoader.LoadAd(new AdRequest());
        //    }
        //}
        //private void HandleNativeAdLoaded(object sender, NativeAdEventArgs e)
        //{
        //    if (PlayerPrefs.GetInt("AdsRemoved") == 0)
        //    {
        //        NativeAd n;

        //        print("Native ad loaded");
        //        n = e.nativeAd;

        //        Texture2D iconTexture = n.GetIconTexture();
        //        Sprite sprite = Sprite.Create(iconTexture, new Rect(0, 0, iconTexture.width, iconTexture.height), Vector2.one * .5f);

        //        ImageToUseToDisplayNativeAd.sprite = sprite;
        //    }

        //}
        //#endregion

        #region RewardedInterstital
        public void LoadRewardedInterstitialAd()
        {
            // Clean up the old ad before loading a new one.
            if (_rewardedInterstitialAd != null)
            {
                _rewardedInterstitialAd.Destroy();
                _rewardedInterstitialAd = null;
            }

            Debug.Log("Loading the rewarded interstitial ad.");

            // create our request used to load the ad.
            var adRequest = new AdRequest();
            adRequest.Keywords.Add("unity-admob-sample");

            // send the request to load the ad.
            RewardedInterstitialAd.Load(rewardedinterstitalId, adRequest,
                (RewardedInterstitialAd ad, LoadAdError error) =>
                {
              // if error is not null, the load request failed.
              if (error != null || ad == null)
                    {
                        Debug.LogError("rewarded interstitial ad failed to load an ad " +
                                       "with error : " + error);
                        return;
                    }

                    Debug.Log("Rewarded interstitial ad loaded with response : "
                              + ad.GetResponseInfo());

                    _rewardedInterstitialAd = ad;

                    // Register to ad events to extend functionality.
                    RegisterEventHandlers(ad);
                });
        }
        public void ShowRewardedInterstitialAd()
        {
            const string rewardMsg =
                "Rewarded interstitial ad rewarded the user. Type: {0}, amount: {1}.";

            if (_rewardedInterstitialAd != null && _rewardedInterstitialAd.CanShowAd())
            {
                _rewardedInterstitialAd.Show((Reward reward) =>
                {
                    // TODO: Reward the user.
                    Debug.Log(String.Format(rewardMsg, reward.Type, reward.Amount));
                });
            }
        }
        private void RegisterEventHandlers(RewardedInterstitialAd ad)
        {
            // Raised when the ad is estimated to have earned money.
            ad.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log(String.Format("Rewarded interstitial ad paid {0} {1}.",
                    adValue.Value,
                    adValue.CurrencyCode));
            };
            // Raised when an impression is recorded for an ad.
            ad.OnAdImpressionRecorded += () =>
            {
                Debug.Log("Rewarded interstitial ad recorded an impression.");
            };
            // Raised when a click is recorded for an ad.
            ad.OnAdClicked += () =>
            {
                Debug.Log("Rewarded interstitial ad was clicked.");
            };
            // Raised when an ad opened full screen content.
            ad.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("Rewarded interstitial ad full screen content opened.");
            };
            // Raised when the ad closed full screen content.
            ad.OnAdFullScreenContentClosed += () =>
            {
                IsAdCompleted = true;
                CheckForAdmobManagerScript();
                Debug.Log("Rewarded interstitial ad full screen content closed.");
                if(EnableRewardedInterstitialAds == true)
                {
                    LoadRewardedInterstitialAd();
                }
                
            };
            // Raised when the ad failed to open full screen content.
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError("Rewarded interstitial ad failed to open " +
                               "full screen content with error : " + error);

                if (EnableRewardedInterstitialAds == true)
                {
                    LoadRewardedInterstitialAd();
                }
            };
        }
        #endregion
        public void OpenInspector()
        {
            MobileAds.OpenAdInspector(error =>
            {
                // Error will be set if there was an issue and the inspector was not displayed.
            });
        }

        // New Native Overlay Ads
        //public void LoadNativeOverlayAd()
        //{
        //    //// Clean up the old ad before loading a new one.
        //    //if (_nativeOverlayAd != null)
        //    //{
        //    //    DestroyNativeOverlayAd();
        //    //}

        //    Debug.Log("Loading native overlay ad.");

        //    // Create a request used to load the ad.
        //    var adRequest = new AdRequest();

        //    // Optional: Define native ad options.
        //    var options = new NativeAdOptions
        //    {
        //        //AdChoicesPlacement = AdChoicesPlacement.BottomLeftCorner,
        //        //MediaAspectRatio = MediaAspectRatio.Any,
        //    };

        //    // Send the request to load the ad.
        //    NativeOverlayAd.Load(nativeoverlayID, adRequest, options,
        //        (NativeOverlayAd ad, LoadAdError error) =>
        //        {
        //            if (error != null)
        //            {
        //                Debug.LogError("Native Overlay ad failed to load an ad " +
        //                       " with error: " + error);
        //                return;
        //            }

        //    // The ad should always be non-null if the error is null, but
        //    // double-check to avoid a crash.
        //    if (ad == null)
        //            {
        //                Debug.LogError("Unexpected error: Native Overlay ad load event " +
        //                       " fired with null ad and null error.");
        //                return;
        //            }

        //    // The operation completed successfully.
        //    Debug.Log("Native Overlay ad loaded with response : " +
        //               ad.GetResponseInfo());
        //            _nativeOverlayAd = ad;

        //    // Register to ad events to extend functionality.
        //    RegisterEventHandlersForNativeOverlayAds(ad);
        //        });

        //    RenderAd();


        //}
        //private void RegisterEventHandlersForNativeOverlayAds(NativeOverlayAd ad)
        //{
        //    // Raised when the ad is estimated to have earned money.
        //    ad.OnAdPaid += (AdValue adValue) =>
        //    {
        //        Debug.Log(String.Format("App open ad paid {0} {1}.",
        //            adValue.Value,
        //            adValue.CurrencyCode));
        //    };
        //    // Raised when an impression is recorded for an ad.
        //    ad.OnAdImpressionRecorded += () =>
        //    {
        //        Debug.Log("App open ad recorded an impression.");
        //    };
        //    // Raised when a click is recorded for an ad.
        //    ad.OnAdClicked += () =>
        //    {
        //        Debug.Log("App open ad was clicked.");
        //    };
        //    // Raised when an ad opened full screen content.
        //    ad.OnAdFullScreenContentOpened += () =>
        //    {
        //        Debug.Log("App open ad full screen content opened.");
        //    };
        //    // Raised when the ad closed full screen content.
        //    ad.OnAdFullScreenContentClosed += () =>
        //    {
        //        Debug.Log("App open ad full screen content closed.");
        //        LoadNativeOverlayAd();
        //    };
        //}
        //public void RenderAd()
        //{
        //    if (_nativeOverlayAd != null)
        //    {
        //        Debug.Log("Rendering Native Overlay ad.");

        //        var style = new NativeTemplateStyle
        //        {
        //            TemplateId = NativeTemplateId.Medium,
        //            MainBackgroundColor = Color.black,
        //            CallToActionText = new NativeTemplateTextStyle
        //            {
        //                BackgroundColor = Color.yellow,
        //                TextColor = Color.black,
        //                FontSize = 15,
        //                Style = NativeTemplateFontStyle.Bold
        //            }
        //        };

        //        // Renders a native overlay ad at the default size
        //        // and anchored to the bottom of the screne.
        //        _nativeOverlayAd.RenderTemplate(style, AdPosition.Bottom);
        //    }
        //}
        //public void ShowNativeOverlayAd()
        //{
        //    if (_nativeOverlayAd != null)
        //    {
        //        Debug.Log("Showing Native Overlay ad.");
            
        //        _nativeOverlayAd.Show();
        //    }
        //}
        //public void HideNativeOverlayAd()
        //{
        //    if (_nativeOverlayAd != null)
        //    {
        //        Debug.Log("Hiding Native Overlay ad.");
        //        _nativeOverlayAd.Hide();
        //    }
        //}
        //public void DestroyNativeOverlayAd()
        //{
        //    if (_nativeOverlayAd != null)
        //    {
        //        Debug.Log("Destroying native overlay ad.");
        //        _nativeOverlayAd.Destroy();
        //        _nativeOverlayAd = null;
        //    }
        //}
    }
}