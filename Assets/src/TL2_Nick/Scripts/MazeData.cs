using UnityEngine;
// using System.Collections.Generic;

namespace Algernon.Maze {

public class MazeData : MonoBehaviour {
    [SerializeField] public MazeConfig Config;
    public int Width;
    public int Height;

    // 2d array with type class Cell
    private Cell[,] Grid;

    // both spawn and exit for the maze
    public Vector2Int spawnCell;
    public Vector2Int exitCell;

    // constructor
    public MazeData(MazeConfig config) {
        Config = config;
        Width = Config.Width;
        Height = Config.Height;
        Grid = new Cell[Width, Height];

        // cycles through the new 2d array, and makes a new cell with a x and y value for each
        for (int x = 0; x < Config.Width; x++)
            for (int y = 0; y < Config.Height; y++)
                Grid[x, y] = new Cell(new Vector2Int(x, y));
    }

    // gets cells in grid by REFERENCE NOT VALUE
    public Cell cellAt(Vector2Int cell) {
        return Grid[cell.x, cell.y];
    }

    public void removeWall(Vector2Int a, Vector2Int b){
        WallFlags side = dirBetween(a, b);
        // logical AND operator with complement operator to remove only the side necessary and leave the rest alone
        cellAt(a).Walls &= ~side;
        // get other wall from other cell and remove that
        side = dirBetween(b, a);
        cellAt(b).Walls &= ~side;
    }
    
    private static WallFlags dirBetween(Vector2Int from, Vector2Int to){
        // vector math
        Vector2Int dir = to - from;
        if (dir == Vector2Int.up) return WallFlags.North;
        if (dir == Vector2Int.down) return WallFlags.South;
        if (dir == Vector2Int.right) return WallFlags.East;
        if (dir == Vector2Int.left) return WallFlags.West;
        else 
            return WallFlags.North; // FIX LATER
    }
}


}