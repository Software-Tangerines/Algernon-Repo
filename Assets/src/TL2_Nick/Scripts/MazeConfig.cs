using System;
using UnityEngine;

// settings for maze

namespace Algernon.Maze {

public class MazeConfig {
    public int Seed;
    public int Width;
    public int Height;
    public int Diff;

    public MazeConfig(int seed, int width, int height, int diff) {
        Seed = seed;
        Width = width;
        Height = height;
        Diff = diff;
    }
}

}