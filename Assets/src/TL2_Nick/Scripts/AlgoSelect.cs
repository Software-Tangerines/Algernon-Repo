using System;
using UnityEngine;

// not actual maze algorithm, but instead the file or fascade that selects the right algo based on difficulty
// part of the gang of four pattern stuff

namespace Algernon.Maze {
 
public abstract class AlgoSelect {

    // carving the maze
    public abstract void Carve(MazeData maze, System.Random rng);

    // dynamic binding, braided algo will use this later with override
    public virtual void PostProcess(MazeData maze, System.Random rng){
        Debug.Log("Posting the process");
    }
}
}
