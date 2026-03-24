using LudoMaster.Signals;
using TMPro;
using UnityEngine;

namespace LudoMaster.UI
{
    /// <summary>
    /// Displays real-time coin balance updates for current player.
    /// </summary>
    public class CoinDisplayUI : MonoBehaviour
    {
        [SerializeField] private string localPlayerId = "P1";
        [SerializeField] private TMP_Text coinText;

        private void OnEnable()
        {
            GameSignals.OnCoinBalanceChanged += HandleCoinChanged;
        }

        private void OnDisable()
        {
            GameSignals.OnCoinBalanceChanged -= HandleCoinChanged;
        }

        private void HandleCoinChanged(string playerId, int balance)
        {
            if (playerId != localPlayerId || coinText == null) return;
            coinText.text = balance.ToString();
        }
    }
}
