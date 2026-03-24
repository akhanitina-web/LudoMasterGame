using System.Text;
using LudoMaster.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LudoMaster.UI
{
    /// <summary>
    /// Multiplayer lobby UI: room list, room-code join, ready toggle, and host-only start control.
    /// </summary>
    public class RoomSelectionUI : MonoBehaviour
    {
        [SerializeField] private RoomManager roomManager;
        [SerializeField] private PhotonManager photonManager;

        [Header("Texts")]
        [SerializeField] private TMP_InputField roomCodeInput;
        [SerializeField] private TMP_Text roomCodeText;
        [SerializeField] private TMP_Text roomListText;
        [SerializeField] private TMP_Text playerListText;
        [SerializeField] private TMP_Text statusText;

        [Header("Buttons")]
        [SerializeField] private Button joinByCodeButton;
        [SerializeField] private Button readyButton;
        [SerializeField] private Button startGameButton;

        [SerializeField] private string localPlayerId = "P1";
        private bool localReady;

        private void Awake()
        {
            if (joinByCodeButton != null) joinByCodeButton.onClick.AddListener(JoinByRoomCode);
            if (readyButton != null) readyButton.onClick.AddListener(ToggleReady);
            if (startGameButton != null) startGameButton.onClick.AddListener(HostStartGame);

            roomManager ??= FindObjectOfType<RoomManager>();
            photonManager ??= FindObjectOfType<PhotonManager>();

            if (roomManager != null)
            {
                roomManager.PlayersUpdated += RefreshLobbyState;
                roomManager.StartAvailabilityChanged += canStart => { if (startGameButton != null) startGameButton.interactable = canStart; };
            }
        }

        private void OnEnable()
        {
            LudoMaster.Signals.GameSignals.OnRoomDataChanged += RefreshLobbyState;
            RefreshLobbyState();
        }

        private void OnDisable()
        {
            LudoMaster.Signals.GameSignals.OnRoomDataChanged -= RefreshLobbyState;
            if (roomManager != null)
            {
                roomManager.PlayersUpdated -= RefreshLobbyState;
            }
        }

        private void JoinByRoomCode()
        {
            if (roomManager == null)
            {
                return;
            }

            string roomCode = roomCodeInput == null ? string.Empty : roomCodeInput.text.Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(roomCode))
            {
                statusText?.SetText("Enter room code.");
                return;
            }

            roomManager.JoinPrivateRoomByCode(roomCode);
            statusText?.SetText($"Joining room {roomCode}...");
        }

        private void ToggleReady()
        {
            if (roomManager == null || roomManager.CurrentRoom == null)
            {
                statusText?.SetText("Join a room first.");
                return;
            }

            localReady = !localReady;
            roomManager.SetReadyState(localPlayerId, localReady);
            statusText?.SetText(localReady ? "You are READY" : "You are NOT READY");
            UpdateReadyButtonLabel();
        }

        private void HostStartGame()
        {
            if (roomManager == null)
            {
                return;
            }

            bool canStart = roomManager.CanStartMatch(photonManager == null || photonManager.IsHost);
            statusText?.SetText(canStart ? "Match starting..." : "Only host can start when all players are ready.");
        }

        private void UpdateReadyButtonLabel()
        {
            if (readyButton == null)
            {
                return;
            }

            TMP_Text readyText = readyButton.GetComponentInChildren<TMP_Text>();
            if (readyText != null)
            {
                readyText.text = localReady ? "Unready" : "Ready";
            }
        }

        private void RefreshLobbyState()
        {
            RefreshRoomList();
            RefreshRoomCode();
            RefreshPlayerList();
        }

        private void RefreshRoomCode()
        {
            if (roomCodeText == null || roomManager == null)
            {
                return;
            }

            roomCodeText.text = string.IsNullOrEmpty(roomManager.CurrentRoomCode)
                ? "Room Code: ----"
                : $"Room Code: {roomManager.CurrentRoomCode}";
        }

        private void RefreshPlayerList()
        {
            if (playerListText == null || roomManager == null || roomManager.CurrentRoom == null)
            {
                return;
            }

            StringBuilder sb = new();
            var players = roomManager.GetPlayersInRoom();
            if (players == null)
            {
                playerListText.text = "";
                return;
            }

            for (int i = 0; i < players.Count; i++)
            {
                string id = players[i];
                string ready = roomManager.IsPlayerReady(id) ? "Ready" : "Not Ready";
                sb.AppendLine($"{id} - {ready}");
            }

            playerListText.text = sb.ToString();
        }

        private void RefreshRoomList()
        {
            if (roomListText == null || roomManager == null)
            {
                return;
            }

            if (roomManager.AvailableRooms.Count == 0)
            {
                roomListText.text = "No rooms available yet.";
                return;
            }

            StringBuilder sb = new();
            for (int i = 0; i < roomManager.AvailableRooms.Count; i++)
            {
                var room = roomManager.AvailableRooms[i];
                sb.AppendLine($"{room.RoomName} | Entry:{room.EntryFee} | Pot:{room.CurrentPot} | {room.PlayerIds.Count}/{room.MaxPlayers}");
            }

            roomListText.text = sb.ToString();
        }
    }
}
