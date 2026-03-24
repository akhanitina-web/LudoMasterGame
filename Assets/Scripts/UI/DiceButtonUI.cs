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
            if (diceButton != null)
            {
                diceButton.onClick.AddListener(diceController.RollDice);
            }
        }
    }
}
