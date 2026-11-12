
using System;
using System.Collections.Generic;
using UnityEngine;

// RECURSIVE BACK TRACKING ALGO
namespace Algernon.Maze
{
    public class RBTalgo : MazeAlgorithm
    {
        public void Carve(MazeData grid, System.Random rng)
        {
            // Stack will act as where we are
            Stack<Vector2Int> stack = new Stack<Vector2Int>();

            // Pick starting cell, mark it as visited
            Vector2Int start = new Vector2Int(0, 0);
            grid.cellAt(start).Visited = true;
            stack.Push(start);

            while (stack.Count > 0)
            {
                Vector2Int current = stack.Peek();

                // Look at unvisited neighbors
                List<Vector2Int> unvisitedNeighbours = getUnvisited(grid, current);

                // If no neighbors, back up to previous cell
                if (unvisitedNeighbours.Count == 0)
                {
                    stack.Pop();
                    continue;
                }

                // Pick a neighbor at random
                Vector2Int next = unvisitedNeighbours[rng.Next(unvisitedNeighbours.Count)];

                // Remove walls between cells
                grid.removeWall(current, next);

                grid.cellAt(next).Visited = true;
                stack.Push(next);
            }
        }

        protected static List<Vector2Int> getUnvisited(MazeData grid, Vector2Int cell)
        {
            // Make list for unvisited neighbors
            List<Vector2Int> result = new List<Vector2Int>(4);

            // Check each possible neighbor
            foreach (Vector2Int offset in new[]
            {
                Vector2Int.up,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right
            })
            {
                Vector2Int candidate = cell + offset;

                // Check maze boundaries
                if (candidate.x >= 0 &&
                    candidate.x < grid.Width &&
                    candidate.y >= 0 &&
                    candidate.y < grid.Height)
                {
                    if (!grid.cellAt(candidate).Visited)
                    {
                        result.Add(candidate);
                    }
                }
            }

            return result;
        }
    }
}
