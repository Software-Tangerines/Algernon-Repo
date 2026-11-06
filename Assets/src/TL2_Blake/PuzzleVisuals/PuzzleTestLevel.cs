using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class PuzzleTestLevel : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private Transform player;
    [SerializeField] private PlayerManager playerManager;

    [Header("Puzzle Prefabs")]
    [SerializeField] private PuzzleVisual platePrefab;
    [SerializeField] private PuzzleVisual keyPrefab;
    [SerializeField] private PuzzleVisual leverPrefab;
    [SerializeField] private PuzzleVisual doorPrefab;
    [SerializeField] private PuzzleVisual hazardPrefab;
    [SerializeField] private PuzzleVisual exitPrefab;
    [SerializeField] private PuzzleVisual cheesePrefab;

    [Header("Hazard Settings")]
    [SerializeField] private int hazardDamage = 10;
    [SerializeField] private float damageInterval = 0.5f;

    [Header("Cheese Settings")]
    [SerializeField] private int cheeseHealAmount = 20;

    private PuzzleSystem puzzleSystem;

    private PressurePlateElement plate;
    private KeyElement key;
    private LeverElement lever;
    private DoorElement door;
    private HazardElement hazard;
    private ExitElement exit;
    private CheeseElement cheese;

    private readonly Dictionary<PuzzleElement, PuzzleVisual>
        visuals = new Dictionary<PuzzleElement, PuzzleVisual>();

    private Vector2Int hazardCell;
    private float nextDamageTime;
    private bool cheeseWasCollected = false;

    private void Start()
    {
        if (tilemap == null || player == null ||
            playerManager == null)
        {
            Debug.LogError(
                "Assign Tilemap, Player and PlayerManager.");
            enabled = false;
            return;
        }

        if (platePrefab == null || keyPrefab == null ||
            leverPrefab == null || doorPrefab == null ||
            hazardPrefab == null || exitPrefab == null || 
            cheesePrefab == null)
        {
            Debug.LogError("Assign all seven puzzle prefabs.");
            enabled = false;
            return;
        }

        BuildTestPuzzles();
    }

    private void BuildTestPuzzles()
    {
        // Input elements.
        plate = new PressurePlateElement(
            new Vector2Int(2, 2));

        key = new KeyElement(
            new Vector2Int(4, 2));

        lever = new LeverElement(
            new Vector2Int(6, 2));

        cheese = new CheeseElement(
        new Vector2Int(3, 1),
        cheeseHealAmount);

        // Dependent elements.
        door = new DoorElement(plate, key, lever);
        hazard = new HazardElement(key, lever);

        // Hazard location for damage detection.
        hazardCell = new Vector2Int(4, 3);

        // All five objectives must be completed.
        var objectives = new List<PuzzleElement>
        {
            plate,
            key,
            lever,
            door,
            hazard,
            cheese
        };

        exit = new ExitElement(objectives);

        var elements = new List<PuzzleElement>
        {
            plate,
            key,
            lever,
            door,
            hazard,
            cheese
        };

        puzzleSystem = new PuzzleSystem(elements, exit);

        puzzleSystem.OnExitUnlocked += () =>
        {
            Debug.Log("All objectives complete! Exit open.");
        };

        // Spawn puzzle prefabs at Tilemap cells.
        Spawn(platePrefab, plate, plate.Cell, "Plate");
        Spawn(keyPrefab, key, key.Cell, "Key");
        Spawn(leverPrefab, lever, lever.Cell, "Lever");
        Spawn(cheesePrefab, cheese, cheese.Cell, "Cheese");

        Spawn(doorPrefab, door,
            new Vector2Int(3, 3), "Door");

        Spawn(hazardPrefab, hazard,
            hazardCell, "Hazard");

        Spawn(exitPrefab, exit,
            new Vector2Int(7, 3), "Exit");
    }

    private void Spawn(
        PuzzleVisual prefab,
        PuzzleElement element,
        Vector2Int cell,
        string objectName)
    {
        Vector3Int tileCell =
            new Vector3Int(cell.x, cell.y, 0);

        Vector3 worldPosition =
            tilemap.GetCellCenterWorld(tileCell);

        // Slightly in front of the tilemap.
        worldPosition.z = -0.1f;

        PuzzleVisual visual = Instantiate(
            prefab,
            worldPosition,
            Quaternion.identity,
            transform
        );

        visual.Initialize(element, objectName, Color.white);
        visuals.Add(element, visual);
    }

    private void Update()
    {
        if (puzzleSystem == null)
            return;

        // Convert player world position to Tilemap cell.
        Vector3Int tileCell =
            tilemap.WorldToCell(player.position);

        Vector2Int playerCell = new Vector2Int(
            tileCell.x,
            tileCell.y
        );

        bool interactPressed =
            Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame;

        // Update puzzle logic.
        PuzzleContext context = new PuzzleContext(
            playerCell,
            interactPressed
        );

        puzzleSystem.Tick(context);

        if (cheese.IsResolved && !cheeseWasCollected)
        {
            playerManager.Heal(cheeseHealAmount);
            cheeseWasCollected = true;
        }

        // Refresh all sprites and colliders.
        foreach (PuzzleVisual visual in visuals.Values)
        {
            visual.Refresh();
        }

        // Check active hazards.
        CheckHazardDamage(playerCell);
    }

    private void CheckHazardDamage(Vector2Int playerCell)
    {
        if (hazard.IsDisabled)
            return;

        if (playerCell != hazardCell)
            return;

        if (Time.time < nextDamageTime)
            return;

        nextDamageTime = Time.time +
            Mathf.Max(0.1f, damageInterval);

        playerManager.TakeDamage(hazardDamage);
    }

    public void ResetLevelPuzzles()
    {
        cheeseWasCollected = false;
        puzzleSystem.ResetPuzzles();
        nextDamageTime = 0f;

        foreach (PuzzleVisual visual in visuals.Values)
        {
            visual.Refresh();
        }
    }
}