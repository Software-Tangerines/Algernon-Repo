using System.Collections.Generic;
using UnityEngine;

public class SessionMachine
{
    public SessionState State { get; private set; } = SessionState.Boot;

    // Legal transitions: each state maps to the states it may move to.
    private static readonly Dictionary<SessionState, HashSet<SessionState>> allowed = new()
    {
        { SessionState.Boot,         new HashSet<SessionState> { SessionState.Menu } },
        { SessionState.Menu,         new HashSet<SessionState> { SessionState.LevelLoading } },
        { SessionState.LevelLoading, new HashSet<SessionState> { SessionState.Playing, SessionState.Menu } },
        { SessionState.Playing,      new HashSet<SessionState> { SessionState.Paused, SessionState.Won, SessionState.Lost, SessionState.LevelLoading, SessionState.Menu } },
        { SessionState.Paused,       new HashSet<SessionState> { SessionState.Playing, SessionState.LevelLoading, SessionState.Menu } },
        { SessionState.Won,          new HashSet<SessionState> { SessionState.LevelLoading, SessionState.RunComplete } },
        { SessionState.Lost,         new HashSet<SessionState> { SessionState.LevelLoading, SessionState.Menu } },
        { SessionState.RunComplete,  new HashSet<SessionState> { SessionState.Menu } },
    };

    // Returns false and leaves State unchanged if the transition is illegal; the caller decides whether to log or throw.
    public bool TransitionTo(SessionState next)
    {
        if (!(allowed.TryGetValue(State, out var targets) && targets.Contains(next)))
        {
            return false;
        }

        Debug.Log($"Transitioning from {State} to {next}");
        State = next;
        return true;
    }
}
