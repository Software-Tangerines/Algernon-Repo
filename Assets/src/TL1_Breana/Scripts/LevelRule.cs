using UnityEngine;

public class LevelRule
{
    public virtual string Evaluate()
    {
        Debug.Log("Evaluated level!");
        return "evaluate level";
    }
}
