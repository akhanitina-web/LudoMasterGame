using UnityEngine;

namespace LudoMaster.Managers
{
    /// <summary>
    /// AdMob-ready manager wrapper for banner, interstitial and rewarded ad flows.
    /// Works in stub mode until Google Mobile Ads SDK is imported.
    /// </summary>
    public class AdManager : MonoBehaviour
    {
        [SerializeField] private bool initializeOnStart = true;
        [SerializeField] private bool bannerVisibleByDefault = true;

        public System.Action OnRewardedCompleted;

        private void Start()
        {
            if (initializeOnStart)
            {
                InitializeAds();
            }
        }

        public virtual void InitializeAds()
        {
            Debug.Log("[AdManager] InitializeAds called. Import GoogleMobileAds SDK to enable live ads.");
            if (bannerVisibleByDefault)
            {
                ShowBanner();
            }
        }

        public virtual void ShowBanner()
        {
            Debug.Log("[AdManager] Banner show requested.");
        }

        public virtual void HideBanner()
        {
            Debug.Log("[AdManager] Banner hide requested.");
        }

        public virtual void ShowInterstitialAd()
        {
            Debug.Log("[AdManager] Interstitial show requested.");
        }

        public virtual void ShowRewardedAd()
        {
            Debug.Log("[AdManager] Rewarded show requested.");
            OnRewardedCompleted?.Invoke();
        }
    }
}
