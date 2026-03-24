using System.Collections.Generic;
using LudoMaster.Signals;
using UnityEngine;

namespace LudoMaster.Managers
{
    /// <summary>
    /// Handles persistent coin economy (entry fee, rewards, and ad rewards).
    /// </summary>
    public class CoinManager : MonoBehaviour
    {
        private readonly Dictionary<string, int> balances = new();

        private const string CoinPrefix = "coin_";

        /// <summary>
        /// Reads player coin balance from persistent storage.
        /// </summary>
        public int GetBalance(string playerId)
        {
            if (!balances.TryGetValue(playerId, out int value))
            {
                value = PlayerPrefs.GetInt(CoinPrefix + playerId, 1000);
                balances[playerId] = value;
            }

            return value;
        }

        /// <summary>
        /// Charges room entry fee if sufficient balance exists.
        /// </summary>
        public bool TryPayEntryFee(string playerId, int fee)
        {
            int current = GetBalance(playerId);
            if (current < fee) return false;

            SetBalance(playerId, current - fee);
            return true;
        }

        /// <summary>
        /// Rewards coins (for match win or rewarded ads).
        /// </summary>
        public void AddCoins(string playerId, int amount)
        {
            int current = GetBalance(playerId);
            SetBalance(playerId, current + Mathf.Max(0, amount));
        }

        private void SetBalance(string playerId, int amount)
        {
            balances[playerId] = amount;
            PlayerPrefs.SetInt(CoinPrefix + playerId, amount);
            PlayerPrefs.Save();
            GameSignals.OnCoinBalanceChanged?.Invoke(playerId, amount);
        }
    }
}
