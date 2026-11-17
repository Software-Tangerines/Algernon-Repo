using UnityEngine;
using TMPro;
using System;

public class TestSpawner : MonoBehaviour
{
    [SerializeField] private TMP_Text numberOfObjectsViewer;
    [SerializeField] private TMP_Text fpsCounter;
    [SerializeField] private GameObject objectToSpawn;
    private int numObjects = 0;
    int currentFPS = int.MaxValue;

    // Update is called once per frame
    void Update()
    {
        if (currentFPS > 60 || numObjects < 20) {
            currentFPS = Mathf.RoundToInt(1f / Time.unscaledDeltaTime);
            Instantiate(objectToSpawn);
            ++numObjects;
            numberOfObjectsViewer.SetText("{0}", numObjects);
            fpsCounter.SetText("fps: {0}", currentFPS);
        }
    }
}