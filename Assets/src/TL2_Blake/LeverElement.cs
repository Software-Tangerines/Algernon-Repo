using UnityEngine;

public class LeverElement : PuzzleElement
{
    public Vector2Int Cell { get; }

    public LeverElement(Vector2Int cell)
    {
        Cell = cell;
    }

    public override void ProcessInput(PuzzleContext context)
    {
        if (context.PlayerCell == Cell && context.InteractPressed)
        {
            IsResolved = true;
        }
    }
}