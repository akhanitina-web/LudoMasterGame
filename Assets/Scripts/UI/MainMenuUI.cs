using LudoMaster.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LudoMaster.UI
{
    /// <summary>
    /// Main menu flow for online play entry, coin display, settings and shop buttons.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private Button playOnlineButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button shopButton;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text coinText;
        [SerializeField] private CoinManager coinManager;
        [SerializeField] private string localPlayerId = "P1";
        [SerializeField] private string lobbySceneName = "GameScene";

        private void Awake()
        {
            if (playOnlineButton != null) playOnlineButton.onClick.AddListener(OnPlayOnlinePressed);
            if (settingsButton != null) settingsButton.onClick.AddListener(ToggleSettingsPanel);
            if (shopButton != null) shopButton.onClick.AddListener(ToggleShopPanel);

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
        /// Loads the lobby/gameplay scene for online matchmaking.
        /// </summary>
        public void OnPlayOnlinePressed()
        {
            SceneManager.LoadScene(lobbySceneName);
        }

        public void ToggleSettingsPanel()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(!settingsPanel.activeSelf);
            }
        }

        public void ToggleShopPanel()
        {
            if (shopPanel != null)
            {
                shopPanel.SetActive(!shopPanel.activeSelf);
            }
        }
    }
}
