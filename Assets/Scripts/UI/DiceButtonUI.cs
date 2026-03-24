using LudoMaster.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace LudoMaster.UI
{
    /// <summary>
    /// Dedicated UI bridge for dice button interactions.
    /// </summary>
    public class DiceButtonUI : MonoBehaviour
    {
        [SerializeField] private Button diceButton;
        [SerializeField] private DiceController diceController;

        private void Awake()
        {
            if (diceButton == null)
            {
                diceButton = GetComponent<Button>();
            }

            if (diceController == null)
            {
                diceController = GetComponent<DiceController>();
            }

            if (diceButton != null && diceController != null)
            {
                diceButton.onClick.RemoveListener(diceController.RollDice);
                diceButton.onClick.AddListener(diceController.RollDice);
            }
        }
    }
}
