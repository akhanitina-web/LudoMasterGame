using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LudoMaster.UI
{
    /// <summary>
    /// Main menu logic for play button and bootstrap flows.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private Button playButton;
        [SerializeField] private string gameplaySceneName = "GameScene";

        private void Awake()
        {
            if (playButton != null)
            {
                playButton.onClick.AddListener(OnPlayPressed);
            }
        }

        /// <summary>
        /// Loads gameplay scene in portrait mobile flow.
        /// </summary>
        public void OnPlayPressed()
        {
            SceneManager.LoadScene(gameplaySceneName);
        }
    }
}
