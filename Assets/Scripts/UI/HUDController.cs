using LudoMaster.Core;
using LudoMaster.Signals;
using TMPro;
using UnityEngine;

namespace LudoMaster.UI
{
    /// <summary>
    /// In-game HUD updates turn indicators and rank announcements.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [SerializeField] private TMP_Text turnText;
        [SerializeField] private TMP_Text resultText;

        private void OnEnable()
        {
            GameSignals.OnTurnChanged += HandleTurnChanged;
            GameSignals.OnPlayerRankAssigned += HandleRankAssigned;
            GameSignals.OnMatchStateChanged += HandleMatchStateChanged;
        }

        private void OnDisable()
        {
            GameSignals.OnTurnChanged -= HandleTurnChanged;
            GameSignals.OnPlayerRankAssigned -= HandleRankAssigned;
            GameSignals.OnMatchStateChanged -= HandleMatchStateChanged;
        }

        private void HandleTurnChanged(PlayerColor color)
        {
            if (turnText != null)
            {
                turnText.text = $"Turn: {color}";
            }
        }

        private void HandleRankAssigned(PlayerColor color, int rank)
        {
            if (resultText != null)
            {
                resultText.text = $"{color} finished at rank #{rank}";
            }
        }

        private void HandleMatchStateChanged(MatchState state)
        {
            if (state == MatchState.Completed && resultText != null)
            {
                resultText.text += "\nMatch Complete";
            }
        }
    }
}
