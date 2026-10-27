/*
F6 concrete HUD widgets. Each reads the HudModel and returns the text to show.
An empty string means "show nothing" (HudFacade hides nothing; the text is just blank).
*/

// Countdown in mm:ss, e.g. "01:24". Blank until F1 sends the first SetTimer of the level.
// The red flash under 10 s is done by HudFacade using HudModel.IsUrgent, not here.
public class TimerWidget : HudWidget
{
    public TimerWidget() : base(StringTable.TimerLabel) { }

    public override string Render(HudModel model)
    {
        return model.HasTimer ? StringTable.Clock(model.DisplayedSeconds) : "";
    }
}

// Objective progress, e.g. "Levers: 2/4". A level with 0 objectives shows "Reach the exit", never "0/0".
public class ObjectiveWidget : HudWidget
{
    public ObjectiveWidget() : base(StringTable.ObjectiveLabel) { }

    public override string Render(HudModel model)
    {
        if (!model.HasObjective) return "";
        if (model.ObjectivesTotal == 0) return StringTable.NoObjectives;
        return string.Format(StringTable.ObjectiveFormat,
            model.ObjectiveLabel, model.ObjectivesCompleted, model.ObjectivesTotal);
    }
}

// Death counter for the run, e.g. "Deaths: 3".
public class AttemptsWidget : HudWidget
{
    public AttemptsWidget() : base(StringTable.AttemptsLabel) { }

    public override string Render(HudModel model)
    {
        return string.Format(StringTable.DeathsFormat, model.Deaths);
    }
}

// Lists F5's active distortions, e.g. "Distorted: Mirror, Rotation". Blank when the view is normal.
public class DistortionWarningWidget : HudWidget
{
    public DistortionWarningWidget() : base(StringTable.DistortionLabel) { }

    public override string Render(HudModel model)
    {
        return model.ActiveDistortions.Count == 0 ? "" : StringTable.DistortionList(model.ActiveDistortions);
    }
}
