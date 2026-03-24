using System.Collections.Generic;
using LudoMaster.Core;
using LudoMaster.Signals;
using UnityEngine;

namespace LudoMaster.Managers
{
    /// <summary>
    /// Room lobby service for create/join flow, fee validation, and room presets.
    /// </summary>
    public class RoomManager : MonoBehaviour
    {
        [SerializeField] private CoinManager coinManager;
        [SerializeField] private string localPlayerId = "P1";

        public List<RoomData> AvailableRooms { get; } = new();
        public RoomData CurrentRoom { get; private set; }

        private void Awake()
        {
            if (coinManager == null)
            {
                coinManager = FindObjectOfType<CoinManager>();
            }

            EnsureDefaultRooms();
        }

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

        public bool JoinRoom(string roomId, string playerId)
        {
            RoomData room = AvailableRooms.Find(r => r.RoomId == roomId);
            if (room == null || !room.HasOpenSlot()) return false;

            if (coinManager != null && !coinManager.TryPayEntryFee(playerId, room.EntryFee))
            {
                return false;
            }

            if (!room.PlayerIds.Contains(playerId))
            {
                room.PlayerIds.Add(playerId);
            }

            CurrentRoom = room;
            GameSignals.OnRoomDataChanged?.Invoke();
            return true;
        }

        public bool JoinRoomAsLocalPlayer(string roomId)
        {
            return JoinRoom(roomId, localPlayerId);
        }

        public void RewardWinner(string playerId)
        {
            if (CurrentRoom != null && coinManager != null)
            {
                coinManager.AddCoins(playerId, CurrentRoom.WinReward);
            }
        }

        private void EnsureDefaultRooms()
        {
            if (AvailableRooms.Count > 0) return;
            CreateRoom("Low Coin Room", 25, 100);
            CreateRoom("Medium Coin Room", 100, 400);
            CreateRoom("High Coin Room", 500, 2400);
            CreateRoom("Private Room", 200, 1000);
        }
    }
}
