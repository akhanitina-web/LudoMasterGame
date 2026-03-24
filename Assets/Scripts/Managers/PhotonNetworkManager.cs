using UnityEngine;

#if PHOTON_UNITY_NETWORKING
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
#endif

namespace LudoMaster.Managers
{
    /// <summary>
    /// Photon room + move-sync bridge. Implements INetworkAdapter used by gameplay systems.
    /// </summary>
    public class PhotonNetworkManager : MonoBehaviour, INetworkAdapter
#if PHOTON_UNITY_NETWORKING
        , IOnEventCallback
#endif
    {
        [SerializeField] private string gameVersion = "1.0";
        [SerializeField] private byte moveEventCode = 11;
        [SerializeField] private byte diceEventCode = 12;

        public System.Action<int> OnRemoteDiceRolled;

        private MultiplayerSyncManager multiplayerSyncManager;

        private void Awake()
        {
            multiplayerSyncManager = FindObjectOfType<MultiplayerSyncManager>();
            if (multiplayerSyncManager != null)
            {
                multiplayerSyncManager.Adapter = this;
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

        public void Connect()
        {
#if PHOTON_UNITY_NETWORKING
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();
#else
            Debug.Log("[PhotonNetworkManager] Photon PUN2 not installed. Running local mode.");
#endif
        }

        public void CreateOrJoinRoom(string roomName, byte maxPlayers = 4)
        {
#if PHOTON_UNITY_NETWORKING
            if (!PhotonNetwork.IsConnectedAndReady)
            {
                Connect();
                return;
            }

            RoomOptions options = new RoomOptions { MaxPlayers = maxPlayers };
            PhotonNetwork.JoinOrCreateRoom(roomName, options, TypedLobby.Default);
#else
            Debug.Log($"[PhotonNetworkManager] CreateOrJoinRoom simulated for {roomName}.");
#endif
        }

        public void SendMove(string playerId, int tokenId, int diceValue)
        {
#if PHOTON_UNITY_NETWORKING
            object[] content = { playerId, tokenId, diceValue };
            RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
            PhotonNetwork.RaiseEvent(moveEventCode, content, options, SendOptions.SendReliable);
#else
            multiplayerSyncManager?.ReceiveRemoteMove(playerId, tokenId, diceValue);
#endif
        }

        public void SendDiceRoll(int value)
        {
#if PHOTON_UNITY_NETWORKING
            RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
            PhotonNetwork.RaiseEvent(diceEventCode, value, options, SendOptions.SendReliable);
#else
            OnRemoteDiceRolled?.Invoke(value);
#endif
        }

#if PHOTON_UNITY_NETWORKING
        public void OnEvent(EventData photonEvent)
        {
            if (photonEvent.Code == moveEventCode)
            {
                object[] data = (object[])photonEvent.CustomData;
                string playerId = data[0] as string;
                int tokenId = (int)data[1];
                int diceValue = (int)data[2];
                multiplayerSyncManager?.ReceiveRemoteMove(playerId, tokenId, diceValue);
            }
            else if (photonEvent.Code == diceEventCode)
            {
                int value = (int)photonEvent.CustomData;
                OnRemoteDiceRolled?.Invoke(value);
            }
        }
#endif
    }
}
