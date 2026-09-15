using UnityEngine;

public class MazeData : MonoBehaviour {
    public int Width;
    public int Height;

    // 2d array with type class Cell
    private Cell[,] cells;

    // both spawn and exit for the maze
    public Vector2Int SpawnCell;
    public Vector2Int ExitCell;

    // constructor
    public MazeData(int width, int height) {
        Width = width;
        Height = height;
        cells = new Cell[width, height];

        // cycles through the new 2d array, and makes a new cell with a x and y value for each
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                cells[x, y] = new Cell(new Vector2Int(x, y));
    }
}
