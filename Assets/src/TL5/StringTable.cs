using System.Text;

/*
F6 StringTable: every piece of text the player can read lives here, and nowhere else.

Completion criterion from the project plan: "no player-facing string is hard-coded outside StringTable".
To reword anything in the menus or HUD, change it here. Format strings use string.Format placeholders.
*/
public static class StringTable
{
    public const string GameTitle = "Algernon";

    // ---------- HUD widgets ----------

    // Static labels returned by HudWidget's base Render (what you see in the dynamic-binding demo).
    public const string TimerLabel = "Time";
    public const string ObjectiveLabel = "Objectives";
    public const string AttemptsLabel = "Deaths";
    public const string DistortionLabel = "Distortion";

    public const string ClockFormat = "{0:00}:{1:00}";         // minutes, seconds
    public const string ObjectiveFormat = "{0}: {1}/{2}";      // label, completed, total -> "Levers: 2/4"
    public const string NoObjectives = "Reach the exit";       // shown instead of "0/0"
    public const string DeathsFormat = "Deaths: {0}";
    public const string DistortionPrefix = "Distorted: ";
    public const string ListSeparator = ", ";

    // ---------- Menus and buttons ----------

    public const string Start = "Start";
    public const string LevelSelect = "Level Select";
    public const string Settings = "Settings";
    public const string QuitGame = "Quit Game";
    public const string Back = "Back";
    public const string Resume = "Resume";
    public const string Retry = "Retry";
    public const string QuitToMenu = "Quit to Menu";
    public const string NextLevel = "Next Level";
    public const string MainMenu = "Main Menu";
    public const string Continue = "Continue";
    public const string Paused = "Paused";
    public const string PressAnyKey = "Press any key to begin";
    public const string LevelNumberFormat = "Level {0}";       // 1-based level number

    public const string MasterVolume = "Master volume";
    public const string MusicVolume = "Music volume";
    public const string SfxVolume = "Effects volume";

    // ---------- Result screens ----------

    public const string LevelWon = "Level complete!";
    public const string LevelLost = "Level failed";
    public const string RunComplete = "Run complete";
    public const string StatsFormat = "Time: {0}\nDeaths: {1}\nPuzzles solved: {2}\nLevels cleared: {3}";

    // ---------- Helpers ----------

    public static string ResultTitle(LevelResult result) => result == LevelResult.Won ? LevelWon : LevelLost;

    public static string LevelNumber(int levelIndex) => string.Format(LevelNumberFormat, levelIndex + 1);

    // Whole seconds as mm:ss. Negative values show as 00:00.
    public static string Clock(int totalSeconds)
    {
        if (totalSeconds < 0) totalSeconds = 0;
        return string.Format(ClockFormat, totalSeconds / 60, totalSeconds % 60);
    }

    // The stats block on the result and run-complete screens. Elapsed time rounds down.
    public static string Stats(RunStatistics s) =>
        string.Format(StatsFormat, Clock((int)s.ElapsedTime), s.Deaths, s.PuzzlesSolved, s.LevelsCleared);

    // Player-facing name for each F5 distortion (used by the HUD's distortion warning).
    public static string DistortionName(DistortionID id)
    {
        switch (id)
        {
            case DistortionID.Mirror:       return "Mirror";
            case DistortionID.Rotation:     return "Rotation";
            case DistortionID.Fog:          return "Fog";
            case DistortionID.Desaturation: return "Desaturation";
            case DistortionID.Blackout:     return "Blackout";
            case DistortionID.Blur:         return "Blur";
            default:                        return id.ToString();
        }
    }

    // "Distorted: Mirror, Rotation"
    public static string DistortionList(System.Collections.Generic.IReadOnlyList<DistortionID> ids)
    {
        var sb = new StringBuilder(DistortionPrefix);
        for (int i = 0; i < ids.Count; i++)
        {
            if (i > 0) sb.Append(ListSeparator);
            sb.Append(DistortionName(ids[i]));
        }
        return sb.ToString();
    }
}
