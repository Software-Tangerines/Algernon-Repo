using System;
using System.Collections.Generic;
using UnityEngine; 

// MVP version of the F5: runs the scheduler + chain and applies the

[RequireComponent(typeof(Camera))]
public class ViewDirector : MonoBehaviour
{
    [Tooltip("Seconds for an effect to ramp to its new intensity.")]
    public float rampSeconds = 1f;

    [Tooltip("One schedule per level (stand-in for the D1 table).")]
    public List<LevelSchedule> levelSchedules = new List<LevelSchedule>();

    [Tooltip("MVP only: load this level's schedule on Start so you can watch it play.")]
    public int autoStartLevel = 0;

    public event Action<DistortionID, float> DistortionChanged;
    public event Action<bool> BlackoutChanged;

    public CameraState LastState { get; private set; }

    private Camera cam;
    private ViewEffectPipeline pipeline;
    private DistortionScheduler scheduler;
    private bool wasBlackedOut;

    // Saved so blackout can be undone.
    private int savedCullingMask;
    private CameraClearFlags savedClearFlags;
    private Color savedBackground;

    void Awake()
    {
        cam = GetComponent<Camera>();
        savedCullingMask = cam.cullingMask;
        savedClearFlags = cam.clearFlags;
        savedBackground = cam.backgroundColor;

        pipeline = new ViewEffectPipeline();
        scheduler = new DistortionScheduler(pipeline, rampSeconds);
        scheduler.DistortionChanged += (id, v) => DistortionChanged?.Invoke(id, v);
    }

    void Start()
    {
        if (autoStartLevel >= 0) LoadSchedule(autoStartLevel);
    }


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
    public void ClearDistortions() => scheduler.Clear();

    // -----------------------------------------------------------

    void LateUpdate()
    {
        scheduler.RampSeconds = rampSeconds;
        scheduler.Tick(Time.deltaTime);   // scaled time pausing timeScale 0 freezes ramps

        var baseState = CameraState.Default(cam.transform.position, cam.orthographicSize);
        var state = pipeline.Run(baseState);
        LastState = state;

        ApplyToCamera(state);

        if (state.blackedOut != wasBlackedOut)
        {
            wasBlackedOut = state.blackedOut;
            BlackoutChanged?.Invoke(state.blackedOut);
        }
    }

    private void ApplyToCamera(CameraState s)
    {
        // Rotation
        cam.transform.rotation = Quaternion.Euler(0f, 0f, s.rotationDeg);

        // Mirror flip the projection horizontally 
        cam.ResetProjectionMatrix();
        if (s.mirrored)
            cam.projectionMatrix = cam.projectionMatrix * Matrix4x4.Scale(new Vector3(-1f, 1f, 1f));

        // Blackout
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

        // Desaturation:  not  yet
        Shader.SetGlobalFloat("_Saturation", s.saturation);
    }

    //component in the mnspector

    [ContextMenu("Test/Rotate 45°")]   void TestRotate()   => ApplyDistortion(DistortionID.Rotation, 0.125f);
    [ContextMenu("Test/Mirror")]       void TestMirror()   => ApplyDistortion(DistortionID.Mirror, 1f);
    [ContextMenu("Test/Desaturate")]   void TestDesat()    => ApplyDistortion(DistortionID.Desaturation, 1f);
    [ContextMenu("Test/Blackout")]     void TestBlackout() => ApplyDistortion(DistortionID.Blackout, 1f);
    [ContextMenu("Test/Clear all")]    void TestClear()    => ClearDistortions();
}