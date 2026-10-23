using NUnit.Framework;
using UnityEngine;

public class RunTimerTests
{
    private const float Tolerance = 0.0001f;

    [Test]
    public void Reset_SetsRemainingTime()
    {
        var timer = new RunTimer();
        timer.Reset(90f);

        Assert.AreEqual(90f, timer.RemainingSeconds, Tolerance);
        Assert.IsFalse(timer.IsExpired);
    }

    [Test]
    public void Advance_CountsDown()
    {
        var timer = new RunTimer();
        timer.Reset(1f);
        timer.Advance(0.25f);

        Assert.AreEqual(0.75f, timer.RemainingSeconds, Tolerance);
    }

    // Task 5.4: reaching exactly 0.0s
    [Test]
    public void Advance_ExactlyToZero_IsExactlyZeroAndExpired()
    {
        var timer = new RunTimer();
        timer.Reset(1f);
        timer.Advance(1f);

        Assert.AreEqual(0f, timer.RemainingSeconds);
        Assert.IsTrue(timer.IsExpired);
    }

    [Test]
    public void Advance_PastZero_ClampsToExactlyZero()
    {
        var timer = new RunTimer();
        timer.Reset(0.01f);
        timer.Advance(0.016f);

        Assert.AreEqual(0f, timer.RemainingSeconds);
    }

    [Test]
    public void Advance_AfterExpired_StaysAtZero()
    {
        var timer = new RunTimer();
        timer.Reset(0.5f);
        timer.Advance(1f);
        timer.Advance(1f);

        Assert.AreEqual(0f, timer.RemainingSeconds);
    }

    [Test]
    public void Advance_NegativeDelta_Ignored()
    {
        var timer = new RunTimer();
        timer.Reset(5f);
        timer.Advance(-1f);

        Assert.AreEqual(5f, timer.RemainingSeconds, Tolerance);
    }

    // Task 5.3: zero and negative limits
    [TestCase(0f)]
    [TestCase(-10f)]
    public void Reset_ZeroOrNegativeLimit_ExpiredImmediately(float limit)
    {
        var timer = new RunTimer();
        timer.Reset(limit);

        Assert.AreEqual(0f, timer.RemainingSeconds);
        Assert.IsTrue(timer.IsExpired);
    }

    // Timer and rule together: what the coordinator will do every frame
    [Test]
    public void ExpiredTimer_MakesTimedSurvivalRuleLose()
    {
        var timer = new RunTimer();
        timer.Reset(1f);
        timer.Advance(2f);

        LevelRule rule = new TimedSurvivalRule();
        var snapshot = new SessionSnapshot(Vector2Int.zero, timer.RemainingSeconds, false, false);

        Assert.AreEqual(RuleVerdict.Lose, rule.Evaluate(snapshot));
    }
}
