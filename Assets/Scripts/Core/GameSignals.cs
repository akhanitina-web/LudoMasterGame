using System;
using LudoMaster.Core;

namespace LudoMaster.Signals
{
    /// <summary>
    /// Lightweight event bus used to decouple managers and UI.
    /// </summary>
    public static class GameSignals
    {
        public static Action<int> OnDiceRolled;
        public static Action<PlayerColor> OnTurnChanged;
        public static Action<string, int> OnCoinBalanceChanged;
        public static Action OnRoomDataChanged;
        public static Action<PlayerColor, int> OnPlayerRankAssigned;
        public static Action<MatchState> OnMatchStateChanged;
    }
}
