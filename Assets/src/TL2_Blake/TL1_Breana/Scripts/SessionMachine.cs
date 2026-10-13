using UnityEngine;

public class SessionMachine : MonoBehaviour
{
    public SessionState State { get; private set; } = SessionState.MenuState;

    public void TransitionTo(SessionState next)
    {
        Debug.Log($"Transitioning from {State} to {next}");
        State = next;
    }
}