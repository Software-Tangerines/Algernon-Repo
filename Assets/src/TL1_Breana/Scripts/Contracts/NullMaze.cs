// Stand-in until F2 exists: an open 5x5 grid, spawn bottom-left, exit top-right.
using UnityEngine;

public class NullMaze : IMazeQuery
{
    private const int Size = 5;

    public bool IsWalkable(Vector2Int cell)
    {
        return cell.x >= 0 && cell.x < Size && cell.y >= 0 && cell.y < Size;
    }

    public Vector2Int SpawnCell => new Vector2Int(0, 0);
    public Vector2Int ExitCell => new Vector2Int(Size - 1, Size - 1);

    public Vector3 CellToWorld(Vector2Int cell)
    {
        return new Vector3(cell.x, cell.y, 0f);
    }
}
