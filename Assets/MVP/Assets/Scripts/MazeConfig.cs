using System;
using UnityEngine;

public class MazeConfig : MonoBehaviour {
    public int Seed;
    public int Width;
    public int Height;
    //public int Difficulty; IMPLMEMENET LATER

    public MazeConfig(int seed, int width, int height/*, int difficulty*/) {
        Seed = seed;
        Width = width;
        Height = height;
        //Difficulty = difficulty;
    }
}
