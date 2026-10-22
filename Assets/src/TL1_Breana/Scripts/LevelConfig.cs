using System;

public readonly struct LevelConfig
{
    public readonly int MazeWidth;
    public readonly int MazeHeight;
    public readonly float HazardDensity;
    public readonly DistortionType[] ActiveDistortions;
    public readonly float TimeLimitSeconds;
    public readonly int PuzzlesRequired;
    /*
    public LevelConfig(
        int mazeWidth,
        int mazeHeight,
        float hazardDensity,
        //DistortionType[] activeDisortions,
        float timeLimitSeconds,
        int puzzlesRequired)
    {
        MazeWidth = mazeWidth;
        MazeHeight = mazeHeight;
        HazardDensity = hazardDensity;
        
        ActiveDistortions = activeDisortions == null
            ? Array.Empty<DistortionType>()
            : (DistortionType[])ActiveDistortions.Clone();
        
        //TimeLimitSeconds = timeLimitSeconds;
        //PuzzlesRequired = puzzlesRequired;
        
    }
    */
}