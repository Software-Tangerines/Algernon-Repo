using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.XR;

public class GameCoordinator : MonoBehaviour
{
    public static GameCoordinator Instance { get; private set; }

    //The coordinator owns the machine (composition, per your class diagram)
    private readonly SessionMachine sessionMachine = new SessionMachine();

    //Read-only view of the state; there is no second copy, it always asks the machine
    public SessionState State => sessionMachine.State;
    private GameState currentState;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        sessionMachine.TransitionTo(SessionState.Menu);
        ChangeState(new MenuState());
    }

    public void StartRun(int startingLevelIndex)
    {
        sessionMachine.TransitionTo(SessionState.Menu);
        sessionMachine.TransitionTo(SessionState.LevelLoading);
        sessionMachine.TransitionTo(SessionState.Playing);
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
