using LudoMaster.Core;
using UnityEngine;

namespace LudoMaster.Gameplay
{
    /// <summary>
    /// Builds runtime board path transforms from static layout data without generating tile visuals.
    /// </summary>
    public class BoardLayoutBuilder : MonoBehaviour
    {
        [SerializeField] private BoardPathData boardPathData;
        [SerializeField] private Transform pointRoot;
        [SerializeField] private float tileSize = 0.55f;

        public void BuildLayout()
        {
            if (boardPathData == null)
            {
                Debug.LogWarning("BoardLayoutBuilder requires BoardPathData.", this);
                return;
            }

            Transform root = pointRoot == null ? transform : pointRoot;
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(root.GetChild(i).gameObject);
            }

            boardPathData.MainPathPoints.Clear();
            boardPathData.SafeTileIndexes.Clear();
            boardPathData.HomePaths.Clear();

            for (int i = 0; i < LudoBoardLayoutData.MainPath.Count; i++)
            {
                Vector2Int coord = LudoBoardLayoutData.MainPath[i];
                Transform point = new GameObject($"PathPoint_{i}").transform;
                point.SetParent(root, false);
                point.localPosition = CoordToWorld(coord);
                boardPathData.MainPathPoints.Add(point);
            }

            for (int i = 0; i < LudoBoardLayoutData.SafeTileIndices.Count; i++)
            {
                boardPathData.SafeTileIndexes.Add(LudoBoardLayoutData.SafeTileIndices[i]);
            }

            foreach (PlayerColor color in System.Enum.GetValues(typeof(PlayerColor)))
            {
                BoardPathData.HomePathGroup group = new() { Color = color };
                if (!LudoBoardLayoutData.HomePaths.TryGetValue(color, out var points))
                {
                    boardPathData.HomePaths.Add(group);
                    continue;
                }

                for (int i = 0; i < points.Count; i++)
                {
                    Transform homePoint = new GameObject($"{color}_Home_{i}").transform;
                    homePoint.SetParent(root, false);
                    homePoint.localPosition = CoordToWorld(points[i]);
                    group.Points.Add(homePoint);
                }

                boardPathData.HomePaths.Add(group);
            }
        }

        private Vector3 CoordToWorld(Vector2Int coord)
        {
            return new Vector3(coord.x * tileSize, coord.y * tileSize, 0f);
        }
    }
}
