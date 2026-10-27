using System.Collections.Generic;
using UnityEngine;

/*
F6 AudioCueTable: maps each AudioCueId to the AudioClip that plays for it.

Clips assigned in the AudioDirector's Inspector win. Any cue without a clip gets a short
generated placeholder tone, so every cue is audible from day one, before anyone has
picked real sound effects. Replace them by dragging clips into AudioDirector > Cue Clips.
*/
public class AudioCueTable
{
    private const int SampleRate = 44100;
    private readonly Dictionary<AudioCueId, AudioClip> clips = new Dictionary<AudioCueId, AudioClip>();

    public AudioCueTable(IEnumerable<AudioDirector.CueClip> assigned)
    {
        if (assigned == null) return;
        foreach (var entry in assigned)
            if (entry.clip != null) clips[entry.cue] = entry.clip;
    }

    // Never returns null: falls back to (and caches) a placeholder tone.
    public AudioClip Get(AudioCueId cue)
    {
        if (!clips.TryGetValue(cue, out var clip) || clip == null)
        {
            clip = Placeholder(cue);
            clips[cue] = clip;
        }
        return clip;
    }

    // A tiny jingle per cue: note frequencies in Hz (0 = rest) and how long each note lasts.
    private static AudioClip Placeholder(AudioCueId cue)
    {
        switch (cue)
        {
            case AudioCueId.ButtonClick:       return Tone(cue, 0.04f, 1200f);
            case AudioCueId.LevelStart:        return Tone(cue, 0.10f, 523f, 659f, 784f);
            case AudioCueId.LevelWon:          return Tone(cue, 0.12f, 523f, 659f, 784f, 1047f);
            case AudioCueId.LevelLost:         return Tone(cue, 0.18f, 392f, 330f, 262f);
            case AudioCueId.RunComplete:       return Tone(cue, 0.12f, 523f, 659f, 784f, 1047f, 1319f);
            case AudioCueId.Pause:             return Tone(cue, 0.08f, 660f, 440f);
            case AudioCueId.Resume:            return Tone(cue, 0.08f, 440f, 660f);
            case AudioCueId.TimerWarning:      return Tone(cue, 0.10f, 880f, 0f, 880f);
            case AudioCueId.ObjectiveProgress: return Tone(cue, 0.07f, 988f, 1319f);
            case AudioCueId.PlayerDeath:       return Tone(cue, 0.20f, 220f, 165f);
            default:                           return Tone(cue, 0.05f, 600f);
        }
    }

    // Builds a mono sine-wave clip; each note fades out linearly so it doesn't click.
    private static AudioClip Tone(AudioCueId cue, float noteSeconds, params float[] notes)
    {
        int perNote = Mathf.Max(1, (int)(SampleRate * noteSeconds));
        var data = new float[perNote * notes.Length];
        for (int n = 0; n < notes.Length; n++)
        {
            float freq = notes[n];
            if (freq <= 0f) continue;   // rest: leave silence
            for (int s = 0; s < perNote; s++)
            {
                float t = (float)s / SampleRate;
                float fade = 1f - (float)s / perNote;
                data[n * perNote + s] = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.3f * fade;
            }
        }
        var clip = AudioClip.Create("F6_" + cue, data.Length, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
