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
// IF NO NEIGHBORS
// back up prev cell
// check for neighbors again

public class RBTalgo : MonoBehaviour {

    public void Carve(MazeData grid, Random rng) {

        // stack will act as where we are
        Stack<Vector2Int> stack = new Stack<Vector2Int>();

        // pick starting cell, mark it as visited
        Vector2Int start = new Vector2Int(0, 0);
        grid.cellAt(start).Visited = true;
        stack.Push(start);

        while (stack.Count > 0) {
            Vector2Int current = stack.Peek();

            // look at unvisited neighbors
            List<Vector2Int> unvisitedNeighbours = getUnvisited(grid, current);

            // if no neighbors, back up prev cell and repeat
            if (unvisitedNeighbours.Count == 0) {
                stack.Pop();
                continue;
            }

            // if unvisited neighbors, pick one at random, remove walls, and move into
            Vector2Int next = unvisitedNeighbours[rng.Next(unvisitedNeighbours.Count)];
            grid.removeWall(current, next);

            grid.cellAt(next).Visited = true;
            stack.Push(next);
        }
    }

    protected static List<Vector2Int> getUnvisited(MazeData grid, Vector2Int cell) {
        // make list for unvisited neighbors
        List<Vector2Int> result = new List<Vector2Int>(4);

        // iterates through each possible neighbor of a cell (up/down/left/right)
        foreach (Vector2Int offset in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right }) {
            Vector2Int candidate = cell + offset;

            // if not visited, add to list
            if (!grid.cellAt(candidate).Visited)
                result.Add(candidate);
        }

        // return list
        return result;
    }
}

}
