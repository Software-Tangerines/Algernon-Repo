using System;
using UnityEngine;

/*
F5 CameraRig: decides WHERE the camera is and HOW FAR it is zoomed out, before any distortion.

Plain C# class (not a MonoBehaviour). ViewDirector owns one, forwards BindFollowTarget/FrameMaze
to it, and calls Tick() once per frame. The CameraState that Tick() returns is the "base state"
that then goes down the ViewEffect chain (mirror, rotation, ...). The rig never knows about effects.

Level-start flow (Champion doc, scenario FL1):
  1. F1 calls BindFollowTarget(mouse)   -> remembers what to follow
  2. F1 calls FrameMaze(maze bounds)    -> snaps to the maze centre, zooms so the whole maze + 1 cell fits
  3. over the next 0.5 s the camera eases from the maze centre to the mouse
  4. when the ease finishes, Framed fires (exactly once per valid FrameMaze)
  5. after that it follows the mouse with smoothing

Not done yet (later tasks): Shake (task 4.4), clamping to max size switching the minimap to overview (task 8).
*/
public class CameraRig
{
    // Contract limits from the Champion doc, section 3.2.
    public const float MaxOrthoSize = 40f;       // FrameMaze never zooms out further than this
    public const float FrameMarginCells = 1f;    // empty border around the maze (1 cell = 1 world unit, F2's grid)
    public const float FrameEaseSeconds = 0.5f;  // time to ease from the framed view onto the mouse

    // How fast follow catches up. With 20 the camera closes about 99 % of the gap in 0.25 s,
    // which meets the contract bound "|camera - target| <= 0.05 units within 0.25 s of the target stopping".
    public float FollowSharpness { get; set; } = 20f;

    // Raised when the framing ease completes. ViewDirector re-raises it to F1.
    public event Action<Bounds> Framed;

    public Transform Target { get; private set; }
    public Vector2 Position { get; private set; }
    public float OrthoSize { get; private set; }

    private Bounds framedBounds;
    private float easeElapsed;
    private bool easing;            // true between a valid FrameMaze and the Framed event

    // Starts wherever the Unity camera already is, so a scene without F1 still looks normal.
    public CameraRig(Vector2 startPosition, float startOrthoSize)
    {
        Position = startPosition;
        OrthoSize = startOrthoSize;
    }

    // null is allowed: follow turns off and the camera holds where it is (one warning, per contract).
    public void Bind(Transform target)
    {
        if (target == null)
            Debug.LogWarning("[F5] BindFollowTarget(null): follow disabled, camera holds its position.");
        Target = target;
    }

    // Returns false (and changes nothing) if the bounds are zero, negative, or not finite.
    public bool Frame(Bounds b, float aspect)
    {
        Vector3 size = b.size;
        if (!IsFinite(size.x) || !IsFinite(size.y) || size.x <= 0f || size.y <= 0f
            || !IsFinite(b.center.x) || !IsFinite(b.center.y))
        {
            Debug.LogError($"[F5] FrameMaze rejected invalid bounds: {b}. Camera unchanged; F1 must call again.");
            return false;
        }
        if (!IsFinite(aspect) || aspect <= 0f) aspect = 1f;

        // orthographicSize is HALF the visible height. Pick whichever axis needs more room:
        // height needs h/2 + margin, width needs (w/2 + margin) / aspect.
        float halfH = size.y * 0.5f + FrameMarginCells;
        float halfW = size.x * 0.5f + FrameMarginCells;
        float needed = Mathf.Max(halfH, halfW / aspect);

        if (needed > MaxOrthoSize)
            Debug.LogWarning($"[F5] Maze needs ortho size {needed:F1}; clamped to {MaxOrthoSize}.");

        OrthoSize = Mathf.Min(needed, MaxOrthoSize);
        Position = b.center;            // snap to the maze centre, so it is fully framed right away
        framedBounds = b;
        easeElapsed = 0f;
        easing = true;
        return true;
    }

    // Call once per frame with SCALED delta time, so pausing (timeScale 0) freezes the ease.
    // Returns the undistorted camera state for this frame.
    public CameraState Tick(float dt)
    {
        if (dt < 0f || !IsFinite(dt)) dt = 0f;

        if (easing)
        {
            easeElapsed += dt;
            float t = Mathf.Clamp01(easeElapsed / FrameEaseSeconds);

            // Blend from the maze centre to the mouse. SmoothStep = gentle start and stop.
            if (Target != null)
                Position = Vector2.Lerp(framedBounds.center, Target.position, Mathf.SmoothStep(0f, 1f, t));

            if (t >= 1f)
            {
                easing = false;
                Framed?.Invoke(framedBounds);
            }
        }
        else if (Target != null)
        {
            // Frame-rate independent exponential smoothing: the same feel at 30 or 144 fps.
            float k = 1f - Mathf.Exp(-FollowSharpness * dt);
            Position = Vector2.Lerp(Position, Target.position, k);
        }

        return CameraState.Default(Position, OrthoSize);
    }

    private static bool IsFinite(float f) => !float.IsNaN(f) && !float.IsInfinity(f);
}
