using System.Collections;
using System.Collections.Generic;
using LudoMaster.Core;
using UnityEngine;

namespace LudoMaster.Gameplay
{
    /// <summary>
    /// Owns all 16 tokens, validates moves, resolves captures, and dispatches turn outcomes.
    /// </summary>
    public class TokenSystem : MonoBehaviour
    {
        [SerializeField] private BoardManager boardManager;
        [SerializeField] private Transform tokenRoot;

        private readonly Dictionary<PlayerColor, List<TokenController>> tokensByPlayer = new();

        /// <summary>
        /// Initializes 4 players with 4 tokens each from spawned token prefabs.
        /// </summary>
        public void RegisterTokens(IEnumerable<TokenController> allTokens)
        {
            tokensByPlayer.Clear();
            foreach (PlayerColor color in System.Enum.GetValues(typeof(PlayerColor)))
            {
                tokensByPlayer[color] = new List<TokenController>(4);
            }

            foreach (TokenController token in allTokens)
            {
                tokensByPlayer[token.OwnerColor].Add(token);
            }
        }

        /// <summary>
        /// Returns true if current player has any valid move for rolled value.
        /// </summary>
        public bool HasAnyMove(PlayerData player, int diceValue)
        {
            if (player == null || boardManager == null) return false;
            for (int i = 0; i < player.Tokens.Count; i++)
            {
                if (CanMoveToken(player, player.Tokens[i], diceValue))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Finds first token that can be moved for a given roll.
        /// </summary>
        public CoreTokenData GetFirstMovableToken(PlayerData player, int diceValue)
        {
            if (player == null || boardManager == null) return null;
            for (int i = 0; i < player.Tokens.Count; i++)
            {
                if (CanMoveToken(player, player.Tokens[i], diceValue))
                {
                    return player.Tokens[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Executes movement and capture logic for chosen token.
        /// </summary>
        public IEnumerator MoveToken(PlayerData player, CoreTokenData tokenData, int diceValue, System.Action<TurnResult> callback)
        {
            TurnResult result = TurnResult.None;

            if (player == null || tokenData == null || boardManager == null)
            {
                callback?.Invoke(TurnResult.NoMoves);
                yield break;
            }

            if (!CanMoveToken(player, tokenData, diceValue))
            {
                callback?.Invoke(TurnResult.NoMoves);
                yield break;
            }

            TokenController token = FindToken(player.Color, tokenData.TokenId);
            if (token == null)
            {
                callback?.Invoke(TurnResult.NoMoves);
                yield break;
            }

            if (tokenData.State == TokenState.InBase && diceValue == 6)
            {
                tokenData.State = TokenState.OnBoard;
                tokenData.BoardIndex = GetPlayerStartIndex(player.Color);
                tokenData.StepsMoved = 1;
                yield return token.MoveTo(boardManager.GetMainPathPosition(tokenData.BoardIndex));
                result |= TurnResult.Moved | TurnResult.ExtraTurn;
            }
            else
            {
                for (int i = 0; i < diceValue; i++)
                {
                    AdvanceTokenOneStep(player.Color, tokenData);
                    Vector3 destination = ResolveTokenWorldPosition(player.Color, tokenData);
                    yield return token.MoveTo(destination);
                }

                result |= TurnResult.Moved;
                if (diceValue == 6) result |= TurnResult.ExtraTurn;
            }

            if (tokenData.State == TokenState.Finished)
            {
                result |= TurnResult.FinishedToken;
            }

            if (TryCaptureOpponents(player.Color, tokenData.BoardIndex))
            {
                result |= TurnResult.Captured | TurnResult.ExtraTurn;
            }

            callback?.Invoke(result);
        }

        private bool CanMoveToken(PlayerData player, CoreTokenData tokenData, int diceValue)
        {
            if (tokenData.State == TokenState.Finished) return false;
            if (tokenData.State == TokenState.InBase) return diceValue == 6;

            int simulatedSteps = tokenData.StepsMoved + diceValue;
            return simulatedSteps <= boardManager.BoardLoopLength + 6;
        }

        private void AdvanceTokenOneStep(PlayerColor color, CoreTokenData token)
        {
            token.StepsMoved++;

            if (token.StepsMoved <= boardManager.BoardLoopLength)
            {
                token.State = TokenState.OnBoard;
                token.BoardIndex = (token.BoardIndex + 1) % boardManager.BoardLoopLength;
                return;
            }

            int homeStep = token.StepsMoved - boardManager.BoardLoopLength - 1;
            token.State = TokenState.InHomePath;
            token.HomePathIndex = homeStep;

            if (homeStep >= 5)
            {
                token.State = TokenState.Finished;
            }
        }

        private Vector3 ResolveTokenWorldPosition(PlayerColor color, CoreTokenData token)
        {
            if (token.State == TokenState.InHomePath || token.State == TokenState.Finished)
            {
                int index = Mathf.Clamp(token.HomePathIndex, 0, 5);
                return boardManager.GetHomePathPosition(color, index);
            }

            return boardManager.GetMainPathPosition(token.BoardIndex);
        }

        private bool TryCaptureOpponents(PlayerColor currentColor, int landingBoardIndex)
        {
            if (landingBoardIndex < 0 || boardManager.IsSafeTile(landingBoardIndex))
            {
                return false;
            }

            bool captured = false;
            foreach (var pair in tokensByPlayer)
            {
                if (pair.Key == currentColor) continue;

                for (int i = 0; i < pair.Value.Count; i++)
                {
                    var token = pair.Value[i];
                    if (token.Data.State == TokenState.OnBoard && token.Data.BoardIndex == landingBoardIndex)
                    {
                        token.Data.State = TokenState.InBase;
                        token.Data.StepsMoved = 0;
                        token.Data.BoardIndex = -1;
                        token.Data.HomePathIndex = -1;
                        if (tokenRoot != null)
                        {
                            token.ResetToSpawn();
                        }
                        captured = true;
                    }
                }
            }

            return captured;
        }

        private TokenController FindToken(PlayerColor color, int tokenId)
        {
            if (!tokensByPlayer.TryGetValue(color, out List<TokenController> list)) return null;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Data.TokenId == tokenId)
                {
                    return list[i];
                }
            }

            return null;
        }

        private int GetPlayerStartIndex(PlayerColor color)
        {
            return color switch
            {
                PlayerColor.Red => 0,
                PlayerColor.Blue => 13,
                PlayerColor.Green => 26,
                PlayerColor.Yellow => 39,
                _ => 0
            };
        }
    }
}
