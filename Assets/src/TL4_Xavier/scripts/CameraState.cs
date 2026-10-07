


using UnityEngine;

//struct so passing it downt the chaiin allpcates nothing (Stress test: 0 B GC/frame)

public struct CameraState
{
    public Vector2 positon;
    public float orthoSize;
    public float rotationDeg; //[0,360)
    public float mirrored;
    public float saturation; // [0,1]
    public float fogOpacity; // [0,1]
    public bool blackedOut
    //public FogMask mask; will add later during task 7 

    // accumalting in order chain 

    public Matrix4x4 distortion;

    public static Camera Default(Vector2 pos, float ortho) => new CameraState
    {
        positon = pos,
        orthoSize = ortho,
        rotationDeg = 0f,
        mirrored = false,
        saturation = 1f,
        fogOpacity = 0f,
        blackedOut = false,
        distortion = Matrix4x4.identity
    };



    //derived T.R.S
    public Matrix4x4 ViewMatrix =>
        Matrix4x4.TRS(positonm Quaternion.Euler(0,0, rotationDeg),
            new Vector3(mirrored ? -1f : 1f, 1f, 1f));

}