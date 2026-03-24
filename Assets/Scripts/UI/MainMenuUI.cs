using LudoMaster.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LudoMaster.UI
{
    /// <summary>
    /// Main menu logic with play flow, coin display, and settings panel toggles.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text coinText;
        [SerializeField] private CoinManager coinManager;
        [SerializeField] private string localPlayerId = "P1";
        [SerializeField] private string gameplaySceneName = "GameScene";

        private void Awake()
        {
            if (playButton != null) playButton.onClick.AddListener(OnPlayPressed);
            if (settingsButton != null) settingsButton.onClick.AddListener(ToggleSettingsPanel);

            if (titleText != null && string.IsNullOrEmpty(titleText.text))
            {
                titleText.text = "LUDO ROYAL";
            }

            RefreshCoins();
        }

        public void RefreshCoins()
        {
            if (coinManager != null && coinText != null)
            {
                coinText.text = $"Coins: {coinManager.GetBalance(localPlayerId)}";
            }
        }

        /// <summary>
        /// Loads gameplay scene in portrait mobile flow.
        /// </summary>
        public void OnPlayPressed()
        {
            SceneManager.LoadScene(gameplaySceneName);
        }

        public void ToggleSettingsPanel()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(!settingsPanel.activeSelf);
            }
        }
    }
}
