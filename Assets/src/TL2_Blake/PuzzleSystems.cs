using System;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleSystem
{
    private readonly List<PuzzleElement> elements;
    private readonly ExitElement exit;

    public bool AllObjectivesComplete => exit.IsResolved;

    public event Action OnExitUnlocked;

    public PuzzleSystem(
        List<PuzzleElement> elements,
        ExitElement exit)
    {
        this.elements = elements;
        this.exit = exit;
    }

    public void Tick(PuzzleContext context)
    {
        // 1. Process player input.
        foreach (PuzzleElement element in elements)
        {
            element.ProcessInput(context);
        }

        // 2. Evaluate doors, hazards, and other dependencies.
        foreach (PuzzleElement element in elements)
        {
            element.EvaluateDependencies();
        }

        // 3. Check the exit after all other elements.
        bool wasOpen = exit.IsOpen;
        exit.EvaluateDependencies();

        // Notify listeners once, when the exit first opens.
        if (!wasOpen && exit.IsOpen)
        {
            OnExitUnlocked?.Invoke();
        }
    }

    public void ResetPuzzles()
    {
        foreach (PuzzleElement element in elements)
        {
            element.Reset();
        }

        exit.Reset();
    }
}