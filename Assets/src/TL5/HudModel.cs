using System;
using System.Collections.Generic;

/*
F6 HudModel: everything the in-game HUD displays, as plain data (no Unity objects).

HudFacade writes into it when F1 calls SetTimer / SetObjective / etc., and the HudWidgets
read from it to produce their text. Widgets never ask other features for anything.

Version goes up ONLY when something visible changes. HudFacade re-renders widget text only
when Version changes, so a timer that F1 updates every frame rebuilds its string once per
displayed second instead of 60 times (plan stress test: no allocation growth).
*/
public class HudModel
{
    // The timer flashes red when strictly below this (10.0 itself is not urgent).
    public const float UrgentBelowSeconds = 10f;
    // Largest time mm:ss can show (99:59). Anything larger, including infinity, is clamped.
    public const int MaxDisplaySeconds = 99 * 60 + 59;

    private readonly List<DistortionID> distortions = new List<DistortionID>();

    public int Version { get; private set; }

    // ---------- Timer ----------

    public bool HasTimer { get; private set; }              // false until the first SetTimer of a level
    public float SecondsRemaining { get; private set; }
    public float TimerStartSeconds { get; private set; }    // largest value seen this level = the level's time limit
    public int DisplayedSeconds { get; private set; }       // what mm:ss shows (rounded UP, so 0.4 s shows 00:01)

    public bool IsUrgent => HasTimer && SecondsRemaining < UrgentBelowSeconds;

    // 0 at the start of the level, 1 when time runs out. Drives the music intensity.
    public float TimeElapsedFraction =>
        TimerStartSeconds > 0f ? Clamp01(1f - SecondsRemaining / TimerStartSeconds) : 0f;

    // ---------- Objectives, deaths, distortions ----------

    public bool HasObjective { get; private set; }          // false until F1 calls SetObjective
    public string ObjectiveLabel { get; private set; } = StringTable.ObjectiveLabel;
    public int ObjectivesCompleted { get; private set; }
    public int ObjectivesTotal { get; private set; }
    public int Deaths { get; private set; }
    public IReadOnlyList<DistortionID> ActiveDistortions => distortions;

    // ---------- Writes (each returns true if something visible changed) ----------

    // NaN is ignored; negatives clamp to 0; huge values and infinity clamp to 99:59.
    public bool SetTimer(float seconds)
    {
        if (float.IsNaN(seconds)) return false;
        seconds = Math.Min(Math.Max(seconds, 0f), MaxDisplaySeconds);

        bool hadTimer = HasTimer;
        bool wasUrgent = IsUrgent;

        HasTimer = true;
        SecondsRemaining = seconds;
        if (seconds > TimerStartSeconds) TimerStartSeconds = seconds;

        int shown = (int)Math.Ceiling(seconds);
        if (!hadTimer || shown != DisplayedSeconds || wasUrgent != IsUrgent)
        {
            DisplayedSeconds = shown;
            Version++;
            return true;
        }
        return false;
    }

    // Empty/null label uses the default "Objectives". Counts are clamped so completed is in [0, total].
    public bool SetObjective(string label, int completed, int total)
    {
        if (string.IsNullOrEmpty(label)) label = StringTable.ObjectiveLabel;
        total = Math.Max(0, total);
        completed = Math.Min(Math.Max(0, completed), total);

        if (HasObjective && label == ObjectiveLabel && completed == ObjectivesCompleted && total == ObjectivesTotal)
            return false;

        HasObjective = true;
        ObjectiveLabel = label;
        ObjectivesCompleted = completed;
        ObjectivesTotal = total;
        Version++;
        return true;
    }

    public bool SetDeaths(int deaths)
    {
        deaths = Math.Max(0, deaths);
        if (deaths == Deaths) return false;
        Deaths = deaths;
        Version++;
        return true;
    }

    // Copies the list (the caller may reuse theirs). null = no distortions.
    public bool SetDistortions(IReadOnlyList<DistortionID> active)
    {
        int count = active?.Count ?? 0;
        bool same = count == distortions.Count;
        for (int i = 0; same && i < count; i++) same = active[i] == distortions[i];
        if (same) return false;

        distortions.Clear();
        for (int i = 0; i < count; i++) distortions.Add(active[i]);
        Version++;
        return true;
    }

    // Called when a new level starts. Deaths are kept: they count across the whole run.
    public void ResetForLevel()
    {
        HasTimer = false;
        SecondsRemaining = 0f;
        TimerStartSeconds = 0f;
        DisplayedSeconds = 0;
        HasObjective = false;
        ObjectiveLabel = StringTable.ObjectiveLabel;
        ObjectivesCompleted = 0;
        ObjectivesTotal = 0;
        distortions.Clear();
        Version++;
    }

    private static float Clamp01(float v) => v < 0f ? 0f : (v > 1f ? 1f : v);
}
