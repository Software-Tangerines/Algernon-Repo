using UnityEngine;
// using System.Collections.Generic;

public class MazeData : MonoBehaviour {
    [SerializeField] public MazeConfig Config;
    public int Width;
    public int Height;

    // 2d array with type class Cell
    private Cell[,] cells;

    // both spawn and exit for the maze
    public Vector2Int SpawnCell;
    public Vector2Int ExitCell;

    // constructor
    public MazeData(MazeConfig config) {
        Config = config;
        Width = Config.Width;
        Height = Config.Height;
        cells = new Cell[Config.Width, Config.Height];

        // cycles through the new 2d array, and makes a new cell with a x and y value for each
        for (int x = 0; x < Config.Width; x++)
            for (int y = 0; y < Config.Height; y++)
                cells[x, y] = new Cell(new Vector2Int(x, y));
    }
}