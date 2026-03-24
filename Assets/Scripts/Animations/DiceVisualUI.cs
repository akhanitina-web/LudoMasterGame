using System.Collections;
using LudoMaster.Signals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LudoMaster.UI
{
    /// <summary>
    /// Adds rolling animation and richer dice face rendering for values 1-6.
    /// </summary>
    public class DiceVisualUI : MonoBehaviour
    {
        [SerializeField] private Button diceButton;
        [SerializeField] private TMP_Text faceText;
        [SerializeField] private TMP_Text valueText;
        [SerializeField] private Image faceImage;
        [SerializeField] private RectTransform diceTransform;
        [SerializeField] private Image diceBackground;
        [SerializeField] private float rollingFlickerRate = 0.06f;
        [SerializeField] private float rollScale = 1.15f;
        [SerializeField] private float settleDuration = 0.2f;
        [SerializeField] private Color restingBackgroundColor = new(1f, 1f, 1f, 0.97f);
        [SerializeField] private Color rollingBackgroundColor = new(0.93f, 0.96f, 1f, 1f);

        private Coroutine rollingRoutine;
        private Coroutine settleRoutine;
        private Vector3 defaultScale = Vector3.one;

        private void Awake()
        {
            if (diceTransform == null && diceButton != null)
            {
                diceTransform = diceButton.transform as RectTransform;
            }

            if (diceBackground == null && diceButton != null)
            {
                diceBackground = diceButton.GetComponent<Image>();
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
                if (settleRoutine != null)
                {
                    StopCoroutine(settleRoutine);
                    settleRoutine = null;
                }

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

                if (diceBackground != null)
                {
                    diceBackground.color = restingBackgroundColor;
                }
            }
        }

        private IEnumerator RollingVisualRoutine()
        {
            while (true)
            {
                int randomValue = Random.Range(1, 7);
                if (faceText != null)
                {
                    faceText.text = ToPipLayout(randomValue);
                }

                if (faceImage != null)
                {
                    faceImage.sprite = LudoSpriteFactory.GetDiceFaceSprite(randomValue);
                }

                if (valueText != null)
                {
                    valueText.text = randomValue.ToString();
                }

                if (diceTransform != null)
                {
                    diceTransform.localScale = defaultScale * Random.Range(1f, rollScale);
                    diceTransform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-18f, 18f));
                }

                if (diceBackground != null)
                {
                    diceBackground.color = Color.Lerp(rollingBackgroundColor, restingBackgroundColor, Random.value * 0.25f);
                }

                yield return new WaitForSeconds(rollingFlickerRate);
            }
        }

        private void HandleDiceRolled(int value)
        {
            if (faceText != null)
            {
                faceText.text = ToPipLayout(value);
            }

            if (faceImage != null)
            {
                faceImage.sprite = LudoSpriteFactory.GetDiceFaceSprite(value);
            }

            if (valueText != null)
            {
                valueText.text = value.ToString();
            }

            if (diceTransform != null)
            {
                if (settleRoutine != null)
                {
                    StopCoroutine(settleRoutine);
                }

                settleRoutine = StartCoroutine(SettleRoutine());
            }
        }

        private IEnumerator SettleRoutine()
        {
            float elapsed = 0f;
            Vector3 startScale = diceTransform.localScale;
            Quaternion startRotation = diceTransform.localRotation;
            while (elapsed < settleDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / settleDuration);
                diceTransform.localScale = Vector3.Lerp(startScale, defaultScale, t);
                diceTransform.localRotation = Quaternion.Slerp(startRotation, Quaternion.identity, t);
                yield return null;
            }

            diceTransform.localScale = defaultScale;
            diceTransform.localRotation = Quaternion.identity;
            settleRoutine = null;
        }

        private static string ToPipLayout(int value)
        {
            return value switch
            {
                1 => "· · ·\n· ● ·\n· · ·",
                2 => "● · ·\n· · ·\n· · ●",
                3 => "● · ·\n· ● ·\n· · ●",
                4 => "● · ●\n· · ·\n● · ●",
                5 => "● · ●\n· ● ·\n● · ●",
                6 => "● · ●\n● · ●\n● · ●",
                _ => "· · ·\n· ● ·\n· · ·"
            };
        }
    }
}
