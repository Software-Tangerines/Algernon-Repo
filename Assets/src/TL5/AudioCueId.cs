/*
F6 AudioCueId: every sound effect the game can ask for. Pass these to HudFacade.PlayCue (or AudioDirector.PlayCue).

The numbers are saved in the AudioDirector's Inspector list, so add new cues at the END.
HudFacade already plays all of these itself at the right moments (see the "plays" notes);
other features only need PlayCue for sounds F6 can't know about.
*/
public enum AudioCueId
{
    ButtonClick = 0,        // plays: any menu button pressed
    LevelStart = 1,         // plays: level begins (intro dismissed / OnLevelStarted)
    LevelWon = 2,           // plays: win screen shown
    LevelLost = 3,          // plays: lose screen shown
    RunComplete = 4,        // plays: run-complete screen shown
    Pause = 5,              // plays: pause screen opens
    Resume = 6,             // plays: pause screen closes
    TimerWarning = 7,       // plays: timer drops below 10 s
    ObjectiveProgress = 8,  // plays: objective count goes up
    PlayerDeath = 9         // plays: death count goes up
}
