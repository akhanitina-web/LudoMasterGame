using LudoMaster.Gameplay;
using LudoMaster.Managers;
using LudoMaster.UI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LudoMaster.Setup
{
    /// <summary>
    /// Builds missing scene objects at runtime so MainMenu/GameScene are playable in a clean checkout.
    /// </summary>
    public static class SceneBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void BuildSceneIfNeeded()
        {
            string activeSceneName = SceneManager.GetActiveScene().name;
            if (activeSceneName == "MainMenu")
            {
                MainMenuRuntimeBuilder.Build();
            }
            else if (activeSceneName == "GameScene")
            {
                GameSceneRuntimeBuilder.Build();
            }
        }

        private static class MainMenuRuntimeBuilder
        {
            public static void Build()
            {
                EnsureEventSystem();
                CoinManager coinManager = Object.FindObjectOfType<CoinManager>() ?? new GameObject("CoinManager").AddComponent<CoinManager>();
                RoomManager roomManager = Object.FindObjectOfType<RoomManager>() ?? new GameObject("RoomManager").AddComponent<RoomManager>();

                Canvas canvas = EnsureCanvas();
                RectTransform safe = CreateSafeArea(canvas.transform);

                RectTransform top = CreatePanel("TopHUD", safe, new Vector2(0f, 0.84f), new Vector2(1f, 1f), new Color(0.08f, 0.1f, 0.2f, 0.9f));
                TMP_Text coinText = CreateText("CoinText", top, "Coins: 1000", 54, TextAlignmentOptions.Right, new Vector2(0.96f, 0.5f));

                RectTransform center = CreatePanel("MenuCenter", safe, new Vector2(0.08f, 0.28f), new Vector2(0.92f, 0.82f), new Color(0.04f, 0.07f, 0.18f, 0.65f));
                TMP_Text title = CreateText("Title", center, "LUDO ROYAL", 112, TextAlignmentOptions.Center, new Vector2(0.5f, 0.78f));

                Button playButton = CreateButton("PlayButton", center, "Play", new Vector2(0.5f, 0.45f), new Vector2(500f, 150f));
                Button settingsButton = CreateButton("SettingsButton", center, "Settings", new Vector2(0.5f, 0.24f), new Vector2(420f, 120f));

                GameObject settingsPanel = CreatePanel("SettingsPanel", center, new Vector2(0.14f, 0.05f), new Vector2(0.86f, 0.18f), new Color(0f, 0f, 0f, 0.45f)).gameObject;
                settingsPanel.SetActive(false);
                CreateText("SettingsHint", settingsPanel.transform as RectTransform, "Audio / Notifications (stub)", 34, TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f));

                RectTransform roomPanel = CreatePanel("RoomPanel", safe, new Vector2(0.04f, 0.03f), new Vector2(0.96f, 0.26f), new Color(0.03f, 0.04f, 0.1f, 0.75f));
                TMP_InputField roomInput = CreateInputField("RoomNameInput", roomPanel, "Private Room Name", new Vector2(0.5f, 0.8f), new Vector2(700f, 95f));
                Button low = CreateButton("LowCoinRoom", roomPanel, "Low Coin Room", new Vector2(0.2f, 0.46f), new Vector2(300f, 95f));
                Button med = CreateButton("MediumCoinRoom", roomPanel, "Medium Coin Room", new Vector2(0.5f, 0.46f), new Vector2(300f, 95f));
                Button high = CreateButton("HighCoinRoom", roomPanel, "High Coin Room", new Vector2(0.8f, 0.46f), new Vector2(300f, 95f));
                Button priv = CreateButton("PrivateRoom", roomPanel, "Private Room", new Vector2(0.5f, 0.16f), new Vector2(300f, 90f));
                TMP_Text roomListText = CreateText("RoomList", roomPanel, string.Empty, 28, TextAlignmentOptions.Center, new Vector2(0.5f, -0.02f));
                TMP_Text roomStatusText = CreateText("RoomStatus", roomPanel, "Select a room to play", 26, TextAlignmentOptions.Center, new Vector2(0.5f, 0.68f));

                MainMenuUI mainMenuUI = center.gameObject.AddComponent<MainMenuUI>();
                SetPrivateField(mainMenuUI, "playButton", playButton);
                SetPrivateField(mainMenuUI, "settingsButton", settingsButton);
                SetPrivateField(mainMenuUI, "settingsPanel", settingsPanel);
                SetPrivateField(mainMenuUI, "titleText", title);
                SetPrivateField(mainMenuUI, "coinText", coinText);
                SetPrivateField(mainMenuUI, "coinManager", coinManager);

                RoomSelectionUI roomUI = roomPanel.gameObject.AddComponent<RoomSelectionUI>();
                SetPrivateField(roomUI, "roomManager", roomManager);
                SetPrivateField(roomUI, "roomNameInput", roomInput);
                SetPrivateField(roomUI, "roomListText", roomListText);
                SetPrivateField(roomUI, "statusText", roomStatusText);
                SetPrivateField(roomUI, "lowCoinRoomButton", low);
                SetPrivateField(roomUI, "mediumCoinRoomButton", med);
                SetPrivateField(roomUI, "highCoinRoomButton", high);
                SetPrivateField(roomUI, "privateRoomButton", priv);
            }
        }

        private static class GameSceneRuntimeBuilder
        {
            public static void Build()
            {
                EnsureEventSystem();
                EnsureManagers();

                Transform sceneRoot = GetOrCreateTransform("GameScene", null);
                Canvas canvas = EnsureCanvas();
                canvas.transform.SetParent(sceneRoot, false);

                RectTransform safe = CreateSafeArea(canvas.transform);
                BuildBoardImage(safe);

                BoardPathData pathData = ScriptableObject.CreateInstance<BoardPathData>();
                Transform boardRoot = GetOrCreateTransform("BoardPathRoot", sceneRoot);
                boardRoot.position = new Vector3(0f, -0.35f, 0f);
                BoardLayoutBuilder boardLayoutBuilder = boardRoot.GetComponent<BoardLayoutBuilder>() ?? boardRoot.gameObject.AddComponent<BoardLayoutBuilder>();
                SetPrivateField(boardLayoutBuilder, "boardPathData", pathData);
                boardLayoutBuilder.BuildLayout();

                BoardManager boardManager = boardRoot.GetComponent<BoardManager>() ?? boardRoot.gameObject.AddComponent<BoardManager>();
                SetPrivateField(boardManager, "boardPathData", pathData);

                Camera mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    BoardCameraFitter cameraFitter = mainCamera.GetComponent<BoardCameraFitter>() ?? mainCamera.gameObject.AddComponent<BoardCameraFitter>();
                    SetPrivateField(cameraFitter, "boardRoot", boardRoot);
                    SetPrivateField(cameraFitter, "boardWorldSize", 10.5f);
                }

                Transform tokenContainer = GetOrCreateTransform("TokenContainer", sceneRoot);
                TokenSystem tokenSystem = GetOrCreateComponent<TokenSystem>("TokenSystem", sceneRoot);
                SetPrivateField(tokenSystem, "boardManager", boardManager);
                SetPrivateField(tokenSystem, "tokenRoot", tokenContainer);

                TurnSystem turnSystem = GetOrCreateComponent<TurnSystem>("TurnSystem", sceneRoot);
                SetPrivateField(turnSystem, "tokenSystem", tokenSystem);
                TurnManager turnManager = GetOrCreateComponent<TurnManager>("TurnManager", sceneRoot);
                turnManager.RegisterTurnSystem(turnSystem);

                TokenManager tokenManager = GetOrCreateComponent<TokenManager>("TokenManager", sceneRoot);
                tokenManager.RegisterTokenSystem(tokenSystem);

                WinSystem winSystem = GetOrCreateComponent<WinSystem>("WinSystem", sceneRoot);
                MultiplayerSyncManager sync = Object.FindObjectOfType<MultiplayerSyncManager>() ?? new GameObject("MultiplayerSyncManager").AddComponent<MultiplayerSyncManager>();
                sync.transform.SetParent(sceneRoot, false);

                GameManager gameManager = GetOrCreateComponent<GameManager>("GameManager", sceneRoot);
                SetPrivateField(gameManager, "turnManager", turnManager);
                SetPrivateField(gameManager, "tokenSystem", tokenSystem);
                SetPrivateField(gameManager, "tokenManager", tokenManager);
                SetPrivateField(gameManager, "winSystem", winSystem);
                SetPrivateField(gameManager, "multiplayerSync", sync);
                SetPrivateField(gameManager, "roomManager", Object.FindObjectOfType<RoomManager>());

                TokenSpawner tokenSpawner = GetOrCreateComponent<TokenSpawner>("TokenSpawner", sceneRoot);
                tokenSpawner.BuildDefaultTokens(tokenContainer, tokenSystem);

                BuildHud(safe, Object.FindObjectOfType<CoinManager>());
                BuildVictoryPanel(safe);
            }

            private static void BuildBoardImage(RectTransform safeArea)
            {
                RectTransform board = CreatePanel("BoardSprite", safeArea, new Vector2(0.08f, 0.23f), new Vector2(0.92f, 0.83f), Color.white);
                Image boardImage = board.GetComponent<Image>();
                boardImage.sprite = LudoSpriteFactory.GetBoardSprite();
                boardImage.type = Image.Type.Simple;
                boardImage.preserveAspect = true;
            }

            private static void BuildHud(RectTransform safeArea, CoinManager coinManager)
            {
                RectTransform uiRoot = CreatePanel("UI", safeArea, Vector2.zero, Vector2.one, Color.clear);

                RectTransform topHud = CreatePanel("TopHUD", uiRoot, new Vector2(0.04f, 0.93f), new Vector2(0.96f, 0.992f), new Color(0.07f, 0.1f, 0.2f, 0.9f));
                TMP_Text coinText = CreateText("CoinText", topHud, "Coins: 0", 42, TextAlignmentOptions.Left, new Vector2(0.12f, 0.52f));
                TMP_Text turnIndicator = CreateText("TurnIndicator", topHud, "Turn: Red", 44, TextAlignmentOptions.Center, new Vector2(0.5f, 0.52f));
                TMP_Text resultText = CreateText("ResultText", topHud, string.Empty, 30, TextAlignmentOptions.Right, new Vector2(0.95f, 0.52f));
                TMP_Text roomText = CreateText("RoomText", topHud, "Room: Not Joined", 26, TextAlignmentOptions.Center, new Vector2(0.5f, 0.15f));

                RectTransform diceParent = CreatePanel("DiceWidget", uiRoot, new Vector2(0.42f, 0.1f), new Vector2(0.58f, 0.2f), new Color(1f, 1f, 1f, 0.97f));
                Image faceImage = new GameObject("DiceFaceImage", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
                RectTransform faceRect = faceImage.rectTransform;
                faceRect.SetParent(diceParent, false);
                faceRect.anchorMin = new Vector2(0.2f, 0.34f);
                faceRect.anchorMax = new Vector2(0.8f, 0.95f);
                faceRect.offsetMin = Vector2.zero;
                faceRect.offsetMax = Vector2.zero;
                faceImage.sprite = LudoSpriteFactory.GetDiceFaceSprite(1);
                faceImage.preserveAspect = true;

                TMP_Text valueText = CreateText("DiceValue", diceParent, "1", 46, TextAlignmentOptions.Center, new Vector2(0.5f, 0.2f));
                valueText.color = new Color(0.12f, 0.15f, 0.2f, 1f);

                RectTransform rollButtonRoot = CreatePanel("DiceButton", uiRoot, new Vector2(0.34f, 0.03f), new Vector2(0.66f, 0.1f), Color.clear);
                Button diceButton = CreateButton("RollButton", rollButtonRoot, "Roll Dice", new Vector2(0.5f, 0.5f), new Vector2(320f, 110f));

                DiceManager diceManager = diceButton.gameObject.GetComponent<DiceManager>() ?? diceButton.gameObject.AddComponent<DiceManager>();
                diceManager.Configure(diceButton);

                DiceController diceController = diceButton.gameObject.GetComponent<DiceController>() ?? diceButton.gameObject.AddComponent<DiceController>();
                SetPrivateField(diceController, "diceButton", diceButton);
                SetPrivateField(diceController, "diceManager", diceManager);

                DiceVisualUI diceVisual = diceButton.gameObject.GetComponent<DiceVisualUI>() ?? diceButton.gameObject.AddComponent<DiceVisualUI>();
                SetPrivateField(diceVisual, "diceButton", diceButton);
                SetPrivateField(diceVisual, "faceImage", faceImage);
                SetPrivateField(diceVisual, "valueText", valueText);
                SetPrivateField(diceVisual, "diceTransform", diceParent);
                SetPrivateField(diceVisual, "diceBackground", diceParent.GetComponent<Image>());

                HUDController hudController = topHud.gameObject.GetComponent<HUDController>() ?? topHud.gameObject.AddComponent<HUDController>();
                SetPrivateField(hudController, "coinText", coinText);
                SetPrivateField(hudController, "turnText", turnIndicator);
                SetPrivateField(hudController, "resultText", resultText);
                SetPrivateField(hudController, "diceButton", diceButton);

                UIManager uiManager = topHud.gameObject.GetComponent<UIManager>() ?? topHud.gameObject.AddComponent<UIManager>();
                uiManager.ConfigureHud(coinText, turnIndicator, resultText, roomText);

                coinManager?.NotifyBalance("P1");
            }

            private static void BuildVictoryPanel(RectTransform safeArea)
            {
                RectTransform victoryPanel = CreatePanel("VictoryPanel", safeArea, new Vector2(0.2f, 0.43f), new Vector2(0.8f, 0.58f), new Color(0f, 0f, 0f, 0.2f));
                TMP_Text text = CreateText("VictoryText", victoryPanel, "Victory!", 78, TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f));
                text.color = new Color(1f, 0.92f, 0.35f, 0.25f);
                WinCelebrationUI celebration = victoryPanel.gameObject.GetComponent<WinCelebrationUI>() ?? victoryPanel.gameObject.AddComponent<WinCelebrationUI>();
                SetPrivateField(celebration, "label", text);
            }

            private static void EnsureManagers()
            {
                if (Object.FindObjectOfType<CoinManager>() == null)
                {
                    new GameObject("CoinManager").AddComponent<CoinManager>();
                }

                if (Object.FindObjectOfType<RoomManager>() == null)
                {
                    new GameObject("RoomManager").AddComponent<RoomManager>();
                }
            }
        }

        private static Canvas EnsureCanvas()
        {
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                MobilePortraitSetup existingSetup = canvas.GetComponent<MobilePortraitSetup>() ?? canvas.gameObject.AddComponent<MobilePortraitSetup>();
                existingSetup.Configure(canvas.GetComponent<CanvasScaler>() ?? canvas.gameObject.AddComponent<CanvasScaler>());
                return canvas;
            }

            GameObject canvasGo = new("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(MobilePortraitSetup));
            canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            MobilePortraitSetup portrait = canvasGo.GetComponent<MobilePortraitSetup>();
            portrait.Configure(scaler);
            return canvas;
        }

        private static RectTransform CreateSafeArea(Transform parent)
        {
            Transform existing = parent.Find("SafeArea");
            if (existing != null)
            {
                return existing as RectTransform;
            }

            RectTransform safeArea = new GameObject("SafeArea", typeof(RectTransform)).GetComponent<RectTransform>();
            safeArea.SetParent(parent, false);
            safeArea.anchorMin = Vector2.zero;
            safeArea.anchorMax = Vector2.one;
            safeArea.offsetMin = Vector2.zero;
            safeArea.offsetMax = Vector2.zero;

            MobilePortraitSetup setup = parent.GetComponent<MobilePortraitSetup>();
            if (setup != null)
            {
                setup.Configure(parent.GetComponent<CanvasScaler>(), safeArea);
            }

            return safeArea;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
            }
        }

        private static RectTransform CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            RectTransform rt = new GameObject(name, typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.GetComponent<Image>().color = color;
            return rt;
        }

        private static TMP_Text CreateText(string name, RectTransform parent, string value, float fontSize, TextAlignmentOptions alignment, Vector2 anchor)
        {
            TextMeshProUGUI text = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            text.transform.SetParent(parent, false);
            text.text = value;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.enableWordWrapping = false;

            RectTransform rt = text.rectTransform;
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.sizeDelta = new Vector2(620f, 120f);
            rt.anchoredPosition = Vector2.zero;
            return text;
        }

        private static Button CreateButton(string name, RectTransform parent, string label, Vector2 anchor, Vector2 size)
        {
            GameObject go = new(name, typeof(RectTransform), typeof(Image), typeof(Button));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;

            Image image = go.GetComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0.95f);

            TMP_Text txt = CreateText("Label", rect, label, 42, TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f));
            txt.color = new Color(0.1f, 0.1f, 0.14f);
            return go.GetComponent<Button>();
        }

        private static TMP_InputField CreateInputField(string name, RectTransform parent, string placeholder, Vector2 anchor, Vector2 size)
        {
            GameObject go = new(name, typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;
            go.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.95f);

            TMP_Text inputText = CreateText("Text", rect, string.Empty, 30, TextAlignmentOptions.Left, new Vector2(0.03f, 0.5f));
            TMP_Text placeholderText = CreateText("Placeholder", rect, placeholder, 30, TextAlignmentOptions.Left, new Vector2(0.03f, 0.5f));
            placeholderText.color = new Color(0.55f, 0.55f, 0.55f);

            TMP_InputField input = go.GetComponent<TMP_InputField>();
            input.textViewport = rect;
            input.textComponent = inputText;
            input.placeholder = placeholderText;
            return input;
        }

        private static Transform GetOrCreateTransform(string name, Transform parent)
        {
            Transform existing = parent == null ? GameObject.Find(name)?.transform : parent.Find(name);
            if (existing != null)
            {
                return existing;
            }

            GameObject go = new(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static T GetOrCreateComponent<T>(string objectName, Transform parent) where T : Component
        {
            Transform target = GetOrCreateTransform(objectName, parent);
            return target.GetComponent<T>() ?? target.gameObject.AddComponent<T>();
        }

        private static GameObject LoadGeneratedPrefab(string prefabName)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/Generated/{prefabName}.prefab");
#else
            return null;
#endif
        }

        private static void SetPrivateField<TObj, TValue>(TObj obj, string fieldName, TValue value)
        {
            var field = typeof(TObj).GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            field?.SetValue(obj, value);
        }
    }
}
