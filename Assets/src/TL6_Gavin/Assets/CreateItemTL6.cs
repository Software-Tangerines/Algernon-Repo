
using UnityEngine;
using TMPro;

public class CreateItemTL6 : MonoBehaviour
{
    [SerializeField] private TMP_Text numberOfItemsViewer;
    [SerializeField] private TMP_Text fpsCounter;
    [SerializeField] private GameObject item;

    private int numItems = 0;
    private int currentFPS = 1000;

    void Start()
    {
    }

    void Update()
    {
        currentFPS = Mathf.RoundToInt(1f / Time.unscaledDeltaTime);

        if (currentFPS > 60 || numItems < 20)
        {
            Instantiate(item);
            ++numItems;

            numberOfItemsViewer.SetText("{0}", numItems);
            fpsCounter.SetText("fps: {0}", currentFPS);
        }
        else
        {
            Debug.Log("fps dropped below 60 to " + currentFPS
                + " at " + numItems + " items spawned in");
        }
    }
}
