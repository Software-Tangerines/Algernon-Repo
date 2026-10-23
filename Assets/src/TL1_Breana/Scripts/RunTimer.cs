using System;

// Countdown for the active level. Plain C# class, owned by GameCoordinator.
// GameCoordinator calls Advance only while Playing, which is how pausing halts the timer.
public class RunTimer
{
    public float RemainingSeconds { get; private set; }
    public bool IsExpired => RemainingSeconds <= 0f;

    // Starts the countdown. A zero or negative limit means the level is already out of time (Task 5.3).
    public void Reset(float limitSeconds)
    {
        RemainingSeconds = Math.Max(0f, limitSeconds);
    }

    // Counts down by one frame. Clamps at exactly 0.0 so TimedSurvivalRule sees a clean zero.
    public void Advance(float deltaTime)
    {
        if (deltaTime <= 0f || IsExpired)
        {
            return;
        }
        RemainingSeconds = Math.Max(0f, RemainingSeconds - deltaTime);
    }
}
