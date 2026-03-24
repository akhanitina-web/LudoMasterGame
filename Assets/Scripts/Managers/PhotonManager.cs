using System;
using UnityEngine;

#if PHOTON_UNITY_NETWORKING
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
#endif

namespace LudoMaster.Managers
{
    /// <summary>
    /// Photon PUN 2 connection and room transport for dice/turn/token synchronization.
    /// Works in local simulated mode when Photon is not installed.
    /// </summary>
    public class PhotonManager : MonoBehaviour
#if PHOTON_UNITY_NETWORKING
        , IConnectionCallbacks, IMatchmakingCallbacks, IInRoomCallbacks, IOnEventCallback
#endif
    {
        [SerializeField] private string gameVersion = "1.0";
        [SerializeField] private byte diceEventCode = 21;
        [SerializeField] private byte moveEventCode = 22;
        [SerializeField] private byte turnEventCode = 23;

        public event Action ConnectedToLobby;
        public event Action<string> JoinedRoom;
        public event Action<int> RemoteDiceRolled;
        public event Action<string, int, int> RemoteTokenMoved;
        public event Action<int> RemoteTurnChanged;

        public bool IsConnected
        {
            get
            {
#if PHOTON_UNITY_NETWORKING
                return PhotonNetwork.IsConnectedAndReady;
#else
                return true;
#endif
            }
        }

        public bool IsHost
        {
            get
            {
#if PHOTON_UNITY_NETWORKING
                return PhotonNetwork.IsMasterClient;
#else
                return true;
#endif
            }
        }

        private void OnEnable()
        {
#if PHOTON_UNITY_NETWORKING
            PhotonNetwork.AddCallbackTarget(this);
#endif
        }

        private void OnDisable()
        {
#if PHOTON_UNITY_NETWORKING
            PhotonNetwork.RemoveCallbackTarget(this);
#endif
        }

        public void ConnectToServer()
        {
#if PHOTON_UNITY_NETWORKING
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.AutomaticallySyncScene = true;
            if (!PhotonNetwork.IsConnected)
            {
                PhotonNetwork.ConnectUsingSettings();
                return;
            }

            PhotonNetwork.JoinLobby();
#else
            Debug.Log("[PhotonManager] Photon unavailable, running in local simulation mode.");
            ConnectedToLobby?.Invoke();
#endif
        }

        public void CreatePublicRoom(string roomName, byte maxPlayers = 4)
        {
#if PHOTON_UNITY_NETWORKING
            RoomOptions options = new() { MaxPlayers = maxPlayers, IsVisible = true, IsOpen = true };
            PhotonNetwork.CreateRoom(roomName, options, TypedLobby.Default);
#else
            JoinedRoom?.Invoke(roomName);
#endif
        }

        public void CreatePrivateRoom(string roomCode, byte maxPlayers = 4)
        {
#if PHOTON_UNITY_NETWORKING
            RoomOptions options = new() { MaxPlayers = maxPlayers, IsVisible = false, IsOpen = true };
            PhotonNetwork.CreateRoom(roomCode, options, TypedLobby.Default);
#else
            JoinedRoom?.Invoke(roomCode);
#endif
        }

        public void JoinRoomByCode(string roomCode)
        {
#if PHOTON_UNITY_NETWORKING
            PhotonNetwork.JoinRoom(roomCode);
#else
            JoinedRoom?.Invoke(roomCode);
#endif
        }

        public void SendDiceRoll(int diceValue)
        {
#if PHOTON_UNITY_NETWORKING
            PhotonNetwork.RaiseEvent(diceEventCode, diceValue,
                new RaiseEventOptions { Receivers = ReceiverGroup.Others }, SendOptions.SendReliable);
#else
            RemoteDiceRolled?.Invoke(diceValue);
#endif
        }

        public void SendTokenMove(string playerId, int tokenId, int diceValue)
        {
#if PHOTON_UNITY_NETWORKING
            object[] payload = { playerId, tokenId, diceValue };
            PhotonNetwork.RaiseEvent(moveEventCode, payload,
                new RaiseEventOptions { Receivers = ReceiverGroup.Others }, SendOptions.SendReliable);
#else
            RemoteTokenMoved?.Invoke(playerId, tokenId, diceValue);
#endif
        }

        public void SendTurnChange(int playerIndex)
        {
#if PHOTON_UNITY_NETWORKING
            PhotonNetwork.RaiseEvent(turnEventCode, playerIndex,
                new RaiseEventOptions { Receivers = ReceiverGroup.Others }, SendOptions.SendReliable);
#else
            RemoteTurnChanged?.Invoke(playerIndex);
#endif
        }

#if PHOTON_UNITY_NETWORKING
        public void OnConnected() { }
        public void OnConnectedToMaster() => PhotonNetwork.JoinLobby();
        public void OnDisconnected(DisconnectCause cause) => Debug.LogWarning($"Photon disconnected: {cause}");
        public void OnRegionListReceived(RegionHandler regionHandler) { }
        public void OnCustomAuthenticationResponse(System.Collections.Generic.Dictionary<string, object> data) { }
        public void OnCustomAuthenticationFailed(string debugMessage) => Debug.LogWarning(debugMessage);
        public void OnFriendListUpdate(System.Collections.Generic.List<FriendInfo> friendList) { }
        public void OnCreatedRoom() { }
        public void OnCreateRoomFailed(short returnCode, string message) => Debug.LogWarning(message);
        public void OnJoinRandomFailed(short returnCode, string message) => Debug.LogWarning(message);
        public void OnJoinRoomFailed(short returnCode, string message) => Debug.LogWarning(message);
        public void OnLeftRoom() { }
        public void OnLeftLobby() { }
        public void OnPlayerEnteredRoom(Player newPlayer) { }
        public void OnPlayerLeftRoom(Player otherPlayer) { }
        public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged) { }
        public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps) { }
        public void OnMasterClientSwitched(Player newMasterClient) { }
        public void OnJoinedLobby() => ConnectedToLobby?.Invoke();
        public void OnJoinedRoom() => JoinedRoom?.Invoke(PhotonNetwork.CurrentRoom?.Name ?? "Room");

        public void OnEvent(EventData photonEvent)
        {
            if (photonEvent.Code == diceEventCode)
            {
                RemoteDiceRolled?.Invoke((int)photonEvent.CustomData);
            }
            else if (photonEvent.Code == moveEventCode)
            {
                object[] data = (object[])photonEvent.CustomData;
                RemoteTokenMoved?.Invoke((string)data[0], (int)data[1], (int)data[2]);
            }
            else if (photonEvent.Code == turnEventCode)
            {
                RemoteTurnChanged?.Invoke((int)photonEvent.CustomData);
            }
        }
#endif
    }
}
