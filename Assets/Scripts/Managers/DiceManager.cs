using System.Collections;
using LudoMaster.Signals;
using UnityEngine;
using UnityEngine.UI;

namespace LudoMaster.Managers
{
    /// <summary>
    /// Dedicated dice orchestration manager with animation timing and roll events.
    /// </summary>
    public class DiceManager : MonoBehaviour
    {
        [SerializeField] private Button diceButton;
        [SerializeField] private Animator diceAnimator;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip diceRollClip;
        [SerializeField] private float rollDuration = 0.8f;

        public bool IsRolling { get; private set; }

        private void Awake()
        {
            if (diceButton == null)
            {
                diceButton = GetComponent<Button>();
            }

            BindButton();
        }

        public void RollDice()
        {
            if (IsRolling)
            {
                return;
            }

            StartCoroutine(RollRoutine());
        }

        private IEnumerator RollRoutine()
        {
            IsRolling = true;
            if (diceButton != null)
            {
                diceButton.interactable = false;
            }

            GameSignals.OnDiceRollingStateChanged?.Invoke(true);

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

            IsRolling = false;
            if (diceButton != null)
            {
                diceButton.interactable = true;
            }

            GameSignals.OnDiceRollingStateChanged?.Invoke(false);
        }

        public void Configure(Button button)
        {
            if (diceButton != null)
            {
                diceButton.onClick.RemoveListener(RollDice);
            }

            diceButton = button;
            BindButton();
        }

        private void BindButton()
        {
            if (diceButton != null)
            {
                diceButton.onClick.RemoveListener(RollDice);
                diceButton.onClick.AddListener(RollDice);
            }
        }
    }
}
