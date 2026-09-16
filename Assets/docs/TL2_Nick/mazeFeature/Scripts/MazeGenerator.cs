using UnityEngine;
using System;

public class MazeGenerator : MonoBehaviour {

    // MazeConfig good to go
    // Cell should also be good

    // need to keep working on MazeData and MazeGenerator

    // then work on a single algorithm in MazeAlgorithm and the MazeRenderer
    // can work on anchor points later

    public MazeData Generate(MazeConfig config) {
        var rng = new System.Random(config.Seed);
        var maze = new MazeData(config);

        // MazeAlgorithm algorithm = SelectAlgorithm(config, out bool needsRatio);
        // algorithm.Carve(maze, rng);
        // algorithm.PostProcess(maze, rng);

        // PlaceSpawnAndExit(maze, rng);

        // Current = maze;
        // _renderer?.Build(maze);

        return maze;
    }


    

}
