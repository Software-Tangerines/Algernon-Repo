using UnityEngine;

/*
F5 MinimapRenderer: draws the explored cells into a 256 x 256 texture for the HUD.

Explored cells are opaque white, unexplored are transparent (RevealAll shows every cell). If Distorted is on, the minimap is
mirrored and rotated exactly like the main view; during a blackout it is all black.
Plain C# class owned by ViewDirector, which calls Draw only when something visible changed.
*/
public class MinimapRenderer
{
    public const int Size = 256;

    public Texture2D Texture { get; }
    public bool Distorted { get; set; } = true;

    private readonly Color32[] pixels = new Color32[Size * Size];

    private static readonly Color32 Explored = new Color32(255, 255, 255, 255);
    private static readonly Color32 Unexplored = new Color32(0, 0, 0, 0);
    private static readonly Color32 Black = new Color32(0, 0, 0, 255);

    public MinimapRenderer()
    {
        Texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
    }

    public void Draw(FogGrid grid, CameraState s, bool revealAll)
    {
        bool mirror = Distorted && s.mirrored;
        float rad = (Distorted ? s.rotationDeg : 0f) * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad), sin = Mathf.Sin(rad);
        float side = Mathf.Max(grid.Width, grid.Height);    // keeps cells square on a non-square maze

        for (int py = 0; py < Size; py++)
        {
            for (int px = 0; px < Size; px++)
            {
                Color32 c;
                if (s.blackedOut) c = Black;
                else
                {
                    // Pixel -> offset from the maze centre in cells. Undo the view the way the camera applies it:
                    // the screen flip first, then the camera's rotation.
                    float x = ((px + 0.5f) / Size - 0.5f) * side;
                    float y = ((py + 0.5f) / Size - 0.5f) * side;
                    if (mirror) x = -x;
                    float wx = x * cos - y * sin;
                    float wy = x * sin + y * cos;

                    int cx = Mathf.FloorToInt(wx + grid.Width * 0.5f);
                    int cy = Mathf.FloorToInt(wy + grid.Height * 0.5f);
                    bool inside = cx >= 0 && cy >= 0 && cx < grid.Width && cy < grid.Height;
                    c = inside && (revealAll || grid.IsExplored(cx, cy)) ? Explored : Unexplored;
                }
                pixels[py * Size + px] = c;
            }
        }
        Texture.SetPixels32(pixels);
        Texture.Apply(false);
    }

    public void Destroy()
    {
        if (Application.isPlaying) Object.Destroy(Texture);
        else Object.DestroyImmediate(Texture);
    }
}
