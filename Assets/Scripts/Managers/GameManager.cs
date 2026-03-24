using System.Collections.Generic;
using LudoMaster.Core;
using LudoMaster.Gameplay;
using LudoMaster.Signals;
using UnityEngine;

namespace LudoMaster.Managers
{
    /// <summary>
    /// Top-level gameplay orchestrator that wires systems together.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private TurnSystem turnSystem;
        [SerializeField] private TokenSystem tokenSystem;
        [SerializeField] private WinSystem winSystem;
        [SerializeField] private MultiplayerSyncManager multiplayerSync;

        private readonly List<PlayerData> players = new();
        private void OnEnable()
        {
            GameSignals.OnDiceRolled += HandleDiceRolled;
        }

        private void OnDisable()
        {
            GameSignals.OnDiceRolled -= HandleDiceRolled;
        }

        private void Start()
        {
            BootstrapPlayers();
            turnSystem.Initialize(players);
            GameSignals.OnMatchStateChanged?.Invoke(MatchState.Playing);
        }

        /// <summary>
        /// Initializes 4 local/offline players and their 4 token data entries.
        /// </summary>
        private void BootstrapPlayers()
        {
            players.Clear();
            for (int p = 0; p < 4; p++)
            {
                var player = new PlayerData
                {
                    PlayerId = $"P{p + 1}",
                    DisplayName = $"Player {p + 1}",
                    Color = (PlayerColor)p,
                    IsLocalPlayer = p == 0,
                    IsBot = p > 0
                };

                for (int t = 0; t < 4; t++)
                {
                    player.Tokens.Add(new CoreTokenData { TokenId = t });
                }

                players.Add(player);
            }
        }

        private void HandleDiceRolled(int value)
        {
            if (!turnSystem.CurrentPlayerHasAnyMove(value))
            {
                turnSystem.SkipCurrentPlayer();
                return;
            }

            // MVP flow: auto-select first valid token to keep architecture complete.
            var current = turnSystem.CurrentPlayer;
            var token = tokenSystem.GetFirstMovableToken(current, value);
            if (token != null)
            {
                StartCoroutine(tokenSystem.MoveToken(current, token, value, OnTurnResolved));
                multiplayerSync.BroadcastMove(current.PlayerId, token.TokenId, value);
            }
        }

        private void OnTurnResolved(TurnResult result)
        {
            turnSystem.ResolveTurn(result);
            winSystem.EvaluateRanks(players);
        }
    }
}
