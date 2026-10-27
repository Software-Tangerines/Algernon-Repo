using UnityEngine;

/*
F5 CameraState: everything about how one frame should look, as plain data.

CameraRig makes the base state each frame, every ViewEffect in the chain may change it,
and ViewDirector applies the final one to the Unity Camera.

It is a struct (copied by value), so passing it down the chain allocates no memory.
That is needed for the stress test: 0 bytes of garbage per frame.
*/
public struct CameraState
{
    public Vector2 position;   // camera centre in world units
    public float orthoSize;    // half the visible height in world units, always > 0
    public float rotationDeg;  // screen rotation, in [0, 360)
    public bool mirrored;      // true = picture flipped left/right
    public float saturation;   // 1 = full colour, 0 = greyscale
    public float fogOpacity;   // 0 = no fog, 1 = unexplored cells fully hidden
    public bool blackedOut;    // true = render pure black (set by BlackoutViewEffect)
    //public FogMask mask;     // added in task 7 (fog)

    // Mirror and rotation multiplied together in chain order. The "order matters" test reads this:
    // Rotation then Mirror gives a different result than Mirror then Rotation.
    public Matrix4x4 distortion;

    // A clean, undistorted state at the given position and zoom.
    public static CameraState Default(Vector2 pos, float ortho) => new CameraState
    {
        position = pos,
        orthoSize = ortho,
        rotationDeg = 0f,
        mirrored = false,
        saturation = 1f,
        fogOpacity = 0f,
        blackedOut = false,
        distortion = Matrix4x4.identity
    };

    // Translate, rotate, scale, rebuilt from the fields above every time, so it can never get out of sync.
    public Matrix4x4 ViewMatrix =>
        Matrix4x4.TRS(position, Quaternion.Euler(0, 0, rotationDeg),
            new Vector3(mirrored ? -1f : 1f, 1f, 1f));
}
