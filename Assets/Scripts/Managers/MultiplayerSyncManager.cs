using UnityEngine;

namespace LudoMaster.Managers
{
    /// <summary>
    /// Network sync abstraction layer.
    /// Swap internals with Photon/FishNet/Netcode implementation.
    /// </summary>
    public class MultiplayerSyncManager : MonoBehaviour
    {
        /// <summary>
        /// Raised when remote move packet is received.
        /// </summary>
        public System.Action<string, int, int> OnRemoteMoveReceived;

        /// <summary>
        /// Broadcasts local move to room participants.
        /// </summary>
        public void BroadcastMove(string playerId, int tokenId, int diceValue)
        {
            // Integration point: send via selected networking SDK transport.
            Debug.Log($"[Sync] Send Move -> Player:{playerId} Token:{tokenId} Dice:{diceValue}");
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
