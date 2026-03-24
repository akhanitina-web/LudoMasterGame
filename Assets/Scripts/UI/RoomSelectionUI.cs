using LudoMaster.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LudoMaster.UI
{
    /// <summary>
    /// Handles room creation, room join, and room list refresh actions.
    /// </summary>
    public class RoomSelectionUI : MonoBehaviour
    {
        [SerializeField] private RoomManager roomManager;
        [SerializeField] private TMP_InputField roomNameInput;
        [SerializeField] private TMP_Text roomListText;
        [SerializeField] private Button createRoomButton;

        private void Awake()
        {
            if (createRoomButton != null)
            {
                createRoomButton.onClick.AddListener(CreateDefaultRoom);
            }
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

        /// <summary>
        /// Creates a basic room option (entry 50, reward 200) for quick matchmaking.
        /// </summary>
        public void CreateDefaultRoom()
        {
            string roomName = string.IsNullOrWhiteSpace(roomNameInput?.text) ? "Classic Room" : roomNameInput.text;
            roomManager.CreateRoom(roomName, 50, 200);
        }

        private void RefreshRoomList()
        {
            if (roomListText == null || roomManager == null) return;

            if (roomManager.AvailableRooms.Count == 0)
            {
                roomListText.text = "No rooms available.";
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
