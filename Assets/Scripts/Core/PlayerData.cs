using System.Collections.Generic;

namespace LudoMaster.Core
{
    /// <summary>
    /// Runtime player model including tokens, placement, and coin identity data.
    /// </summary>
    [System.Serializable]
    public class PlayerData
    {
        public string PlayerId;
        public string DisplayName;
        public PlayerColor Color;
        public bool IsBot;
        public bool IsLocalPlayer;

        public readonly List<CoreTokenData> Tokens = new();

        public int Placement = -1;

        /// <summary>
        /// Returns true once all 4 tokens are marked finished.
        /// </summary>
        public bool HasFinishedAllTokens()
        {
            int finishedCount = 0;
            for (int i = 0; i < Tokens.Count; i++)
            {
                if (Tokens[i].State == TokenState.Finished)
                {
                    finishedCount++;
                }
            }

            return finishedCount >= 4;
        }
    }

    /// <summary>
    /// Serializable token state container decoupled from scene GameObjects.
    /// </summary>
    [System.Serializable]
    public class CoreTokenData
    {
        public int TokenId;
        public int StepsMoved;
        public int BoardIndex = -1;
        public int HomePathIndex = -1;
        public TokenState State = TokenState.InBase;
    }
}
