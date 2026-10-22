// Stand-in until F4 exists: nothing ever kills the player.
using System;
using UnityEngine;

public class NullHazards : IHazardSystem
{
    public void Tick(float deltaTime, Vector2Int playerCell) { }
    public void SetActive(bool active) { }
    public void ResetHazards() { }
    public event Action<DeathCause> OnPlayerCaught { add { } remove { } }   // never fires
}
