using System;

/*
F6 ScreenRouter: remembers which F6 screen is showing. Plain C#, so the navigation rules are unit-testable.

Only one screen is "current" at a time. HudFacade listens to Changed and shows that screen's panel,
hiding the rest. The in-game HUD bar (timer etc.) stays visible behind Pause and Result screens.

Settings and LevelSelect are sub-screens: Back() returns to whatever screen opened them
(Settings from Pause goes back to Pause; Settings from the main menu goes back to the main menu).
*/
public enum HudScreen
{
    None,         // F6 shows nothing
    MainMenu,
    LevelSelect,
    Settings,
    LevelIntro,   // "Level 2 / press any key" card before play starts
    Hud,          // normal gameplay: only the HUD bar
    Pause,
    Result,       // level won / lost
    RunComplete
}

public class ScreenRouter
{
    public HudScreen Current { get; private set; } = HudScreen.None;

    // Where Back() goes from a sub-screen.
    public HudScreen ReturnTo { get; private set; } = HudScreen.None;

    // Raised with the new screen whenever Current changes (not when showing the same screen again).
    public event Action<HudScreen> Changed;

    // True while the timer/objective bar should be on screen.
    public bool HudBarVisible => IsGameplay(Current) || (Current == HudScreen.Settings && IsGameplay(ReturnTo));

    public void Show(HudScreen screen)
    {
        if (screen == Current) return;
        if (IsSubScreen(screen)) ReturnTo = Current;
        Current = screen;
        Changed?.Invoke(screen);
    }

    // Leaves a sub-screen. Does nothing on other screens.
    public void Back()
    {
        if (IsSubScreen(Current)) Show(ReturnTo);
    }

    private static bool IsSubScreen(HudScreen s) => s == HudScreen.Settings || s == HudScreen.LevelSelect;

    private static bool IsGameplay(HudScreen s) => s == HudScreen.Hud || s == HudScreen.Pause || s == HudScreen.Result;
}
