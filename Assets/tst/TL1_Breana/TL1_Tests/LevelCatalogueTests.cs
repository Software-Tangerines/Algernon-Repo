using System;
using NUnit.Framework;

public class LevelCatalogueTests
{
    // Task 4.4: boundary indices
    [Test]
    public void IsValidIndex_MinusOne_False()
    {
        var catalogue = new LevelCatalogue();
        Assert.IsFalse(catalogue.IsValidIndex(-1));
    }

    [Test]
    public void IsValidIndex_Count_False()
    {
        var catalogue = new LevelCatalogue();
        Assert.IsFalse(catalogue.IsValidIndex(catalogue.Count));
    }

    [Test]
    public void IsValidIndex_FirstAndLast_True()
    {
        var catalogue = new LevelCatalogue();
        Assert.IsTrue(catalogue.IsValidIndex(0));
        Assert.IsTrue(catalogue.IsValidIndex(catalogue.Count - 1));
    }

    [Test]
    public void Resolve_InvalidIndex_Throws()
    {
        var catalogue = new LevelCatalogue();
        Assert.Throws<ArgumentOutOfRangeException>(() => catalogue.Resolve(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => catalogue.Resolve(catalogue.Count));
    }

    // Completion criteria: a run of at least three levels
    [Test]
    public void DefaultCatalogue_HasAtLeastThreeLevels()
    {
        Assert.GreaterOrEqual(new LevelCatalogue().Count, 3);
    }

    // Contract: every entry has TimeLimitSeconds > 0 and 0 <= HazardDensity <= 1
    [Test]
    public void DefaultCatalogue_EveryLevelHasValidSettings()
    {
        var catalogue = new LevelCatalogue();
        for (int i = 0; i < catalogue.Count; i++)
        {
            LevelConfig level = catalogue.Resolve(i);
            Assert.Greater(level.TimeLimitSeconds, 0f, $"Level {i} time limit");
            Assert.That(level.HazardDensity, Is.InRange(0f, 1f), $"Level {i} hazard density");
            Assert.GreaterOrEqual(level.PuzzlesRequired, 0, $"Level {i} cheese count");
        }
    }

    // Custom catalogues are copied, not shared
    [Test]
    public void CustomCatalogue_CopiesList()
    {
        var list = new System.Collections.Generic.List<LevelConfig> { new LevelConfig(5, 5, 0f, null, 30f, 0) };
        var catalogue = new LevelCatalogue(list);
        list.Clear();

        Assert.AreEqual(1, catalogue.Count);
    }
}
