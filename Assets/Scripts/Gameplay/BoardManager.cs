using System.Collections.Generic;
using LudoMaster.Core;
using UnityEngine;

namespace LudoMaster.Gameplay
{
    /// <summary>
    /// Provides all board waypoint positions for token movement.
    /// Supports two sources:
    /// 1) Explicit <see cref="BoardPathData"/> assignment.
    /// 2) Runtime generation from a board RectTransform using normalized coordinates.
    /// </summary>
    public class BoardManager : MonoBehaviour
    {
        [Header("Optional authored path data")]
        [SerializeField] private BoardPathData boardPathData;

        [Header("Runtime path generation (used when BoardPathData is missing or incomplete)")]
        [SerializeField] private RectTransform boardRect;
        [SerializeField] private Transform generatedWaypointRoot;
        [SerializeField] private bool regenerateWaypointsOnAwake = true;

        [Header("Board dimensions")]
        [SerializeField] private int boardLoopLength = 52;
        [SerializeField] private int homePathLength = 6;

        private readonly Dictionary<PlayerColor, List<Transform>> homePaths = new();
        private readonly List<Transform> mainPath = new();
        private readonly HashSet<int> safeTiles = new();

        private static readonly Vector2[] DefaultSafeTileNorms =
        {
            new(0.50f, 0.88f), new(0.83f, 0.50f), new(0.50f, 0.12f), new(0.17f, 0.50f),
            new(0.60f, 0.88f), new(0.83f, 0.60f), new(0.40f, 0.12f), new(0.17f, 0.40f)
        };

        private void Awake()
        {
            BuildPaths();
        }

        /// <summary>
        /// Rebuilds path caches from the best available source.
        /// </summary>
        public void BuildPaths()
        {
            homePaths.Clear();
            mainPath.Clear();
            safeTiles.Clear();

            bool loadedFromAsset = TryLoadFromPathData();
            if (!loadedFromAsset && regenerateWaypointsOnAwake)
            {
                GenerateFromBoardRect();
            }

            if (mainPath.Count == 0)
            {
                Debug.LogWarning("[BoardManager] No main path points available. Assign BoardPathData or boardRect.", this);
            }
        }

        public bool IsSafeTile(int boardIndex)
        {
            if (mainPath.Count == 0)
            {
                return false;
            }

            int wrapped = ((boardIndex % boardLoopLength) + boardLoopLength) % boardLoopLength;
            return safeTiles.Contains(wrapped);
        }

        public Vector3 GetMainPathPosition(int boardIndex)
        {
            if (mainPath.Count == 0)
            {
                return Vector3.zero;
            }

            int wrapped = ((boardIndex % boardLoopLength) + boardLoopLength) % boardLoopLength;
            return mainPath[wrapped].position;
        }

        public Vector3 GetHomePathPosition(PlayerColor color, int homeIndex)
        {
            if (!homePaths.TryGetValue(color, out List<Transform> points) || points.Count == 0)
            {
                return Vector3.zero;
            }

            homeIndex = Mathf.Clamp(homeIndex, 0, points.Count - 1);
            return points[homeIndex].position;
        }

        public bool HasHomePath(PlayerColor color)
        {
            return homePaths.TryGetValue(color, out List<Transform> points) && points.Count >= homePathLength;
        }

        public int BoardLoopLength => boardLoopLength;

        private bool TryLoadFromPathData()
        {
            if (boardPathData == null || boardPathData.MainPathPoints.Count < boardLoopLength)
            {
                return false;
            }

            for (int i = 0; i < boardLoopLength; i++)
            {
                mainPath.Add(boardPathData.MainPathPoints[i]);
            }

            foreach (int safe in boardPathData.SafeTileIndexes)
            {
                safeTiles.Add(Mathf.Clamp(safe, 0, boardLoopLength - 1));
            }

            for (int i = 0; i < boardPathData.HomePaths.Count; i++)
            {
                BoardPathData.HomePathGroup group = boardPathData.HomePaths[i];
                if (group.Points != null && group.Points.Count >= homePathLength)
                {
                    homePaths[group.Color] = group.Points;
                }
            }

            return homePaths.Count == 4;
        }

        private void GenerateFromBoardRect()
        {
            if (boardRect == null)
            {
                return;
            }

            if (generatedWaypointRoot == null)
            {
                GameObject root = new("GeneratedWaypoints");
                root.transform.SetParent(transform, false);
                generatedWaypointRoot = root.transform;
            }

            foreach (Transform child in generatedWaypointRoot)
            {
                Destroy(child.gameObject);
            }

            // Generates a 52-point outer loop around the board image (clockwise).
            for (int i = 0; i < boardLoopLength; i++)
            {
                float t = i / (float)boardLoopLength;
                Vector2 norm = EvaluateSquareLoop(t);
                mainPath.Add(CreateWaypoint($"Main_{i:D2}", norm));
            }

            // 6 home tiles per player moving from entry lane toward center.
            GenerateHomePath(PlayerColor.Red, new Vector2(0.50f, 0.86f), new Vector2(0.50f, 0.54f));
            GenerateHomePath(PlayerColor.Green, new Vector2(0.86f, 0.50f), new Vector2(0.54f, 0.50f));
            GenerateHomePath(PlayerColor.Blue, new Vector2(0.14f, 0.50f), new Vector2(0.46f, 0.50f));
            GenerateHomePath(PlayerColor.Yellow, new Vector2(0.50f, 0.14f), new Vector2(0.50f, 0.46f));

            for (int i = 0; i < DefaultSafeTileNorms.Length; i++)
            {
                int nearest = FindNearestMainIndex(DefaultSafeTileNorms[i]);
                safeTiles.Add(nearest);
            }
        }

        private void GenerateHomePath(PlayerColor color, Vector2 startNorm, Vector2 endNorm)
        {
            List<Transform> points = new(homePathLength);
            for (int i = 0; i < homePathLength; i++)
            {
                float t = i / Mathf.Max(1f, homePathLength - 1f);
                Vector2 norm = Vector2.Lerp(startNorm, endNorm, t);
                points.Add(CreateWaypoint($"Home_{color}_{i}", norm));
            }

            homePaths[color] = points;
        }

        private Transform CreateWaypoint(string name, Vector2 normalized)
        {
            GameObject go = new(name);
            go.transform.SetParent(generatedWaypointRoot, false);
            go.transform.position = ToWorldPosition(normalized);
            return go.transform;
        }

        private Vector3 ToWorldPosition(Vector2 normalized)
        {
            Rect rect = boardRect.rect;
            Vector2 local = new(
                Mathf.Lerp(rect.xMin, rect.xMax, normalized.x),
                Mathf.Lerp(rect.yMin, rect.yMax, normalized.y));

            return boardRect.TransformPoint(local);
        }

        private int FindNearestMainIndex(Vector2 normalized)
        {
            Vector3 target = ToWorldPosition(normalized);
            int nearestIndex = 0;
            float nearestDist = float.MaxValue;

            for (int i = 0; i < mainPath.Count; i++)
            {
                float dist = Vector3.SqrMagnitude(mainPath[i].position - target);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearestIndex = i;
                }
            }

            return nearestIndex;
        }

        private static Vector2 EvaluateSquareLoop(float t)
        {
            t = Mathf.Repeat(t, 1f);
            float p = t * 4f;
            if (p < 1f) return new Vector2(Mathf.Lerp(0.16f, 0.84f, p), 0.84f);
            if (p < 2f) return new Vector2(0.84f, Mathf.Lerp(0.84f, 0.16f, p - 1f));
            if (p < 3f) return new Vector2(Mathf.Lerp(0.84f, 0.16f, p - 2f), 0.16f);
            return new Vector2(0.16f, Mathf.Lerp(0.16f, 0.84f, p - 3f));
        }
    }
}
