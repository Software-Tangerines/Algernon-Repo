
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PuzzleTestLevel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;

    [Header("Grid")]
    [SerializeField] private float cellSize = 1f;

    private PuzzleSystem puzzleSystem;

    private PressurePlateElement plate;
    private KeyElement key;
    private LeverElement lever;
    private DoorElement door;
    private HazardElement hazard;
    private ExitElement exit;

    [SerializeField] private PuzzleVisual visualPrefab;
    private readonly Dictionary<PuzzleElement, PuzzleVisual> visuals = new Dictionary<PuzzleElement, PuzzleVisual>();

    private Sprite squareSprite;

    private void Start()
    {
        if (player == null)
        {
            Debug.LogError(
                "PuzzleTestLevel: Assign the player in the Inspector.");
            enabled = false;
            return;
        }

        // 1. Create puzzle logic at hard-coded grid coordinates.
        plate = new PressurePlateElement(new Vector2Int(2, 2));
        key = new KeyElement(new Vector2Int(4, 2));
        lever = new LeverElement(new Vector2Int(6, 2));

        // Any one of these three elements opens the door.
        door = new DoorElement(plate, key, lever);

        // The key OR lever disables the hazard.
        hazard = new HazardElement(key, lever);

        // All five objectives must resolve before the exit opens.
        var objectives = new List<PuzzleElement>
        {
            plate,
            key,
            lever,
            door,
            hazard
        };

        exit = new ExitElement(objectives);

        var elements = new List<PuzzleElement>
        {
            plate,
            key,
            lever,
            door,
            hazard
        };

        puzzleSystem = new PuzzleSystem(elements, exit);

        puzzleSystem.OnExitUnlocked += HandleExitUnlocked;

        // 2. Create visible sprites at those same coordinates.
        CreateElementVisual(plate, "Pressure Plate",
            new Color(0.2f, 0.6f, 1f));

        CreateElementVisual(key, "Key",
            new Color(1f, 0.85f, 0.1f));

        CreateElementVisual(lever, "Lever",
            new Color(0.7f, 0.3f, 1f));

        CreateElementVisual(door, "Door",
            new Color(0.5f, 0.3f, 0.15f));

        CreateElementVisual(hazard, "Hazard",
            new Color(1f, 0.2f, 0.2f));

        CreateElementVisual(exit, "Exit",
            new Color(0.9f, 0.9f, 0.9f));

        Debug.Log(
            "Puzzle test ready. Stand on the lever and press E.");
    }

    private void Update()
    {
        if (puzzleSystem == null)
            return;

        // Convert the player's world position to a grid cell.
        Vector2Int playerCell = WorldToCell(player.position);

        // Press E to interact with a lever.
        bool interactPressed =
            Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame;

        var context = new PuzzleContext(
            playerCell,
            interactPressed
        );

        // Update puzzle logic.
        puzzleSystem.Tick(context);

        // Update the sprite colors and visibility.
        UpdateVisuals();
    }

    private Vector2Int WorldToCell(Vector3 worldPosition)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPosition.x / cellSize),
            Mathf.RoundToInt(worldPosition.y / cellSize)
        );
    }

    private Vector3 CellToWorld(Vector2Int cell)
    {
        return new Vector3(
            cell.x * cellSize,
            cell.y * cellSize,
            0f
        );
    }

    
    private void CreateElementVisual(
        PuzzleElement element,
        string objectName,
        Color color)
    {
        PuzzleVisual visual = Instantiate(
            visualPrefab,
            GetElementPosition(element),
            Quaternion.identity,
            transform
        );

        visual.Initialize(element, objectName, color);

        visuals.Add(element, visual);
    }


    private Vector3 GetElementPosition(PuzzleElement element)
    {
        if (element == plate)
            return CellToWorld(plate.Cell);

        if (element == key)
            return CellToWorld(key.Cell);

        if (element == lever)
            return CellToWorld(lever.Cell);

        if (element == door)
            return CellToWorld(new Vector2Int(3, 4));

        if (element == hazard)
            return CellToWorld(new Vector2Int(5, 4));

        // Exit has its own fixed test coordinate.
        return CellToWorld(new Vector2Int(7, 4));
    }

    private void UpdateVisuals()
    {
        foreach (PuzzleVisual visual in visuals.Values)
        {
            if (visual != null)
                visual.Refresh();
        }
    }

    private void HandleExitUnlocked()
    {
        Debug.Log("EXIT UNLOCKED! All objectives are resolved.");
    }

    private void OnDestroy()
    {
        if (puzzleSystem != null)
            puzzleSystem.OnExitUnlocked -= HandleExitUnlocked;

        if (squareSprite != null)
            Destroy(squareSprite.texture);

        if (squareSprite != null)
            Destroy(squareSprite);
    }
}
