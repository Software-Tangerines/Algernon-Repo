using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System;

// the 'brains' of the operation, high level instructions

namespace Algernon.Maze {

public class MazeGenerator {

    public MazeData Generate(MazeConfig config, Tilemap tilemap, Sprite[] sprites, int reserveCount, int reserveSpacing) {
        System.Random seed = new System.Random(config.Seed);
        MazeData maze = new MazeData(config);

        ReservedCoords rc = new ReservedCoords(maze, config.Seed);
        List<Vector2Int> reserved;

        // hardcoding rbt rn cause i can't be bothered
        // RBTalgo algo = new RBTalgo();
        AlgoSelect algo;

        // braided and prims algo could be swtiched diffiuclty wise, its debatable
        if (config.Diff == 1)
            algo = new BraidedAlgo();
        else if (config.Diff == 2)
            algo = new PrimsAlgo();
        else if (config.Diff == 3)
            algo = new RBTalgo();
        else
            throw new Exception("Difficulty from 1-3, ints only");

        // carve
        algo.Carve(maze, seed);
        // post process only matters when braided algo goes, IMPLEMENT LATER
        algo.PostProcess(maze, seed);

        // place spawn and exit
        MazeData.placeSpawnExit(maze, seed);

        // solvability
        if (MazeData.solvable(maze, maze.spawnCell, maze.exitCell))
            Debug.Log("good luck");
        else
            throw new Exception("Maze no solvable");

        // reserved coords for cheese/puzzles
        reserved = rc.Reserve(reserveCount, reserveSpacing);


        // render maze
        MazeRenderer rend = new MazeRenderer();
        rend.Build(maze, tilemap, sprites);

        return maze;
    }



}

}