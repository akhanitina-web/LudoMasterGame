using System.Collections;
using LudoMaster.Core;
using LudoMaster.Signals;
using TMPro;
using UnityEngine;

namespace LudoMaster.UI
{
    /// <summary>
    /// Lightweight win celebration pulse used when match completes.
    /// </summary>
    public class WinCelebrationUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private float pulseScale = 1.08f;
        [SerializeField] private float pulseSpeed = 2.6f;

        private Coroutine pulseRoutine;

        private void OnEnable()
        {
            GameSignals.OnMatchStateChanged += HandleMatchStateChanged;
            GameSignals.OnPlayerRankAssigned += HandleRankAssigned;
        }

        private void OnDisable()
        {
            GameSignals.OnMatchStateChanged -= HandleMatchStateChanged;
            GameSignals.OnPlayerRankAssigned -= HandleRankAssigned;
        }

        private void HandleRankAssigned(PlayerColor color, int rank)
        {
            if (rank == 1 && label != null)
            {
                label.text = $"{color} Wins!";
            }
        }

        private void HandleMatchStateChanged(MatchState state)
        {
            if (state == MatchState.Completed)
            {
                if (pulseRoutine == null)
                {
                    pulseRoutine = StartCoroutine(PulseRoutine());
                }
            }
        }

        private IEnumerator PulseRoutine()
        {
            Vector3 baseScale = transform.localScale;
            while (true)
            {
                float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
                transform.localScale = Vector3.LerpUnclamped(baseScale, baseScale * pulseScale, t);
                if (label != null)
                {
                    label.color = new Color(1f, 0.86f, 0.2f, Mathf.Lerp(0.55f, 1f, t));
                }

                yield return null;
            }
        }
    }
}
