using UnityEngine;

public class TL1_DynamicBindingShowcase : MonoBehaviour
{
    LevelRule current = new PuzzleCompletionRule();

    void OnGUI()
    {
        GUI.Label(new Rect(20, 20, 400, 30), current.Evaluate());
        if(GUI.Button(new Rect(20, 60, 160, 30), "Swap"))
        current = (current is PuzzleCompletionRule) ? new LevelRule() : new PuzzleCompletionRule();
    }
}
