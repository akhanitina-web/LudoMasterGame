using System.Collections.Generic;
using LudoMaster.Core;
using UnityEngine;
using System.Linq;

namespace LudoMaster.Gameplay
{
    /// <summary>
    /// Builds a colorful board, safe tile highlights, and path transforms for gameplay systems.
    /// </summary>
    public class BoardVisualGenerator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BoardPathData boardPathData;
        [SerializeField] private Transform tileRoot;

        [Header("Layout")]
        [SerializeField] private float tileSize = 0.75f;
        [SerializeField] private bool generateOnAwake = true;

        [Header("Colors")]
        [SerializeField] private Color neutralTileColor = new(0.95f, 0.95f, 0.95f, 1f);
        [SerializeField] private Color pathTileColor = Color.white;
        [SerializeField] private Color safeTileColor = new(1f, 0.88f, 0.2f, 1f);
        [SerializeField] private Color boardBackgroundColor = new(0.16f, 0.2f, 0.27f, 1f);

        private readonly List<GameObject> generatedObjects = new();

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
            BuildGrid(root);
            BuildMainPath(root);
            BuildHomePaths(root);
            BuildBaseZones(root);
        }

        private void BuildBackground(Transform root)
        {
            var background = CreateQuad("BoardBackground", root, Vector3.zero, 15f * tileSize);
            var sr = background.GetComponent<SpriteRenderer>();
            sr.color = boardBackgroundColor;
            sr.sortingOrder = -10;
        }

        private void BuildGrid(Transform root)
        {
            for (int x = -7; x <= 7; x++)
            {
                for (int y = -7; y <= 7; y++)
                {
                    var cell = CreateQuad($"Grid_{x}_{y}", root, CoordToWorld(new Vector2Int(x, y)), tileSize * 0.95f);
                    var sr = cell.GetComponent<SpriteRenderer>();
                    sr.color = neutralTileColor;
                    sr.sortingOrder = -5;
                }
            }
        }

        private void BuildMainPath(Transform root)
        {
            boardPathData.MainPathPoints.Clear();
            boardPathData.SafeTileIndexes.Clear();

            for (int i = 0; i < LudoBoardLayoutData.MainPath.Count; i++)
            {
                Vector2Int coord = LudoBoardLayoutData.MainPath[i];
                bool isSafe = LudoBoardLayoutData.SafeTileIndices.Contains(i);

                var tile = CreateQuad($"PathTile_{i}", root, CoordToWorld(coord), tileSize * 0.88f);
                var sr = tile.GetComponent<SpriteRenderer>();
                sr.color = isSafe ? safeTileColor : pathTileColor;
                sr.sortingOrder = -2;

                var point = new GameObject($"PathPoint_{i}").transform;
                point.SetParent(root);
                point.position = CoordToWorld(coord);
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
                    var tile = CreateQuad($"{pair.Key}_HomeTile_{i}", root, CoordToWorld(coord), tileSize * 0.8f);
                    var sr = tile.GetComponent<SpriteRenderer>();
                    sr.color = GetPlayerColor(pair.Key, 0.7f);
                    sr.sortingOrder = -1;

                    var point = new GameObject($"{pair.Key}_HomePoint_{i}").transform;
                    point.SetParent(root);
                    point.position = CoordToWorld(coord);
                    generatedObjects.Add(point.gameObject);
                    group.Points.Add(point);
                }

                boardPathData.HomePaths.Add(group);
            }
        }

        private void BuildBaseZones(Transform root)
        {
            BuildZone(root, "RedBase", new Vector2(-4f, 4f), PlayerColor.Red);
            BuildZone(root, "GreenBase", new Vector2(4f, 4f), PlayerColor.Green);
            BuildZone(root, "BlueBase", new Vector2(-4f, -4f), PlayerColor.Blue);
            BuildZone(root, "YellowBase", new Vector2(4f, -4f), PlayerColor.Yellow);
        }

        private void BuildZone(Transform root, string name, Vector2 center, PlayerColor color)
        {
            var zone = CreateQuad(name, root, new Vector3(center.x * tileSize, center.y * tileSize), tileSize * 4.8f);
            var sr = zone.GetComponent<SpriteRenderer>();
            sr.color = GetPlayerColor(color, 0.3f);
            sr.sortingOrder = -4;
        }

        private GameObject CreateQuad(string name, Transform parent, Vector3 position, float size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.localScale = Vector3.one * size;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetSolidSprite();

            generatedObjects.Add(go);
            return go;
        }


        private static Sprite solidSprite;

        private static Sprite GetSolidSprite()
        {
            if (solidSprite != null) return solidSprite;

            var tex = new Texture2D(2, 2);
            tex.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
            tex.Apply();
            solidSprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            return solidSprite;
        }

        private Vector3 CoordToWorld(Vector2Int coord)
        {
            return new Vector3(coord.x * tileSize, coord.y * tileSize, 0f);
        }

        private Color GetPlayerColor(PlayerColor color, float alpha)
        {
            Color baseColor = color switch
            {
                PlayerColor.Red => new Color(0.93f, 0.22f, 0.24f),
                PlayerColor.Blue => new Color(0.16f, 0.47f, 0.96f),
                PlayerColor.Green => new Color(0.2f, 0.73f, 0.27f),
                PlayerColor.Yellow => new Color(0.95f, 0.85f, 0.17f),
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
        }
    }
}
