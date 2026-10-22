using UnityEngine;

public class GameCoordinator : MonoBehaviour
{
    public static GameCoordinator Instance { get; private set; }

    //The coordinator owns the machine (composition, per your class diagram)
    private readonly SessionMachine sessionMachine = new SessionMachine();

    //Read-only view of the state; there is no second copy, it always asks the machine
    public SessionState State => sessionMachine.State;

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
    }

    public void StartRun(int startingLevelIndex)
    {
        sessionMachine.TransitionTo(SessionState.LevelLoading);
        sessionMachine.TransitionTo(SessionState.Playing);
    }

    // Update is called once per frame
    private void Update()
    {

    }
}
