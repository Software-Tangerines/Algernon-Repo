using System;
using System.Collections.Generic;

/*
F5 distortion schedule data (stand-in for the D1 "Distortion Schedules" table).

A level's schedule is a list of rows like "at 30 s, ramp Desaturation to 0.5".
Fill them in on the ViewDirector component in the Inspector: one LevelSchedule per level,
in the same order as F1's LevelCatalogue.
*/

// One row: at startTime seconds after LoadSchedule, ramp distortion `id` to `intensity`.
[Serializable]
public struct ScheduleEntry
{
    public float startTime;      // seconds since LoadSchedule (rows can be in any order; they get sorted)
    public DistortionID id;
    public float intensity;      // target, clamped to [0, 1]; 0 does NOT remove the effect, it just goes to 0
}

// All the rows for one level (a class so it shows up nicely in the Inspector).
[Serializable]
public class LevelSchedule
{
    public List<ScheduleEntry> entries = new List<ScheduleEntry>();
}
