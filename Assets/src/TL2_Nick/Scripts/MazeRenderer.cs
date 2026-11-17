using UnityEngine;
using UnityEngine.Tilemaps;
 
namespace Algernon.Maze {

    public class MazeRenderer {

 
        public void Build(MazeData maze, Tilemap tilemap, Sprite[] sprites) {
 
            Tile[] tiles = new Tile[16];

            tilemap.ClearAllTiles();
 
            for (int x = 0; x < maze.Width; x++) {
                for (int y = 0; y < maze.Height; y++) {
                    var cell = new Vector2Int(x, y);
                    int wallMask = (int)maze.cellAt(cell).Walls;
 
                    if (tiles[wallMask] == null) {
                        var tilex = ScriptableObject.CreateInstance<Tile>();
                        tilex.sprite = sprites[wallMask];
                        tiles[wallMask] = tilex;
                    }

                    TileBase tile = tiles[wallMask];

                    tilemap.SetTile(new Vector3Int(x, y, 0), tile);
                }
            }
        }
    }
}
 