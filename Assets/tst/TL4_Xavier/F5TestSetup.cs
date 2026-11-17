using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

/*
F5 test scene helper (TestMVP scene only, NOT part of the game).

Does what F1 will do once Breana wires F5 in, so every F5 feature can be seen in the MVP:
  Awake: puts a ViewDirector on the player's camera and gives it a demo schedule.
  Start: frames the maze (union of all TilemapRenderers) so the fog grid and minimap exist.
Then number keys trigger each effect by hand (see the on-screen help, bottom-left).
*/
public class F5TestSetup : MonoBehaviour
{
    [Tooltip("Used only if no TilemapRenderer is found in the scene.")]
    public Bounds fallbackMazeBounds = new Bounds(new Vector3(4.5f, 4.5f, 0f), new Vector3(10f, 10f, 0f));

    [Tooltip("Run the demo schedule (fog at 0 s, then desaturate, rotate, mirror, then back to normal).")]
    public bool playDemoSchedule = true;

    private ViewDirector vd;
    private CameraSnapshot saved;
    private string status = "";

    void Awake()
    {
        var cam = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
        if (cam == null) { Debug.LogError("[F5 test] No camera in the scene."); enabled = false; return; }

        vd = cam.GetComponent<ViewDirector>();
        if (vd == null) vd = cam.gameObject.AddComponent<ViewDirector>();   // its Awake runs right here

        // Test scene only: keep the camera's own zoom (same as the MVP) instead of zooming out to the whole maze.
        vd.fixedZoom = cam.orthographicSize;

        if (playDemoSchedule)
        {
            var demo = new LevelSchedule();
            demo.entries.Add(new ScheduleEntry { startTime = 0f,  id = DistortionID.Fog,          intensity = 1f });
            demo.entries.Add(new ScheduleEntry { startTime = 3f,  id = DistortionID.Desaturation, intensity = 0.7f });
            demo.entries.Add(new ScheduleEntry { startTime = 6f,  id = DistortionID.Rotation,     intensity = 0.125f });
            demo.entries.Add(new ScheduleEntry { startTime = 9f,  id = DistortionID.Mirror,       intensity = 1f });
            demo.entries.Add(new ScheduleEntry { startTime = 14f, id = DistortionID.Desaturation, intensity = 0f });
            demo.entries.Add(new ScheduleEntry { startTime = 14f, id = DistortionID.Rotation,     intensity = 0f });
            demo.entries.Add(new ScheduleEntry { startTime = 14f, id = DistortionID.Mirror,       intensity = 0f });
            vd.levelSchedules = new List<LevelSchedule> { demo };
            vd.autoStartLevel = 0;      // ViewDirector.Start loads it
        }
    }

    void Start()
    {
        if (vd == null) return;
        vd.FrameMaze(MazeBounds());
        vd.ExplorationChanged += f => status = $"explored {f:P0}";
    }

    private Bounds MazeBounds()
    {
        var tilemaps = FindObjectsByType<TilemapRenderer>();
        if (tilemaps.Length == 0)
        {
            Debug.LogWarning("[F5 test] No TilemapRenderer found; using fallbackMazeBounds.");
            return fallbackMazeBounds;
        }
        Bounds b = tilemaps[0].bounds;
        for (int i = 1; i < tilemaps.Length; i++) b.Encapsulate(tilemaps[i].bounds);
        Debug.Log($"[F5 test] Framing maze bounds {b}");
        return b;
    }

    void Update()
    {
        var k = Keyboard.current;
        if (vd == null || k == null) return;

        if (k.digit1Key.wasPressedThisFrame) Toggle(DistortionID.Mirror, 1f);
        if (k.digit2Key.wasPressedThisFrame) Toggle(DistortionID.Rotation, 0.125f);
        if (k.digit3Key.wasPressedThisFrame) Toggle(DistortionID.Desaturation, 1f);
        if (k.digit4Key.wasPressedThisFrame) Toggle(DistortionID.Fog, 1f);
        if (k.digit5Key.wasPressedThisFrame) Toggle(DistortionID.Blackout, 1f);
        if (k.digit6Key.wasPressedThisFrame) { vd.Shake(0.5f, 1f); status = "shake"; }
        if (k.digit7Key.wasPressedThisFrame) { revealAll = !revealAll; vd.RevealAll(revealAll); status = $"reveal all {revealAll}"; }
        if (k.digit8Key.wasPressedThisFrame) { saved = vd.CaptureState(); status = "captured"; }
        if (k.digit9Key.wasPressedThisFrame)
        {
            if (saved != null) { vd.RestoreState(saved); status = "restored"; }
            else status = "nothing captured yet (press 8)";
        }
        if (k.digit0Key.wasPressedThisFrame) { vd.ClearDistortions(); status = "cleared"; }
    }

    private bool revealAll;

    // On if it's off, off if it's on.
    private void Toggle(DistortionID id, float intensity)
    {
        bool active = false;
        foreach (var activeId in ActiveIds()) if (activeId == id) active = true;
        if (active) { vd.RemoveDistortion(id); status = $"{id} off"; }
        else { vd.ApplyDistortion(id, intensity); status = $"{id} on"; }
    }

    // Which effects are in the chain right now (read from a snapshot, so no F5 internals are touched).
    private IEnumerable<DistortionID> ActiveIds()
    {
        foreach (var e in vd.CaptureState().Effects)
            if (!e.removing && e.target > 0f) yield return e.id;
    }

    // Small key list hugging the left edge, vertically centred (clear of the hearts above and the joystick below).
    private const string Keys =
        "W A S D  move\n" +
        "Space  switch controls\n" +
        "E  interact (puzzle)\n" +
        "H / J  damage / heal\n" +
        "F  freeze / unfreeze\n" +
        "P / O  save / load player\n" +
        "R  restart level\n" +
        "M  menu\n" +
        "\n" +
        "F5 VIEW\n" +
        "1  mirror\n" +
        "2  rotate\n" +
        "3  desaturate\n" +
        "4  fog\n" +
        "5  blackout\n" +
        "6  shake\n" +
        "7  hide / show fog\n" +
        "8  capture snapshot\n" +
        "9  restore snapshot\n" +
        "0  clear distortions";

    private GUIStyle keyStyle;

    void OnGUI()
    {
        if (keyStyle == null)
            keyStyle = new GUIStyle(GUI.skin.label) { fontSize = 10, wordWrap = false, padding = new RectOffset(3, 3, 2, 2) };

        var content = new GUIContent(string.IsNullOrEmpty(status) ? Keys : Keys + "\n\n> " + status);
        Vector2 size = keyStyle.CalcSize(content);
        var r = new Rect(0f, (Screen.height - size.y) * 0.5f, size.x, size.y);

        var old = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.5f);         // faint dark backing so it's readable on any background
        GUI.DrawTexture(r, Texture2D.whiteTexture);
        GUI.color = old;
        GUI.Label(r, content, keyStyle);
    }
}
