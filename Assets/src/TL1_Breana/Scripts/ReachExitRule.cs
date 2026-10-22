using UnityEngine;

// Wins the level once the player reaches the maze's exit cell while the exit is open.
// The exit opens when F3 reports all cheese collected (with no cheese, it starts open).
public class ReachExitRule : LevelRule
{
    private readonly Vector2Int exitCell;

    public ReachExitRule(Vector2Int exitCell)
    {
        this.exitCell = exitCell;
    }

    public override RuleVerdict Evaluate(SessionSnapshot snapshot)
    {
        bool onExit = snapshot.PlayerCell == exitCell;
        bool exitOpen = snapshot.ObjectiveComplete;
        return onExit && exitOpen ? RuleVerdict.Win : RuleVerdict.InProgress;
    }
}
