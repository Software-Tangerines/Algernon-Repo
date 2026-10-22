// Stand-in until F5 exists: the scene's camera stays where it is.
using UnityEngine;

public class NullView : IViewDirector
{
    public void BindFollowTarget(Transform target) => Debug.Log("NullView: follow target ignored");
    public void FrameMaze(Bounds mazeBounds) { }
    public void ClearDistortions() { }
}
