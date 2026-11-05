using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
F5 CameraSnapshot: the Memento for pause / restart (Champion doc 3.1, Supplement 2.3).

ViewDirector.CaptureState() builds one; F1 stores it without looking inside; RestoreState() puts
everything back. Immutable: every field is set once in the constructor, and the explored bits are
deep-copied in and out, so later fog or chain changes can never alter a snapshot.

Excludes shake and the follow target (F1 rebinds the target itself).
The members are internal: TL1's assembly cannot read them, which keeps F5's internals hidden.
*/
public sealed class CameraSnapshot : IEquatable<CameraSnapshot>
{
    //its target and current intensity, its place in the chain,
    // and whether it was ramping down to be removed.
    public readonly struct EffectEntry : IEquatable<EffectEntry>
    {
        public readonly DistortionID id;
        public readonly float target;
        public readonly float current;
        public readonly int order;
        public readonly bool removing;

        public EffectEntry(DistortionID id, float target, float current, int order, bool removing)
        {
            this.id = id; this.target = target; this.current = current; this.order = order; this.removing = removing;
        }

        public bool Equals(EffectEntry o) =>
            id == o.id && target == o.target && current == o.current && order == o.order && removing == o.removing;
    }

    internal Vector2 Position { get; }
    internal float OrthoSize { get; }
    internal IReadOnlyList<EffectEntry> Effects { get; }
    internal float ScheduleClock { get; }
    internal int LevelIndex { get; }
    internal float VisionRadius { get; }
    internal bool RevealAll { get; }
    internal int GridWidth { get; }      // 0 x 0 = no fog grid yet
    internal int GridHeight { get; }

    private readonly BitArray explored;  // null when there is no grid

    internal CameraSnapshot(Vector2 position, float orthoSize, IReadOnlyList<EffectEntry> effects,
        float scheduleClock, int levelIndex, float visionRadius, bool revealAll,
        BitArray explored, int gridWidth, int gridHeight)
    {
        Position = position;
        OrthoSize = orthoSize;
        Effects = new List<EffectEntry>(effects ?? Array.Empty<EffectEntry>()).AsReadOnly();
        ScheduleClock = scheduleClock;
        LevelIndex = levelIndex;
        VisionRadius = visionRadius;
        RevealAll = revealAll;
        this.explored = explored != null ? new BitArray(explored) : null;
        GridWidth = gridWidth;
        GridHeight = gridHeight;
    }

    internal BitArray CopyExplored() => explored != null ? new BitArray(explored) : null;

    public bool Equals(CameraSnapshot o)
    {
        if (ReferenceEquals(this, o)) return true;
        if (o is null) return false;
        if (Position != o.Position || OrthoSize != o.OrthoSize || ScheduleClock != o.ScheduleClock
            || LevelIndex != o.LevelIndex || VisionRadius != o.VisionRadius || RevealAll != o.RevealAll
            || GridWidth != o.GridWidth || GridHeight != o.GridHeight || Effects.Count != o.Effects.Count)
            return false;

        for (int i = 0; i < Effects.Count; i++)
            if (!Effects[i].Equals(o.Effects[i])) return false;

        if ((explored == null) != (o.explored == null)) return false;
        if (explored != null)
        {
            if (explored.Length != o.explored.Length) return false;
            for (int i = 0; i < explored.Length; i++)
                if (explored[i] != o.explored[i]) return false;
        }
        return true;
    }

    public override bool Equals(object obj) => Equals(obj as CameraSnapshot);

    public override int GetHashCode() => HashCode.Combine(Position, OrthoSize, ScheduleClock, LevelIndex, Effects.Count);
}
