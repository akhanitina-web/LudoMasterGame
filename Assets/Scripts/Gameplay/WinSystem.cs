using System.Collections.Generic;
using LudoMaster.Core;
using LudoMaster.Signals;
using UnityEngine;

namespace LudoMaster.Gameplay
{
    /// <summary>
    /// Tracks finish order and emits rank updates when players complete all tokens.
    /// </summary>
    public class WinSystem : MonoBehaviour
    {
        private readonly List<PlayerData> rankedPlayers = new();

        /// <summary>
        /// Evaluates players and assigns new ranks for newly-finished players.
        /// </summary>
        public void EvaluateRanks(List<PlayerData> players)
        {
            for (int i = 0; i < players.Count; i++)
            {
                var player = players[i];
                if (player.Placement == -1 && player.HasFinishedAllTokens())
                {
                    player.Placement = rankedPlayers.Count + 1;
                    rankedPlayers.Add(player);
                    GameSignals.OnPlayerRankAssigned?.Invoke(player.Color, player.Placement);
                }
            }

            if (rankedPlayers.Count >= players.Count - 1)
            {
                GameSignals.OnMatchStateChanged?.Invoke(MatchState.Completed);
            }
        }
    }
}
