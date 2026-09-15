using System;
using UnityEngine;

public class MazeConfig : MonoBehaviour {
    public int Diff;
    public int Seed;

    public MazeConfig(int diff, int seed) {
        Diff = diff;
        Seed = seed;
    }
}
