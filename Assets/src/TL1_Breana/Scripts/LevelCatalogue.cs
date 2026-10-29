using System;
using System.Collections.Generic;

// Resolves a level index to its LevelConfig. Plain C# class, owned by GameCoordinator.
public class LevelCatalogue
{
    private readonly List<LevelConfig> levels;

    // Default level set used by the game (Task 4.3). Placeholder values: tune once F2-F4 are playable.
    public LevelCatalogue() : this(new[]
    {
        //              width height hazards  distortions                                      time  cheese
        new LevelConfig(9,    9,     0.1f,    null,                                            90f,  3),
        new LevelConfig(13,   13,    0.2f,    null,                                            120f, 5),
        new LevelConfig(17,   17,    0.3f,    new[] { DistortionType.MirroredControls },       150f, 7),
    })
    {
    }

    // Custom level set, so tests can build a catalogue of any size.
    public LevelCatalogue(IEnumerable<LevelConfig> levels)
    {
        this.levels = new List<LevelConfig>(levels);
    }

    public int Count => levels.Count;

    public bool IsValidIndex(int index)
    {
        return index >= 0 && index < levels.Count;
    }

    // Throws for an invalid index; callers check IsValidIndex first (GameCoordinator logs the error).
    public LevelConfig Resolve(int index)
    {
        if (!IsValidIndex(index))
        {
            throw new ArgumentOutOfRangeException(nameof(index), $"Level index {index} is outside 0..{levels.Count - 1}");
        }
        return levels[index];
    }
}
