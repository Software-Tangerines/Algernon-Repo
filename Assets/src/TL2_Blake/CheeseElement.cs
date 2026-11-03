using UnityEngine;

public class CheeseElement : PuzzleElement
{
    public Vector2Int Cell { get; }
    private int healAmount;

    public CheeseElement(Vector2Int cell, int healAmount)
    {
        Cell = cell;
        this.healAmount = healAmount;
    }

    public override void ProcessInput(PuzzleContext context)
    {
        if (context.PlayerCell == Cell)
        {
            IsResolved = true;
        }
    }
}