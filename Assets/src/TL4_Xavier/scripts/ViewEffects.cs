using UnityEngine;

/*
F5 concrete distortion effects. Each one reads its current intensity I (0..1), changes the
CameraState, and forwards it with base.Process(s). Blackout is the exception: it can stop the chain.
These only change how the frame LOOKS; game logic and collision never see them.
*/

// Flips the picture left/right when intensity >= 0.5. Two mirrors in a chain cancel out.
public class MirrorViewEffect : ViewEffect
{
    public override DistortionID Id => DistortionID.Mirror;

    public override CameraState Process(CameraState s)
    {
        if (I >= 0.5f)
        {
            s.mirrored = !s.mirrored;
            s.distortion = Matrix4x4.Scale(new Vector3(-1f, 1f, 1f)) * s.distortion;
        }
        return base.Process(s);   // forward to Next
    }
}

// Rotates the screen by 360 * intensity degrees (0.25 = 90°, 1.0 = full turn = 0°).
public class RotationViewEffect : ViewEffect
{
    public override DistortionID Id => DistortionID.Rotation;

    public override CameraState Process(CameraState s)
    {
        float deg = (360f * I) % 360f;
        s.rotationDeg = deg;
        s.distortion = Matrix4x4.Rotate(Quaternion.Euler(0, 0, deg)) * s.distortion;
        return base.Process(s);
    }
}

// Drains colour: intensity 0 = full colour, 1 = greyscale. Ramp it with the level timer for a slow fade.
public class DesaturationViewEffect : ViewEffect
{
    public override DistortionID Id => DistortionID.Desaturation;

    public override CameraState Process(CameraState s)
    {
        s.saturation = 1f - I;
        return base.Process(s);
    }
}

// Turns the screen black when intensity >= 0.5 and STOPS the chain: effects after it never run.
// Below 0.5 it behaves like a normal pass-through link.
public class BlackoutViewEffect : ViewEffect
{
    public override DistortionID Id => DistortionID.Blackout;

    public override CameraState Process(CameraState s)
    {
        if (I >= 0.5f)
        {
            s.blackedOut = true;
            return s;              // HALT: Next never runs (this is what makes it CoR, not Decorator)
        }
        return base.Process(s);
    }
}
