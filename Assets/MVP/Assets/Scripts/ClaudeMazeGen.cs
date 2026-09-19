using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates a simple TRUE 2D maze (XY plane) using recursive backtracking,
/// and builds it out of SpriteRenderer quads. No external sprite assets
/// required - a 1x1 white pixel texture is generated at runtime, so it
/// works out of the box. Assign your own sprites if you want custom art.
///
/// Setup:
/// 1. Set your Main Camera to Orthographic.
/// 2. Create an empty GameObject in your scene, add this script.
/// 3. Set Width / Height / Cell Size in the inspector.
/// 4. Press Play - the maze is generated at Start().
/// </summary>
public class MazeGenerator2D : MonoBehaviour
{
    [Header("Maze Dimensions")]
    [Min(2)] public int width = 10;
    [Min(2)] public int height = 10;
    public float cellSize = 1f;

    [Header("Sprites (optional - leave empty to auto-generate)")]
    public Sprite wallSprite;
    public Sprite floorSprite;
    public Color wallColor = Color.white;
    public Color floorColor = new Color(0.15f, 0.15f, 0.15f);

    [Header("Wall Appearance")]
    public float wallThickness = 0.1f;

    [Header("Sorting")]
    public string sortingLayerName = "Default";
    public int floorSortingOrder = 0;
    public int wallSortingOrder = 1;

    [Header("Generation")]
    public int seed = 0;
    public bool useRandomSeed = true;

    private Cell[,] _cells;
    private System.Random _rng;
    private Transform _mazeRoot;
    private static Sprite _generatedPixelSprite;

    private class Cell
    {
        public bool visited = false;
        public bool wallUp = true;
        public bool wallDown = true;
        public bool wallLeft = true;
        public bool wallRight = true;
    }

    private void Start()
    {
        Generate();
    }

    [ContextMenu("Generate Maze")]
    public void Generate()
    {
        ClearMaze();

        _rng = useRandomSeed ? new System.Random() : new System.Random(seed);
        _cells = new Cell[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                _cells[x, y] = new Cell();

        CarveMaze(0, 0);
        BuildMazeGeometry();
    }

    // ---------- Maze Algorithm: Recursive Backtracking ----------

    private void CarveMaze(int startX, int startY)
    {
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        Vector2Int current = new Vector2Int(startX, startY);
        _cells[current.x, current.y].visited = true;
        stack.Push(current);

        while (stack.Count > 0)
        {
            current = stack.Peek();
            List<Vector2Int> neighbours = GetUnvisitedNeighbours(current);

            if (neighbours.Count == 0)
            {
                stack.Pop();
                continue;
            }

            Vector2Int next = neighbours[_rng.Next(neighbours.Count)];
            RemoveWallBetween(current, next);

            _cells[next.x, next.y].visited = true;
            stack.Push(next);
        }
    }

    private List<Vector2Int> GetUnvisitedNeighbours(Vector2Int cell)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        Vector2Int[] candidates =
        {
            new Vector2Int(cell.x, cell.y + 1), // up
            new Vector2Int(cell.x, cell.y - 1), // down
            new Vector2Int(cell.x - 1, cell.y), // left
            new Vector2Int(cell.x + 1, cell.y), // right
        };

        foreach (var c in candidates)
        {
            if (c.x >= 0 && c.x < width && c.y >= 0 && c.y < height && !_cells[c.x, c.y].visited)
                result.Add(c);
        }

        return result;
    }

    private void RemoveWallBetween(Vector2Int a, Vector2Int b)
    {
        if (b.x == a.x && b.y == a.y + 1) // b is above a
        {
            _cells[a.x, a.y].wallUp = false;
            _cells[b.x, b.y].wallDown = false;
        }
        else if (b.x == a.x && b.y == a.y - 1) // b is below a
        {
            _cells[a.x, a.y].wallDown = false;
            _cells[b.x, b.y].wallUp = false;
        }
        else if (b.x == a.x - 1 && b.y == a.y) // b is left of a
        {
            _cells[a.x, a.y].wallLeft = false;
            _cells[b.x, b.y].wallRight = false;
        }
        else if (b.x == a.x + 1 && b.y == a.y) // b is right of a
        {
            _cells[a.x, a.y].wallRight = false;
            _cells[b.x, b.y].wallLeft = false;
        }
    }

    // ---------- Geometry Building (2D) ----------

    private void ClearMaze()
    {
        Transform existing = transform.Find("GeneratedMaze2D");
        if (existing != null)
        {
            if (Application.isPlaying) Destroy(existing.gameObject);
            else DestroyImmediate(existing.gameObject);
        }
    }

    private void BuildMazeGeometry()
    {
        GameObject root = new GameObject("GeneratedMaze2D");
        root.transform.SetParent(transform, false);
        _mazeRoot = root.transform;

        SpawnFloor();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = _cells[x, y];
                Vector3 cellCenter = new Vector3(x * cellSize, y * cellSize, 0f);

                if (cell.wallUp)
                    SpawnWall(cellCenter + new Vector3(0, cellSize / 2f, 0), true);

                if (cell.wallLeft)
                    SpawnWall(cellCenter + new Vector3(-cellSize / 2f, 0, 0), false);

                // Right border wall (only needed for last column)
                if (x == width - 1 && cell.wallRight)
                    SpawnWall(cellCenter + new Vector3(cellSize / 2f, 0, 0), false);

                // Bottom border wall (only needed for first row)
                if (y == 0 && cell.wallDown)
                    SpawnWall(cellCenter + new Vector3(0, -cellSize / 2f, 0), true);
            }
        }
    }

    private void SpawnFloor()
    {
        GameObject floor = new GameObject("Floor");
        floor.transform.SetParent(_mazeRoot, false);

        SpriteRenderer sr = floor.AddComponent<SpriteRenderer>();
        sr.sprite = floorSprite != null ? floorSprite : GetPixelSprite();
        sr.color = floorColor;
        sr.sortingLayerName = sortingLayerName;
        sr.sortingOrder = floorSortingOrder;

        float centerX = (width - 1) * cellSize / 2f;
        float centerY = (height - 1) * cellSize / 2f;
        floor.transform.position = new Vector3(centerX, centerY, 0f);

        // Pixel sprite is 1x1 unit by default (8 PPU on an 8x8 texture).
        float sizeX = width * cellSize;
        float sizeY = height * cellSize;
        if (floorSprite == null)
        {
            floor.transform.localScale = new Vector3(sizeX, sizeY, 1f);
        }
        else
        {
            Vector2 spriteSize = floorSprite.bounds.size;
            floor.transform.localScale = new Vector3(sizeX / spriteSize.x, sizeY / spriteSize.y, 1f);
        }
    }

    /// <param name="horizontal">True if wall runs along X axis (horizontal segment), false if along Y axis (vertical segment).</param>
    private void SpawnWall(Vector3 position, bool horizontal)
    {
        GameObject wall = new GameObject("Wall");
        wall.transform.SetParent(_mazeRoot, false);
        wall.transform.position = position;

        SpriteRenderer sr = wall.AddComponent<SpriteRenderer>();
        sr.sprite = wallSprite != null ? wallSprite : GetPixelSprite();
        sr.color = wallColor;
        sr.sortingLayerName = sortingLayerName;
        sr.sortingOrder = wallSortingOrder;

        Vector2 baseSize = wallSprite != null ? (Vector2)wallSprite.bounds.size : Vector2.one;
        Vector3 targetSize = horizontal
            ? new Vector3(cellSize, wallThickness, 1f)
            : new Vector3(wallThickness, cellSize, 1f);

        wall.transform.localScale = new Vector3(targetSize.x / baseSize.x, targetSize.y / baseSize.y, 1f);

        // Add a BoxCollider2D so the maze is usable for movement/collision.
        BoxCollider2D col = wall.AddComponent<BoxCollider2D>();
        col.size = baseSize; // matches the sprite's native size; scaling handles the rest
    }

    /// <summary>
    /// Generates (once) a simple white pixel sprite so the maze
    /// works without needing any imported art assets.
    /// </summary>
    private static Sprite GetPixelSprite()
    {
        if (_generatedPixelSprite != null) return _generatedPixelSprite;

        Texture2D tex = new Texture2D(8, 8);
        Color[] pixels = new Color[8 * 8];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
        tex.SetPixels(pixels);
        tex.filterMode = FilterMode.Point;
        tex.Apply();

        // 8x8 texture at 8 pixels-per-unit => 1x1 world unit sprite.
        _generatedPixelSprite = Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 8f);
        return _generatedPixelSprite;
    }

    // ---------- Gizmos for quick preview in Editor ----------

    private void OnDrawGizmosSelected()
    {
        if (_cells == null) return;

        Gizmos.color = Color.green;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 cellCenter = transform.position + new Vector3(x * cellSize, y * cellSize, 0);
                Cell cell = _cells[x, y];

                if (cell.wallUp)
                    Gizmos.DrawLine(cellCenter + new Vector3(-cellSize / 2f, cellSize / 2f, 0),
                                     cellCenter + new Vector3(cellSize / 2f, cellSize / 2f, 0));
                if (cell.wallLeft)
                    Gizmos.DrawLine(cellCenter + new Vector3(-cellSize / 2f, -cellSize / 2f, 0),
                                     cellCenter + new Vector3(-cellSize / 2f, cellSize / 2f, 0));
                if (x == width - 1 && cell.wallRight)
                    Gizmos.DrawLine(cellCenter + new Vector3(cellSize / 2f, -cellSize / 2f, 0),
                                     cellCenter + new Vector3(cellSize / 2f, cellSize / 2f, 0));
                if (y == 0 && cell.wallDown)
                    Gizmos.DrawLine(cellCenter + new Vector3(-cellSize / 2f, -cellSize / 2f, 0),
                                     cellCenter + new Vector3(cellSize / 2f, -cellSize / 2f, 0));
            }
        }
    }
}