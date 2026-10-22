using UnityEngine;

// On-screen dynamic-binding demo. Toggle the objective; the label shows the verdict.
// Demo: remove "virtual" in LevelRule and change "override" to "new" in PuzzleCompletionRule,
// then the verdict stays InProgress no matter what, because the base method runs.
public class TL1_DynamicBindingShowcase : MonoBehaviour
{
    private readonly LevelRule rule = new PuzzleCompletionRule();
    private bool objectiveComplete;

    void OnGUI()
    {
        var snapshot = new SessionSnapshot(Vector2Int.zero, 10f, objectiveComplete, false);

        GUI.Label(new Rect(20, 20, 400, 30), $"Objective complete: {objectiveComplete}  ->  Verdict: {rule.Evaluate(snapshot)}");
        if (GUI.Button(new Rect(20, 60, 160, 30), "Toggle objective"))
            objectiveComplete = !objectiveComplete;
    }
}
