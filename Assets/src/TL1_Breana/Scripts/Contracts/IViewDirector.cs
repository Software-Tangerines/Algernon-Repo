// What F1 needs from F5. ApplyDistortion(...) will be added once the distortion enum is agreed with TL4.
using UnityEngine;

public interface IViewDirector
{
    void BindFollowTarget(Transform target);
    void FrameMaze(Bounds mazeBounds);
    void ClearDistortions();
}
