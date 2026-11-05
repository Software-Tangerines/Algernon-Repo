using System.Collections;
using UnityEngine;

/*
fog grid part of my feature, distorts cells with fog
class will be owned by view director, Framemaze creates a new grid sized to the maze . All unexplored
each frame viewdirector will call reveail around the mouses cell, a revelaed cell will stay revielfed for the reest of the level
. only goes for 0-1 so to set fog you would do somethign like 0.5 ect... 

cell size is 1 one world unit based off feature 2s grid 

*/

public class FogGrid
{
    public const float MaxVisionRadius = 50f; // contract: setVisionRadius clamps to [0,50]

    public int Width { get; }
    public int  Height {get;}
    public Vector2 Origin {get;}

    public int ExploredCount {get; private set;}
    public float ExploredFraction => (float)ExploredCount / (Width * Height);

    private BitArray explored; // index = y * Width +x
    
    public FogGrid(int width, int height, Vector2 origin)
    {
        Width = Mathf.Max(1, width);
        Height = Mathf.Max(1, height);
        Origin = origin;
        explored = new BitArray(Width * Height);
    }

    // outside of the grid counts as unexplored
    public bool IsExplored(int x, int y) =>
        x >= 0 && y >= 0 && x < Width && y < Height && explored[y* Width + x];

    //the cell world position falls in migfht be outside the grid 
    public Vector2Int WorldToCell(Vector2 world) => Vector2Int.RoundToInt(world - Origin);

    //marks every cell whose centre is withing the radius of center a exlored
    // returns how many cells were NEW in this call (0 = nothing changed).  radius of 0 reveals nothign
    public int Reveal(Vector2Int center, float radius)
    {
        if (!(radius > 0f)) return 0; // will also catch if not a number
        int r = Mathf.CeilToInt(radius);
        float r2 = radius * radius;
        int added = 0;

        for (int y = center.y - r; y <= center.y + r; y++)
        {
            if (y < 0 || y >= Height) continue;
            for(int x = center.x - r; x <= center.x + r; x++)
            {
                if(x < 0 || x >= Width) continue;
                int dx = x - center.x, dy = y - center.y;
                if( (dx * dx) + (dy * dy) > r2 ) continue; // this might be the cause of an error or bug not sure how c# handles greater than precendence in mathmatical operations so i put in those parens 

                int i = y * Width + x;
                if (explored[i]) continue;
                explored[i] = true;
                added++;
            }
        }
        ExploredCount +=added;
        return added;
    }

    //copy for the camera snapshot
    public BitArray CopyExplored() => new BitArray(explored);

    //replace the explored bits to the restore state returns false if the size is wrong and nothing changes 
    public bool LoadExplored(BitArray bits)
    {
        if(bits == null || bits.Length != explored.Length) return false;
        explored = new BitArray(bits);
        ExploredCount = 0;
        for(int i = 0; i < explored.Length; i++)
            if(explored[i]) ExploredCount++;
        return true;
    }
}