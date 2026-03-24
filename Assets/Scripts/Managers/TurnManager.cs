using System.Collections.Generic;
using LudoMaster.Core;
using LudoMaster.Gameplay;
using UnityEngine;

namespace LudoMaster.Managers
{
    /// <summary>
    /// Manager wrapper over TurnSystem to keep scene architecture manager-centric.
    /// </summary>
    public class TurnManager : MonoBehaviour
    {
        [SerializeField] private TurnSystem turnSystem;

        public PlayerData CurrentPlayer => turnSystem == null ? null : turnSystem.CurrentPlayer;

        public void RegisterTurnSystem(TurnSystem system)
        {
            turnSystem = system;
        }

        public void Initialize(List<PlayerData> players)
        {
            turnSystem?.Initialize(players);
        }

        public bool CurrentPlayerHasAnyMove(int diceValue)
        {
            return turnSystem != null && turnSystem.CurrentPlayerHasAnyMove(diceValue);
        }

        public void ResolveTurn(TurnResult result)
        {
            turnSystem?.ResolveTurn(result);
        }

        public void SkipCurrentPlayer()
        {
            turnSystem?.SkipCurrentPlayer();
        }
    }
}
