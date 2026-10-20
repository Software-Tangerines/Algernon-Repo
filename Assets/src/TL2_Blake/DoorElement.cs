using System.Collections.Generic;

public class DoorElement : PuzzleElement
{
    private readonly List<PuzzleElement> triggers;

    public bool IsOpen => IsResolved;

    public DoorElement(params PuzzleElement[] triggers)
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