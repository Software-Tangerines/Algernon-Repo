// What F1 needs from F7. Matches Gavin's contract.
using UnityEngine;

public interface IPlayerCharacter
{
    void SpawnAt(Vector3 worldPosition);
    Vector2Int CurrentCell { get; }
    bool IsAlive { get; }               // false when player dies
    void Kill(DeathCause cause);        // called by F1 only, never by F4
    void EnableInput(bool enabled);     // false while paused
}
