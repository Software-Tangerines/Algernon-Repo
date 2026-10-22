using System;

public interface ISessionCoordinator
{
    // Inputs
    void StartRun(int startingLevelIndex);
    void RestartLevel();
    void AdvanceLevel();
    void Pause(bool paused);
    void AbortRun();

    // Outputs
    SessionState State { get; }
    LevelConfig CurrentLevel { get; }
    RunStatistics Statistics { get; }

    // Events
    event Action OnLevelStarted;
    event Action OnLevelEnded;
    event Action OnRunEnded;
}