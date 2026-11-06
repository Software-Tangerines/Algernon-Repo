// Stand-in until F7 exists: no mouse on screen, stays on its spawn cell.
using UnityEngine;

public class NullPlayer : IPlayerCharacter
{
    public bool IsAlive => true;
    public Vector2Int CurrentCell { get; private set; }

    public void SpawnAt(Vector3 worldPosition)
    {
        CurrentCell = new Vector2Int(Mathf.FloorToInt(worldPosition.x), Mathf.FloorToInt(worldPosition.y));
        Debug.Log($"NullPlayer: spawned at {CurrentCell}");
    }

    public void Kill(DeathCause cause) => Debug.Log($"NullPlayer: killed by {cause}");
    public void EnableInput(bool enabled) { }
}
