// Stand-in until F3 exists: no puzzles, so objectives count as complete ("exit always open").
using UnityEngine;

public class NullPuzzles : IPuzzleSystem
{
    public void NotifyPlayerCell(Vector2Int cell) { }
    public bool AllObjectivesComplete => true;
    public void ResetPuzzles() { }
}
