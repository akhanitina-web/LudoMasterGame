using UnityEngine;

namespace LudoMaster.Managers
{
    /// <summary>
    /// Wrapper for rewarded and interstitial ad hooks.
    /// </summary>
    public class AdsManager : MonoBehaviour
    {
        public System.Action OnRewardedCompleted;

        /// <summary>
        /// Loads ad SDK objects. Call during app init.
        /// </summary>
        public void InitializeAds()
        {
            // Integration point: initialize Unity Ads / AdMob / AppLovin SDK.
            Debug.Log("Ads initialized.");
        }

        /// <summary>
        /// Shows rewarded ad and emits completion callback.
        /// </summary>
        public void ShowRewardedAd()
        {
            // Integration point: show rewarded ad and invoke callback on reward.
            Debug.Log("Showing rewarded ad...");
            OnRewardedCompleted?.Invoke();
        }

        /// <summary>
        /// Shows interstitial ad between rounds or screens.
        /// </summary>
        public void ShowInterstitialAd()
        {
            // Integration point: show interstitial ad on safe UX moments.
            Debug.Log("Showing interstitial ad...");
        }
    }
}
