using NUnit.Framework;
using UnityEngine;

// Each rule is called through a LevelRule reference, the way GameCoordinator will hold them.
public class LevelRuleTests
{
    private static readonly Vector2Int Exit = new Vector2Int(4, 4);

    private static SessionSnapshot Snap(Vector2Int cell = default, float time = 10f, bool cheeseDone = false, bool died = false)
    {
        return new SessionSnapshot(cell, time, cheeseDone, died);
    }

    // TimedSurvivalRule
    [Test]
    public void Timed_ExactlyZero_Loses()
    {
        LevelRule rule = new TimedSurvivalRule();
        Assert.AreEqual(RuleVerdict.Lose, rule.Evaluate(Snap(time: 0f)));
    }

    [Test]
    public void Timed_TimeLeft_InProgress()
    {
        LevelRule rule = new TimedSurvivalRule();
        Assert.AreEqual(RuleVerdict.InProgress, rule.Evaluate(Snap(time: 0.01f)));
    }

    [Test]
    public void Timed_IgnoresCheese_StillLosesAtZero()
    {
        LevelRule rule = new TimedSurvivalRule();
        Assert.AreEqual(RuleVerdict.Lose, rule.Evaluate(Snap(time: 0f, cheeseDone: true)));
    }

    // NoDeathRule
    [Test]
    public void NoDeath_CaughtThisFrame_Loses()
    {
        LevelRule rule = new NoDeathRule();
        Assert.AreEqual(RuleVerdict.Lose, rule.Evaluate(Snap(died: true)));
    }

    [Test]
    public void NoDeath_NotCaught_InProgress()
    {
        LevelRule rule = new NoDeathRule();
        Assert.AreEqual(RuleVerdict.InProgress, rule.Evaluate(Snap()));
    }

    // ReachExitRule
    [Test]
    public void ReachExit_OnExitWithAllCheese_Wins()
    {
        LevelRule rule = new ReachExitRule(Exit);
        Assert.AreEqual(RuleVerdict.Win, rule.Evaluate(Snap(cell: Exit, cheeseDone: true)));
    }

    [Test]
    public void ReachExit_OnExitButCheeseMissing_InProgress()
    {
        LevelRule rule = new ReachExitRule(Exit);
        Assert.AreEqual(RuleVerdict.InProgress, rule.Evaluate(Snap(cell: Exit, cheeseDone: false)));
    }

    [Test]
    public void ReachExit_AllCheeseButNotOnExit_InProgress()
    {
        LevelRule rule = new ReachExitRule(Exit);
        Assert.AreEqual(RuleVerdict.InProgress, rule.Evaluate(Snap(cell: Vector2Int.zero, cheeseDone: true)));
    }
}
