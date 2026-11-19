
using System.Collections.Generic;
using UnityEngine;

// i know this is messy
// probably janky asf with small mazes, but just lower count and/or spacing
 
namespace Algernon.Maze {

public class ReservedCoords {     
    int maxAttempts = 50;

    MazeData maze;
    System.Random rng;
    List<Vector2Int> reserved = new List<Vector2Int>();

    public ReservedCoords(MazeData Maze, int seed) {
        maze = Maze;
        rng = new System.Random(seed);
    }

    public List<Vector2Int> Reserve(int count, int spacing) {
        List<Vector2Int> result = new List<Vector2Int>();

        reserved.Add(maze.spawnCell);
        reserved.Add(maze.exitCell);

        // iterate through how many reserved spots we want, and attempt to add a spot 50 times, since its random could just be boned
        for (int i = 0; i < count; i++) {
            for (int attempt = 0; attempt < maxAttempts; attempt++) {

                Vector2Int candidate = new Vector2Int(rng.Next(maze.Width), rng.Next(maze.Height));

                // if candidate isn't in reserved, and is far enoguh away from anything in reserved, place it, reserve it, and then break the attempt cycle
                // can fail, but with small enough mazes
                if (!reserved.Contains(candidate)) {
                    foreach (Vector2Int cell in reserved) {
                        if (Mathf.Abs(candidate.x - cell.x) < spacing && Mathf.Abs(candidate.y - cell.y) < spacing) {
                            reserved.Add(candidate);
                            result.Add(candidate);
                            break;
                        }
                    }
                }
            }
        }

        return result;
    }
}
}