using System;
using UnityEngine;

public class GameCoordinator : MonoBehaviour, ISessionCoordinator
{
    public static GameCoordinator Instance { get; private set; }

    //The coordinator owns the machine (composition, per your class diagram)
    private readonly SessionMachine sessionMachine = new SessionMachine();

    // ---------- Outputs ----------

    //Read-only view of the state; there is no second copy, it always asks the machine
    public SessionState State => sessionMachine.State;

    //Placeholders: return empty structs until LevelCatalogue and stat tracking exist
    public LevelConfig CurrentLevel => default;
    public RunStatistics Statistics => default;

    // ---------- Events ----------

    //Other features subscribe; fired later with e.g. OnLevelStarted?.Invoke();
    public event Action<LevelConfig> OnLevelStarted;
    public event Action<LevelResult> OnLevelEnded;
    public event Action<RunStatistics> OnRunEnded;

    // ---------- Unity lifecycle ----------

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

    // Update is called once per frame
    private void Update()
    {

    }

    // ---------- Inputs ----------

    public void StartRun(int startingLevelIndex)
    {
        sessionMachine.TransitionTo(SessionState.LevelLoading);
        sessionMachine.TransitionTo(SessionState.Playing);
    }

    //Stubs: filled in Week 2
    public void RestartLevel()
    {
    }

    public void AdvanceLevel()
    {
    }

    public void Pause(bool paused)
    {
    }

    public void AbortRun()
    {
    }
}
