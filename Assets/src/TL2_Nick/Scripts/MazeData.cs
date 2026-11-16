using System;
using UnityEngine;
using Random = System.Random;
using System.Collections.Generic;

// maze info and nitty gritty details
// PLUS ALL THE HELPER FUNCTIONS

namespace Algernon.Maze {

public class MazeData : MonoBehaviour {
    public MazeConfig Config;
    public int Width;
    public int Height;

    // 2d array with type class Cell for maze itself
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

    // takes 2 walls and removes the wall between them
    public void removeWall(Vector2Int a, Vector2Int b){
        WallFlags side = dirBetween(a, b);
        // logical AND operator with complement operator to remove only the side necessary and leave the rest alone
        cellAt(a).Walls &= ~side;
        // get other wall from other cell and remove that
        side = dirBetween(b, a);
        cellAt(b).Walls &= ~side;
    }
    
    // takes 2 cells and finds the wall between them
    public static WallFlags dirBetween(Vector2Int from, Vector2Int to){
        // vector math
        Vector2Int dir = to - from;
        if (dir == Vector2Int.up) return WallFlags.North;
        if (dir == Vector2Int.down) return WallFlags.South;
        if (dir == Vector2Int.right) return WallFlags.East;
        if (dir == Vector2Int.left) return WallFlags.West;
        else 
            return WallFlags.North; // FIX LATER
    }

    // places spawn at origin, places exit at furthest possible loc
    public static void placeSpawnExit(MazeData maze, System.Random rng) {
        Vector2Int spawn = new Vector2Int(0, 0);
        maze.spawnCell = spawn;
        // exit cell is farthest from spawn, so mazes aren't easy
        maze.exitCell = furthestCell(maze, spawn);
    }

    // 2 cells, finds furthest cell from first
    // BFS WITH EXTRA STEPS
    public static Vector2Int furthestCell(MazeData maze, Vector2Int from) {

            // dictionary for cell: distance
            Dictionary<Vector2Int, int> visited = new Dictionary<Vector2Int, int>();
            visited.Add(from, 0);

            // queue for holding cells
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            queue.Enqueue(from);
 
            // initial vars
            Vector2Int furthest = from;
            int furthestDist = 0;
            int dist = 0;
 
            while (queue.Count > 0) {
                // take new cell for consideration
                Vector2Int current = queue.Dequeue();
                dist = visited[current];
 
                // if dist is greatest, set it and furthesetdist
                if (dist > furthestDist) {
                    furthestDist = dist;
                    furthest = current;
                }
 
                foreach (Vector2Int next in maze.neighbors(current)) {
                    // skip over if dict contains next cell
                    if (visited.ContainsKey(next)) 
                        continue;
                    // enqueue next cells and increment dist
                    visited[next] = dist + 1;
                    queue.Enqueue(next);
                }
            }
 
            return furthest;
        }

    // takes a cell and gets the neighbors in the form of a list, depending on the walls
    public List<Vector2Int> neighbors(Vector2Int cell){
        List<Vector2Int> neighbors = new List<Vector2Int>();
        Cell curr = cellAt(cell);
        if ((curr.Walls & WallFlags.North) == 0)
            neighbors.Add(cell + Vector2Int.up);
        if ((curr.Walls & WallFlags.South) == 0)
            neighbors.Add(cell + Vector2Int.down);
        if ((curr.Walls & WallFlags.East) == 0)
            neighbors.Add(cell + Vector2Int.right);
        if ((curr.Walls & WallFlags.West) == 0)
            neighbors.Add(cell + Vector2Int.left);
        return neighbors;
    }

    // DETERMINES IF PATH BETWEEN 2 CELLS, LITERALLY JUST BFS
    public static bool solvable(MazeData maze, Vector2Int from, Vector2Int to) {

        // hashset allows no duplicates
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        visited.Add(from);
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(from);

        // iterates through queue
        while (queue.Count > 0) {

            Vector2Int current = queue.Dequeue();

            // gets neighbors, if neighbor is destination, return, else enqueue neighbor, repeat
            foreach (Vector2Int next in maze.neighbors(current)) {
                if (next == to) 
                    return true;
                if (visited.Add(next)) 
                    queue.Enqueue(next);
            }

        }

        return false;
    }
}


}