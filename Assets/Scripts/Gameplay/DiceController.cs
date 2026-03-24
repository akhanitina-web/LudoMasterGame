using System.Collections;
using LudoMaster.Signals;
using UnityEngine;
using UnityEngine.UI;

namespace LudoMaster.Gameplay
{
    /// <summary>
    /// Handles random dice rolling, visual animation, and SFX playback.
    /// </summary>
    public class DiceController : MonoBehaviour
    {
        [SerializeField] private Button diceButton;
        [SerializeField] private Animator diceAnimator;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip diceRollClip;
        [SerializeField] private float rollDuration = 0.8f;

        private bool isRolling;

        private void Awake()
        {
            if (diceButton != null)
            {
                diceButton.onClick.AddListener(RollDice);
            }
        }

        /// <summary>
        /// Public entry to roll dice values between 1 and 6.
        /// </summary>
        public void RollDice()
        {
            if (isRolling) return;
            StartCoroutine(RollRoutine());
        }

        private IEnumerator RollRoutine()
        {
            isRolling = true;

            if (diceAnimator != null)
            {
                diceAnimator.SetTrigger("Roll");
            }

            if (audioSource != null && diceRollClip != null)
            {
                audioSource.PlayOneShot(diceRollClip);
            }

            yield return new WaitForSeconds(rollDuration);

            int value = Random.Range(1, 7);
            GameSignals.OnDiceRolled?.Invoke(value);

            isRolling = false;
        }
    }
}
