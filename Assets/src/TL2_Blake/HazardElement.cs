using System.Collections.Generic;

public class HazardElement : PuzzleElement
{
    private readonly List<PuzzleElement> triggers;

    public bool IsDisabled => IsResolved;

    public HazardElement(params PuzzleElement[] triggers)
    {
        this.triggers = new List<PuzzleElement>(triggers);
    }

    public override void EvaluateDependencies()
    {
        foreach (PuzzleElement trigger in triggers)
        {
            if (trigger.IsResolved)
            {
                IsResolved = true;
                return;
            }
        }
    }
}