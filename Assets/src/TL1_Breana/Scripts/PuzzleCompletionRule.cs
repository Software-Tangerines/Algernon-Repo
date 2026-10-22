// Wins the level once F3 reports every objective complete.
public class PuzzleCompletionRule : LevelRule
{
    public override RuleVerdict Evaluate(SessionSnapshot snapshot)
    {
        return snapshot.ObjectiveComplete ? RuleVerdict.Win : RuleVerdict.InProgress;
    }
}
