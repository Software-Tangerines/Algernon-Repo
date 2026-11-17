
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.TestTools;

public class CollisionTests
{
    private GameObject gridObject;
    private GameObject playerObject;
    private Tilemap tilemap;
    private Rigidbody2D rb;
    private Tile wallTile;

    private Vector2 startPosition = new Vector2(0.5f, 0.5f);

    [UnitySetUp]
    public IEnumerator Setup()
    {
        gridObject = new GameObject("Test Grid");
        Grid grid = gridObject.AddComponent<Grid>();

        GameObject tilemapObject = new GameObject("Test Tilemap");
        tilemapObject.transform.SetParent(gridObject.transform);

        tilemap = tilemapObject.AddComponent<Tilemap>();
        tilemapObject.AddComponent<TilemapRenderer>();
        tilemapObject.AddComponent<TilemapCollider2D>();

        wallTile = ScriptableObject.CreateInstance<Tile>();
        wallTile.colliderType = Tile.ColliderType.Grid;

        playerObject = new GameObject("Test Player");
        playerObject.transform.position = startPosition;

        rb = playerObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;

        playerObject.AddComponent<BoxCollider2D>();

        Physics2D.SyncTransforms();

        yield return new WaitForFixedUpdate();
    }

    [UnityTearDown]
    public IEnumerator Cleanup()
    {
        Object.Destroy(playerObject);
        Object.Destroy(gridObject);
        Object.Destroy(wallTile);

        yield return null;
    }

    private void PlaceWall(Vector2Int cell)
    {
        tilemap.SetTile(
            new Vector3Int(cell.x, cell.y, 0),
            wallTile
        );

        Physics2D.SyncTransforms();
    }

    private IEnumerator MovePlayer(Vector2 direction)
    {
        rb.linearVelocity = direction * 3f;

        for (int i = 0; i < 60; i++)
            yield return new WaitForFixedUpdate();

        rb.linearVelocity = Vector2.zero;
    }

    [UnityTest]
    public IEnumerator CollisionRightTest()
    {
        PlaceWall(new Vector2Int(1, 0));

        yield return MovePlayer(Vector2.right);

        Assert.Less(rb.position.x, 1f);
    }

    [UnityTest]
    public IEnumerator CollisionLeftTest()
    {
        PlaceWall(new Vector2Int(-1, 0));

        yield return MovePlayer(Vector2.left);

        Assert.Greater(rb.position.x, 0f);
    }

    [UnityTest]
    public IEnumerator CollisionUpTest()
    {
        PlaceWall(new Vector2Int(0, 1));

        yield return MovePlayer(Vector2.up);

        Assert.Less(rb.position.y, 1f);
    }

    [UnityTest]
    public IEnumerator CollisionDownTest()
    {
        PlaceWall(new Vector2Int(0, -1));

        yield return MovePlayer(Vector2.down);

        Assert.Greater(rb.position.y, 0f);
    }

    [UnityTest]
    public IEnumerator MoveRightWithoutWallTest()
    {
        yield return MovePlayer(Vector2.right);

        Assert.Greater(rb.position.x, startPosition.x);
    }

    [UnityTest]
    public IEnumerator MoveLeftWithoutWallTest()
    {
        yield return MovePlayer(Vector2.left);

        Assert.Less(rb.position.x, startPosition.x);
    }

    [UnityTest]
    public IEnumerator MoveUpWithoutWallTest()
    {
        yield return MovePlayer(Vector2.up);

        Assert.Greater(rb.position.y, startPosition.y);
    }

    [UnityTest]
    public IEnumerator MoveDownWithoutWallTest()
    {
        yield return MovePlayer(Vector2.down);

        Assert.Less(rb.position.y, startPosition.y);
    }
}
