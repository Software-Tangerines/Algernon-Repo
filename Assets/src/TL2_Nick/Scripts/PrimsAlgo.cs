// AI FUNCTION
// used chatgpt, gave it my RBTalgo file and asked to make it same structure, but instead of rbt, use prims algrithm

// expands from a starting cell, adjacent cells to generated ones, is called "frontier"
// expands from there like roots
// pick random frontier, cell there, repeat until no frontier left

using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

// PRIM'S ALGORITHM

namespace Algernon.Maze {

public class PrimsAlgo : AlgoSelect {

    public override void Carve(MazeData maze, Random rng) {

        // frontier stores cells next to visited cells
        List<Vector2Int> frontier = new List<Vector2Int>();

        // pick starting cell, mark it as visited
        Vector2Int start = new Vector2Int(
            rng.Next(maze.Width),
            rng.Next(maze.Height)
        );

        maze.cellAt(start).Visited = true;

        // add starting cell's unvisited neighbors
        foreach (Vector2Int offset in new[] {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        }) {

            Vector2Int candidate = start + offset;

            if (candidate.x >= 0 &&
                candidate.x < maze.Width &&
                candidate.y >= 0 &&
                candidate.y < maze.Height &&
                !maze.cellAt(candidate).Visited) {

                if (!frontier.Contains(candidate)) {
                    frontier.Add(candidate);
                }
            }
        }

        // keep going while frontier has cells
        while (frontier.Count > 0) {

            // pick random frontier cell
            int randomIndex = rng.Next(frontier.Count);
            Vector2Int current = frontier[randomIndex];

            // remove selected cell from frontier
            frontier.RemoveAt(randomIndex);

            // find visited neighbors
            List<Vector2Int> visitedNeighbours =
                new List<Vector2Int>(4);

            foreach (Vector2Int offset in new[] {
                Vector2Int.up,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right
            }) {

                Vector2Int candidate = current + offset;

                if (candidate.x >= 0 &&
                    candidate.x < maze.Width &&
                    candidate.y >= 0 &&
                    candidate.y < maze.Height &&
                    maze.cellAt(candidate).Visited) {

                    visitedNeighbours.Add(candidate);
                }
            }

            // if there is a visited neighbor
            if (visitedNeighbours.Count > 0) {

                // pick random visited neighbor
                Vector2Int next = visitedNeighbours[
                    rng.Next(visitedNeighbours.Count)
                ];

                // remove wall between cells
                maze.removeWall(current, next);

                // mark current cell as visited
                maze.cellAt(current).Visited = true;

                // add current cell's unvisited neighbors
                foreach (Vector2Int offset in new[] {
                    Vector2Int.up,
                    Vector2Int.down,
                    Vector2Int.left,
                    Vector2Int.right
                }) {

                    Vector2Int candidate = current + offset;

                    if (candidate.x >= 0 &&
                        candidate.x < maze.Width &&
                        candidate.y >= 0 &&
                        candidate.y < maze.Height &&
                        !maze.cellAt(candidate).Visited) {

                        if (!frontier.Contains(candidate)) {
                            frontier.Add(candidate);
                        }
                    }
                }
            }
        }
    }
}

}