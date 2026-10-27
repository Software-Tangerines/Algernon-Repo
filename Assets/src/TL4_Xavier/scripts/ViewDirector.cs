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

Events to subscribe to:
  Framed(Bounds)                   framing ease finished (once per valid FrameMaze)
  DistortionChanged(id, intensity) a target intensity changed (0 = removed); once per change, not per frame
  BlackoutChanged(bool)            the screen went black / came back

F5 only changes the PICTURE. It never moves the mouse, reads input, or touches collision.
Mirrored CONTROLS belong to F7 (TL6), not here.

Not done yet: fog + vision radius (task 7), minimap (task 8), Shake, CaptureState/RestoreState (task 10).
*/
[RequireComponent(typeof(Camera))]
public class ViewDirector : MonoBehaviour, IViewDirector
{
    [Tooltip("Seconds for an effect to ramp to its new intensity.")]
    public float rampSeconds = 1f;

    [Tooltip("One schedule per level, indexed like F1's LevelCatalogue (stand-in for the D1 table).")]
    public List<LevelSchedule> levelSchedules = new List<LevelSchedule>();

    [Tooltip("MVP only: load this level's schedule on Start so you can watch it play. -1 = don't auto-load (use this once F1 calls LoadSchedule).")]
    public int autoStartLevel = 0;

    public event Action<Bounds> Framed;
    public event Action<DistortionID, float> DistortionChanged;
    public event Action<bool> BlackoutChanged;

    // The final state of the last rendered frame. Handy for debugging and tests.
    public CameraState LastState { get; private set; }

    private Camera cam;
    private CameraRig rig;
    private ViewEffectPipeline pipeline;
    private DistortionScheduler scheduler;
    private bool wasBlackedOut;

    // The camera's original settings, saved so a blackout can be undone exactly.
    private int savedCullingMask;
    private CameraClearFlags savedClearFlags;
    private Color savedBackground;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (!cam.orthographic)
            Debug.LogWarning("[F5] ViewDirector expects an orthographic camera; FrameMaze zoom will have no effect.");

        savedCullingMask = cam.cullingMask;
        savedClearFlags = cam.clearFlags;
        savedBackground = cam.backgroundColor;

        // Start the rig where the camera already is, so nothing jumps before F1 calls FrameMaze.
        rig = new CameraRig(cam.transform.position, cam.orthographicSize);
        rig.Framed += b => Framed?.Invoke(b);

        pipeline = new ViewEffectPipeline();
        scheduler = new DistortionScheduler(pipeline, rampSeconds);
        scheduler.DistortionChanged += (id, v) => DistortionChanged?.Invoke(id, v);
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
    public void FrameMaze(Bounds mazeBounds) => rig.Frame(mazeBounds, cam.aspect);

    // ---------- Distortions ----------

    // Clears the chain and restarts the schedule clock at 0. Unknown index = no distortions + warning.
    public void LoadSchedule(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levelSchedules.Count)
        {
            Debug.LogWarning($"[F5] No schedule for level {levelIndex}; view stays undistorted.");
            scheduler.LoadSchedule(null);
            return;
        }
        scheduler.LoadSchedule(levelSchedules[levelIndex].entries);
    }

    public void ApplyDistortion(DistortionID id, float intensity) => scheduler.Apply(id, intensity);
    public void RemoveDistortion(DistortionID id) => scheduler.Remove(id);

    // Part of IViewDirector: F1 calls this between levels so nothing carries over.
    public void ClearDistortions() => scheduler.Clear();

    // ---------- Per-frame ----------

    void LateUpdate()
    {
        scheduler.RampSeconds = rampSeconds;

        // Scaled time: when the game pauses (timeScale 0) follow, ease and ramps all freeze.
        float dt = Time.deltaTime;
        var baseState = rig.Tick(dt);
        scheduler.Tick(dt);

        var state = pipeline.Run(baseState);
        LastState = state;

        ApplyToCamera(state);

        // Only raise on a change, never twice in a row with the same value.
        if (state.blackedOut != wasBlackedOut)
        {
            wasBlackedOut = state.blackedOut;
            BlackoutChanged?.Invoke(state.blackedOut);
        }
    }

    private void ApplyToCamera(CameraState s)
    {
        // Position and zoom. Keep the camera's z so it stays in front of the 2D scene.
        var t = cam.transform;
        t.position = new Vector3(s.position.x, s.position.y, t.position.z);
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

        // Desaturation: sent to shaders as a global. No shader reads _Saturation yet (task 6.3).
        Shader.SetGlobalFloat("_Saturation", s.saturation);
    }

    // Right-click the component in the Inspector (Play Mode) to try each effect by hand.
    [ContextMenu("Test/Rotate 45°")]   void TestRotate()   => ApplyDistortion(DistortionID.Rotation, 0.125f);
    [ContextMenu("Test/Mirror")]       void TestMirror()   => ApplyDistortion(DistortionID.Mirror, 1f);
    [ContextMenu("Test/Desaturate")]   void TestDesat()    => ApplyDistortion(DistortionID.Desaturation, 1f);
    [ContextMenu("Test/Blackout")]     void TestBlackout() => ApplyDistortion(DistortionID.Blackout, 1f);
    [ContextMenu("Test/Clear all")]    void TestClear()    => ClearDistortions();
}
