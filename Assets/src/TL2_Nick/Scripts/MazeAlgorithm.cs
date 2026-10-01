using UnityEngine;

public class MazeAlgorithm
{
    public virtual string PostProcess() {
        Debug.Log("Posting the process");
        return "Regular postprocess";
    }
}
