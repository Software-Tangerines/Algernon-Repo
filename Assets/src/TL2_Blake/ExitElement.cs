using System.Collections.Generic;

public class ExitElement : PuzzleElement
{
    private readonly List<PuzzleElement> objectives;

    public bool IsOpen => IsResolved;

    public ExitElement(List<PuzzleElement> objectives)
    {
        this.objectives = objectives;
    }

    public override void EvaluateDependencies()
    {
        foreach (PuzzleElement objective in objectives)
        {
            if (!objective.IsResolved)
            {
                return;
            }
        }

        IsResolved = true;
    }
}