using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/*
F6 HudFacade: the ONLY F6 class other features talk to (Facade pattern). HUD, menus and audio.

It implements F1's IHud, so GameCoordinator finds it automatically at level start and starts
calling SetTimer every frame. It is also created automatically when the gameplay scene ("MVP")
loads, so nobody has to add it to a scene. Set AutoSpawn = false to turn that off.

What works with NO changes to F1 (it listens to GameCoordinator's events):
  - live countdown timer, flashing red under 10 s, with a warning sound
  - win / lose screen with run statistics when a level ends (OnLevelEnded)
  - run-complete screen when the run ends (OnRunEnded)
  - death counter, refreshed at each level start
  - sound cues for all of the above, and every menu button

Public calls (all safe: bad input is logged or clamped, never thrown):
  ShowHud(bool)                                  show/hide the in-game HUD bar
  SetTimer(float secondsRemaining)               F1, every frame while playing
  SetObjective(string label, int done, int total) e.g. ("Cheese", 2, 5) -> "Cheese: 2/5"
  SetDeaths(int)                                 death counter
  SetDistortionIndicator(IReadOnlyList<DistortionID>) which F5 distortions are active
  ShowLevelIntro(int levelIndex, string title, Action onDismissed)  card before a level; onDismissed runs once
  ShowResult(LevelResult, RunStatistics)         win/lose screen
  ShowRunComplete(RunStatistics)                 end-of-run screen
  ShowPause(bool)                                F1 calls this when it actually pauses/resumes
  ShowMainMenu(int levelCount)                   title screen with level select and settings
  PlayCue(AudioCueId) / SetMusicIntensity(float) audio

Events (button presses; F1 decides what they mean):
  OnStartPressed, OnRetryPressed, OnQuitPressed   (from IHud)
  OnResumePressed, OnNextLevelPressed, OnPauseRequested (Esc), OnLevelSelected(int)

TEMPORARY BRIDGE: while F1 isn't subscribed to an event, pressing that button calls the matching
GameCoordinator method directly (StartRun, RestartLevel, AdvanceLevel, Pause, AbortRun). Once F1
subscribes, the bridge switches itself off. Note RestartLevel/AdvanceLevel/Pause/AbortRun are still
empty stubs in GameCoordinator, so those buttons do nothing visible until TL1 fills them in.
Pause works the same way: Esc asks F1 to pause, and the pause screen appears only when F1 says so.

Right-click the component in Play Mode for "Preview/..." items that show each screen without F1.
*/
public class HudFacade : MonoBehaviour, IHud
{
    // Create a HudFacade automatically when the gameplay scene loads (if the scene doesn't have one).
    public static bool AutoSpawn = true;
    private const string GameplaySceneName = "MVP";

    // ---------- IHud events ----------
    public event Action OnStartPressed;
    public event Action OnRetryPressed;
    public event Action OnQuitPressed;

    // ---------- Extra events ----------
    public event Action OnResumePressed;
    public event Action OnNextLevelPressed;
    public event Action OnPauseRequested;
    public event Action<int> OnLevelSelected;

    public HudScreen CurrentScreen => router.Current;

    private readonly HudModel model = new HudModel();
    private readonly ScreenRouter router = new ScreenRouter();
    private readonly List<HudWidget> widgets = new List<HudWidget>();
    private readonly List<TMPro.TMP_Text> widgetTexts = new List<TMPro.TMP_Text>();
    private readonly Dictionary<HudScreen, HudCanvas.Panel> panels = new Dictionary<HudScreen, HudCanvas.Panel>();

    private HudCanvas ui;
    private HudCanvas.Panel mainMenu, levelSelect, settings, intro, pause, result, runComplete;
    private UnityEngine.UI.Button nextLevelButton;
    private int renderedVersion = -1;
    private int levelSelectCount = -1;

    private Action pendingIntroDismissed;
    private int introShownFrame;

    private GameCoordinator coordinator;    // null when the scene is played without F1

    // ---------- Auto-spawn ----------

    // Hooks scene loading before any scene loads, so the HUD exists before GameCoordinator looks for it.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InstallAutoSpawn()
    {
        SceneManager.sceneLoaded -= SpawnIfMissing;   // avoid double hooks when domain reload is off
        SceneManager.sceneLoaded += SpawnIfMissing;
    }

    private static void SpawnIfMissing(Scene scene, LoadSceneMode mode)
    {
        if (!AutoSpawn || scene.name != GameplaySceneName) return;
        if (FindAnyObjectByType<HudFacade>() != null) return;
        new GameObject("F6_HudFacade").AddComponent<HudFacade>();
    }

    // ---------- Unity lifecycle ----------

    void Awake()
    {
        ui = new HudCanvas(transform);
        BuildWidgets();
        BuildPanels();
        router.Changed += ApplyScreen;
        ApplyScreen(router.Current);
        _ = AudioDirector.Instance;   // make sure the audio player exists before the first cue
    }

    void OnEnable()
    {
        coordinator = GameCoordinator.Instance;
        if (coordinator == null) return;
        coordinator.OnLevelStarted += HandleLevelStarted;
        coordinator.OnLevelEnded += HandleLevelEnded;
        coordinator.OnRunEnded += ShowRunComplete;
    }

    void OnDisable()
    {
        if (coordinator == null) return;
        coordinator.OnLevelStarted -= HandleLevelStarted;
        coordinator.OnLevelEnded -= HandleLevelEnded;
        coordinator.OnRunEnded -= ShowRunComplete;
        coordinator = null;
    }

    void Update()
    {
        HandleKeyboard();

        // Red flash under 10 s: on/off twice a second, using real time so it still blinks if time is frozen.
        bool flashOn = model.IsUrgent && Mathf.Repeat(Time.unscaledTime * 2f, 1f) < 0.5f;
        ui.TimerText.color = flashOn ? HudCanvas.UrgentColor : HudCanvas.TextColor;
    }

    void LateUpdate()
    {
        // Rebuild widget text only when something visible changed (see HudModel.Version).
        if (model.Version == renderedVersion) return;
        renderedVersion = model.Version;
        for (int i = 0; i < widgets.Count; i++)
            widgetTexts[i].text = widgets[i].Render(model);   // dynamic binding: which Render runs depends on the widget's type
    }

    // ---------- IHud + HUD calls ----------

    public void ShowHud(bool visible) => router.Show(visible ? HudScreen.Hud : HudScreen.None);

    public void SetTimer(float secondsRemaining)
    {
        bool wasUrgent = model.IsUrgent;
        model.SetTimer(secondsRemaining);
        if (!wasUrgent && model.IsUrgent && model.SecondsRemaining > 0f) PlayCue(AudioCueId.TimerWarning);
        SetMusicIntensity(model.TimeElapsedFraction);
    }

    public void SetObjective(string label, int completed, int total)
    {
        int before = model.ObjectivesCompleted;
        bool hadObjective = model.HasObjective;
        model.SetObjective(label, completed, total);
        if (hadObjective && model.ObjectivesCompleted > before) PlayCue(AudioCueId.ObjectiveProgress);
    }

    public void SetDeaths(int deaths)
    {
        int before = model.Deaths;
        model.SetDeaths(deaths);
        if (model.Deaths > before) PlayCue(AudioCueId.PlayerDeath);
    }

    public void SetDistortionIndicator(IReadOnlyList<DistortionID> active) => model.SetDistortions(active);

    // ---------- Screens ----------

    public void ShowLevelIntro(int levelIndex, string title, Action onDismissed)
    {
        intro.Title.text = StringTable.LevelNumber(levelIndex);
        intro.SetBody(string.IsNullOrEmpty(title) ? StringTable.PressAnyKey : title + "\n\n" + StringTable.PressAnyKey);
        pendingIntroDismissed = onDismissed;
        introShownFrame = Time.frameCount;     // ignore the key press that opened it
        router.Show(HudScreen.LevelIntro);
    }

    public void ShowResult(LevelResult levelResult, RunStatistics stats)
    {
        bool won = levelResult == LevelResult.Won;
        result.Title.text = StringTable.ResultTitle(levelResult);
        result.SetBody(StringTable.Stats(stats));
        nextLevelButton.gameObject.SetActive(won);
        SetMusicIntensity(0f);
        PlayCue(won ? AudioCueId.LevelWon : AudioCueId.LevelLost);
        router.Show(HudScreen.Result);
    }

    public void ShowRunComplete(RunStatistics stats)
    {
        runComplete.SetBody(StringTable.Stats(stats));
        AudioDirector.Instance?.StopMusic();
        PlayCue(AudioCueId.RunComplete);
        router.Show(HudScreen.RunComplete);
    }

    // F1 calls this once it has actually paused or resumed the game.
    public void ShowPause(bool paused)
    {
        if (paused)
        {
            if (router.Current == HudScreen.Pause) return;
            router.Show(HudScreen.Pause);
            PlayCue(AudioCueId.Pause);
        }
        else if (router.Current == HudScreen.Pause || (router.Current == HudScreen.Settings && router.ReturnTo == HudScreen.Pause))
        {
            router.Show(HudScreen.Hud);
            PlayCue(AudioCueId.Resume);
        }
    }

    public void ShowMainMenu(int levelCount)
    {
        RebuildLevelSelect(Mathf.Max(1, levelCount));
        AudioDirector.Instance?.StopMusic();
        router.Show(HudScreen.MainMenu);
    }

    // ---------- Audio ----------

    public void PlayCue(AudioCueId cue) => AudioDirector.Instance?.PlayCue(cue);

    public void SetMusicIntensity(float normalised) => AudioDirector.Instance?.SetMusicIntensity(normalised);

    // ---------- F1 event handlers ----------

    private void HandleLevelStarted(LevelConfig level)
    {
        model.ResetForLevel();
        model.SetDeaths(coordinator.Statistics.Deaths);
        ShowHud(true);
        PlayCue(AudioCueId.LevelStart);
        AudioDirector.Instance?.PlayMusic();
    }

    private void HandleLevelEnded(LevelResult levelResult)
    {
        model.SetDeaths(coordinator.Statistics.Deaths);
        ShowResult(levelResult, coordinator.Statistics);
    }

    // ---------- Building ----------

    // Order here = order on screen. Each widget gets its own text slot.
    private void BuildWidgets()
    {
        AddWidget(new TimerWidget(), ui.TimerText);
        AddWidget(new DistortionWarningWidget(), ui.DistortionText);
        AddWidget(new ObjectiveWidget(), ui.ObjectiveText);
        AddWidget(new AttemptsWidget(), ui.DeathsText);
    }

    private void AddWidget(HudWidget widget, TMPro.TMP_Text text)
    {
        widgets.Add(widget);
        widgetTexts.Add(text);
    }

    private void BuildPanels()
    {
        mainMenu = NewPanel(HudScreen.MainMenu, StringTable.GameTitle);
        AddButton(mainMenu, StringTable.Start, () => Raise(OnStartPressed, () => coordinator.StartRun(0), "Start"));
        AddButton(mainMenu, StringTable.LevelSelect, () => router.Show(HudScreen.LevelSelect));
        AddButton(mainMenu, StringTable.Settings, () => router.Show(HudScreen.Settings));
        AddButton(mainMenu, StringTable.QuitGame, QuitGame);

        levelSelect = NewPanel(HudScreen.LevelSelect, StringTable.LevelSelect);   // buttons added by RebuildLevelSelect

        settings = NewPanel(HudScreen.Settings, StringTable.Settings);
        var audio = AudioDirector.Instance;
        AddSlider(settings, StringTable.MasterVolume, VolumeChannel.Master, audio);
        AddSlider(settings, StringTable.MusicVolume, VolumeChannel.Music, audio);
        AddSlider(settings, StringTable.SfxVolume, VolumeChannel.Sfx, audio);
        AddButton(settings, StringTable.Back, router.Back);

        intro = NewPanel(HudScreen.LevelIntro, "");
        AddButton(intro, StringTable.Continue, DismissIntro);

        pause = NewPanel(HudScreen.Pause, StringTable.Paused);
        AddButton(pause, StringTable.Resume, RequestResume);
        AddButton(pause, StringTable.Retry, () => Raise(OnRetryPressed, () => coordinator.RestartLevel(), "Retry"));
        AddButton(pause, StringTable.Settings, () => router.Show(HudScreen.Settings));
        AddButton(pause, StringTable.QuitToMenu, () => Raise(OnQuitPressed, () => coordinator.AbortRun(), "Quit"));

        result = NewPanel(HudScreen.Result, "");
        nextLevelButton = AddButton(result, StringTable.NextLevel, () => Raise(OnNextLevelPressed, () => coordinator.AdvanceLevel(), "Next level"));
        AddButton(result, StringTable.Retry, () => Raise(OnRetryPressed, () => coordinator.RestartLevel(), "Retry"));
        AddButton(result, StringTable.QuitToMenu, () => Raise(OnQuitPressed, () => coordinator.AbortRun(), "Quit"));

        runComplete = NewPanel(HudScreen.RunComplete, StringTable.RunComplete);
        AddButton(runComplete, StringTable.MainMenu, () => Raise(OnQuitPressed, () => coordinator.AbortRun(), "Main menu"));
    }

    private void RebuildLevelSelect(int levelCount)
    {
        if (levelCount == levelSelectCount) return;
        levelSelectCount = levelCount;
        ui.ClearControls(levelSelect);
        for (int i = 0; i < levelCount; i++)
        {
            int index = i;   // capture a copy for the lambda
            AddButton(levelSelect, StringTable.LevelNumber(index), () => SelectLevel(index));
        }
        AddButton(levelSelect, StringTable.Back, router.Back);
    }

    private HudCanvas.Panel NewPanel(HudScreen screen, string title)
    {
        var p = ui.CreatePanel(screen.ToString(), title);
        panels[screen] = p;
        return p;
    }

    // Every button clicks, then does its job.
    private UnityEngine.UI.Button AddButton(HudCanvas.Panel panel, string label, Action onClick) =>
        ui.AddButton(panel, label, () => { PlayCue(AudioCueId.ButtonClick); onClick(); });

    private void AddSlider(HudCanvas.Panel panel, string label, VolumeChannel channel, AudioDirector audio)
    {
        float start = audio != null ? audio.GetVolume(channel) : 1f;
        ui.AddSlider(panel, label, start, v => AudioDirector.Instance?.SetVolume(channel, v));
    }

    // Shows the current screen's panel, hides the rest, and puts keyboard focus on its first button.
    private void ApplyScreen(HudScreen screen)
    {
        ui.HudBar.SetActive(router.HudBarVisible);
        foreach (var pair in panels)
            pair.Value.Root.SetActive(pair.Key == screen);
        HudCanvas.Focus(panels.TryGetValue(screen, out var shown) ? shown.FirstControl : null);
    }

    // ---------- Input ----------

    // Esc: pause in game, resume from pause, back out of sub-screens. Any key dismisses the level intro.
    private void HandleKeyboard()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (router.Current == HudScreen.LevelIntro && Time.frameCount > introShownFrame && keyboard.anyKey.wasPressedThisFrame)
        {
            DismissIntro();
            return;
        }

        if (!keyboard.escapeKey.wasPressedThisFrame) return;
        switch (router.Current)
        {
            case HudScreen.Hud:         RequestPause(); break;
            case HudScreen.Pause:       RequestResume(); break;
            case HudScreen.Settings:
            case HudScreen.LevelSelect: router.Back(); break;
        }
    }

    // ---------- Button actions ----------

    private void DismissIntro()
    {
        if (router.Current != HudScreen.LevelIntro) return;
        var callback = pendingIntroDismissed;
        pendingIntroDismissed = null;           // run it once, even if Enter both clicks and counts as "any key"
        router.Show(HudScreen.Hud);
        PlayCue(AudioCueId.LevelStart);
        callback?.Invoke();
    }

    private void RequestPause() =>
        Raise(OnPauseRequested, () => { coordinator.Pause(true); ShowPause(coordinator.State == SessionState.Paused); }, "Pause");

    // With no F1 at all (e.g. a Preview), Resume still closes the pause screen so you're never stuck in it.
    private void RequestResume()
    {
        bool handled = Raise(OnResumePressed, () => { coordinator.Pause(false); if (coordinator.State != SessionState.Paused) ShowPause(false); }, "Resume");
        if (!handled) ShowPause(false);
    }

    private void SelectLevel(int index)
    {
        if (OnLevelSelected != null) { OnLevelSelected(index); return; }
        if (coordinator != null || (coordinator = GameCoordinator.Instance) != null) { coordinator.StartRun(index); return; }
        Debug.Log($"[F6] Level {index + 1} selected, but nothing is listening yet.");
    }

    private static void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Raises the event if F1 subscribed; otherwise uses the temporary GameCoordinator bridge (see header).
    // Returns false if neither exists (scene played without F1).
    private bool Raise(Action evt, Action bridge, string what)
    {
        if (evt != null) { evt(); return true; }
        if (coordinator == null) coordinator = GameCoordinator.Instance;
        if (coordinator != null) { bridge(); return true; }
        Debug.Log($"[F6] {what} pressed, but nothing is listening yet.");
        return false;
    }

    // ---------- Preview (Inspector right-click menu, Play Mode) ----------

    [ContextMenu("Preview/Main menu")]   void PreviewMenu()    => ShowMainMenu(3);
    [ContextMenu("Preview/HUD")]         void PreviewHud()
    {
        ShowHud(true);
        SetTimer(75f);
        SetObjective("Cheese", 2, 5);
        SetDistortionIndicator(new[] { DistortionID.Mirror, DistortionID.Rotation });
    }
    [ContextMenu("Preview/HUD low time")] void PreviewUrgent() { ShowHud(true); SetTimer(9.5f); }
    [ContextMenu("Preview/Level intro")] void PreviewIntro()   => ShowLevelIntro(1, "The Second Maze", () => Debug.Log("[F6] Intro dismissed"));
    [ContextMenu("Preview/Pause")]       void PreviewPause()   => ShowPause(true);
    [ContextMenu("Preview/Won")]         void PreviewWon()     => ShowResult(LevelResult.Won, new RunStatistics { ElapsedTime = 83f, Deaths = 1, PuzzlesSolved = 3, LevelsCleared = 2 });
    [ContextMenu("Preview/Lost")]        void PreviewLost()    => ShowResult(LevelResult.Lost, new RunStatistics { ElapsedTime = 150f, Deaths = 4 });
    [ContextMenu("Preview/Run complete")] void PreviewRunDone() => ShowRunComplete(new RunStatistics { ElapsedTime = 412f, Deaths = 5, PuzzlesSolved = 9, LevelsCleared = 3 });
}
