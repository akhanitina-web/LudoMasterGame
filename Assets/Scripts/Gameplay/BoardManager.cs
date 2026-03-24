using System.Collections.Generic;
using LudoMaster.Core;
using UnityEngine;

namespace LudoMaster.Gameplay
{
    /// <summary>
    /// Resolves board positions, safe tiles, and home path lookups.
    /// </summary>
    public class BoardManager : MonoBehaviour
    {
        [SerializeField] private BoardPathData boardPathData;
        [SerializeField] private int boardLoopLength = 52;

        private readonly Dictionary<PlayerColor, List<Transform>> homePaths = new();

        private void Awake()
        {
            homePaths.Clear();
            if (boardPathData == null) return;

            for (int i = 0; i < boardPathData.HomePaths.Count; i++)
            {
                var group = boardPathData.HomePaths[i];
                homePaths[group.Color] = group.Points;
            }
        }

        /// <summary>
        /// Checks if board index is configured as a safe tile.
        /// </summary>
        public bool IsSafeTile(int boardIndex)
        {
            return boardPathData != null && boardPathData.SafeTileIndexes.Contains(boardIndex);
        }

        /// <summary>
        /// Returns world position for a main path index.
        /// </summary>
        public Vector3 GetMainPathPosition(int boardIndex)
        {
            if (boardPathData == null || boardPathData.MainPathPoints.Count == 0)
            {
                return Vector3.zero;
            }

            int wrapped = ((boardIndex % boardLoopLength) + boardLoopLength) % boardLoopLength;
            return boardPathData.MainPathPoints[wrapped].position;
        }

        /// <summary>
        /// Returns world position for player's home path step.
        /// </summary>
        public Vector3 GetHomePathPosition(PlayerColor color, int homeIndex)
        {
            if (!homePaths.TryGetValue(color, out List<Transform> points) || points.Count == 0)
            {
                return Vector3.zero;
            }

            homeIndex = Mathf.Clamp(homeIndex, 0, points.Count - 1);
            return points[homeIndex].position;
        }

        /// <summary>
        /// Returns true if home path entry is available for given color.
        /// </summary>
        public bool HasHomePath(PlayerColor color)
        {
            return homePaths.TryGetValue(color, out List<Transform> points) && points.Count > 0;
        }

        public int BoardLoopLength => boardLoopLength;
    }
}
