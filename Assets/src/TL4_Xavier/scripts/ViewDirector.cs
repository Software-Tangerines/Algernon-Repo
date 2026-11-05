using System;
using System.Collections.Generic;
using UnityEngine;

/*
F5 ViewDirector: the ONLY F5 class other features should talk to (Facade pattern).

Put it on the main Camera in the gameplay scene. GameCoordinator (F1) finds it automatically
at level start because it implements IViewDirector (see TL1_Breana/Scripts/Contracts).

What it does each frame (LateUpdate, so it runs after the player has moved):
  1. CameraRig.Tick       -> base camera position + zoom (follows the mouse)
  2. DistortionScheduler  -> starts scheduled distortions, ramps intensities
  3. ViewEffectPipeline   -> runs the base state through the effect chain
  4. ApplyToCamera        -> pushes the final state onto the Unity Camera

Public calls for other features (all safe: bad input is logged, never thrown):
  BindFollowTarget(Transform)          F1: what the camera follows (the mouse from F7)
  FrameMaze(Bounds)                    F1: fit the whole maze on screen at level start (bounds from F2)
  LoadSchedule(int levelIndex)         F1: load that level's distortion schedule (pending F1 agreement)
  ApplyDistortion(DistortionID, float) start or change a distortion, intensity 0..1, ramps over rampSeconds
  RemoveDistortion(DistortionID)       ramp a distortion back to 0, then remove it
  ClearDistortions()                   remove every distortion immediately
  SetVisionRadius(float cells)         how far around the mouse the fog lifts (clamped to [0, 50])
  RevealAll(bool)                      F1: true on the win screen hides the fog; false brings it back
  Shake(float magnitude, float secs)   screen shake (render offset only; the logical camera doesn't move)
  CaptureState() / RestoreState(snap)  F1: Memento for pause / restart
  Minimap                              256 x 256 texture of the explored cells, for the HUD

Events to subscribe to:
  Framed(Bounds)                   framing ease finished (once per valid FrameMaze)
  DistortionChanged(id, intensity) a target intensity changed (0 = removed); once per change, not per frame
  BlackoutChanged(bool)            the screen went black / came back
  ExplorationChanged(float)        explored fraction went up (at most once per frame)

F5 only changes the PICTURE. It never moves the mouse, reads input, or touches collision.
Mirrored CONTROLS belong to F7 (TL6), not here.

Fog only shows while the Fog distortion is active: schedule "Fog, 1" at time 0 for levels that use it.
*/
[DefaultExecutionOrder(100)] // run after TL6's CameraLockRotation so rotation distortions aren't undone
[RequireComponent(typeof(Camera))]
public class ViewDirector : MonoBehaviour, IViewDirector
{
    [Tooltip("Seconds for an effect to ramp to its new intensity.")]
    public float rampSeconds = 1f;

    [Tooltip("0 = FrameMaze zooms out to fit the whole maze (contract). > 0 = FrameMaze keeps this zoom (ortho size) instead.")]
    public float fixedZoom = 0f;

    [Tooltip("One schedule per level, indexed like F1's LevelCatalogue (stand-in for the D1 table).")]
    public List<LevelSchedule> levelSchedules = new List<LevelSchedule>();

    [Tooltip("MVP only: load this level's schedule on Start so you can watch it play. -1 = don't auto-load (use this once F1 calls LoadSchedule).")]
    public int autoStartLevel = 0;

    [Header("Fog of war")]
    [Tooltip("Cells around the mouse that get revealed (Euclidean distance). Clamped to [0,50]")]
    public float visionRadius = 3f;

    [Tooltip("Sorting layer for the fog sprite. Leave empty = the topmost sorting layer, so the fog covers walls, items and the mouse.")]
    public string fogSortingLayer = "";
    public int fogSortingOrder = 100;

    [Header("Minimap")]
    [Tooltip("Mirror and rotate the minimap like the main view.")]
    public bool minimapDistorted = true;
    [Tooltip("Debug: draw the minimap in the top-right corner until the HUD shows it.")]
    public bool showMinimapDebug = false;

    [Header("Desaturation")]
    [Tooltip("Drag in Hidden/F5/Desaturate so builds include the shader.")]
    public Shader desaturateShader;

    public event Action<Bounds> Framed;
    public event Action<DistortionID, float> DistortionChanged;
    public event Action<bool> BlackoutChanged;
    public event Action<float> ExplorationChanged;

    // 256 x 256 picture of the explored cells (null until FrameMaze). F1/F6 show it in the HUD.
    public Texture Minimap => minimap?.Texture;

    // The final state of the last rendered frame. Handy for debugging and tests.
    public CameraState LastState { get; private set; }

    private Camera cam;
    private CameraRig rig;
    private ViewEffectPipeline pipeline;
    private DistortionScheduler scheduler;
    private bool wasBlackedOut;
    private int currentLevelIndex = -1;    // last LoadSchedule index, stored in snapshots

    // fog + minimap(created by FrameMaze, sized to maze)
    private FogGrid fog;
    private FogOverlay fogOverlay;
    private bool revealAll;
    private bool fogDirty;      // cells changed since tge overlay and minimap were last drawn i guess
    private MinimapRenderer minimap;
    private bool minimapMirrored, minimapBlack;
    private float minimapRotation;

    // desaturation full-screen pass
    private Material desaturateMat;
    private float saturation = 1f;

    // for tests current fog grid
    internal FogGrid Fog => fog;

    // The camera's original settings, saved so a blackout can be undone exactly.
    private int savedCullingMask;
    private CameraClearFlags savedClearFlags;
    private Color savedBackground;

    void Awake() => Init();

    // Everything Awake does. Internal so Edit Mode tests can call it (Unity doesn't run Awake there).
    internal void Init()
    {
        if (rig != null) return;    // already initialised
        cam = GetComponent<Camera>();
        if (!cam.orthographic)
            Debug.LogWarning("[F5] ViewDirector expects an orthographic camera; FrameMaze zoom will have no effect.");

        savedCullingMask = cam.cullingMask;
        savedClearFlags = cam.clearFlags;
        savedBackground = cam.backgroundColor;

        // Start the rig where the camera already is, so nothing jumps before F1 calls FrameMaze.
        rig = new CameraRig(cam.transform.position, cam.orthographicSize);
        rig.Framed += b => Framed?.Invoke(b);

        //if the camera sits unnder player tl6 prefab it will follow old parent until f1 calls
        Transform oldParent = transform.parent;
        if(oldParent != null){
            transform.SetParent(null, worldPositionStays: true);
            rig.Bind(oldParent);
        }

        pipeline = new ViewEffectPipeline();
        scheduler = new DistortionScheduler(pipeline, rampSeconds);
        scheduler.DistortionChanged += (id, v) => DistortionChanged?.Invoke(id, v);

        if (desaturateShader == null) desaturateShader = Shader.Find("Hidden/F5/Desaturate");
        if (desaturateShader != null) desaturateMat = new Material(desaturateShader);
    }

    void OnDestroy()
    {
        fogOverlay?.Destroy();
        minimap?.Destroy();
    }


    void Start()
    {
        if (autoStartLevel >= 0) LoadSchedule(autoStartLevel);
    }

    // ---------- Camera (IViewDirector) ----------

    // Follow this Transform (normally the mouse). null = stop following and hold still.
    public void BindFollowTarget(Transform target) => rig.Bind(target);

    // Centre on the maze and zoom out so all of it, plus a 1-cell border, is visible.
    // The camera then eases onto the follow target over 0.5 s and raises Framed.
    // Invalid bounds (zero/negative/NaN size) are logged and ignored.
    // a valid call aslo starts a new fog grid ceil w ciel h all unexplored and minimap
    public void FrameMaze(Bounds mazeBounds)
    {
        rig.FixedOrthoSize = fixedZoom;
        if(!rig.Frame(mazeBounds, cam.aspect)) return; // invalid camera and fog unchanged

        int w = Mathf.CeilToInt(mazeBounds.size.x);
        int h = Mathf.CeilToInt(mazeBounds.size.y);
        //bound.min is the outer edge of cell (0,0) its centre is half a cell in
        fog = new FogGrid(w, h, (Vector2)mazeBounds.min + new Vector2(0.5f, 0.5f));

        fogOverlay?.Destroy();
        fogOverlay = new FogOverlay(fog, FogLayer(), fogSortingOrder);
        if (minimap == null) minimap = new MinimapRenderer();
        fogDirty = true;
    }

    // Unity draws sorting layers in list order (Project Settings > Tags and Layers), so the last one is on top.
    // Empty or unknown name = topmost layer, so the fog stays above the maze even if the team adds layers.
    private string FogLayer()
    {
        var layers = SortingLayer.layers;
        if (!string.IsNullOrEmpty(fogSortingLayer))
        {
            foreach (var l in layers) if (l.name == fogSortingLayer) return fogSortingLayer;
            Debug.LogWarning($"[F5] Sorting layer '{fogSortingLayer}' doesn't exist; fog uses the topmost layer.");
        }
        return layers.Length > 0 ? layers[layers.Length - 1].name : "Default";
    }

    // ---------- Distortions ----------

    // Clears the chain and restarts the schedule clock at 0. Unknown index = no distortions + warning.
    public void LoadSchedule(int levelIndex)
    {
        var rows = ScheduleFor(levelIndex);
        if (rows == null)
            Debug.LogWarning($"[F5] No schedule for level {levelIndex}; view stays undistorted.");
        currentLevelIndex = levelIndex;
        scheduler.LoadSchedule(rows);
    }

    // That level's rows, or null if there is no schedule for it.
    private IList<ScheduleEntry> ScheduleFor(int levelIndex) =>
        levelIndex >= 0 && levelIndex < levelSchedules.Count ? levelSchedules[levelIndex].entries : null;

    public void ApplyDistortion(DistortionID id, float intensity) => scheduler.Apply(id, intensity);
    public void RemoveDistortion(DistortionID id) => scheduler.Remove(id);

    // Part of IViewDirector: F1 calls this between levels so nothing carries over.
    public void ClearDistortions() => scheduler.Clear();

    //(f) goes here --------------------------------------------
    public void SetVisionRadius(float cells)
    {
        if(float.IsNaN(cells))
        {
            Debug.LogError("[F5] SetVisionRadius(NaN) ignored.");
            return;
        }
        visionRadius = Mathf.Clamp(cells, 0f, FogGrid.MaxVisionRadius);

    }

    //true meand the fog is hidden , the unexplored cells are still untouched false = show it agian
    public void RevealAll(bool reveal)
    {
        if(revealAll == reveal) return;
        revealAll = reveal;
        fogDirty = true;
    }

    // ---------- Shake ----------

    // Random render offset, at most `magnitude` units, fading to 0 over `seconds`.
    public void Shake(float magnitude, float seconds) => rig.Shake(magnitude, seconds);

    // ---------- Snapshot (Memento) ----------

    // Everything F5 needs to put the view back exactly: pose, active effects, schedule clock, fog.
    public CameraSnapshot CaptureState()
    {
        var chain = pipeline.Chain;
        var effects = new CameraSnapshot.EffectEntry[chain.Count];
        for (int i = 0; i < chain.Count; i++)
        {
            var fx = chain[i];
            effects[i] = new CameraSnapshot.EffectEntry(fx.Id, fx.Target, fx.Current, i, scheduler.IsRemoving(fx.Id));
        }
        return new CameraSnapshot(rig.Position, rig.OrthoSize, effects, scheduler.Clock, currentLevelIndex,
            visionRadius, revealAll, fog?.CopyExplored(), fog?.Width ?? 0, fog?.Height ?? 0);
    }

    // Puts the view back to a captured snapshot. null or a different fog grid size = error, no change.
    public void RestoreState(CameraSnapshot snapshot)
    {
        if (snapshot == null)
        {
            Debug.LogError("[F5] RestoreState(null) ignored.");
            return;
        }
        int w = fog?.Width ?? 0, h = fog?.Height ?? 0;
        if (snapshot.GridWidth != w || snapshot.GridHeight != h)
        {
            Debug.LogError($"[F5] RestoreState: snapshot grid {snapshot.GridWidth}x{snapshot.GridHeight} doesn't match {w}x{h}; ignored.");
            return;
        }

        currentLevelIndex = snapshot.LevelIndex;
        scheduler.LoadSchedule(ScheduleFor(snapshot.LevelIndex));   // re-queue that level's rows
        scheduler.SetClock(snapshot.ScheduleClock);                 // rows already due count as applied
        scheduler.RestoreEffects(snapshot.Effects);                 // same chain, order and intensities

        rig.SetPose(snapshot.Position, snapshot.OrthoSize);
        rig.CancelShake();

        visionRadius = snapshot.VisionRadius;
        revealAll = snapshot.RevealAll;
        if (fog != null) fog.LoadExplored(snapshot.CopyExplored());
        fogDirty = true;
    }

    // ---------- Per-frame ----------

    void LateUpdate()
    {
        scheduler.RampSeconds = rampSeconds;

        // Scaled time: when the game pauses (timeScale 0) follow, ease and ramps all freeze.
        float dt = Time.deltaTime;
        var baseState = rig.Tick(dt);
        RevealAroundTarget();
        scheduler.Tick(dt);

        var state = pipeline.Run(baseState);
        LastState = state;

        ApplyToCamera(state);
        DrawFogAndMinimap(state);

        // Only raise on a change, never twice in a row with the same value.
        if (state.blackedOut != wasBlackedOut)
        {
            wasBlackedOut = state.blackedOut;
            BlackoutChanged?.Invoke(state.blackedOut);
        }
    }

    //lifts all fog around the follow target and reports the progress
    private void RevealAroundTarget()
    {
        if (fog == null || rig.Target == null) return;
        if(fog.Reveal(fog.WorldToCell(rig.Target.position), visionRadius) > 0)
        {
            fogDirty = true;
            ExplorationChanged?.Invoke(fog.ExploredFraction); // only when it went up so at most once per frames
        }
    }

    //Redraws only what has changed / the fog picel cells wer revealed in the minimap cells when its disortoion changed
    private void DrawFogAndMinimap(CameraState s)
    {
        if(fog == null) return;
        if(fogDirty) fogOverlay.Redraw(fog);
        fogOverlay.SetOpacity(revealAll ? 0f : s.fogOpacity);

        bool mirrored = minimapDistorted && s.mirrored;
        float rotation = minimapDistorted ? s.rotationDeg : 0f;
        minimap.Distorted = minimapDistorted;
        if (fogDirty || mirrored != minimapMirrored || rotation != minimapRotation || s.blackedOut != minimapBlack)
        {
            minimap.Draw(fog, s, revealAll);
            minimapMirrored = mirrored;
            minimapRotation = rotation;
            minimapBlack = s.blackedOut;
        }

        fogDirty = false;
    }


    private void ApplyToCamera(CameraState s)
    {
        // Position and zoom. Keep the camera's z so it stays in front of the 2D scene.
        // Shake is added here only, so the logical position (and the follow) never moves.
        var t = cam.transform;
        Vector2 p = s.position + rig.ShakeOffset;
        t.position = new Vector3(p.x, p.y, t.position.z);
        cam.orthographicSize = s.orthoSize;

        // Rotation. NOTE: TL6's CameraLockRotation also writes rotation in LateUpdate;
        // if both are on the same camera, whichever runs last wins.
        t.rotation = Quaternion.Euler(0f, 0f, s.rotationDeg);

        // Mirror: flip the projection horizontally. Reset first so flips don't stack up frame to frame.
        cam.ResetProjectionMatrix();
        if (s.mirrored)
            cam.projectionMatrix = cam.projectionMatrix * Matrix4x4.Scale(new Vector3(-1f, 1f, 1f));

        // Blackout: draw nothing, clear to black. Restore the saved settings when it ends.
        if (s.blackedOut)
        {
            cam.cullingMask = 0;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
        }
        else
        {
            cam.cullingMask = savedCullingMask;
            cam.clearFlags = savedClearFlags;
            cam.backgroundColor = savedBackground;
        }

        // Desaturation: read by the Hidden/F5/Desaturate full-screen pass in OnRenderImage.
        saturation = s.saturation;
        Shader.SetGlobalFloat("_Saturation", s.saturation);
    }

    // Built-in render pipeline full-screen pass: greys the whole frame when saturation < 1.
    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (desaturateMat != null && saturation < 1f) Graphics.Blit(src, dst, desaturateMat);
        else Graphics.Blit(src, dst);
    }

    // Temporary: shows the minimap in the top-right corner until the HUD displays it.
    // Unity calls OnGUI by itself every frame; don't call it from code.
    void OnGUI()
    {
        if (!showMinimapDebug || Minimap == null) return;
        const float size = 160f;
        GUI.DrawTexture(new Rect(Screen.width - size - 10f, 10f, size, size), Minimap);
    }

    // Right-click the component in the Inspector (Play Mode) to try each effect by hand.
    [ContextMenu("Test/Rotate 45°")]   void TestRotate()   => ApplyDistortion(DistortionID.Rotation, 0.125f);
    [ContextMenu("Test/Mirror")]       void TestMirror()   => ApplyDistortion(DistortionID.Mirror, 1f);
    [ContextMenu("Test/Desaturate")]   void TestDesat()    => ApplyDistortion(DistortionID.Desaturation, 1f);
    [ContextMenu("Test/Blackout")]     void TestBlackout() => ApplyDistortion(DistortionID.Blackout, 1f);
    [ContextMenu("Test/Fog")]          void TestFog()      => ApplyDistortion(DistortionID.Fog, 1f);
    [ContextMenu("Test/Reveal all")]   void TestReveal()   => RevealAll(!revealAll);
    [ContextMenu("Test/Shake")]        void TestShake()    => Shake(0.5f, 1f);
    [ContextMenu("Test/Clear all")]    void TestClear()    => ClearDistortions();
}
