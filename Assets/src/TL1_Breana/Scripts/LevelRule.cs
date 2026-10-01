using System.Data;
using UnityEditor;
using UnityEngine;

public class LevelRule
{
    public virtual string Evaluate()
    {
        Debug.Log("Evaluated level!");
        return "evaluate level";
    }
}
