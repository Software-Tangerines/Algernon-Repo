// What F1 needs from F6. ShowResult(LevelResult, RunStatistics) will be added in Week 2.
using System;

public interface IHud
{
    void SetTimer(float secondsRemaining);
    void ShowPause(bool paused);
    event Action OnStartPressed;
    event Action OnRetryPressed;
    event Action OnQuitPressed;
}
