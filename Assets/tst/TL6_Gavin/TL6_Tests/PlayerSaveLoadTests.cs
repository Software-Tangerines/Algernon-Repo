
using NUnit.Framework;
using UnityEngine;

public class PlayerSaveLoadTests
{
    private GameObject playerObject;
    private PlayerManager player;

    [SetUp]
    public void Setup()
    {
        playerObject = new GameObject("Test Player");
        player = playerObject.AddComponent<PlayerManager>();
    }

    [TearDown]
    public void Cleanup()
    {
        Object.DestroyImmediate(playerObject);
    }

    [Test]
    public void SaveStoresPlayerPosition()
    {
        player.transform.position = new Vector3(5, 3, 0);

        player.SaveState();

        player.transform.position = Vector3.zero;
        player.LoadState();

        Assert.AreEqual(new Vector3(5, 3, 0),
            player.transform.position);
    }

    [Test]
    public void SaveStoresPlayerHealth()
    {
        player.TakeDamage(30);
        player.SaveState();

        player.TakeDamage(20);
        player.LoadState();

        Assert.AreEqual(70, player.data.health);
    }

    [Test]
    public void LoadRestoresHealthAfterDamage()
    {
        player.SaveState();

        player.TakeDamage(40);
        player.LoadState();

        Assert.AreEqual(100, player.data.health);
    }

    [Test]
    public void LoadRestoresPositionAfterMovement()
    {
        player.transform.position = new Vector3(2, 4, 0);
        player.SaveState();

        player.transform.position = new Vector3(10, 10, 0);
        player.LoadState();

        Assert.AreEqual(new Vector3(2, 4, 0),
            player.transform.position);
    }

    [Test]
    public void SaveStoresPlayerScore()
    {
        player.data.score = 50;
        player.SaveState();

        player.data.score = 100;
        player.LoadState();

        Assert.AreEqual(50, player.data.score);
    }

    [Test]
    public void SaveStoresPlayerMoveSpeed()
    {
        player.data.moveSpeed = 8f;
        player.SaveState();

        player.data.moveSpeed = 2f;
        player.LoadState();

        Assert.AreEqual(8f, player.data.moveSpeed);
    }

    [Test]
    public void SaveStoresPlayerFacingDirection()
    {
        player.data.facingDirection = Vector2.right;
        player.SaveState();

        player.data.facingDirection = Vector2.left;
        player.LoadState();

        Assert.AreEqual(Vector2.right,
            player.data.facingDirection);
    }

    [Test]
    public void SaveStoresPlayerState()
    {
        player.SetFrozen(true);
        player.SaveState();

        player.SetFrozen(false);
        player.LoadState();

        Assert.AreEqual(PlayerState.Frozen,
            player.data.currentState);
    }

    [Test]
    public void LoadRestoresMaxHealth()
    {
        player.data.maxHealth = 150;
        player.SaveState();

        player.data.maxHealth = 50;
        player.LoadState();

        Assert.AreEqual(150, player.data.maxHealth);
    }

    [Test]
    public void SavingAgainUpdatesPreviousSave()
    {
        player.SaveState();

        player.TakeDamage(20);
        player.SaveState();

        player.TakeDamage(30);
        player.LoadState();

        Assert.AreEqual(80, player.data.health);
    }

    [Test]
    public void LoadRestoresHealthAndPositionTogether()
    {
        player.TakeDamage(20);
        player.transform.position = new Vector3(4, 6, 0);

        player.SaveState();

        player.TakeDamage(30);
        player.transform.position = Vector3.zero;

        player.LoadState();

        Assert.AreEqual(80, player.data.health);
        Assert.AreEqual(new Vector3(4, 6, 0),
            player.transform.position);
    }

    [Test]
    public void LoadRestoresDataAsACopy()
    {
        player.SaveState();

        player.LoadState();
        player.data.health = 50;

        player.LoadState();

        Assert.AreEqual(100, player.data.health);
    }
}
