// Loses the level the frame F4 reports the mouse was caught.
public class NoDeathRule : LevelRule
{
    public override RuleVerdict Evaluate(SessionSnapshot snapshot)
    {
        return snapshot.DeathThisFrame ? RuleVerdict.Lose : RuleVerdict.InProgress;
    }
}
