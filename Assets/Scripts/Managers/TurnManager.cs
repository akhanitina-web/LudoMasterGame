using System.Collections.Generic;
using LudoMaster.Core;
using LudoMaster.Gameplay;
using UnityEngine;

namespace LudoMaster.Managers
{
    /// <summary>
    /// Turn orchestrator wrapper over <see cref="TurnSystem"/> with optional Photon turn sync hooks.
    /// </summary>
    public class TurnManager : MonoBehaviour
    {
        [SerializeField] private TurnSystem turnSystem;
        [SerializeField] private PhotonManager photonManager;

        public PlayerData CurrentPlayer => turnSystem == null ? null : turnSystem.CurrentPlayer;

        private void Awake()
        {
            photonManager ??= FindObjectOfType<PhotonManager>();
        }

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
            BroadcastCurrentTurnIndex();
        }

        public void SkipCurrentPlayer()
        {
            turnSystem?.SkipCurrentPlayer();
            BroadcastCurrentTurnIndex();
        }

        private void BroadcastCurrentTurnIndex()
        {
            if (photonManager == null || CurrentPlayer == null)
            {
                return;
            }

            photonManager.SendTurnChange((int)CurrentPlayer.Color);
        }
    }
}
