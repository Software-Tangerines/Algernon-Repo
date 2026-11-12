
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class PuzzleTestLevel2 : MonoBehaviour, IPuzzleSystem
{
    [Header("Scene References")]
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private Transform player;

    [Header("Puzzle Prefabs")]
    [SerializeField] private PuzzleVisual platePrefab;
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

    private readonly List<PressurePlateElement> plates =
        new List<PressurePlateElement>();

    private readonly List<LeverElement> levers =
        new List<LeverElement>();

    private readonly List<DoorElement> doors =
        new List<DoorElement>();

    private readonly List<HazardElement> hazards =
        new List<HazardElement>();

    private readonly List<CheeseElement> cheeses =
        new List<CheeseElement>();

    private ExitElement exit;

    private readonly Dictionary<PuzzleElement, PuzzleVisual>
        visuals = new Dictionary<PuzzleElement, PuzzleVisual>();

    private readonly Dictionary<HazardElement, Vector2Int>
        hazardCells = new Dictionary<HazardElement, Vector2Int>();

    private readonly Dictionary<HazardElement, float>
        nextDamageTimes = new Dictionary<HazardElement, float>();

    private readonly HashSet<CheeseElement> collectedCheeses =
        new HashSet<CheeseElement>();

    public bool AllObjectivesComplete =>
        puzzleSystem != null && puzzleSystem.AllObjectivesComplete;

    private void Awake()
    {
        if (tilemap == null || player == null ||
            playerManager == null)
        {
            Debug.LogError(
                "Assign Tilemap, Player and PlayerManager.");
            enabled = false;
            return;
        }

        if (platePrefab == null || leverPrefab == null ||
            doorPrefab == null || hazardPrefab == null ||
            exitPrefab == null || cheesePrefab == null)
        {
            Debug.LogError("Assign all six required puzzle prefabs.");
            enabled = false;
            return;
        }

        BuildTestPuzzles();
    }

    private void Update()
    {
        if (player == null || tilemap == null)
            return;

        Vector3Int cell = tilemap.WorldToCell(player.position);

        Vector2Int playerCell = new Vector2Int(
            cell.x,
            cell.y
        );

        NotifyPlayerCell(playerCell);
    }

    private void BuildTestPuzzles()
    {
        // ---------------------------------
        // PRESSURE PLATES
        // ---------------------------------

        var plate1 = new PressurePlateElement(
            new Vector2Int(-10, -7));

        var plate2 = new PressurePlateElement(
            new Vector2Int(0, -7));

        var plate3 = new PressurePlateElement(
            new Vector2Int(-8, 0));

        plates.Add(plate1);
        plates.Add(plate2);
        plates.Add(plate3);

        // ---------------------------------
        // LEVERS
        // ---------------------------------

        var lever1 = new LeverElement(
            new Vector2Int(-5, -5));

        var lever2 = new LeverElement(
            new Vector2Int(5, -3));

        levers.Add(lever1);
        levers.Add(lever2);

        // ---------------------------------
        // CHEESE COLLECTIBLES
        // ---------------------------------

        var cheese1 = new CheeseElement(
            new Vector2Int(-11, -3), cheeseHealAmount);

        var cheese2 = new CheeseElement(
            new Vector2Int(-4, 0), cheeseHealAmount);

        var cheese3 = new CheeseElement(
            new Vector2Int(0, 1), cheeseHealAmount);

        var cheese4 = new CheeseElement(
            new Vector2Int(5, 1), cheeseHealAmount);

        var cheese5 = new CheeseElement(
            new Vector2Int(-2, -9), cheeseHealAmount);

        cheeses.Add(cheese1);
        cheeses.Add(cheese2);
        cheeses.Add(cheese3);
        cheeses.Add(cheese4);
        cheeses.Add(cheese5);

        // ---------------------------------
        // DOORS AND HAZARDS
        // Each has exactly one trigger.
        // ---------------------------------

        var door1 = new DoorElement(plate1);
        var door2 = new DoorElement(plate2);
        var door3 = new DoorElement(lever2);

        var hazard1 = new HazardElement(lever1);
        var hazard2 = new HazardElement(plate3);

        doors.Add(door1);
        doors.Add(door2);
        doors.Add(door3);

        hazards.Add(hazard1);
        hazards.Add(hazard2);

        // Hazard locations.
        hazardCells.Add(
            hazard1, new Vector2Int(-3, -5));

        hazardCells.Add(
            hazard2, new Vector2Int(-6, 0));

        foreach (HazardElement hazard in hazards)
        {
            nextDamageTimes.Add(hazard, 0f);
        }

        // ---------------------------------
        // EXIT
        // All five cheeses are required.
        // ---------------------------------

        var objectives = new List<PuzzleElement>();

        objectives.AddRange(cheeses);

        exit = new ExitElement(objectives);

        // ---------------------------------
        // PUZZLE SYSTEM
        // ---------------------------------

        var elements = new List<PuzzleElement>();

        elements.AddRange(plates);
        elements.AddRange(levers);
        elements.AddRange(doors);
        elements.AddRange(hazards);
        elements.AddRange(cheeses);

        puzzleSystem = new PuzzleSystem(elements, exit);

        puzzleSystem.OnExitUnlocked += () =>
        {
            Debug.Log("All five cheeses collected! Exit unlocked.");
        };

        // ---------------------------------
        // SPAWN VISUALS
        // ---------------------------------

        for (int i = 0; i < plates.Count; i++)
        {
            Spawn(
                platePrefab,
                plates[i],
                plates[i].Cell,
                "Pressure Plate " + (i + 1)
            );
        }

        for (int i = 0; i < levers.Count; i++)
        {
            Spawn(
                leverPrefab,
                levers[i],
                levers[i].Cell,
                "Lever " + (i + 1)
            );
        }

        for (int i = 0; i < cheeses.Count; i++)
        {
            Spawn(
                cheesePrefab,
                cheeses[i],
                cheeses[i].Cell,
                "Cheese " + (i + 1)
            );
        }

        Spawn(doorPrefab, door1,
            new Vector2Int(-8, -7), "Door 1");

        Spawn(doorPrefab, door2,
            new Vector2Int(2, -7), "Door 2");

        Spawn(doorPrefab, door3,
            new Vector2Int(6, -1), "Door 3");

        Spawn(hazardPrefab, hazard1,
            hazardCells[hazard1], "Hazard 1");

        Spawn(hazardPrefab, hazard2,
            hazardCells[hazard2], "Hazard 2");

        Spawn(exitPrefab, exit,
            new Vector2Int(7, 2), "Exit");
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

    public void NotifyPlayerCell(Vector2Int playerCell)
    {
        if (puzzleSystem == null)
            return;

        bool interactPressed =
            Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame;

        PuzzleContext context = new PuzzleContext(
            playerCell,
            interactPressed
        );

        // Process all puzzle elements.
        puzzleSystem.Tick(context);

        // Check cheese collection and healing.
        foreach (CheeseElement cheese in cheeses)
        {
            if (cheese.IsResolved &&
                collectedCheeses.Add(cheese))
            {
                playerManager.Heal(cheeseHealAmount);

                Debug.Log(
                    "Cheese collected: " +
                    collectedCheeses.Count + "/5"
                );
            }
        }

        // Refresh all puzzle visuals.
        foreach (PuzzleVisual visual in visuals.Values)
        {
            visual.Refresh();
        }

        // Check every active hazard.
        CheckHazardDamage(playerCell);
    }

    private void CheckHazardDamage(Vector2Int playerCell)
    {
        foreach (HazardElement hazard in hazards)
        {
            if (hazard.IsDisabled)
                continue;

            if (playerCell != hazardCells[hazard])
                continue;

            if (Time.time < nextDamageTimes[hazard])
                continue;

            nextDamageTimes[hazard] =
                Time.time + Mathf.Max(0.1f, damageInterval);

            playerManager.TakeDamage(hazardDamage);
        }
    }

    public void ResetPuzzles()
    {
        if (puzzleSystem == null)
            return;

        collectedCheeses.Clear();

        foreach (HazardElement hazard in hazards)
        {
            nextDamageTimes[hazard] = 0f;
        }

        puzzleSystem.ResetPuzzles();

        foreach (PuzzleVisual visual in visuals.Values)
        {
            visual.Refresh();
        }
    }
}