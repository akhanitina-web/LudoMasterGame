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

        [Header("Room Buttons")]
        [SerializeField] private Button lowCoinRoomButton;
        [SerializeField] private Button mediumCoinRoomButton;
        [SerializeField] private Button highCoinRoomButton;
        [SerializeField] private Button privateRoomButton;

        private void Awake()
        {
            if (lowCoinRoomButton != null) lowCoinRoomButton.onClick.AddListener(CreateLowCoinRoom);
            if (mediumCoinRoomButton != null) mediumCoinRoomButton.onClick.AddListener(CreateMediumCoinRoom);
            if (highCoinRoomButton != null) highCoinRoomButton.onClick.AddListener(CreateHighCoinRoom);
            if (privateRoomButton != null) privateRoomButton.onClick.AddListener(CreatePrivateRoom);
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

        public void CreateLowCoinRoom() => CreateRoom("Low Coin Room", 25, 100);
        public void CreateMediumCoinRoom() => CreateRoom("Medium Coin Room", 100, 400);
        public void CreateHighCoinRoom() => CreateRoom("High Coin Room", 500, 2400);

        public void CreatePrivateRoom()
        {
            string custom = string.IsNullOrWhiteSpace(roomNameInput?.text) ? "Private Room" : roomNameInput.text.Trim();
            CreateRoom(custom, 200, 1000);
        }

        private void CreateRoom(string defaultName, int fee, int reward)
        {
            if (roomManager == null) return;
            string custom = roomNameInput != null && !string.IsNullOrWhiteSpace(roomNameInput.text) ? roomNameInput.text.Trim() : defaultName;
            roomManager.CreateRoom(custom, fee, reward);
        }

        private void RefreshRoomList()
        {
            if (roomListText == null || roomManager == null) return;

            if (roomManager.AvailableRooms.Count == 0)
            {
                roomListText.text = "No rooms available yet. Create one to start.";
                return;
            }

            System.Text.StringBuilder sb = new();
            for (int i = 0; i < roomManager.AvailableRooms.Count; i++)
            {
                var room = roomManager.AvailableRooms[i];
                sb.AppendLine($"{room.RoomName} | Fee:{room.EntryFee} | Reward:{room.WinReward} | {room.PlayerIds.Count}/{room.MaxPlayers}");
            }

            roomListText.text = sb.ToString();
        }
    }
}
