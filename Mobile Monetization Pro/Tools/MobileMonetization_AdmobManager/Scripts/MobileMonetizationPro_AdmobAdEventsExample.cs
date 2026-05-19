using System.Collections;
using UnityEngine;

namespace MobileMonetizationPro
{
    public class MobileMonetizationPro_AdmobAdEventsExample : MonoBehaviour
    {
        private MobileMonetizationPro.MobileMonetizationPro_AdmobAdsInitializer admobInitializer;

        private void Start()
        {
            admobInitializer = MobileMonetizationPro.MobileMonetizationPro_AdmobAdsInitializer.instance;
            if (admobInitializer != null)
            {
                StartCoroutine(ShowInterstitialAutomatically());
            }
            else
            {
                Debug.LogWarning("AdmobAdsInitializer instance not found!");
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && admobInitializer != null)
            {
                admobInitializer.HideBanner();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player") && admobInitializer != null)
            {
                admobInitializer.ShowBanner();
            }
        }

        private IEnumerator ShowInterstitialAutomatically()
        {
            yield return new WaitForSeconds(5f);
            if (admobInitializer != null)
            {
                admobInitializer.ShowInterstitialAd(true);
                admobInitializer.ResetInterstitialAdTimer();
            }
        }
    }
}