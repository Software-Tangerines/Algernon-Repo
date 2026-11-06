
using NUnit.Framework;
using UnityEngine;

public class PlayerHealthTests
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
    public void PlayerStartsWithFullHealth()
    {
        Assert.AreEqual(100, player.data.health);
    }

    [Test]
    public void PlayerStartsWithMaxHealth()
    {
        Assert.AreEqual(100, player.data.maxHealth);
    }

    [Test]
    public void TakingDamageReducesHealth()
    {
        player.TakeDamage(10);

        Assert.AreEqual(90, player.data.health);
    }

    [Test]
    public void TakingDamageMultipleTimesReducesHealth()
    {
        player.TakeDamage(10);
        player.TakeDamage(20);

        Assert.AreEqual(70, player.data.health);
    }

    [Test]
    public void HealthCannotGoBelowZero()
    {
        player.TakeDamage(150);

        Assert.AreEqual(0, player.data.health);
    }

    [Test]
    public void PlayerDiesAtZeroHealth()
    {
        player.TakeDamage(100);

        Assert.AreEqual(PlayerState.Dead, player.data.currentState);
        Assert.IsFalse(player.IsAlive);
    }

    [Test]
    public void HealingIncreasesHealth()
    {
        player.TakeDamage(30);
        player.Heal(10);

        Assert.AreEqual(80, player.data.health);
    }

    [Test]
    public void HealingCannotExceedMaxHealth()
    {
        player.Heal(50);

        Assert.AreEqual(100, player.data.health);
    }

    [Test]
    public void DeadPlayerCannotBeHealed()
    {
        player.TakeDamage(100);
        player.Heal(50);

        Assert.AreEqual(0, player.data.health);
    }

    [Test]
    public void FrozenPlayerCannotTakeDamage()
    {
        player.SetFrozen(true);
        player.TakeDamage(10);

        Assert.AreEqual(100, player.data.health);
    }

    [Test]
    public void ZeroDamageDoesNotChangeHealth()
    {
        player.TakeDamage(0);

        Assert.AreEqual(100, player.data.health);
    }

    [Test]
    public void NegativeDamageDoesNotChangeHealth()
    {
        player.TakeDamage(-10);

        Assert.AreEqual(100, player.data.health);
    }
}
