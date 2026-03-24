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
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private TokenSystem tokenSystem;
        [SerializeField] private TokenManager tokenManager;
        [SerializeField] private WinSystem winSystem;
        [SerializeField] private MultiplayerSyncManager multiplayerSync;
        [SerializeField] private TokenSpawner tokenSpawner;
        [SerializeField] private RoomManager roomManager;

        private readonly List<PlayerData> players = new();
        private int pendingDiceValue = -1;
        private bool isAwaitingTokenSelection;
        private bool isResolvingMove;

        private bool IsReady => turnManager != null && tokenManager != null && winSystem != null;
        private void OnEnable()
        {
            GameSignals.OnDiceRolled += HandleDiceRolled;
            GameSignals.OnTokenSelected += HandleTokenSelected;
            GameSignals.OnPlayerRankAssigned += HandlePlayerRankAssigned;
        }

        private void OnDisable()
        {
            GameSignals.OnDiceRolled -= HandleDiceRolled;
            GameSignals.OnTokenSelected -= HandleTokenSelected;
            GameSignals.OnPlayerRankAssigned -= HandlePlayerRankAssigned;
        }

        private void Start()
        {
            tokenManager = tokenManager ?? FindObjectOfType<TokenManager>();
            if (tokenManager == null)
            {
                tokenManager = new GameObject("TokenManager").AddComponent<TokenManager>();
            }

            tokenSystem = tokenSystem ?? FindObjectOfType<TokenSystem>();
            tokenManager.RegisterTokenSystem(tokenSystem);

            if (!IsReady)
            {
                Debug.LogWarning("GameManager is missing required system references.", this);
                enabled = false;
                return;
            }

            BootstrapPlayers();
            EnsureTokensReady();
            roomManager = roomManager ?? FindObjectOfType<RoomManager>();
            turnManager.Initialize(players);
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
            if (!IsReady || turnManager.CurrentPlayer == null || isResolvingMove || isAwaitingTokenSelection)
            {
                return;
            }

            if (!turnManager.CurrentPlayerHasAnyMove(value))
            {
                turnManager.SkipCurrentPlayer();
                return;
            }

            var current = turnManager.CurrentPlayer;
            pendingDiceValue = value;
            isAwaitingTokenSelection = true;
            tokenManager.SetSelectableForMove(current, value);

            if (current.IsBot)
            {
                var token = tokenManager.GetFirstMovableToken(current, value);
                if (token != null)
                {
                    HandleTokenSelected(current.Color, token.TokenId);
                }
            }
        }

        private void HandleTokenSelected(PlayerColor color, int tokenId)
        {
            if (!isAwaitingTokenSelection || isResolvingMove || !IsReady)
            {
                return;
            }

            PlayerData current = turnManager.CurrentPlayer;
            if (current == null || current.Color != color)
            {
                return;
            }

            if (!tokenManager.IsMovableToken(current, tokenId, pendingDiceValue))
            {
                return;
            }

            CoreTokenData token = tokenManager.GetTokenData(current, tokenId);
            if (token == null)
            {
                return;
            }

            isAwaitingTokenSelection = false;
            isResolvingMove = true;
            tokenManager.SetSelectableForAll(false);
            StartCoroutine(tokenManager.MoveToken(current, token, pendingDiceValue, OnTurnResolved));
            multiplayerSync?.BroadcastMove(current.PlayerId, token.TokenId, pendingDiceValue);
        }

        private void OnTurnResolved(TurnResult result)
        {
            pendingDiceValue = -1;
            isAwaitingTokenSelection = false;
            isResolvingMove = false;
            tokenManager.SetSelectableForAll(false);
            turnManager.ResolveTurn(result);
            winSystem.EvaluateRanks(players);
        }


        private void HandlePlayerRankAssigned(PlayerColor color, int placement)
        {
            if (placement != 1 || roomManager == null)
            {
                return;
            }

            PlayerData winner = players.Find(p => p.Color == color);
            if (winner != null)
            {
                roomManager.RewardWinner(winner.PlayerId);
            }
        }
        private void EnsureTokensReady()
        {
            if (tokenManager.TotalTokenCount > 0)
            {
                return;
            }

            tokenSpawner = tokenSpawner ?? FindObjectOfType<TokenSpawner>();
            if (tokenSpawner == null)
            {
                tokenSpawner = new GameObject("TokenSpawner").AddComponent<TokenSpawner>();
            }

            tokenSpawner.BuildDefaultTokens(tokenManager.EnsureTokenRoot(), tokenSystem);
        }
    }
}
