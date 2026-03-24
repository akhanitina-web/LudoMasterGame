using LudoMaster.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LudoMaster.UI
{
    /// <summary>
    /// Handles lobby room options and quick joins for different coin tiers.
    /// </summary>
    public class RoomSelectionUI : MonoBehaviour
    {
        [SerializeField] private RoomManager roomManager;
        [SerializeField] private TMP_InputField roomNameInput;
        [SerializeField] private TMP_Text roomListText;
        [SerializeField] private TMP_Text statusText;

        [Header("Room Buttons")]
        [SerializeField] private Button lowCoinRoomButton;
        [SerializeField] private Button mediumCoinRoomButton;
        [SerializeField] private Button highCoinRoomButton;
        [SerializeField] private Button privateRoomButton;

        private void Awake()
        {
            if (lowCoinRoomButton != null) lowCoinRoomButton.onClick.AddListener(() => JoinByName("Low Coin Room"));
            if (mediumCoinRoomButton != null) mediumCoinRoomButton.onClick.AddListener(() => JoinByName("Medium Coin Room"));
            if (highCoinRoomButton != null) highCoinRoomButton.onClick.AddListener(() => JoinByName("High Coin Room"));
            if (privateRoomButton != null) privateRoomButton.onClick.AddListener(JoinPrivateRoom);
            RefreshButtonLabels();
        }

        private void OnEnable()
        {
            LudoMaster.Signals.GameSignals.OnRoomDataChanged += RefreshRoomList;
            RefreshRoomList();
        }

        private void OnDisable()
        {
            LudoMaster.Signals.GameSignals.OnRoomDataChanged -= RefreshRoomList;
        }

        private void JoinByName(string roomName)
        {
            if (roomManager == null)
            {
                return;
            }

            var room = roomManager.AvailableRooms.Find(r => r.RoomName == roomName);
            if (room == null)
            {
                statusText?.SetText("Room not found.");
                return;
            }

            bool success = roomManager.JoinRoomAsLocalPlayer(room.RoomId);
            statusText?.SetText(success ? $"Joined {room.RoomName}" : $"Not enough coins for {room.RoomName}");
        }

        private void JoinPrivateRoom()
        {
            if (roomManager == null)
            {
                return;
            }

            string requestedName = string.IsNullOrWhiteSpace(roomNameInput?.text) ? "Private Room" : roomNameInput.text.Trim();
            var room = roomManager.AvailableRooms.Find(r => r.RoomName == requestedName) ?? roomManager.CreateRoom(requestedName, 500, 2000);
            bool success = roomManager.JoinRoomAsLocalPlayer(room.RoomId);
            statusText?.SetText(success ? $"Joined {room.RoomName}" : "Unable to join private room.");
        }

        private void RefreshButtonLabels()
        {
            SetButtonLabel(lowCoinRoomButton, "Low Coin Room (Entry: 100)");
            SetButtonLabel(mediumCoinRoomButton, "Medium Coin Room (Entry: 300)");
            SetButtonLabel(highCoinRoomButton, "High Coin Room (Entry: 700)");
            SetButtonLabel(privateRoomButton, "Private Room (Entry: 500)");
        }

        private static void SetButtonLabel(Button button, string label)
        {
            if (button == null)
            {
                return;
            }

            TMP_Text text = button.GetComponentInChildren<TMP_Text>();
            if (text != null)
            {
                text.text = label;
            }
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

            System.Text.StringBuilder sb = new();
            for (int i = 0; i < roomManager.AvailableRooms.Count; i++)
            {
                var room = roomManager.AvailableRooms[i];
                sb.AppendLine($"{room.RoomName} | Entry:{room.EntryFee} | Pot:{room.CurrentPot} | {room.PlayerIds.Count}/{room.MaxPlayers}");
            }

            roomListText.text = sb.ToString();
        }
    }
}
