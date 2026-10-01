using UnityEngine;

public class BraidedAlgo : MazeAlgorithm
{
    public override string PostProcess() {
        Debug.Log("Overrided the posting of the process");
        return "Override";
    }
}
