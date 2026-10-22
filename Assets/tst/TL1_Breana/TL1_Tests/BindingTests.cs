using NUnit.Framework;
using UnityEngine;

// Calls Evaluate through a LevelRule reference (Risk R6): if virtual/override is removed,
// the base method runs instead and these tests go red before the oral exam does.
public class BindingTests
{
    [Test]
    public void BaseRef_ToSubclass_UsesOverride()
    {
        LevelRule rule = new PuzzleCompletionRule();
        var snapshot = new SessionSnapshot(Vector2Int.zero, 10f, true, false);

        Assert.AreEqual(RuleVerdict.Win, rule.Evaluate(snapshot));
    }

    [Test]
    public void BaseRef_ToSubclass_ObjectiveIncomplete_StaysInProgress()
    {
        LevelRule rule = new PuzzleCompletionRule();
        var snapshot = new SessionSnapshot(Vector2Int.zero, 10f, false, false);

        Assert.AreEqual(RuleVerdict.InProgress, rule.Evaluate(snapshot));
    }
}
