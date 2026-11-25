using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

// RECURSIVE BACK TRACKING ALGO

namespace Algernon.Maze {

// HOW RBT WORKS
// pick starting cell, mark it as visited
// look at neighbors that havent been visited
// pick one at random
// remove wall from between
// move into new cell
// IF NO (unvisited) NEIGHBORS
// back up prev cell
// check for (unvisited) neighbors again

// inherit from algoselect so we can override methods for right algo
public class RBTalgo : AlgoSelect {

    public override void Carve(MazeData maze, Random rng) {

        // stack will act as where we are
        Stack<Vector2Int> stack = new Stack<Vector2Int>();

        // pick starting cell, mark it as visited
        Vector2Int start = new Vector2Int(0, 0);
        maze.cellAt(start).Visited = true;
        stack.Push(start);

        while (stack.Count > 0) {
            Vector2Int current = stack.Peek();

            // look at unvisited neighbors
            List<Vector2Int> unvisitedNeighbours = getUnvisited(maze, current);

            // if no neighbors, back up prev cell and repeat
            if (unvisitedNeighbours.Count == 0) {
                stack.Pop();
                continue;
            }

            // if unvisited neighbors, pick one at random, remove walls, and move into
            Vector2Int next = unvisitedNeighbours[rng.Next(unvisitedNeighbours.Count)];
            maze.removeWall(current, next);

            maze.cellAt(next).Visited = true;
            stack.Push(next);
        }
    }

    private static List<Vector2Int> getUnvisited(MazeData maze, Vector2Int cell) {
        // make list for unvisited neighbors
        List<Vector2Int> result = new List<Vector2Int>(4);

        // iterates through each possible neighbor of a cell (up/down/left/right)
        foreach (Vector2Int offset in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right }) {
            
            Vector2Int candidate = cell + offset;


            // if not visited, add to list

            if (candidate.x >= 0 && candidate.x < maze.Width && candidate.y >= 0 && candidate.y < maze.Height && !maze.cellAt(candidate).Visited)
                result.Add(candidate);
        }

        // return list
        return result;
    }
}

}
