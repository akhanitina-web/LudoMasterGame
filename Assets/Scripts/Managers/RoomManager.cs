using System;
using System.Collections.Generic;
using LudoMaster.Core;
using LudoMaster.Signals;
using UnityEngine;

namespace LudoMaster.Managers
{
    /// <summary>
    /// Handles lobby/room state for local + Photon-backed flows:
    /// create/join rooms, private room code, ready states, and host-only start checks.
    /// </summary>
    public class RoomManager : MonoBehaviour
    {
        [SerializeField] private CoinManager coinManager;
        [SerializeField] private PhotonManager photonManager;
        [SerializeField] private string localPlayerId = "P1";

        public List<RoomData> AvailableRooms { get; } = new();
        public RoomData CurrentRoom { get; private set; }
        public string CurrentRoomCode { get; private set; }

        private readonly Dictionary<string, bool> readinessByPlayer = new();

        public event Action PlayersUpdated;
        public event Action<bool> StartAvailabilityChanged;

        private void Awake()
        {
            coinManager ??= FindObjectOfType<CoinManager>();
            photonManager ??= FindObjectOfType<PhotonManager>();
            EnsureDefaultRooms();
        }

        public RoomData CreateRoom(string roomName, int entryFee, int winReward)
        {
            RoomData room = new()
            {
                RoomId = Guid.NewGuid().ToString("N"),
                RoomName = roomName,
                EntryFee = Mathf.Max(0, entryFee),
                WinReward = Mathf.Max(0, winReward),
                CurrentPot = 0
            };

            AvailableRooms.Add(room);
            GameSignals.OnRoomDataChanged?.Invoke();
            return room;
        }

        public RoomData CreatePrivateRoom(string roomName, int entryFee, int winReward, string roomCode = null)
        {
            RoomData room = CreateRoom(roomName, entryFee, winReward);
            CurrentRoomCode = string.IsNullOrWhiteSpace(roomCode) ? GenerateRoomCode() : roomCode.Trim().ToUpperInvariant();
            photonManager?.CreatePrivateRoom(CurrentRoomCode, (byte)room.MaxPlayers);
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
            readinessByPlayer[playerId] = false;
            room.CurrentPot += room.EntryFee;
            CurrentRoom = room;
            CurrentRoomCode = CurrentRoomCode ?? GenerateRoomCode();

            GameSignals.OnRoomDataChanged?.Invoke();
            PlayersUpdated?.Invoke();
            NotifyStartState();
            return true;
        }

        public bool JoinRoomAsLocalPlayer(string roomId)
        {
            return JoinRoom(roomId, localPlayerId);
        }

        public bool JoinPrivateRoomByCode(string roomCode)
        {
            if (string.IsNullOrWhiteSpace(roomCode))
            {
                return false;
            }

            CurrentRoomCode = roomCode.Trim().ToUpperInvariant();
            photonManager?.JoinRoomByCode(CurrentRoomCode);
            return true;
        }

        public void SetReadyState(string playerId, bool isReady)
        {
            if (CurrentRoom == null || string.IsNullOrWhiteSpace(playerId) || !CurrentRoom.PlayerIds.Contains(playerId))
            {
                return;
            }

            readinessByPlayer[playerId] = isReady;
            PlayersUpdated?.Invoke();
            NotifyStartState();
        }

        public bool IsPlayerReady(string playerId)
        {
            return readinessByPlayer.TryGetValue(playerId, out bool ready) && ready;
        }

        public bool CanStartMatch(bool localPlayerIsHost)
        {
            if (!localPlayerIsHost || CurrentRoom == null || CurrentRoom.PlayerIds.Count < 2)
            {
                return false;
            }

            for (int i = 0; i < CurrentRoom.PlayerIds.Count; i++)
            {
                string id = CurrentRoom.PlayerIds[i];
                if (!IsPlayerReady(id))
                {
                    return false;
                }
            }

            return true;
        }

        public IReadOnlyList<string> GetPlayersInRoom()
        {
            return CurrentRoom?.PlayerIds;
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

        private void NotifyStartState()
        {
            StartAvailabilityChanged?.Invoke(CanStartMatch(photonManager == null || photonManager.IsHost));
        }

        private static string GenerateRoomCode()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            System.Text.StringBuilder sb = new(6);
            for (int i = 0; i < 6; i++)
            {
                sb.Append(chars[UnityEngine.Random.Range(0, chars.Length)]);
            }

            return sb.ToString();
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
