#if UNITY_EDITOR
using LudoMaster.Core;
using LudoMaster.Gameplay;
using LudoMaster.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace LudoMaster.EditorTools
{
    /// <summary>
    /// Generates token and dice prefabs used by the visual/UI setup.
    /// </summary>
    public static class LudoPrefabGenerator
    {
        private const string OutputPath = "Assets/Prefabs/Generated";

        [MenuItem("Ludo/Generate Visual Prefabs")]
        public static void Generate()
        {
            EnsureFolder("Assets/Prefabs", "Generated");

            CreateTokenPrefab(PlayerColor.Red, new Color(0.93f, 0.22f, 0.24f));
            CreateTokenPrefab(PlayerColor.Blue, new Color(0.16f, 0.47f, 0.96f));
            CreateTokenPrefab(PlayerColor.Green, new Color(0.2f, 0.73f, 0.27f));
            CreateTokenPrefab(PlayerColor.Yellow, new Color(0.95f, 0.85f, 0.17f));
            CreateDiceButtonPrefab();
            CreateBoardPrefab();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Generated Ludo visual prefabs in Assets/Prefabs/Generated");
        }

        private static void CreateTokenPrefab(PlayerColor color, Color tint)
        {
            var root = new GameObject($"Token_{color}");
            var sprite = root.AddComponent<SpriteRenderer>();
            sprite.sprite = CreateCircleSprite();
            sprite.color = tint;

            root.AddComponent<CircleCollider2D>();
            root.AddComponent<TokenController>();

            string prefabPath = $"{OutputPath}/Token_{color}.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
        }

        private static void CreateDiceButtonPrefab()
        {
            var go = new GameObject("DiceButton", typeof(RectTransform), typeof(Image), typeof(Button));
            var image = go.GetComponent<Image>();
            image.color = Color.white;

            var textObj = new GameObject("Face", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(go.transform, false);
            var text = textObj.GetComponent<TextMeshProUGUI>();
            text.text = "6";
            text.fontSize = 52;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.black;

            var rect = textObj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            go.AddComponent<DiceVisualUI>();

            string prefabPath = $"{OutputPath}/DiceButton.prefab";
            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);
        }


        private static void CreateBoardPrefab()
        {
            var board = new GameObject("LudoBoard", typeof(BoardVisualGenerator), typeof(BoardManager));
            var generator = board.GetComponent<BoardVisualGenerator>();
            var pathData = ScriptableObject.CreateInstance<BoardPathData>();

            var pathAssetPath = "Assets/Prefabs/Generated/BoardPathData.asset";
            AssetDatabase.CreateAsset(pathData, pathAssetPath);

            var soGen = new SerializedObject(generator);
            soGen.FindProperty("boardPathData").objectReferenceValue = pathData;
            soGen.ApplyModifiedPropertiesWithoutUndo();

            var manager = board.GetComponent<BoardManager>();
            var soMgr = new SerializedObject(manager);
            soMgr.FindProperty("boardPathData").objectReferenceValue = pathData;
            soMgr.ApplyModifiedPropertiesWithoutUndo();

            generator.GenerateBoardVisuals();

            string prefabPath = $"{OutputPath}/LudoBoard.prefab";
            PrefabUtility.SaveAsPrefabAsset(board, prefabPath);
            Object.DestroyImmediate(board);
        }
        private static Sprite CreateCircleSprite()
        {
            var tex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            Vector2 center = new(31.5f, 31.5f);
            float radius = 30f;

            for (int x = 0; x < tex.width; x++)
            {
                for (int y = 0; y < tex.height; y++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    tex.SetPixel(x, y, distance <= radius ? Color.white : Color.clear);
                }
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        }

        private static void EnsureFolder(string parent, string child)
        {
            if (!AssetDatabase.IsValidFolder($"{parent}/{child}"))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }
    }
}
#endif
