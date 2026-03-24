using LudoMaster.Core;
using LudoMaster.Signals;
using TMPro;
using UnityEngine;

namespace LudoMaster.Managers
{
    /// <summary>
    /// Central UI mediator for gameplay HUD, leaving room for future platform-specific views.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text coinText;
        [SerializeField] private TMP_Text turnText;
        [SerializeField] private TMP_Text resultText;
        [SerializeField] private TMP_Text roomText;
        [SerializeField] private string localPlayerId = "P1";
        [SerializeField] private RoomManager roomManager;

        private void Awake()
        {
            if (roomManager == null)
            {
                roomManager = FindObjectOfType<RoomManager>();
            }
        }

        private void OnEnable()
        {
            GameSignals.OnCoinBalanceChanged += HandleCoinChanged;
            GameSignals.OnTurnChanged += HandleTurnChanged;
            GameSignals.OnPlayerRankAssigned += HandleRankAssigned;
            GameSignals.OnMatchStateChanged += HandleMatchStateChanged;
            GameSignals.OnRoomDataChanged += HandleRoomChanged;
        }

        private void OnDisable()
        {
            GameSignals.OnCoinBalanceChanged -= HandleCoinChanged;
            GameSignals.OnTurnChanged -= HandleTurnChanged;
            GameSignals.OnPlayerRankAssigned -= HandleRankAssigned;
            GameSignals.OnMatchStateChanged -= HandleMatchStateChanged;
            GameSignals.OnRoomDataChanged -= HandleRoomChanged;
        }

        private void HandleCoinChanged(string playerId, int balance)
        {
            if (playerId == localPlayerId && coinText != null)
            {
                coinText.text = $"Coins: {balance}";
            }
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

        private void HandleRoomChanged()
        {
            if (roomText == null)
            {
                return;
            }

            if (roomManager == null || roomManager.CurrentRoom == null)
            {
                roomText.text = "Room: Not Joined";
                return;
            }

            roomText.text = $"Room: {roomManager.CurrentRoom.RoomName} | Pot: {roomManager.CurrentRoom.CurrentPot}";
        }

        public void ConfigureHud(TMP_Text coins, TMP_Text turn, TMP_Text result, TMP_Text room = null)
        {
            coinText = coins;
            turnText = turn;
            resultText = result;
            roomText = room;
        }
    }
}
