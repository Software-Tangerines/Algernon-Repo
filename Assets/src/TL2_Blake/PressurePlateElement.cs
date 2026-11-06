using UnityEngine;

public class PressurePlateElement : PuzzleElement
{
    public Vector2Int Cell { get; }

    public PressurePlateElement(Vector2Int cell)
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