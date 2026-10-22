// What F1 needs from F4. SpawnHazards(HazardConfig, ...) will be added once TL2+ defines HazardConfig.
using System;
using UnityEngine;

public interface IHazardSystem
{
    void Tick(float deltaTime, Vector2Int playerCell);   // pushed by F1 each frame
    void SetActive(bool active);                         // false while paused
    void ResetHazards();
    event Action<DeathCause> OnPlayerCaught;
}
