using LudoMaster.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace LudoMaster.Gameplay
{
    /// <summary>
    /// Dice button controller that rolls local dice and broadcasts result in multiplayer.
    /// </summary>
    public class DiceController : MonoBehaviour
    {
        [SerializeField] private DiceManager diceManager;
        [SerializeField] private Button diceButton;
        [SerializeField] private PhotonManager photonManager;

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

            photonManager ??= FindObjectOfType<PhotonManager>();

            if (diceButton != null)
            {
                diceManager.Configure(diceButton);
            }

            LudoMaster.Signals.GameSignals.OnDiceRolled += BroadcastDice;
        }

        private void OnDestroy()
        {
            LudoMaster.Signals.GameSignals.OnDiceRolled -= BroadcastDice;
        }

        public void RollDice()
        {
            diceManager?.RollDice();
        }

        private void BroadcastDice(int value)
        {
            photonManager?.SendDiceRoll(value);
        }
    }
}
