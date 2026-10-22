// Base of the pluggable win/loss rule set. GameCoordinator holds rules as LevelRule references
// and calls Evaluate on each one every frame: the dynamic-binding demo for TL1.
public abstract class LevelRule
{
    // Base behavior: "no verdict yet". Each subclass overrides this with its own win/lose condition.
    public virtual RuleVerdict Evaluate(SessionSnapshot snapshot)
    {
        return RuleVerdict.InProgress;
    }
}
