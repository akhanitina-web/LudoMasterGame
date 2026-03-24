using System.Collections.Generic;
using UnityEngine;

namespace LudoMaster.Gameplay
{
    /// <summary>
    /// Immutable coordinate sets used to draw a classic 15x15 Ludo board.
    /// </summary>
    public static class LudoBoardLayoutData
    {
        /// <summary>
        /// Main ring path coordinates in move order for a standard 52 tile loop.
        /// </summary>
        public static readonly IReadOnlyList<Vector2Int> MainPath = new List<Vector2Int>
        {
            new(-1, 6), new(-2, 6), new(-3, 6), new(-4, 6), new(-5, 6), new(-6, 6),
            new(-6, 5), new(-6, 4), new(-6, 3), new(-6, 2), new(-6, 1), new(-6, 0), new(-6, -1),
            new(-6, -2), new(-6, -3), new(-6, -4), new(-6, -5), new(-6, -6),
            new(-5, -6), new(-4, -6), new(-3, -6), new(-2, -6), new(-1, -6), new(0, -6), new(1, -6),
            new(2, -6), new(3, -6), new(4, -6), new(5, -6), new(6, -6),
            new(6, -5), new(6, -4), new(6, -3), new(6, -2), new(6, -1), new(6, 0), new(6, 1),
            new(6, 2), new(6, 3), new(6, 4), new(6, 5), new(6, 6),
            new(5, 6), new(4, 6), new(3, 6), new(2, 6), new(1, 6), new(0, 6),
            new(0, 5), new(0, 4), new(0, 3), new(0, 2)
        };

        /// <summary>
        /// Default safe tile indexes on the 52 tile loop.
        /// </summary>
        public static readonly IReadOnlyList<int> SafeTileIndices = new List<int> { 0, 8, 13, 21, 26, 34, 39, 47 };

        /// <summary>
        /// 6 home-lane coordinates for each player color.
        /// </summary>
        public static readonly Dictionary<LudoMaster.Core.PlayerColor, IReadOnlyList<Vector2Int>> HomePaths = new()
        {
            { LudoMaster.Core.PlayerColor.Red, new List<Vector2Int> { new(-5, 0), new(-4, 0), new(-3, 0), new(-2, 0), new(-1, 0), new(0, 0) } },
            { LudoMaster.Core.PlayerColor.Blue, new List<Vector2Int> { new(0, -5), new(0, -4), new(0, -3), new(0, -2), new(0, -1), new(0, 0) } },
            { LudoMaster.Core.PlayerColor.Green, new List<Vector2Int> { new(5, 0), new(4, 0), new(3, 0), new(2, 0), new(1, 0), new(0, 0) } },
            { LudoMaster.Core.PlayerColor.Yellow, new List<Vector2Int> { new(0, 5), new(0, 4), new(0, 3), new(0, 2), new(0, 1), new(0, 0) } }
        };
    }
}
