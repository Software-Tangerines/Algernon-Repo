using UnityEngine;

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

public class DesaturationViewEffect : ViewEffect
{
    public override DistortionID Id => DistortionID.Desaturation;

    public override CameraState Process(CameraState s)
    {
        s.saturation = 1f - I;
        return base.Process(s);
    }
}

public class BlackoutViewEffect : ViewEffect
{
    public override DistortionID Id => DistortionID.Blackout;

    public override CameraState Process(CameraState s)
    {
        if (I >= 0.5f)
        {
            s.blackedOut = true;
            return s;              // HALT Next never runs (this is what makes it CoR, not Decorator)
        }
        return base.Process(s);
    }
}