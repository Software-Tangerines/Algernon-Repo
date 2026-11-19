using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System;

// the 'brains' of the operation, high level instructions

namespace Algernon.Maze {

public class MazeGenerator {

    public MazeData Generate(MazeConfig config, Tilemap tilemap, Sprite[] sprites, int reserveCount, int reserveSpacing) {
        System.Random rng = new System.Random(config.Seed);
        MazeData maze = new MazeData(config);

        ReservedCoords rc = new ReservedCoords(maze, config.Seed);
        List<Vector2Int> reserved;

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

        // reserved coords for cheese/puzzles
        reserved = rc.Reserve(reserveCount, reserveSpacing);


        // render maze
        MazeRenderer rend = new MazeRenderer();
        rend.Build(maze, tilemap, sprites);

        return maze;
    }



}

}