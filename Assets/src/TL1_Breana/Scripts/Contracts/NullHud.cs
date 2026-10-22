// Stand-in until F6 exists: logs instead of drawing, and has no buttons.
using System;
using UnityEngine;

public class NullHud : IHud
{
    public void SetTimer(float secondsRemaining) { }   // called every frame, so no log
    public void ShowPause(bool paused) => Debug.Log($"NullHud: pause screen {(paused ? "shown" : "hidden")}");
    public event Action OnStartPressed { add { } remove { } }
    public event Action OnRetryPressed { add { } remove { } }
    public event Action OnQuitPressed  { add { } remove { } }
}
