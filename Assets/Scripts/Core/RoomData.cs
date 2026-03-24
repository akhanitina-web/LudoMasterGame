using System.Collections.Generic;

namespace LudoMaster.Core
{
    /// <summary>
    /// Represents room metadata, entry fee, and current participants.
    /// </summary>
    [System.Serializable]
    public class RoomData
    {
        public string RoomId;
        public string RoomName;
        public int MaxPlayers = 4;
        public int EntryFee;
        public int WinReward;
        public int CurrentPot;

        public readonly List<string> PlayerIds = new();

        /// <summary>
        /// Returns true if room has available slots.
        /// </summary>
        public bool HasOpenSlot() => PlayerIds.Count < MaxPlayers;
    }
}
