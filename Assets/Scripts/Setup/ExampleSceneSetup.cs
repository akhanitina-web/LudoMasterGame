using LudoMaster.Gameplay;
using LudoMaster.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LudoMaster.Setup
{
    /// <summary>
    /// One-click sample setup for menu/game UI in a portrait mobile scene.
    /// </summary>
    public class ExampleSceneSetup : MonoBehaviour
    {
        [SerializeField] private bool includeRoomSelection = true;

        [ContextMenu("Build Example Scene UI")]
        public void BuildExampleSceneUi()
        {
            var canvasGo = new GameObject("UICanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(MobilePortraitSetup));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var portrait = canvasGo.GetComponent<MobilePortraitSetup>();
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            portrait.Configure(scaler);

            BuildMainMenu(canvasGo.transform);

            if (includeRoomSelection)
            {
                BuildRoomSelection(canvasGo.transform);
            }

            BuildHud(canvasGo.transform);
        }

        [ContextMenu("Build Example Board")]
        public void BuildExampleBoard()
        {
            var boardGo = new GameObject("BoardVisuals", typeof(BoardVisualGenerator));
            var generator = boardGo.GetComponent<BoardVisualGenerator>();
            generator.GenerateBoardVisuals();
        }

        private void BuildMainMenu(Transform root)
        {
            var panel = CreatePanel("MainMenuPanel", root, new Color(0.09f, 0.1f, 0.18f, 0.6f));
            panel.anchorMin = new Vector2(0f, 0.5f);
            panel.anchorMax = new Vector2(1f, 1f);
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;

            CreateText("Title", panel, "Ludo Master", 86, TextAlignmentOptions.Center, new Vector2(0.5f, 0.78f));
            CreateText("Coins", panel, "Coins: 1000", 48, TextAlignmentOptions.Center, new Vector2(0.5f, 0.62f));
            CreateButton("PlayButton", panel, "Play", new Vector2(0.5f, 0.42f));
            CreateButton("SettingsButton", panel, "Settings", new Vector2(0.5f, 0.28f));
        }

        private void BuildRoomSelection(Transform root)
        {
            var panel = CreatePanel("RoomSelectionPanel", root, new Color(0.06f, 0.07f, 0.14f, 0.65f));
            panel.anchorMin = new Vector2(0f, 0.17f);
            panel.anchorMax = new Vector2(1f, 0.5f);
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;

            CreateText("LobbyTitle", panel, "Select Room", 52, TextAlignmentOptions.Center, new Vector2(0.5f, 0.83f));
            CreateButton("LowCoinRoom", panel, "Low Coin Room", new Vector2(0.25f, 0.58f));
            CreateButton("MediumCoinRoom", panel, "Medium Coin Room", new Vector2(0.75f, 0.58f));
            CreateButton("HighCoinRoom", panel, "High Coin Room", new Vector2(0.25f, 0.33f));
            CreateButton("PrivateRoom", panel, "Private Room", new Vector2(0.75f, 0.33f));
        }

        private void BuildHud(Transform root)
        {
            var panel = CreatePanel("HUDPanel", root, new Color(0f, 0f, 0f, 0.32f));
            panel.anchorMin = new Vector2(0f, 0f);
            panel.anchorMax = new Vector2(1f, 0.17f);
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;

            CreateText("CoinHUD", panel, "Coins: 1000", 38, TextAlignmentOptions.Left, new Vector2(0.1f, 0.62f));
            CreateText("TurnHUD", panel, "Turn: Red", 38, TextAlignmentOptions.Center, new Vector2(0.5f, 0.62f));
            CreateButton("DiceButton", panel, "Roll", new Vector2(0.88f, 0.5f), new Vector2(190f, 90f));
        }

        private RectTransform CreatePanel(string name, Transform parent, Color color)
        {
            var panel = new GameObject(name, typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
            panel.SetParent(parent, false);
            panel.GetComponent<Image>().color = color;
            return panel;
        }

        private void CreateText(string name, RectTransform parent, string value, float fontSize, TextAlignmentOptions align, Vector2 anchor)
        {
            var text = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            text.transform.SetParent(parent, false);
            text.text = value;
            text.fontSize = fontSize;
            text.alignment = align;
            text.color = Color.white;

            var rect = text.rectTransform;
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.sizeDelta = new Vector2(500f, 100f);
            rect.anchoredPosition = Vector2.zero;
        }

        private Button CreateButton(string name, RectTransform parent, string label, Vector2 anchor, Vector2? size = null)
        {
            var buttonGo = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            var buttonRect = buttonGo.GetComponent<RectTransform>();
            buttonRect.SetParent(parent, false);
            buttonRect.anchorMin = anchor;
            buttonRect.anchorMax = anchor;
            buttonRect.sizeDelta = size ?? new Vector2(280f, 90f);
            buttonRect.anchoredPosition = Vector2.zero;

            var image = buttonGo.GetComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0.92f);

            var text = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            text.transform.SetParent(buttonRect, false);
            text.text = label;
            text.fontSize = 34;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.black;

            var textRect = text.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return buttonGo.GetComponent<Button>();
        }
    }
}
