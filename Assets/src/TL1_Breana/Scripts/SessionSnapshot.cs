using UnityEngine;

// Per-frame data GameCoordinator builds and passes to every active LevelRule.
// Readonly: rules can read it but not change it.
public readonly struct SessionSnapshot
{
    public readonly Vector2Int PlayerCell;      // from F2 WorldToCell(player position)
    public readonly float RemainingTime;        // from RunTimer, in seconds
    public readonly bool ObjectiveComplete;     // polled from F3
    public readonly bool DeathThisFrame;        // polled from F4

    public SessionSnapshot(
        Vector2Int playerCell,
        float remainingTime,
        bool objectiveComplete,
        bool deathThisFrame)
    {
        PlayerCell = playerCell;
        RemainingTime = remainingTime;
        ObjectiveComplete = objectiveComplete;
        DeathThisFrame = deathThisFrame;
    }
}
