// What F1, F4 and F7 need from F2. Generate(MazeConfig) and ReserveAnchors(...) will be added once TL2 defines those types.
using UnityEngine;

public interface IMazeQuery
{
    bool IsWalkable(Vector2Int cell);
    Vector2Int SpawnCell { get; }
    Vector2Int ExitCell { get; }
    Vector3 CellToWorld(Vector2Int cell);
}
