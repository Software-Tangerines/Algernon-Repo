using UnityEngine;
using UnityEngine.Tilemaps;
using Algernon.Maze;

public class mazeTest : MonoBehaviour {

    [SerializeField] Tilemap tilemap;
    [SerializeField] Sprite[] sprites = new Sprite[16];
    [SerializeField] int seed = 1234567;
    [SerializeField] int width = 20;
    [SerializeField] int height = 20;
    [SerializeField] int difficulty = 1;
    [SerializeField] int reserveCount = 4;
    [SerializeField] int reserveSpacing = 4;
    
    
    MazeGenerator generator = new MazeGenerator();

    void Start() {
        MazeConfig config = new MazeConfig(seed, width, height, difficulty);
        MazeData maze = generator.Generate(config, tilemap, sprites, reserveCount, reserveSpacing);

        // can access the vector2int reserved list by generator.reserved
        // can access spawn/exit cells by maze.spawnCell and maze.exitCell
        // these vectors are (x coord, y coord) for the maze grid
    }
}