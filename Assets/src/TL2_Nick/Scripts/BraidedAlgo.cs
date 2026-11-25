using System;
using System.Collections.Generic;
using UnityEngine;

// BRAIDED ALGORITHM NO DEAD ENDS
// literally just RBT but with actual post processing
// just goes through and knocks downs walls for dead ends to ensure 1 massive connected maze

namespace Algernon.Maze {

// inhereit from rbtalgo so we can steal carve function
public class BraidedAlgo : RBTalgo {


    public override void PostProcess(MazeData maze, System.Random rng) {

        // 1 = no dead ends, range from 0 to 1
        float braidFactor = 1;

        // loop over the whole grid
        for (int x = 0; x < maze.Width; x++) {
            for (int y = 0; y < maze.Height; y++) {

                Vector2Int cell = new Vector2Int(x, y);

                // neighbors only returns cells with no wall between, so 1 = dead end
                List<Vector2Int> openNeighbors = maze.neighbors(cell);
                if (openNeighbors.Count != 1) 
                    continue;

                // chance to skip
                if (rng.NextDouble() > braidFactor) 
                    continue;

                // find neighbors still behind a wall
                List<Vector2Int> options = new List<Vector2Int>();
                foreach (Vector2Int offset in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right }) {
                    Vector2Int candidate = cell + offset;

                    if (candidate.x >= 0 && candidate.x < maze.Width && candidate.y >= 0 && candidate.y < maze.Height && !openNeighbors.Contains(candidate))
                        options.Add(candidate);
                }

                if (options.Count == 0) 
                    continue;

                // pick a random one and knock the wall out
                Vector2Int target = options[rng.Next(options.Count)];
                maze.removeWall(cell, target);
            }
        }
    }
}

}