using System;

// Per-level settings resolved by LevelCatalogue (maze size, hazards, distortions, time limit, puzzles).
// Readonly: set once in the constructor, never changed during a level.
public readonly struct LevelConfig
{
    public readonly int MazeWidth;
    public readonly int MazeHeight;
    public readonly float HazardDensity;              // 0.0 to 1.0
    public readonly DistortionType[] ActiveDistortions;
    public readonly float TimeLimitSeconds;
    public readonly int PuzzlesRequired;

    public LevelConfig(
        int mazeWidth,
        int mazeHeight,
        float hazardDensity,
        DistortionType[] activeDistortions,
        float timeLimitSeconds,
        int puzzlesRequired)
    {
        MazeWidth = mazeWidth;
        MazeHeight = mazeHeight;
        HazardDensity = hazardDensity;

        // Copy the array so the caller can't change this level's distortions afterwards
        ActiveDistortions = activeDistortions == null
            ? Array.Empty<DistortionType>()
            : (DistortionType[])activeDistortions.Clone();

        TimeLimitSeconds = timeLimitSeconds;
        PuzzlesRequired = puzzlesRequired;
    }
}
