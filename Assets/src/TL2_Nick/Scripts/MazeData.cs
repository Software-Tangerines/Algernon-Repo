
using UnityEngine;

namespace Algernon.Maze
{
    public class MazeData : MonoBehaviour
    {
        [SerializeField] public MazeConfig Config;
        public int Width;
        public int Height;

        // 2D array containing Cells
        private Cell[,] Grid;

        // Both spawn and exit for the maze
        public Vector2Int spawnCell;
        public Vector2Int exitCell;

        // Constructor
        public MazeData(MazeConfig config)
        {
            Config = config;
            Width = Config.Width;
            Height = Config.Height;

            Grid = new Cell[Config.Width, Config.Height];

            // Create a new Cell with an x and y value for each position
            for (int x = 0; x < Config.Width; x++)
            {
                for (int y = 0; y < Config.Height; y++)
                {
                    Grid[x, y] = new Cell(new Vector2Int(x, y));
                }
            }
        }

        // Gets cells in grid by reference, not value
        public ref Cell cellAt(Vector2Int cell)
        {
            return ref Grid[cell.x, cell.y];
        }

        public void removeWall(Vector2Int a, Vector2Int b)
        {
            WallFlags side = dirBetween(a, b);

            // Remove only the necessary wall
            cellAt(a).Walls &= ~side;

            // Get the opposite wall from the other cell
            side = dirBetween(b, a);
            cellAt(b).Walls &= ~side;
        }

        private static WallFlags dirBetween(Vector2Int from, Vector2Int to)
        {
            // Vector math
            Vector2Int dir = to - from;

            if (dir == Vector2Int.up) return WallFlags.North;
            if (dir == Vector2Int.down) return WallFlags.South;
            if (dir == Vector2Int.right) return WallFlags.East;
            if (dir == Vector2Int.left) return WallFlags.West;

            throw new System.ArgumentException("Cells must be adjacent.");
        }
    }
}
