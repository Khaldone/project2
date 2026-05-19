using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Reflection;
using UnityEngine.Advertisements;

namespace MobileMonetizationPro
{
    public class MobileMonetizationPro_AdmobAdsManager : MonoBehaviour
    {
        [Tooltip("Enable or disable the debugging of Ad Inspector.")]
        public bool DebugAdInspector = true;

        // Button to open the Ad Inspector for debugging purposes
        [Tooltip("Button to open Ad Inspector for debugging.")]
        public Button AdInspectorButton;

        // Button to trigger the display of a banner ad
        [Tooltip("Button to show the banner ad.")]
        public Button ShowBannerAdButton;

        // Image used for displaying a native ad
        //[Tooltip("Image used to display the native ad.")]
        //public Image ImageToUseToDisplayNativeAd;

        [Serializable]
        public class FunctionInfo
        {
            // Flag to indicate whether the rewarded interstitial ad should be shown
            [Tooltip("If true, shows a rewarded interstitial ad.")]
            public bool ShowRewardedInterstial = false;

            // Button used to trigger the rewarded ad
            [Tooltip("Button associated with the rewarded ad.")]
            public Button RewardedButton;

            // Script attached to the button that handles the function
            [Tooltip("Script attached to the button that will call the selected function.")]
            public MonoBehaviour script;

            // Name of the script attached to the button
            [Tooltip("Name of the script associated with the button.")]
            public string scriptName;

            // List of function names in the script that can be invoked
            [Tooltip("List of function names to call from the script.")]
            public List<string> functionNames;

            // The index of the selected function from the list
            [Tooltip("Index of the selected function from the list.")]
            public int selectedFunctionIndex;
        }

        // List of buttons that trigger interstitial ads
        [Tooltip("Buttons that trigger the interstitial ads.")]
        public Button[] ActionButtonsToInvokeInterstitalAds;

        // List of buttons that trigger rewarded ads
        [Tooltip("List of buttons that will trigger rewarded ads.")]
        public List<Button> rewardedButtons = new List<Button>();

        // List of function information for tracking rewarded ads and corresponding actions
        [Tooltip("List of function information for tracking rewarded ads.")]
        public List<FunctionInfo> functions = new List<FunctionInfo>();

        FunctionInfo functionInfo;

        private void OnValidate()
        {
            foreach (var function in functions)
            {
                function.functionNames = GetFunctionNames(function.script);
            }
        }
        public List<Button> GetRewardedButtons()
        {
            foreach (var functionInfo in functions)
            {
                rewardedButtons.Add(functionInfo.RewardedButton);
            }
            return rewardedButtons;
        }
        public void OnButtonClick()
        {
            if (functionInfo != null)
            {
                // Call the selected function when the button is clicked
                string selectedFunctionName = functionInfo.functionNames[functionInfo.selectedFunctionIndex];
                MethodInfo method = functionInfo.script.GetType().GetMethod(selectedFunctionName);
                if (method != null)
                {
                    method.Invoke(functionInfo.script, null);
                    functionInfo = null;
                }
            }
        }
        private List<string> GetFunctionNames(MonoBehaviour script)
        {
            List<string> functionNames = new List<string>();
            if (script != null)
            {
                Type type = script.GetType();
                MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public);
                foreach (MethodInfo method in methods)
                {
                    functionNames.Add(method.Name);
                }
            }
            return functionNames;
        }
        private void Start()
        {
            if (PlayerPrefs.GetInt("AdsRemoved") == 0)
            {
                if (MobileMonetizationPro_AdmobAdsInitializer.instance != null)
                {
                    //MobileMonetizationPro_AdmobAdsInitializer.instance.ImageToUseToDisplayNativeAd = ImageToUseToDisplayNativeAd;

                    if (MobileMonetizationPro_AdmobAdsInitializer.instance.ShowBannerAdsInStart == false)
                    {
                        if (ShowBannerAdButton != null)
                        {
                            ShowBannerAdButton.onClick.AddListener(() =>
                            {
                                MobileMonetizationPro_AdmobAdsInitializer.instance.LoadBanner();
                            });
                        }

                    }

                    if (DebugAdInspector == true)
                    {
                        AdInspectorButton.onClick.AddListener(() => MobileMonetizationPro_AdmobAdsInitializer.instance.OpenInspector());
                    }
                }


                for (int i = 0; i < ActionButtonsToInvokeInterstitalAds.Length; i++)
                {
                    if (ActionButtonsToInvokeInterstitalAds[i] != null)
                    {
                        ActionButtonsToInvokeInterstitalAds[i].onClick.AddListener(() =>
                        {
                            // Call a function when the button is clicked
                            ShowInterstitial();
                        });

                    }

                }
            }



            List<Button> rewardedButtons = GetRewardedButtons();

            // Now you can work with the `rewardedButtons` list
            foreach (Button rewardedButton in rewardedButtons)
            {
                // Do something with each rewarded button
                // For example, you can add a click listener
                if (rewardedButton != null)
                {
                    rewardedButton.onClick.AddListener(() => ShowRewarded(rewardedButton));
                }

            }
        }
        public void ShowInterstitial()
        {

            if (PlayerPrefs.GetInt("AdsRemoved") == 0)
            {
                if (MobileMonetizationPro_AdmobAdsInitializer.instance != null)
                {
                    if (MobileMonetizationPro_AdmobAdsInitializer.instance.EnableTimedInterstitalAds == false)
                    {
                        MobileMonetizationPro_AdmobAdsInitializer.instance.ShowInterstitialAd(false);
                    }
                    else
                    {

                        if (MobileMonetizationPro_AdmobAdsInitializer.instance.IsInterstitialAdTimerCompleted == true)
                        {
                            MobileMonetizationPro_AdmobAdsInitializer.instance.ShowInterstitialAd(false);
                        }
                    }
                }
            }
        }

        public void ShowRewarded(Button clickedButton)
        {
            if (MobileMonetizationPro_AdmobAdsInitializer.instance != null)
            {
                functionInfo = functions.Find(info => info.RewardedButton == clickedButton);
                if(functionInfo.ShowRewardedInterstial == false)
                {
                    Debug.Log("Showing Rewarded Video Ad");
                    MobileMonetizationPro_AdmobAdsInitializer.instance.ShowRewardedAd();
                }
                else
                {
                    Debug.Log("Showing Rewarded Interstitial Video Ad");
                    MobileMonetizationPro_AdmobAdsInitializer.instance.ShowRewardedInterstitialAd();
                }          
            }
        }
        public void ResetAndReloadFullAds()
        {
            if (MobileMonetizationPro_AdmobAdsInitializer.instance != null)
            {
                if (MobileMonetizationPro_AdmobAdsInitializer.instance.ResetInterstitalAdTimerOnRewardedAd == true)
                {
                    MobileMonetizationPro_AdmobAdsInitializer.instance.IsInterstitialAdTimerCompleted = false;
                    MobileMonetizationPro_AdmobAdsInitializer.instance.Timer = 0f;
                }

                if (MobileMonetizationPro_AdmobAdsInitializer.instance.EnableTimedInterstitalAds == true)
                {
                    MobileMonetizationPro_AdmobAdsInitializer.instance.IsInterstitialAdTimerCompleted = false;
                    MobileMonetizationPro_AdmobAdsInitializer.instance.Timer = 0f;
                }
                MobileMonetizationPro_AdmobAdsInitializer.instance.IsAdCompleted = false;
                MobileMonetizationPro_AdmobAdsInitializer.instance.IsAdSkipped = false;
                MobileMonetizationPro_AdmobAdsInitializer.instance.IsAdUnknown = false;

                if(MobileMonetizationPro_AdmobAdsInitializer.instance.EnableInterstitialAds == true)
                {
                    MobileMonetizationPro_AdmobAdsInitializer.instance.LoadInterstitial();
                }

                if (MobileMonetizationPro_AdmobAdsInitializer.instance.EnableRewardedAds == true)
                {
                    MobileMonetizationPro_AdmobAdsInitializer.instance.LoadRewarded();
                }

                if (MobileMonetizationPro_AdmobAdsInitializer.instance.EnableRewardedInterstitialAds == true)
                {
                    MobileMonetizationPro_AdmobAdsInitializer.instance.LoadRewardedInterstitialAd();
                }
            }
        }
        public void CheckForAdCompletion()
        {
            if(MobileMonetizationPro_AdmobAdsInitializer.instance != null)
            {
                if (MobileMonetizationPro_AdmobAdsInitializer.instance.IsAdCompleted == true)
                {
                    ResetAndReloadFullAds();
                    if (MobileMonetizationPro_AdmobAdsInitializer.instance != null)
                    {
                        OnButtonClick();
                    }
                }
                else if (MobileMonetizationPro_AdmobAdsInitializer.instance.IsAdSkipped == true)
                {
                    ResetAndReloadFullAds();
                }
                else if (MobileMonetizationPro_AdmobAdsInitializer.instance.IsAdUnknown == true)
                {
                    ResetAndReloadFullAds();
                }
            }
          
        }
    }
}