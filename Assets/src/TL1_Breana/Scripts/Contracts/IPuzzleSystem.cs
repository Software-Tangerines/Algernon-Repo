// What F1 needs from F3. BuildPuzzles(PuzzleConfig, ...) will be added once TL2+ defines PuzzleConfig.
using UnityEngine;

public interface IPuzzleSystem
{
    void NotifyPlayerCell(Vector2Int cell);   // pushed by F1 each frame
    bool AllObjectivesComplete { get; }
    void ResetPuzzles();
}
