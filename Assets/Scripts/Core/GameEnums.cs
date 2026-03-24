using System;

namespace LudoMaster.Core
{
    /// <summary>
    /// Shared enums used across gameplay, UI, and services.
    /// </summary>
    public enum PlayerColor
    {
        Red = 0,
        Blue = 1,
        Green = 2,
        Yellow = 3
    }

    /// <summary>
    /// Current runtime state for a token.
    /// </summary>
    public enum TokenState
    {
        InBase = 0,
        OnBoard = 1,
        InHomePath = 2,
        Finished = 3
    }

    /// <summary>
    /// High-level game lifecycle states.
    /// </summary>
    public enum MatchState
    {
        WaitingForPlayers = 0,
        Playing = 1,
        Completed = 2
    }

    /// <summary>
    /// Turn action result helps UI and turn flow decisions.
    /// </summary>
    [Flags]
    public enum TurnResult
    {
        None = 0,
        Moved = 1,
        Captured = 2,
        FinishedToken = 4,
        ExtraTurn = 8,
        NoMoves = 16
    }
}
