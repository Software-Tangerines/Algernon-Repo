using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.XR;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private GameState currentState;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        ChangeState(new MenuState());
    }

    // Update is called once per frame
    private void Update()
    {
        currentState?.Update();
    }

    public void ChangeState(GameState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }

    public void StartGame()
    {
        //ChangeState(new PlayingState());
    }
     public void PauseGame()
    {
        //ChangeState(new PausedState());
    }
     public void ResumeGame()
    {
        //ChangeState(new PlayingState());
    }


}
