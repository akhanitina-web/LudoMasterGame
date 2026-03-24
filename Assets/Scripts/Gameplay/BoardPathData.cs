using System.Collections.Generic;
using UnityEngine;

namespace LudoMaster.Gameplay
{
    /// <summary>
    /// ScriptableObject board config. Assign path points and safe tiles in inspector.
    /// </summary>
    [CreateAssetMenu(fileName = "BoardPathData", menuName = "Ludo/Board Path Data")]
    public class BoardPathData : ScriptableObject
    {
        [Tooltip("Main ring path positions in movement order.")]
        public List<Transform> MainPathPoints = new();

        [Tooltip("Safe main-path indexes where captures are disabled.")]
        public List<int> SafeTileIndexes = new();

        [Tooltip("Per-color home path points (6 for traditional Ludo).")]
        public List<HomePathGroup> HomePaths = new();

        [System.Serializable]
        public class HomePathGroup
        {
            public LudoMaster.Core.PlayerColor Color;
            public List<Transform> Points = new();
        }
    }
}
