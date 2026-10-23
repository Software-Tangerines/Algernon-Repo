using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameCoordinator : MonoBehaviour, ISessionCoordinator
{
    public static GameCoordinator Instance { get; private set; }
    private const string GameplayScene = "MVP";

    //The coordinator owns the machine and its helpers (composition, per your class diagram)
    private readonly SessionMachine sessionMachine = new SessionMachine();
    private readonly LevelCatalogue catalogue = new LevelCatalogue();
    private readonly RunTimer timer = new RunTimer();
    private readonly List<LevelRule> activeRules = new List<LevelRule>();

    private int currentLevelIndex;
    private LevelConfig currentLevel;
    private RunStatistics statistics;

    //Other features: Null stand-ins until a real implementation is found in the loaded scene
    private IMazeQuery maze = new NullMaze();
    private IPlayerCharacter player = new NullPlayer();
    private IPuzzleSystem puzzles = new NullPuzzles();
    private IHazardSystem hazards = new NullHazards();
    private IViewDirector view = new NullView();
    private IHud hud = new NullHud();

    // ---------- Outputs ----------

    //Read-only view of the state; there is no second copy, it always asks the machine
    public SessionState State => sessionMachine.State;

    public LevelConfig CurrentLevel => currentLevel;
    public RunStatistics Statistics => statistics;

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

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        sessionMachine.TransitionTo(SessionState.Menu);
    }

    // Update is called once per frame
    private void Update()
    {

    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Only the surviving coordinator reacts, and only when a level is waiting to load
        if (Instance == this && State == SessionState.LevelLoading)
        {
            BeginLevel();
        }
    }

    // ---------- Inputs ----------

    public void StartRun(int startingLevelIndex)
    {
        if (!catalogue.IsValidIndex(startingLevelIndex))
        {
            Debug.LogError($"StartRun: level index {startingLevelIndex} is outside 0..{catalogue.Count - 1}");
            return;
        }
        if (!sessionMachine.TransitionTo(SessionState.LevelLoading))
        {
            Debug.LogError($"StartRun: a run can't start from {State}");
            return;
        }

        statistics = default;
        currentLevelIndex = startingLevelIndex;
        SceneManager.LoadScene(GameplayScene);
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

    // ---------- Level setup ----------

    // Runs once the gameplay scene has loaded: find features, configure the level, then start playing.
    private void BeginLevel()
    {
        FindFeatures();

        currentLevel = catalogue.Resolve(currentLevelIndex);
        timer.Reset(currentLevel.TimeLimitSeconds);
        BuildRules();

        puzzles.ResetPuzzles();
        hazards.ResetHazards();
        hazards.SetActive(true);
        player.SpawnAt(maze.CellToWorld(maze.SpawnCell));
        player.EnableInput(true);

        sessionMachine.TransitionTo(SessionState.Playing);
        OnLevelStarted?.Invoke(currentLevel);
    }

    // Cheese levels: lose rules first so a loss wins a same-frame tie, then the exit.
    private void BuildRules()
    {
        activeRules.Clear();
        activeRules.Add(new NoDeathRule());
        activeRules.Add(new TimedSurvivalRule());
        activeRules.Add(new ReachExitRule(maze.ExitCell));
    }

    // Swap each Null stand-in for a real feature if one exists in the loaded scene.
    private void FindFeatures()
    {
        maze = FindFeature(maze);
        player = FindFeature(player);
        puzzles = FindFeature(puzzles);
        hazards = FindFeature(hazards);
        view = FindFeature(view);
        hud = FindFeature(hud);
    }

    private static T FindFeature<T>(T fallback) where T : class
    {
        foreach (MonoBehaviour behaviour in FindObjectsByType<MonoBehaviour>())
        {
            if (behaviour is T feature)
            {
                return feature;
            }
        }
        return fallback;
    }
}
