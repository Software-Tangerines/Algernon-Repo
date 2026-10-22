// Wins the level as soon as F3 reports every objective complete.
// For "solve to win" levels; cheese levels use ReachExitRule instead (cheese opens the exit).
public class PuzzleCompletionRule : LevelRule
{
    public override RuleVerdict Evaluate(SessionSnapshot snapshot)
    {
        return snapshot.ObjectiveComplete ? RuleVerdict.Win : RuleVerdict.InProgress;
    }
}
