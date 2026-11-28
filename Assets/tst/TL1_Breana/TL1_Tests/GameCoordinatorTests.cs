using System.Collections;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class GameCoordinatorTests
{
    private GameObject host;
    private GameCoordinator coordinator;
    [UnitySetUp]
    public IEnumerator SetUp()
    {
        host = new GameObject("TestCoordinator");
        coordinator = host.AddComponent<GameCoordinator>();
        yield return null;
    }
    [UnityTearDown]
    public IEnumerator TearDown()
    {
        LogAssert.ignoreFailingMessages = false;
        Object.Destroy(host);
        yield return null;
    }
   private IEnumerator WaitForState(SessionState expected, int maxFrames = 300)
    {
        for (int frame = 0; frame < maxFrames && coordinator.State != expected; frame++)
        {
            yield return null;
        }
        Assert.AreEqual(expected, coordinator.State, $"Timed out waiting for {expected}");
    }

    // Starts level 1 and waits until the MVP scene has loaded and the level is Playing.
    private IEnumerator StartAndWaitForPlaying()
    {
        // Other features (player, puzzles, camera) load with MVP; their log messages shouldn't fail F1's tests.
        LogAssert.ignoreFailingMessages = true;
        coordinator.StartRun(0);
        yield return WaitForState(SessionState.Playing);
    }

    // 8.1: StartRun(-1) and StartRun(Count) each leave State unchanged and log exactly 1 error.
    [UnityTest]
    public IEnumerator StartRun_InvalidIndex_LogsOneError_StateUnchanged()
    {
        LogAssert.Expect(LogType.Error, new Regex("StartRun: level index -1"));
        coordinator.StartRun(-1);
        Assert.AreEqual(SessionState.Menu, coordinator.State);

        // 3 = the LevelCatalogue's Count (levels 0, 1, 2), the first index past the end
        LogAssert.Expect(LogType.Error, new Regex("StartRun: level index 3"));
        coordinator.StartRun(3);
        Assert.AreEqual(SessionState.Menu, coordinator.State);

        LogAssert.NoUnexpectedReceived();
        yield return null;
    }

    // 8.2: two RestartLevel calls in the same frame yield exactly 1 OnLevelStarted.
    [UnityTest]
    public IEnumerator RestartLevel_TwiceSameFrame_FiresLevelStartedOnce()
    {
        yield return StartAndWaitForPlaying();

        int started = 0;
        coordinator.OnLevelStarted += () => started++;

        coordinator.RestartLevel();
        coordinator.RestartLevel();   // refused: already LevelLoading (logs a warning only)

        yield return WaitForState(SessionState.Playing);
        for (int i = 0; i < 10; i++) yield return null;   // give a second start the chance to (wrongly) happen

        Assert.AreEqual(1, started);
    }

    // 8.3: AdvanceLevel called while Playing throws InvalidOperationException.
    [UnityTest]
    public IEnumerator AdvanceLevel_WhilePlaying_Throws()
    {
        yield return StartAndWaitForPlaying();

        Assert.Throws<System.InvalidOperationException>(() => coordinator.AdvanceLevel());
        Assert.AreEqual(SessionState.Playing, coordinator.State, "A refused advance must not change the state");
    }

    // 8.4: Pause(true) called twice leaves State == Paused and the run's time unchanged.
    [UnityTest]
    public IEnumerator Pause_TwiceInARow_StaysPaused()
    {
        yield return StartAndWaitForPlaying();

        coordinator.Pause(true);
        coordinator.Pause(true);   // refused: already Paused (logs a warning only)
        Assert.AreEqual(SessionState.Paused, coordinator.State);

        float timeWhenPaused = coordinator.Statistics.ElapsedTime;
        for (int i = 0; i < 10; i++) yield return null;

        Assert.AreEqual(SessionState.Paused, coordinator.State);
        Assert.AreEqual(timeWhenPaused, coordinator.Statistics.ElapsedTime, 0.001f, "Time must not advance while paused");
    }

    // 8.5: AbortRun during LevelLoading: OnLevelStarted never fires, OnRunEnded fires exactly once,
    // OnLevelEnded fires zero times, and State == Menu within 1 frame.
    [UnityTest]
    public IEnumerator AbortRun_WhileLoading_LevelNeverStarts()
    {
        LogAssert.ignoreFailingMessages = true;
        int started = 0, levelEnded = 0, runEnded = 0;
        coordinator.OnLevelStarted += () => started++;
        coordinator.OnLevelEnded += () => levelEnded++;
        coordinator.OnRunEnded += () => runEnded++;

        coordinator.StartRun(0);
        Assert.AreEqual(SessionState.LevelLoading, coordinator.State);

        coordinator.AbortRun();   // same frame: the MVP scene hasn't finished loading yet
        Assert.AreEqual(SessionState.Menu, coordinator.State);

        for (int i = 0; i < 30; i++) yield return null;   // let both scene loads finish

        Assert.AreEqual(0, started, "The aborted level must never start");
        Assert.AreEqual(1, runEnded, "OnRunEnded must fire exactly once");
        Assert.AreEqual(0, levelEnded, "No level ended, because none started");
        Assert.AreEqual(SessionState.Menu, coordinator.State);
    }
}