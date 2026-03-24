using System.Collections.Generic;
using LudoMaster.Core;
using LudoMaster.Signals;
using UnityEngine;

namespace LudoMaster.Managers
{
    /// <summary>
    /// Room lobby service for create/join flow and player slot management.
    /// </summary>
    public class RoomManager : MonoBehaviour
    {
        public List<RoomData> AvailableRooms { get; } = new();
        public RoomData CurrentRoom { get; private set; }

        /// <summary>
        /// Creates a room with fee and reward values.
        /// </summary>
        public RoomData CreateRoom(string roomName, int entryFee, int winReward)
        {
            RoomData room = new()
            {
                RoomId = System.Guid.NewGuid().ToString("N"),
                RoomName = roomName,
                EntryFee = entryFee,
                WinReward = winReward
            };

            AvailableRooms.Add(room);
            GameSignals.OnRoomDataChanged?.Invoke();
            return room;
        }

        /// <summary>
        /// Attempts to join room if slot is available.
        /// </summary>
        public bool JoinRoom(string roomId, string playerId)
        {
            RoomData room = AvailableRooms.Find(r => r.RoomId == roomId);
            if (room == null || !room.HasOpenSlot()) return false;

            if (!room.PlayerIds.Contains(playerId))
            {
                room.PlayerIds.Add(playerId);
            }

            CurrentRoom = room;
            GameSignals.OnRoomDataChanged?.Invoke();
            return true;
        }
    }
}
