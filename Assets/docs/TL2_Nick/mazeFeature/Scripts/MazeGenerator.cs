using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
// TOP PRIORITIES
// in mazedata
    MazeData Generate(MazeConfig config) { get; }
//
    //bool IsWalkable(Vector2Int cell);

// LATER AFTER MVP
    MazeData Current { get; }
    // Vector2Int SpawnCell { get; }
    // Vector2Int ExitCell { get; }
    // Bounds WorldBounds { get; }
    // Vector3 CellToWorld(Vector2Int cell);
    // Vector2Int WorldToCell(Vector3 worldPosition);
    // IReadOnlyList<Vector2Int> ReserveAnchors(AnchorKind kind, int count);
    // bool PathExists(Vector2Int from, Vector2Int to);
    // void Clear();

}
