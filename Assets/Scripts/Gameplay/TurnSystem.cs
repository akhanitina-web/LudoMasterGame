using System.Collections.Generic;
using LudoMaster.Core;
using LudoMaster.Signals;
using UnityEngine;

namespace LudoMaster.Gameplay
{
    /// <summary>
    /// Manages turn order, extra-turn behavior, and skipping when no move exists.
    /// </summary>
    public class TurnSystem : MonoBehaviour
    {
        [SerializeField] private TokenSystem tokenSystem;

        private readonly List<PlayerData> players = new();
        private int currentPlayerIndex;

        public PlayerData CurrentPlayer => players.Count == 0 ? null : players[currentPlayerIndex];

        public void Initialize(List<PlayerData> initialPlayers)
        {
            players.Clear();
            players.AddRange(initialPlayers);
            currentPlayerIndex = 0;
            AnnounceTurn();
        }

        /// <summary>
        /// Applies resolved turn result and decides whether to keep or rotate turn.
        /// </summary>
        public void ResolveTurn(TurnResult result)
        {
            bool hasExtra = (result & TurnResult.ExtraTurn) != 0;
            if (!hasExtra)
            {
                MoveToNextActivePlayer();
            }

            AnnounceTurn();
        }

        /// <summary>
        /// Forces skip when player rolled but had no legal move.
        /// </summary>
        public void SkipCurrentPlayer()
        {
            MoveToNextActivePlayer();
            AnnounceTurn();
        }

        public bool CurrentPlayerHasAnyMove(int diceValue)
        {
            return CurrentPlayer != null && tokenSystem != null && tokenSystem.HasAnyMove(CurrentPlayer, diceValue);
        }

        private void MoveToNextActivePlayer()
        {
            if (players.Count == 0) return;

            int guard = 0;
            do
            {
                currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
                guard++;
            }
            while (players[currentPlayerIndex].HasFinishedAllTokens() && guard <= players.Count);
        }

        private void AnnounceTurn()
        {
            if (CurrentPlayer != null)
            {
                GameSignals.OnTurnChanged?.Invoke(CurrentPlayer.Color);
            }
        }
    }
}
