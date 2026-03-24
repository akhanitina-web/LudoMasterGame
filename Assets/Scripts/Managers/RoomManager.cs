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
            coinManager ??= FindObjectOfType<CoinManager>();
            EnsureDefaultRooms();
        }

        public RoomData CreateRoom(string roomName, int entryFee, int winReward)
        {
            RoomData room = new()
            {
                RoomId = System.Guid.NewGuid().ToString("N"),
                RoomName = roomName,
                EntryFee = Mathf.Max(0, entryFee),
                WinReward = Mathf.Max(0, winReward),
                CurrentPot = 0
            };

            AvailableRooms.Add(room);
            GameSignals.OnRoomDataChanged?.Invoke();
            return room;
        }

        public bool JoinRoom(string roomId, string playerId)
        {
            if (string.IsNullOrWhiteSpace(roomId) || string.IsNullOrWhiteSpace(playerId))
            {
                return false;
            }

            RoomData room = AvailableRooms.Find(r => r.RoomId == roomId);
            if (room == null || !room.HasOpenSlot() || room.PlayerIds.Contains(playerId))
            {
                return false;
            }

            if (coinManager != null && !coinManager.TryPayEntryFee(playerId, room.EntryFee))
            {
                return false;
            }

            room.PlayerIds.Add(playerId);
            room.CurrentPot += room.EntryFee;
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
            if (CurrentRoom == null || coinManager == null || string.IsNullOrWhiteSpace(playerId))
            {
                return;
            }

            int reward = Mathf.Max(CurrentRoom.CurrentPot, CurrentRoom.WinReward);
            coinManager.AddCoins(playerId, reward);
            CurrentRoom.CurrentPot = 0;
            GameSignals.OnRoomDataChanged?.Invoke();
        }

        private void EnsureDefaultRooms()
        {
            if (AvailableRooms.Count > 0)
            {
                return;
            }

            CreateRoom("Free Room", 0, 0);
            CreateRoom("Low Coin Room", 100, 400);
            CreateRoom("Medium Coin Room", 1000, 4000);
            CreateRoom("High Coin Room", 10000, 40000);
        }
    }
}
