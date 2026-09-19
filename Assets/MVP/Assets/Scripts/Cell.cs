using System;
using UnityEngine;

public class Cell : MonoBehaviour {

    // need to track, where it is, if its been visited, and where the walls are
    public Vector2Int Position;
    // for backtracking later on
    public bool Visited;
    public WallFlags Walls;

    // constructor
    public Cell(Vector2Int position) {
        Position = position;
        Visited = false;
        // we enable all walls right now, and we will remove them as we make the paths
        Walls = WallFlags.All;
    }

    [Flags]
    public enum WallFlags {
        None = 0,
        North = 1 << 0,
        East = 1 << 1,
        South = 1 << 2,
        West = 1 << 3,
        All = North | East | South | West
    }
}
