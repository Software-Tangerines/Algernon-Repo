using UnityEngine;
using System.Collections.Generic;

public abstract class PuzzleElement
{
    public bool IsResolved { get; protected set; }

    // Handles direct player input, if the element needs it.
    public virtual void ProcessInput(PuzzleContext context) { }

    // Handles conditions that depend on other elements.
    public virtual void EvaluateDependencies() { }

    public virtual void Reset()
    {
        IsResolved = false;
    }
}

public class PuzzleContext
{
    public Vector2Int PlayerCell { get; set; }
    public bool InteractPressed { get; set; }

    public PuzzleContext(Vector2Int playerCell, bool interactPressed)
    {
        PlayerCell = playerCell;
        InteractPressed = interactPressed;
    }
}