using UnityEngine;
using UnityEngine.Tilemaps;
using Algernon.Maze;

public class mazeTest : MonoBehaviour {

    [SerializeField] Tilemap tilemap;
    [SerializeField] Sprite[] sprites = new Sprite[16];
    [SerializeField] int seed = 1234567;
    [SerializeField] int width = 20;
    [SerializeField] int height = 20;
    
    MazeGenerator generator = new MazeGenerator();

    void Start()
    {
        var config = new MazeConfig(seed, width, height);
        MazeData maze = generator.Generate(config, tilemap, sprites);
    }
}