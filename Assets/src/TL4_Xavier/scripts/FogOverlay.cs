using UnityEngine;

/*
fog overlay draws the foggrid in the world as a black sprite over the maze,  one pixel per cell.

unexlplored pixels are finna be opaque on god. explored pixels are transparent, how dark the fog is overall is the sprites aplha so ramping the fog never will rewrite the pixels or some shit.
it lives in the world so the camera rotation and mirror will be applied automagically 
 plain c shart  class ownded by the view director it will create and destroy its own game objects

*/

public class FogOverlay
{
    private readonly GameObject go;
    private readonly SpriteRenderer sr;
    private readonly Texture2D tex;
    private readonly Color32[] pixels;

    private static readonly Color32 Hidden = new Color32(0, 0, 0, 255);
    private static readonly Color32 Seen = new Color32(0,0,0,0);
    public FogOverlay(FogGrid grid, string sortingLayer, int sortingOrder)
    {
        tex = new Texture2D(grid.Width, grid.Height, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,  // hard cell edges no blur on the edges
            wrapMode = TextureWrapMode.Clamp 
        };
        pixels = new Color32[grid.Width * grid.Height];

        go = new GameObject("F5_Fog");
        sr = go.AddComponent<SpriteRenderer>();
        // pixel per unit is a pixel per cell , gonna place in the middle 
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, grid.Width, grid.Height), new Vector2(0.5f, 0.5f), 1f);
        sr.sortingLayerName = sortingLayer;
        sr.sortingOrder = sortingOrder;

        Vector2 centre = grid.Origin - new Vector2(0.5f, 0.5f) + new Vector2(grid.Width, grid.Height) * 0.5f;
        go.transform.position = new Vector3(centre.x, centre.y, 0f);

        Redraw(grid);
        SetOpacity(0f);
    }

    // this will rewrite every pixel from the grid. call only when cells were revealed or restored
    public void Redraw(FogGrid grid)
    {
        for(int y = 0; y < grid.Height; y++)
            for(int x = 0; x < grid.Width; x++)
                pixels[y * grid.Width + x] = grid.IsExplored(x, y) ? Seen : Hidden;
        tex.SetPixels32(pixels);
        tex.Apply(false);
    }

    // 0 = no visible, 1. = unexplored cells fully black
    public void SetOpacity(float opacity) => sr.color = new Color(1f,1f,1f, Mathf.Clamp01(opacity));

    public void Destroy()
    {
        if(Application.isPlaying){ Object.Destroy(go); Object.Destroy(tex); }
        else { Object.DestroyImmediate(go); Object.DestroyImmediate(tex); } // edit mode test
    }
}

