using System.Data;
using UnityEditor;
using UnityEngine;

public class PuzzleCompletionRule : LevelRule
{
    public override string Evaluate()
    {
        Debug.Log("Evaluated puzzle!");
        return "evaluate puzzle";
    }
}
