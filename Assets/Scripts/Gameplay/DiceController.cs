using LudoMaster.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace LudoMaster.Gameplay
{
    /// <summary>
    /// Compatibility component that forwards dice interaction to DiceManager.
    /// </summary>
    public class DiceController : MonoBehaviour
    {
        [SerializeField] private DiceManager diceManager;
        [SerializeField] private Button diceButton;

        private void Awake()
        {
            if (diceManager == null)
            {
                diceManager = GetComponent<DiceManager>();
            }

            if (diceManager == null)
            {
                diceManager = gameObject.AddComponent<DiceManager>();
            }

            if (diceButton == null)
            {
                diceButton = GetComponent<Button>();
            }

            if (diceButton != null)
            {
                diceManager.Configure(diceButton);
            }
        }

        public void RollDice()
        {
            diceManager?.RollDice();
        }
    }
}
