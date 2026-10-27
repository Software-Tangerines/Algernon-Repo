/*
F5 DistortionID: every kind of visual distortion F5 can apply.

The numbers are fixed (they are saved in level schedules in the Inspector), so don't reorder them.
Pass these to ViewDirector.ApplyDistortion / RemoveDistortion.

How intensity (0..1) maps to each one:
  Mirror        flipped when intensity >= 0.5
  Rotation      rotationDeg = 360 * intensity   (0.25 = 90°)
  Fog           fogOpacity  = intensity          (task 7, not implemented yet)
  Desaturation  saturation  = 1 - intensity      (1 = fully grey)
  Blackout      screen black when intensity >= 0.5; later effects don't run
  Blur          reserved stretch goal; ViewDirector ignores it with a warning

These only change the picture. Mirrored CONTROLS are F7's job (TL6).
*/
public enum DistortionID
{
    Mirror = 0,
    Rotation = 1,
    Fog = 2,
    Desaturation = 3,
    Blackout = 4,
    Blur = 5
}
