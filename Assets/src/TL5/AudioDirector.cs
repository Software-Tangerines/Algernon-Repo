using System;
using System.Collections.Generic;
using UnityEngine;

/*
F6 AudioDirector: plays every sound effect and the background music. Singleton.

Why a Singleton (plan's justification): Unity allows only one AudioListener, and cues are fired from
menus, HUD widgets and F1 alike, so one shared player is a real constraint rather than a convenience.
Access it with AudioDirector.Instance. If none is in the scene, one is created and kept across scenes.

Setup (optional): add AudioDirector to a GameObject in your first scene and drag real clips into
Cue Clips / Music Clip. Without that, each cue plays a generated placeholder tone and there is no music.

Volumes (Master / Music / Effects) are 0..1, saved in PlayerPrefs, and set from the Settings screen.
*/
public enum VolumeChannel
{
    Master = 0,
    Music = 1,
    Sfx = 2
}

public class AudioDirector : MonoBehaviour
{
    [Serializable]
    public struct CueClip
    {
        public AudioCueId cue;
        public AudioClip clip;
    }

    [Tooltip("Real sound for each cue. Cues left out play a generated placeholder tone.")]
    public List<CueClip> cueClips = new List<CueClip>();

    [Tooltip("Looping background music. Leave empty for no music.")]
    public AudioClip musicClip;

    [Tooltip("Music pitch when the timer is about to run out (1 = normal speed).")]
    public float maxMusicPitch = 1.25f;

    private static AudioDirector instance;
    private static bool quitting;   // stops a new instance being created while the game shuts down

    private static readonly string[] VolumeKeys = { "F6.Volume.Master", "F6.Volume.Music", "F6.Volume.Sfx" };
    private readonly float[] volumes = { 1f, 1f, 1f };

    private AudioSource sfxSource;
    private AudioSource musicSource;
    private AudioCueTable table;

    public static AudioDirector Instance
    {
        get
        {
            if (instance == null && !quitting)
            {
                instance = FindAnyObjectByType<AudioDirector>();
                if (instance == null)
                    new GameObject("F6_AudioDirector").AddComponent<AudioDirector>();   // Awake sets instance
            }
            // Real null (not a destroyed object) during shutdown, so callers can safely use ?.
            return instance != null ? instance : null;
        }
    }

    // Statics survive between Play sessions when domain reload is off, so reset them each time.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instance = null;
        quitting = false;
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        transform.SetParent(null);           // DontDestroyOnLoad only works on root objects
        DontDestroyOnLoad(gameObject);
        Application.quitting += () => quitting = true;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;

        table = new AudioCueTable(cueClips);

        for (int i = 0; i < volumes.Length; i++)
            volumes[i] = PlayerPrefs.GetFloat(VolumeKeys[i], 1f);
        ApplyVolumes();
    }

    // ---------- Sound effects ----------

    public void PlayCue(AudioCueId cue)
    {
        if (!Enum.IsDefined(typeof(AudioCueId), cue))
        {
            Debug.LogWarning($"[F6] PlayCue ignored unknown cue {(int)cue}.");
            return;
        }
        sfxSource.PlayOneShot(table.Get(cue), volumes[(int)VolumeChannel.Sfx]);
    }

    // ---------- Music ----------

    // Starts the music if a clip is assigned and it isn't already playing.
    public void PlayMusic()
    {
        if (musicClip == null || musicSource.isPlaying) return;
        musicSource.clip = musicClip;
        musicSource.Play();
    }

    public void StopMusic() => musicSource.Stop();

    // 0 = calm, 1 = time almost up. Speeds the music up as the timer drops. NaN is ignored.
    public void SetMusicIntensity(float normalised)
    {
        if (float.IsNaN(normalised)) return;
        musicSource.pitch = Mathf.Lerp(1f, maxMusicPitch, Mathf.Clamp01(normalised));
    }

    // ---------- Volume ----------

    public float GetVolume(VolumeChannel channel) => volumes[(int)channel];

    // Clamped to [0, 1] and saved so it is remembered next time the game runs.
    public void SetVolume(VolumeChannel channel, float value)
    {
        if (float.IsNaN(value)) return;
        volumes[(int)channel] = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(VolumeKeys[(int)channel], volumes[(int)channel]);
        ApplyVolumes();
    }

    // Master scales everything through the AudioListener; Effects volume is applied per PlayOneShot.
    private void ApplyVolumes()
    {
        AudioListener.volume = volumes[(int)VolumeChannel.Master];
        musicSource.volume = volumes[(int)VolumeChannel.Music];
    }
}
