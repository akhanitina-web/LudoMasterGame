using UnityEngine;
using UnityEngine.SceneManagement;

namespace LudoMaster.UI
{
    public class MainMenuController : MonoBehaviour
    {
        public void StartGame()
        {
            SceneManager.LoadScene("GameScene");
        }
    }
}
