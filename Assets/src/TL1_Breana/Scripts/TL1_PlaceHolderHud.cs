using UnityEngine;
using System;

public class TL1_PlaceHolderHud : MonoBehaviour, IHud
{
    [SerializeField] private GameObject pausePanel;
    public event Action OnStartPressed { add { } remove { } }
    public event Action OnRetryPressed { add { } remove { } }
    public event Action OnQuitPressed  { add { } remove { } }
    public void ShowPause(bool paused)
    {
        pausePanel.SetActive(paused);
    }

    public void SetTimer(float secondsRemaining)
    {
        
    }
}
