using LudoMaster.Core;
using LudoMaster.Signals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LudoMaster.UI
{
    /// <summary>
    /// In-game HUD updates player coin, turn indicator, and dice availability.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [SerializeField] private TMP_Text coinText;
        [SerializeField] private TMP_Text turnText;
        [SerializeField] private TMP_Text resultText;
        [SerializeField] private Button diceButton;
        [SerializeField] private string localPlayerId = "P1";

        private void OnEnable()
        {
            GameSignals.OnCoinBalanceChanged += HandleCoinChanged;
            GameSignals.OnTurnChanged += HandleTurnChanged;
            GameSignals.OnPlayerRankAssigned += HandleRankAssigned;
            GameSignals.OnMatchStateChanged += HandleMatchStateChanged;
            GameSignals.OnDiceRollingStateChanged += HandleDiceRollingState;
        }

        private void OnDisable()
        {
            GameSignals.OnCoinBalanceChanged -= HandleCoinChanged;
            GameSignals.OnTurnChanged -= HandleTurnChanged;
            GameSignals.OnPlayerRankAssigned -= HandleRankAssigned;
            GameSignals.OnMatchStateChanged -= HandleMatchStateChanged;
            GameSignals.OnDiceRollingStateChanged -= HandleDiceRollingState;
        }

        private void HandleCoinChanged(string playerId, int balance)
        {
            if (playerId != localPlayerId || coinText == null) return;
            coinText.text = $"Coins: {balance}";
        }

        private void HandleTurnChanged(PlayerColor color)
        {
            if (turnText != null)
            {
                turnText.text = $"Turn: {color}";
            }
        }

        private void HandleRankAssigned(PlayerColor color, int rank)
        {
            if (resultText != null)
            {
                resultText.text = $"{color} finished at rank #{rank}";
            }
        }

        private void HandleMatchStateChanged(MatchState state)
        {
            if (state == MatchState.Completed && resultText != null)
            {
                resultText.text += "\nMatch Complete";
            }
        }

        private void HandleDiceRollingState(bool isRolling)
        {
            if (diceButton != null)
            {
                diceButton.interactable = !isRolling;
            }
        }
    }
}
