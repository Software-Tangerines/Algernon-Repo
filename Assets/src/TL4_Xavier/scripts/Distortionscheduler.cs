using System;
using System.Collections.Generic;
using UnityEngine;

/*
F5 DistortionScheduler: decides WHICH effects are in the pipeline and HOW STRONG they are over time.

- LoadSchedule: a level's rows ("at 30 s, Desaturation -> 0.5") are queued by start time.
- Tick (every frame): advances the clock, applies rows that are now due, and ramps each effect's
  Current toward its Target linearly over RampSeconds.
- Apply / Remove / Clear: the same operations F1 can trigger directly through ViewDirector.

Plain C# class owned by ViewDirector. Other features should call ViewDirector, not this.
*/
public class DistortionScheduler
{
    // (id, new target). Raised once per change, never once per ramp frame. 0 = being removed.
    public event Action<DistortionID, float> DistortionChanged;

    // Seconds since LoadSchedule (scaled time, so it stops while paused).
    public float Clock { get; private set; }
    // How long an effect takes to go from 0 to 1 (or 1 to 0). 0 = instant.
    public float RampSeconds { get; set; }

    private readonly ViewEffectPipeline pipeline;
    private readonly List<ScheduleEntry> queue = new List<ScheduleEntry>();  // this level's rows, sorted by startTime
    private int nextEntry;                                   // index of the first row not applied yet
    private readonly bool[] removing = new bool[6];          // per DistortionID: ramping down to be removed?

    public DistortionScheduler(ViewEffectPipeline pipeline, float rampSeconds = 1f)
    {
        this.pipeline = pipeline;
        RampSeconds = rampSeconds;
    }

    // ---- Schedule -------------------------------------------------------

    // Clears the chain, resets the clock to 0 and queues the rows by start time.
    // Pass null for no schedule row
    public void LoadSchedule(IList<ScheduleEntry> entries)
    {
        Clear();
        Clock = 0f;
        nextEntry = 0;
        queue.Clear();
        if (entries != null) queue.AddRange(entries);
        queue.Sort((a, b) => a.startTime.CompareTo(b.startTime));
    }

    // Call once per frame with scaled delta time (pause => dt = 0 => everything freezes)
    public void Tick(float dt)
    {
        if (dt < 0f || float.IsNaN(dt)) return;
        Clock += dt;

        while (nextEntry < queue.Count && queue[nextEntry].startTime <= Clock)
        {
            var e = queue[nextEntry++];
            Apply(e.id, e.intensity);
        }

        // Ramp each effect's Current toward its Target.
        // Loop backwards because an effect that finished ramping down is removed from the list mid-loop.
        float step = RampSeconds > 0f ? dt / RampSeconds : float.PositiveInfinity;
        var chain = pipeline.Chain;
        for (int i = chain.Count - 1; i >= 0; i--)
        {
            var fx = chain[i];
            fx.Current = Mathf.MoveTowards(fx.Current, fx.Target, step);

            if (removing[(int)fx.Id] && fx.Current <= 0f)
            {
                removing[(int)fx.Id] = false;
                pipeline.Remove(fx.Id);
            }
        }
    }

    // ---- Direct commands ------------------------------------------------

    // Starts a distortion or changes its target. New ids go on the END of the chain and ramp up from 0;
    // an id already in the chain keeps its position and just gets the new target.
    // Rejects NaN/infinite intensity and undefined ids (error), and Blur (warning).
    // Returns false if the input was rejected.
    public bool Apply(DistortionID id, float intensity)
    {
        if (!Enum.IsDefined(typeof(DistortionID), id) || float.IsNaN(intensity) || float.IsInfinity(intensity))
        {
            Debug.LogError($"[F5] ApplyDistortion rejected: id={(int)id}, intensity={intensity}");
            return false;
        }
        if (id == DistortionID.Blur)
        {
            Debug.LogWarning("[F5] Blur is reserved (stretch goal) and ignored.");
            return false;
        }

        float target = Mathf.Clamp01(intensity);
        var fx = pipeline.Find(id);
        bool isNew = fx == null;

        if (isNew)
        {
            fx = CreateEffect(id);
            if (fx == null) return false;
            fx.Current = 0f;            // ramps up from nothing
            pipeline.Add(fx);           // new id goes to the end of the chain
        }

        removing[(int)id] = false;      // re-applying cancels a pending removal

        if (isNew || !Mathf.Approximately(fx.Target, target))
        {
            fx.Target = target;
            DistortionChanged?.Invoke(id, target);   
        }
        return true;
    }

    // Ramps to 0 over RampSeconds, then Tick removes it. Not in the chain = no-op, no event.
    public void Remove(DistortionID id)
    {
        var fx = pipeline.Find(id);
        if (fx == null || removing[(int)id]) return;
        fx.Target = 0f;
        removing[(int)id] = true;
        DistortionChanged?.Invoke(id, 0f);
    }

    // Empties the chain immediately (no ramp). Raises DistortionChanged(id, 0) for each removed effect.
    public void Clear()
    {
        var chain = pipeline.Chain;
        for (int i = chain.Count - 1; i >= 0; i--)
        {
            var id = chain[i].Id;
            removing[(int)id] = false;
            pipeline.Remove(id);
            DistortionChanged?.Invoke(id, 0f);
        }
    }

    // ---- Factory --------------------------------------------------------

    // Makes the effect object for an id. Add new effect classes here.

    private static ViewEffect CreateEffect(DistortionID id)
    {
        switch (id)
        {
            case DistortionID.Mirror:       return new MirrorViewEffect();
            case DistortionID.Rotation:     return new RotationViewEffect();
            case DistortionID.Desaturation: return new DesaturationViewEffect();
            case DistortionID.Blackout:     return new BlackoutViewEffect();
            // case DistortionId.Fog:       return new FogViewEffect();   // task 7
            default:
                Debug.LogWarning($"[F5] {id} is not implemented yet.");
                return null;
        }
    }
}