using System.Collections.Generic;
using LudoMaster.Signals;
using UnityEngine;

namespace LudoMaster.Managers
{
    /// <summary>
    /// Handles persistent coin economy (entry fee, rewards, and startup defaults).
    /// </summary>
    public class CoinManager : MonoBehaviour
    {
        [SerializeField] private int defaultStartingCoins = 2000;

        private readonly Dictionary<string, int> balances = new();
        private const string CoinPrefix = "coin_";

        public int GetBalance(string playerId)
        {
            if (!balances.TryGetValue(playerId, out int value))
            {
                value = PlayerPrefs.GetInt(CoinPrefix + playerId, defaultStartingCoins);
                balances[playerId] = value;
            }

            return value;
        }

        public bool TryPayEntryFee(string playerId, int fee)
        {
            int current = GetBalance(playerId);
            if (fee < 0 || current < fee) return false;
            SetBalance(playerId, current - fee);
            return true;
        }

        public void AddCoins(string playerId, int amount)
        {
            int current = GetBalance(playerId);
            SetBalance(playerId, current + Mathf.Max(0, amount));
        }

        public void NotifyBalance(string playerId)
        {
            GameSignals.OnCoinBalanceChanged?.Invoke(playerId, GetBalance(playerId));
        }

        private void Start()
        {
            NotifyBalance("P1");
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
