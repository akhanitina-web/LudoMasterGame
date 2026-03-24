using System.Collections;
using System.Collections.Generic;
using LudoMaster.Core;
using LudoMaster.Gameplay;
using UnityEngine;

namespace LudoMaster.Managers
{
    /// <summary>
    /// High-level manager wrapper over TokenSystem to keep future network integrations isolated.
    /// </summary>
    public class TokenManager : MonoBehaviour
    {
        [SerializeField] private TokenSystem tokenSystem;

        public int TotalTokenCount => tokenSystem == null ? 0 : tokenSystem.TotalTokenCount;

        public Transform EnsureTokenRoot() => tokenSystem == null ? null : tokenSystem.EnsureTokenRoot();

        public void RegisterTokenSystem(TokenSystem system)
        {
            tokenSystem = system;
        }

        public void RegisterTokens(IEnumerable<TokenController> allTokens)
        {
            tokenSystem?.RegisterTokens(allTokens);
        }

        public bool HasAnyMove(PlayerData player, int diceValue)
        {
            return tokenSystem != null && tokenSystem.HasAnyMove(player, diceValue);
        }

        public CoreTokenData GetFirstMovableToken(PlayerData player, int diceValue)
        {
            return tokenSystem == null ? null : tokenSystem.GetFirstMovableToken(player, diceValue);
        }

        public IEnumerator MoveToken(PlayerData player, CoreTokenData tokenData, int diceValue, System.Action<TurnResult> callback)
        {
            if (tokenSystem == null)
            {
                callback?.Invoke(TurnResult.NoMoves);
                yield break;
            }

            yield return tokenSystem.MoveToken(player, tokenData, diceValue, callback);
        }

        public void SetSelectableForMove(PlayerData player, int diceValue)
        {
            tokenSystem?.SetSelectableForMove(player, diceValue);
        }

        public bool IsMovableToken(PlayerData player, int tokenId, int diceValue)
        {
            return tokenSystem != null && tokenSystem.IsMovableToken(player, tokenId, diceValue);
        }

        public CoreTokenData GetTokenData(PlayerData player, int tokenId)
        {
            return tokenSystem == null ? null : tokenSystem.GetTokenData(player, tokenId);
        }

        public void SetSelectableForAll(bool selectable)
        {
            tokenSystem?.SetSelectableForAll(selectable);
        }
    }
}
