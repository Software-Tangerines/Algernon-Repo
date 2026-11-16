using UnityEngine;
using System;

// the 'brains' of the operation, high level instructions

namespace Algernon.Maze {

public class MazeGenerator : MonoBehaviour {

    public MazeData Generate(MazeConfig config) {
        System.Random rng = new System.Random(config.Seed);
        MazeData maze = new MazeData(config);

        // hardcoding rbt rn cause i can't be bothered
        RBTalgo algo = new RBTalgo();

        // carve
        algo.Carve(maze, rng);
        // post process only matters when braided algo goes, IMPLEMENT LATER

        // place spawn and exit
        MazeData.placeSpawnExit(maze, rng);

        // solvability
        if (MazeData.solvable(maze, maze.spawnCell, maze.exitCell))
            Debug.Log("good luck");
        else
            throw new Exception("Maze no solvable");


        // reserved coords and anchors?
        // render maze

        return maze;
    }



}

}