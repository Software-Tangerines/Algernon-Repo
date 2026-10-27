/*
F6 HudWidget: abstract base class for every piece of in-game HUD text (timer, objectives, deaths, distortion warning).

Each widget turns the HudModel into one string. HudFacade keeps a List<HudWidget> and, whenever the
model changes, calls Render on each one and puts the result in that widget's on-screen text.
Widgets are plain C# (not MonoBehaviours), so they are pure functions you can unit test without Unity.

DYNAMIC BINDING (oral exam demo):
HudFacade only holds HudWidget references, so which Render() runs is decided at runtime by the
widget's real type. Remove `virtual` below (and change `override` to `new` in HudWidgets.cs so it
still compiles) and every call binds to THIS method, which returns the static Label. The HUD collapses
to four placeholder words: the timer stops counting down on screen even though the level is still
timed, the objective counter freezes, and the red low-time flash never fires.

To add a widget: subclass this, pass its static label to the base constructor, override Render,
and add it in HudFacade.BuildWidgets.
*/
public abstract class HudWidget
{
    // The widget's raw name with no state, e.g. "Time". Set once; never virtual (keeps the demo honest).
    public string Label { get; }

    protected HudWidget(string label)
    {
        Label = label;
    }

    // Base version: just the static label. Subclasses override to show live values.
    public virtual string Render(HudModel model)
    {
        return Label;
    }
}
