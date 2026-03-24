using System.Collections.Generic;
using LudoMaster.Core;
using UnityEngine;

namespace LudoMaster.Gameplay
{
    /// <summary>
    /// Builds a modernized classic Ludo board visual while preserving gameplay path transforms.
    /// </summary>
    public class BoardVisualGenerator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BoardPathData boardPathData;
        [SerializeField] private Transform tileRoot;

        [Header("Layout")]
        [SerializeField] private float tileSize = 0.7f;
        [SerializeField] private bool generateOnAwake = true;

        [Header("Colors")]
        [SerializeField] private Color boardBackgroundColor = new(0.11f, 0.15f, 0.24f, 1f);
        [SerializeField] private Color boardFrameColor = new(0.96f, 0.98f, 1f, 1f);
        [SerializeField] private Color boardInnerColor = new(0.9f, 0.95f, 1f, 1f);
        [SerializeField] private Color pathTileColor = new(0.98f, 0.99f, 1f, 1f);
        [SerializeField] private Color pathTileShadowColor = new(0.13f, 0.18f, 0.32f, 0.18f);
        [SerializeField] private Color safeTileColor = new(1f, 0.95f, 0.55f, 1f);
        [SerializeField] private Color safeStarColor = new(1f, 0.62f, 0.09f, 1f);

        private readonly List<GameObject> generatedObjects = new();

        private const float OuterBoardTiles = 15f;
        private const float InnerBoardTiles = 13.9f;
        private const float BoardCornerRadius = 0.17f;

        private void Awake()
        {
            if (generateOnAwake)
            {
                GenerateBoardVisuals();
            }
        }

        [ContextMenu("Generate Board Visuals")]
        public void GenerateBoardVisuals()
        {
            if (boardPathData == null)
            {
                Debug.LogWarning("BoardVisualGenerator requires a BoardPathData asset.", this);
                return;
            }

            ClearGenerated();
            Transform root = tileRoot == null ? transform : tileRoot;

            BuildBackground(root);
            BuildBoardFrame(root);
            BuildBaseZones(root);
            BuildHomeLanes(root);
            BuildMainPath(root);
            BuildHomePaths(root);
            BuildCenterVictory(root);
        }

        private void BuildBackground(Transform root)
        {
            CreateSpriteObject(
                "BoardBackdrop",
                root,
                Vector3.zero,
                Vector2.one * (OuterBoardTiles * tileSize * 1.18f),
                boardBackgroundColor,
                -30,
                GetRoundedRectSprite(0.22f));
        }

        private void BuildBoardFrame(Transform root)
        {
            CreateSpriteObject(
                "BoardFrame",
                root,
                Vector3.zero,
                Vector2.one * (OuterBoardTiles * tileSize),
                boardFrameColor,
                -25,
                GetRoundedRectSprite(BoardCornerRadius));

            CreateSpriteObject(
                "BoardInner",
                root,
                Vector3.zero,
                Vector2.one * (InnerBoardTiles * tileSize),
                boardInnerColor,
                -24,
                GetRoundedRectSprite(BoardCornerRadius));
        }

        private void BuildBaseZones(Transform root)
        {
            BuildZone(root, "RedBase", new Vector2(-4.5f, 4.5f), PlayerColor.Red);
            BuildZone(root, "GreenBase", new Vector2(4.5f, 4.5f), PlayerColor.Green);
            BuildZone(root, "BlueBase", new Vector2(-4.5f, -4.5f), PlayerColor.Blue);
            BuildZone(root, "YellowBase", new Vector2(4.5f, -4.5f), PlayerColor.Yellow);
        }

        private void BuildZone(Transform root, string name, Vector2 centerCoord, PlayerColor color)
        {
            Color zoneColor = GetPlayerColor(color, 1f);
            Vector3 center = new(centerCoord.x * tileSize, centerCoord.y * tileSize, 0f);

            CreateSpriteObject(
                name,
                root,
                center,
                Vector2.one * (tileSize * 5.2f),
                zoneColor,
                -20,
                GetRoundedRectSprite(0.2f));

            CreateSpriteObject(
                $"{name}_Inner",
                root,
                center,
                Vector2.one * (tileSize * 3.2f),
                Color.white,
                -19,
                GetRoundedRectSprite(0.35f));

            const float slotOffset = 0.88f;
            for (int i = 0; i < 4; i++)
            {
                float xSign = i % 2 == 0 ? -1f : 1f;
                float ySign = i < 2 ? 1f : -1f;
                Vector3 slotPosition = center + new Vector3(xSign * slotOffset * tileSize, ySign * slotOffset * tileSize, 0f);

                CreateSpriteObject(
                    $"{name}_Slot_{i}",
                    root,
                    slotPosition,
                    Vector2.one * (tileSize * 0.78f),
                    Color.Lerp(zoneColor, Color.white, 0.55f),
                    -18,
                    GetCircleSprite());
            }
        }

        private void BuildHomeLanes(Transform root)
        {
            BuildLane(root, new Vector2(-3f, 0f), new Vector2(6f, 1f), GetPlayerColor(PlayerColor.Red, 0.25f), "RedLane");
            BuildLane(root, new Vector2(3f, 0f), new Vector2(6f, 1f), GetPlayerColor(PlayerColor.Green, 0.25f), "GreenLane");
            BuildLane(root, new Vector2(0f, -3f), new Vector2(1f, 6f), GetPlayerColor(PlayerColor.Blue, 0.25f), "BlueLane");
            BuildLane(root, new Vector2(0f, 3f), new Vector2(1f, 6f), GetPlayerColor(PlayerColor.Yellow, 0.25f), "YellowLane");
        }

        private void BuildLane(Transform root, Vector2 centerCoord, Vector2 sizeInTiles, Color color, string name)
        {
            CreateSpriteObject(
                name,
                root,
                new Vector3(centerCoord.x * tileSize, centerCoord.y * tileSize, 0f),
                sizeInTiles * tileSize,
                color,
                -16,
                GetRoundedRectSprite(0.3f));
        }

        private void BuildMainPath(Transform root)
        {
            boardPathData.MainPathPoints.Clear();
            boardPathData.SafeTileIndexes.Clear();

            for (int i = 0; i < LudoBoardLayoutData.MainPath.Count; i++)
            {
                Vector2Int coord = LudoBoardLayoutData.MainPath[i];
                bool isSafe = LudoBoardLayoutData.SafeTileIndices.Contains(i);
                Vector3 world = CoordToWorld(coord);

                CreateSpriteObject(
                    $"PathTileShadow_{i}",
                    root,
                    world + new Vector3(0.03f, -0.03f, 0f) * tileSize,
                    Vector2.one * tileSize * 0.92f,
                    pathTileShadowColor,
                    -13,
                    GetRoundedRectSprite(0.28f));

                CreateSpriteObject(
                    $"PathTile_{i}",
                    root,
                    world,
                    Vector2.one * tileSize * 0.9f,
                    isSafe ? safeTileColor : pathTileColor,
                    -12,
                    GetRoundedRectSprite(0.28f));

                if (isSafe)
                {
                    CreateSpriteObject(
                        $"SafeStar_{i}",
                        root,
                        world,
                        Vector2.one * tileSize * 0.44f,
                        safeStarColor,
                        -11,
                        GetStarSprite());
                }

                var point = new GameObject($"PathPoint_{i}").transform;
                point.SetParent(root);
                point.position = world;
                generatedObjects.Add(point.gameObject);
                boardPathData.MainPathPoints.Add(point);

                if (isSafe)
                {
                    boardPathData.SafeTileIndexes.Add(i);
                }
            }
        }

        private void BuildHomePaths(Transform root)
        {
            boardPathData.HomePaths.Clear();

            foreach (var pair in LudoBoardLayoutData.HomePaths)
            {
                var group = new BoardPathData.HomePathGroup
                {
                    Color = pair.Key,
                    Points = new List<Transform>()
                };

                for (int i = 0; i < pair.Value.Count; i++)
                {
                    Vector2Int coord = pair.Value[i];
                    Vector3 world = CoordToWorld(coord);

                    CreateSpriteObject(
                        $"{pair.Key}_HomeTile_{i}",
                        root,
                        world,
                        Vector2.one * tileSize * 0.82f,
                        GetPlayerColor(pair.Key, 0.72f),
                        -10,
                        GetRoundedRectSprite(0.28f));

                    var point = new GameObject($"{pair.Key}_HomePoint_{i}").transform;
                    point.SetParent(root);
                    point.position = world;
                    generatedObjects.Add(point.gameObject);
                    group.Points.Add(point);
                }

                boardPathData.HomePaths.Add(group);
            }
        }

        private void BuildCenterVictory(Transform root)
        {
            float triScale = tileSize * 2.3f;

            CreateTriangle(root, "Center_Red", new Vector3(-0.56f, 0.56f, 0f) * tileSize, 225f, GetPlayerColor(PlayerColor.Red, 1f), triScale);
            CreateTriangle(root, "Center_Green", new Vector3(0.56f, 0.56f, 0f) * tileSize, 135f, GetPlayerColor(PlayerColor.Green, 1f), triScale);
            CreateTriangle(root, "Center_Blue", new Vector3(-0.56f, -0.56f, 0f) * tileSize, 315f, GetPlayerColor(PlayerColor.Blue, 1f), triScale);
            CreateTriangle(root, "Center_Yellow", new Vector3(0.56f, -0.56f, 0f) * tileSize, 45f, GetPlayerColor(PlayerColor.Yellow, 1f), triScale);

            CreateSpriteObject(
                "CenterCore",
                root,
                Vector3.zero,
                Vector2.one * tileSize * 0.52f,
                Color.white,
                -7,
                GetCircleSprite());
        }

        private void CreateTriangle(Transform root, string name, Vector3 position, float zRotation, Color color, float scale)
        {
            GameObject triangle = CreateSpriteObject(
                name,
                root,
                position,
                Vector2.one * scale,
                color,
                -8,
                GetTriangleSprite());

            triangle.transform.localRotation = Quaternion.Euler(0f, 0f, zRotation);
        }

        private GameObject CreateSpriteObject(string name, Transform parent, Vector3 position, Vector2 size, Color color, int sortingOrder, Sprite sprite)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = sortingOrder;

            generatedObjects.Add(go);
            return go;
        }

        private static Sprite circleSprite;
        private static Sprite starSprite;
        private static Sprite triangleSprite;
        private static readonly Dictionary<int, Sprite> roundedRectSprites = new();

        private static Sprite GetCircleSprite()
        {
            if (circleSprite != null) return circleSprite;

            const int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Vector2 center = new((size - 1) * 0.5f, (size - 1) * 0.5f);
            float radius = (size - 3) * 0.5f;

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    tex.SetPixel(x, y, distance <= radius ? Color.white : Color.clear);
                }
            }

            tex.Apply();
            circleSprite = Sprite.Create(tex, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 128f);
            return circleSprite;
        }

        private static Sprite GetStarSprite()
        {
            if (starSprite != null) return starSprite;

            const int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Vector2 center = new((size - 1) * 0.5f, (size - 1) * 0.5f);
            float outerRadius = size * 0.44f;
            float innerRadius = outerRadius * 0.45f;

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    Vector2 p = new(x, y);
                    Vector2 toPoint = p - center;
                    float angle = Mathf.Atan2(toPoint.y, toPoint.x);
                    float normalized = Mathf.Repeat(angle, Mathf.PI * 2f);
                    float sector = normalized / (Mathf.PI / 5f);
                    float blend = Mathf.Abs((sector % 2f) - 1f);
                    float radialLimit = Mathf.Lerp(innerRadius, outerRadius, blend);
                    tex.SetPixel(x, y, toPoint.magnitude <= radialLimit ? Color.white : Color.clear);
                }
            }

            tex.Apply();
            starSprite = Sprite.Create(tex, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 128f);
            return starSprite;
        }

        private static Sprite GetTriangleSprite()
        {
            if (triangleSprite != null) return triangleSprite;

            const int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Vector2 a = new(size * 0.5f, size * 0.08f);
            Vector2 b = new(size * 0.1f, size * 0.9f);
            Vector2 c = new(size * 0.9f, size * 0.9f);

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    bool inside = PointInTriangle(new Vector2(x, y), a, b, c);
                    tex.SetPixel(x, y, inside ? Color.white : Color.clear);
                }
            }

            tex.Apply();
            triangleSprite = Sprite.Create(tex, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 128f);
            return triangleSprite;
        }

        private static Sprite GetRoundedRectSprite(float cornerRadiusNormalized)
        {
            int key = Mathf.RoundToInt(cornerRadiusNormalized * 1000f);
            if (roundedRectSprites.TryGetValue(key, out Sprite sprite))
            {
                return sprite;
            }

            const int size = 128;
            float cornerRadius = Mathf.Clamp01(cornerRadiusNormalized) * size * 0.5f;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float px = Mathf.Abs(x - (size - 1) * 0.5f);
                    float py = Mathf.Abs(y - (size - 1) * 0.5f);
                    float dx = Mathf.Max(px - ((size * 0.5f) - cornerRadius), 0f);
                    float dy = Mathf.Max(py - ((size * 0.5f) - cornerRadius), 0f);
                    bool inside = (dx * dx) + (dy * dy) <= cornerRadius * cornerRadius;
                    tex.SetPixel(x, y, inside ? Color.white : Color.clear);
                }
            }

            tex.Apply();
            sprite = Sprite.Create(tex, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 128f);
            roundedRectSprites[key] = sprite;
            return sprite;
        }

        private static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float area = 0.5f * (-b.y * c.x + a.y * (-b.x + c.x) + a.x * (b.y - c.y) + b.x * c.y);
            float sign = area < 0f ? -1f : 1f;
            float s = (a.y * c.x - a.x * c.y + (c.y - a.y) * p.x + (a.x - c.x) * p.y) * sign;
            float t = (a.x * b.y - a.y * b.x + (a.y - b.y) * p.x + (b.x - a.x) * p.y) * sign;
            return s > 0f && t > 0f && (s + t) < 2f * area * sign;
        }

        private Vector3 CoordToWorld(Vector2Int coord)
        {
            return new Vector3(coord.x * tileSize, coord.y * tileSize, 0f);
        }

        private Color GetPlayerColor(PlayerColor color, float alpha)
        {
            Color baseColor = color switch
            {
                PlayerColor.Red => new Color(0.96f, 0.31f, 0.35f),
                PlayerColor.Blue => new Color(0.24f, 0.54f, 0.99f),
                PlayerColor.Green => new Color(0.24f, 0.82f, 0.43f),
                PlayerColor.Yellow => new Color(0.99f, 0.83f, 0.26f),
                _ => Color.white
            };

            baseColor.a = alpha;
            return baseColor;
        }

        private void ClearGenerated()
        {
            for (int i = generatedObjects.Count - 1; i >= 0; i--)
            {
                if (generatedObjects[i] != null)
                {
                    DestroyImmediate(generatedObjects[i]);
                }
            }

            generatedObjects.Clear();

            Transform root = tileRoot == null ? transform : tileRoot;
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(root.GetChild(i).gameObject);
            }
        }
    }
}
