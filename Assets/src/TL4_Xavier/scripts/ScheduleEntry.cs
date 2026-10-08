using System;
using System.Collections.Generic;

// One row of the D1 Distortion Schedules table.
[Serializable]
public struct ScheduleEntry
{
    public float startTime;      // seconds since LoadSchedule
    public DistortionID id;
    public float intensity;      // target clamped to [0, 1]
}

// All the rows for one level (so it shows up nicely in the Inspector
[Serializable]
public class LevelSchedule
{
    public List<ScheduleEntry> entries = new List<ScheduleEntry>();
}