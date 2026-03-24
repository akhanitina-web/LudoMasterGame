using System.Collections;
using LudoMaster.Signals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LudoMaster.UI
{
    /// <summary>
    /// Adds rolling animation and dice face text to a UI button.
    /// </summary>
    public class DiceVisualUI : MonoBehaviour
    {
        [SerializeField] private Button diceButton;
        [SerializeField] private TMP_Text faceText;
        [SerializeField] private TMP_Text valueText;
        [SerializeField] private RectTransform diceTransform;
        [SerializeField] private float rollingFlickerRate = 0.07f;
        [SerializeField] private float rollScale = 1.15f;

        private Coroutine rollingRoutine;
        private Vector3 defaultScale = Vector3.one;

        private void Awake()
        {
            if (diceTransform == null && diceButton != null)
            {
                diceTransform = diceButton.transform as RectTransform;
            }

            if (diceTransform != null)
            {
                defaultScale = diceTransform.localScale;
            }
        }

        private void OnEnable()
        {
            GameSignals.OnDiceRollingStateChanged += HandleRollingState;
            GameSignals.OnDiceRolled += HandleDiceRolled;
        }

        private void OnDisable()
        {
            GameSignals.OnDiceRollingStateChanged -= HandleRollingState;
            GameSignals.OnDiceRolled -= HandleDiceRolled;
        }

        private void HandleRollingState(bool isRolling)
        {
            if (isRolling)
            {
                if (rollingRoutine == null)
                {
                    rollingRoutine = StartCoroutine(RollingVisualRoutine());
                }
            }
            else
            {
                if (rollingRoutine != null)
                {
                    StopCoroutine(rollingRoutine);
                    rollingRoutine = null;
                }

                if (diceTransform != null)
                {
                    diceTransform.localScale = defaultScale;
                    diceTransform.localRotation = Quaternion.identity;
                }
            }
        }

        private IEnumerator RollingVisualRoutine()
        {
            while (true)
            {
                if (faceText != null)
                {
                    int randomValue = Random.Range(1, 7);
                    faceText.text = ToFaceGlyph(randomValue);
                    if (valueText != null)
                    {
                        valueText.text = randomValue.ToString();
                    }
                }

                if (diceTransform != null)
                {
                    diceTransform.localScale = defaultScale * rollScale;
                    diceTransform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-18f, 18f));
                }

                yield return new WaitForSeconds(rollingFlickerRate);
            }
        }

        private void HandleDiceRolled(int value)
        {
            if (faceText != null)
            {
                faceText.text = ToFaceGlyph(value);
            }

            if (valueText != null)
            {
                valueText.text = value.ToString();
            }
        }

        private static string ToFaceGlyph(int value)
        {
            return value switch
            {
                1 => "⚀",
                2 => "⚁",
                3 => "⚂",
                4 => "⚃",
                5 => "⚄",
                6 => "⚅",
                _ => "⚀"
            };
        }
    }
}
