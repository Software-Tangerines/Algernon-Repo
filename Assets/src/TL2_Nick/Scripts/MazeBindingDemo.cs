using UnityEngine;

public class MazeBindingDemo : MonoBehaviour
{
    void Start() {
        MazeAlgorithm algo = new BraidedAlgo();

        Debug.Log(algo.PostProcess());
    }
}
