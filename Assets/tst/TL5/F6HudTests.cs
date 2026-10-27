using NUnit.Framework;

// EditMode tests for F6. Widgets, the model and the router are plain C#, so no scene is needed.
public class F6HudTests
{
    static HudModel ModelWithTimer(float seconds)
    {
        var m = new HudModel();
        m.SetTimer(seconds);
        return m;
    }

    // ---------- Timer ----------

    [Test]
    public void Timer_FormatsAsMinutesSeconds_RoundingUp()
    {
        Assert.AreEqual("01:24", new TimerWidget().Render(ModelWithTimer(83.2f)));
        Assert.AreEqual("00:01", new TimerWidget().Render(ModelWithTimer(0.4f)));
        Assert.AreEqual("00:00", new TimerWidget().Render(ModelWithTimer(0f)));
    }

    [Test]
    public void Timer_BlankBeforeFirstSetTimer()
    {
        Assert.AreEqual("", new TimerWidget().Render(new HudModel()));
        Assert.IsFalse(new HudModel().IsUrgent);
    }

    [Test]
    public void Timer_UrgentStrictlyBelowTenSeconds()   // plan boundary test: exactly 10.0 s
    {
        Assert.IsFalse(ModelWithTimer(10f).IsUrgent);
        Assert.IsTrue(ModelWithTimer(9.99f).IsUrgent);
    }

    [Test]
    public void Timer_InvalidInputIsClampedOrIgnored()
    {
        Assert.AreEqual("00:00", new TimerWidget().Render(ModelWithTimer(-5f)));
        Assert.AreEqual("99:59", new TimerWidget().Render(ModelWithTimer(float.PositiveInfinity)));

        var m = ModelWithTimer(30f);
        int v = m.Version;
        Assert.IsFalse(m.SetTimer(float.NaN));
        Assert.AreEqual(v, m.Version);
        Assert.AreEqual(30f, m.SecondsRemaining);
    }

    // Plan stress test: 60 SetTimer calls per second for 10 minutes without allocation growth.
    // The HUD only rebuilds text when Version changes, so it must change once per displayed second, not per call.
    [Test]
    public void Timer_TenMinutesAt60Fps_VersionChangesOncePerSecond()
    {
        var m = new HudModel();
        float t = 600f;
        int changes = 0;
        for (int frame = 0; frame < 600 * 60; frame++)
        {
            if (m.SetTimer(t)) changes++;
            t -= 1f / 60f;
        }
        // 600 second boundaries + the first call + the urgency flip at 10 s, with a little float slack.
        Assert.That(changes, Is.LessThanOrEqualTo(605));
        Assert.That(changes, Is.GreaterThanOrEqualTo(595));
    }

    [Test]
    public void Timer_TimeElapsedFraction_RisesFromZeroToOne()
    {
        var m = ModelWithTimer(100f);
        Assert.AreEqual(0f, m.TimeElapsedFraction, 1e-5f);
        m.SetTimer(25f);
        Assert.AreEqual(0.75f, m.TimeElapsedFraction, 1e-5f);
        m.SetTimer(0f);
        Assert.AreEqual(1f, m.TimeElapsedFraction, 1e-5f);
    }

    // ---------- Objectives ----------

    [Test]
    public void Objective_ShowsLabelAndCount()
    {
        var m = new HudModel();
        m.SetObjective("Levers", 2, 4);
        Assert.AreEqual("Levers: 2/4", new ObjectiveWidget().Render(m));
    }

    [Test]
    public void Objective_ZeroOfZero_NeverReads0Over0()   // plan boundary test
    {
        var m = new HudModel();
        m.SetObjective("Cheese", 0, 0);
        string text = new ObjectiveWidget().Render(m);
        Assert.AreEqual(StringTable.NoObjectives, text);
        StringAssert.DoesNotContain("0/0", text);
    }

    [Test]
    public void Objective_CountsAreClamped_AndEmptyLabelUsesDefault()
    {
        var m = new HudModel();
        m.SetObjective(null, 9, 3);
        Assert.AreEqual(3, m.ObjectivesCompleted);
        Assert.AreEqual(StringTable.ObjectiveLabel + ": 3/3", new ObjectiveWidget().Render(m));
        m.SetObjective("Keys", -2, -1);
        Assert.AreEqual(0, m.ObjectivesTotal);
    }

    // ---------- Deaths and distortions ----------

    [Test]
    public void Attempts_ShowsDeaths_NeverNegative()
    {
        var m = new HudModel();
        m.SetDeaths(-3);
        Assert.AreEqual("Deaths: 0", new AttemptsWidget().Render(m));
        m.SetDeaths(4);
        Assert.AreEqual("Deaths: 4", new AttemptsWidget().Render(m));
    }

    [Test]
    public void Distortion_BlankWhenNone_ListsNamesInOrder()
    {
        var m = new HudModel();
        Assert.AreEqual("", new DistortionWarningWidget().Render(m));
        m.SetDistortions(new[] { DistortionID.Mirror, DistortionID.Rotation });
        Assert.AreEqual("Distorted: Mirror, Rotation", new DistortionWarningWidget().Render(m));
    }

    [Test]
    public void Distortion_SameListTwice_DoesNotBumpVersion()
    {
        var m = new HudModel();
        m.SetDistortions(new[] { DistortionID.Fog });
        int v = m.Version;
        Assert.IsFalse(m.SetDistortions(new[] { DistortionID.Fog }));
        Assert.AreEqual(v, m.Version);
    }

    [Test]
    public void ResetForLevel_ClearsLevelState_KeepsRunDeaths()
    {
        var m = ModelWithTimer(50f);
        m.SetObjective("Cheese", 1, 3);
        m.SetDeaths(2);
        m.SetDistortions(new[] { DistortionID.Blackout });
        m.ResetForLevel();
        Assert.IsFalse(m.HasTimer);
        Assert.IsFalse(m.HasObjective);
        Assert.AreEqual(0, m.ActiveDistortions.Count);
        Assert.AreEqual(2, m.Deaths);
    }

    // ---------- Dynamic binding ----------

    // Every widget is used through a HudWidget reference; each must produce its own text, not the base label.
    // If `virtual` is removed from HudWidget.Render (the oral-exam demo), this test fails: all four return their Label.
    [Test]
    public void DynamicBinding_EachWidgetOverridesRender()
    {
        var m = ModelWithTimer(83f);
        m.SetObjective("Cheese", 1, 2);
        m.SetDistortions(new[] { DistortionID.Mirror });
        HudWidget[] all = { new TimerWidget(), new ObjectiveWidget(), new AttemptsWidget(), new DistortionWarningWidget() };
        foreach (HudWidget w in all)
            Assert.AreNotEqual(w.Label, w.Render(m), w.GetType().Name);
    }

    // ---------- Screen navigation ----------

    [Test]
    public void Router_SettingsFromPause_BackReturnsToPause()
    {
        var r = new ScreenRouter();
        r.Show(HudScreen.Hud);
        r.Show(HudScreen.Pause);
        r.Show(HudScreen.Settings);
        Assert.IsTrue(r.HudBarVisible, "HUD bar stays behind settings opened from pause");
        r.Back();
        Assert.AreEqual(HudScreen.Pause, r.Current);
    }

    [Test]
    public void Router_SettingsFromMainMenu_HidesHudBar_BackReturnsToMenu()
    {
        var r = new ScreenRouter();
        r.Show(HudScreen.MainMenu);
        r.Show(HudScreen.Settings);
        Assert.IsFalse(r.HudBarVisible);
        r.Back();
        Assert.AreEqual(HudScreen.MainMenu, r.Current);
    }

    [Test]
    public void Router_ChangedFiresOnlyOnRealChanges()
    {
        var r = new ScreenRouter();
        int fired = 0;
        r.Changed += _ => fired++;
        r.Show(HudScreen.Hud);
        r.Show(HudScreen.Hud);
        r.Back();                       // not on a sub-screen: no-op
        Assert.AreEqual(1, fired);
    }

    // ---------- Strings ----------

    [Test]
    public void Stats_FormatsElapsedTimeRoundedDown()
    {
        var s = new RunStatistics { ElapsedTime = 83.9f, Deaths = 1, PuzzlesSolved = 2, LevelsCleared = 3 };
        Assert.AreEqual("Time: 01:23\nDeaths: 1\nPuzzles solved: 2\nLevels cleared: 3", StringTable.Stats(s));
    }
}
