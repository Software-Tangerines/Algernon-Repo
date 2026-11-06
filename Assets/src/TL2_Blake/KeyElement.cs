using UnityEngine;

public class KeyElement : PuzzleElement
{
    public Vector2Int Cell { get; }

    public KeyElement(Vector2Int cell)
    {
        Cell = cell;
    }

    public override void ProcessInput(PuzzleContext context)
    {
        if (context.PlayerCell == Cell)
        {
            IsResolved = true;
        }
    }
}