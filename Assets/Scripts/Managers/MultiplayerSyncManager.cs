using UnityEngine;

namespace LudoMaster.Managers
{
    /// <summary>
    /// Network sync abstraction layer prepared for future Photon integration.
    /// </summary>
    public class MultiplayerSyncManager : MonoBehaviour
    {
        [SerializeField] private bool useLocalLoopback = true;

        /// <summary>
        /// Adapter implementation can be assigned by networking bootstrap (Photon, NGO, etc.).
        /// </summary>
        public INetworkAdapter Adapter { get; set; }

        /// <summary>
        /// Raised when remote move packet is received.
        /// </summary>
        public System.Action<string, int, int> OnRemoteMoveReceived;

        /// <summary>
        /// Broadcasts local move to room participants.
        /// </summary>
        public void BroadcastMove(string playerId, int tokenId, int diceValue)
        {
            if (Adapter != null)
            {
                Adapter.SendMove(playerId, tokenId, diceValue);
                return;
            }

            Debug.Log($"[Sync] No adapter registered. Move queued for local mode -> Player:{playerId} Token:{tokenId} Dice:{diceValue}");

            if (useLocalLoopback)
            {
                ReceiveRemoteMove(playerId, tokenId, diceValue);
            }
        }

        /// <summary>
        /// Call from network callback to apply remote player action.
        /// </summary>
        public void ReceiveRemoteMove(string playerId, int tokenId, int diceValue)
        {
            OnRemoteMoveReceived?.Invoke(playerId, tokenId, diceValue);
        }
    }
}
