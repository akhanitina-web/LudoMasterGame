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
        [SerializeField] private TokenManager tokenManager;
        [SerializeField] private WinSystem winSystem;
        [SerializeField] private MultiplayerSyncManager multiplayerSync;
        [SerializeField] private TokenSpawner tokenSpawner;

        private readonly List<PlayerData> players = new();
        private int pendingDiceValue = -1;
        private bool isAwaitingTokenSelection;
        private bool isResolvingMove;

        private bool IsReady => turnSystem != null && tokenManager != null && winSystem != null;
        private void OnEnable()
        {
            GameSignals.OnDiceRolled += HandleDiceRolled;
            GameSignals.OnTokenSelected += HandleTokenSelected;
        }

        private void OnDisable()
        {
            GameSignals.OnDiceRolled -= HandleDiceRolled;
            GameSignals.OnTokenSelected -= HandleTokenSelected;
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
            if (!IsReady || turnSystem.CurrentPlayer == null || isResolvingMove || isAwaitingTokenSelection)
            {
                return;
            }

            if (!turnSystem.CurrentPlayerHasAnyMove(value))
            {
                turnSystem.SkipCurrentPlayer();
                return;
            }

            var current = turnSystem.CurrentPlayer;
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

            PlayerData current = turnSystem.CurrentPlayer;
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
            turnSystem.ResolveTurn(result);
            winSystem.EvaluateRanks(players);
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
