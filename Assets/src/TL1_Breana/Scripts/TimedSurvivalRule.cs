// Loses the level the frame the countdown reaches 0.0s.
public class TimedSurvivalRule : LevelRule
{
    public override RuleVerdict Evaluate(SessionSnapshot snapshot)
    {
        return snapshot.RemainingTime <= 0f ? RuleVerdict.Lose : RuleVerdict.InProgress;
    }
}
