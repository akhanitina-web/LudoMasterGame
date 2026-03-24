namespace LudoMaster.Managers
{
    /// <summary>
    /// Multiplayer transport abstraction used by MultiplayerSyncManager.
    /// Photon can implement this interface later without changing gameplay logic.
    /// </summary>
    public interface INetworkAdapter
    {
        void SendMove(string playerId, int tokenId, int diceValue);
    }
}
