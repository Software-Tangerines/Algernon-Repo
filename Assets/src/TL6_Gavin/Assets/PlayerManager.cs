using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour, IPlayerCharacter
{
    /*
    blake: since the tile map for the game is offset, the transform position at the center 
    of the game is different than the tile map cell position and was causing an issue with 
    the player interacting with puzzle elements. essentially the player world position is 
    differant than its cell position.
    please let me know if this change is something that you wouldn't want and I will try to 
    find a work around. the fix could also just be making all the tilemap tranformation set to (0,0,0)
    */
    [Header("Grid Reference")]
    [SerializeField] private Tilemap tilemap;

    public Vector2Int CurrentCell
    {
        get
        {
            Vector3Int cell = tilemap.WorldToCell(transform.position);
            return new Vector2Int(cell.x, cell.y);
        }
    }

    public PlayerData data = new PlayerData();

    public bool IsAlive => data.currentState != PlayerState.Dead;
    /*
    previous implementation of the players current cell.
    public Vector2Int CurrentCell => Vector2Int.RoundToInt(transform.position);
    */
    public void SpawnAt(Vector3 worldPosition) => transform.position = worldPosition;
    public void Kill(DeathCause cause) => data.currentState = PlayerState.Dead;
    public void EnableInput(bool enabled) => SetFrozen(!enabled);

    private PlayerMemento savedState;

    [Header("Health UI")]
    public HealthUI healthUI;

    

private void Update()
{
    if (Keyboard.current == null)
        return;

    if (Keyboard.current.pKey.wasPressedThisFrame)
        SaveState();

    if (Keyboard.current.oKey.wasPressedThisFrame)
        LoadState();

    if (Keyboard.current.hKey.wasPressedThisFrame)
        TakeDamage(10);

    if (Keyboard.current.fKey.wasPressedThisFrame)
        SetFrozen(data.currentState != PlayerState.Frozen);

    if (Keyboard.current.jKey.wasPressedThisFrame)
        Heal(10);
}



    private void Awake()
{
    data.position = transform.position;

    if (healthUI != null)
        healthUI.UpdateHealth(data.health);
}

    public void SaveState()
    {
        data.position = transform.position;

        savedState = new PlayerMemento(data);

        if (healthUI != null)
        healthUI.UpdateHealth(data.health);

        Debug.Log("Player state saved!");
    }

    public void LoadState()
{
    if (savedState == null)
    {
        Debug.LogWarning("No saved state exists!");
        return;
    }

    data = savedState.GetSavedData();

    transform.position = data.position;

    // Restore health display
    if (healthUI != null)
        healthUI.UpdateHealth(data.health);

    // Restore player state
    Debug.Log("Player state restored!");
    Debug.Log("Health: " + data.health);
    Debug.Log("State: " + data.currentState);
}

 
public void TakeDamage(int amount)
{
    if (data.currentState != PlayerState.Alive)
        return;

    if (amount <= 0)
        return;

    data.health = Mathf.Max(0, data.health - amount);

    if (healthUI != null)
        healthUI.UpdateHealth(data.health);

    Debug.Log("Player health: " + data.health);

    if (data.health == 0)
    {
        data.currentState = PlayerState.Dead;
        Debug.Log("Player died!");
        // RestartScene();
    }
}

public void Heal(int amount)
{
    if (data.currentState != PlayerState.Alive)
        return;

    if (amount <= 0)
        return;

    data.health = Mathf.Min(data.maxHealth, data.health + amount);

    if (healthUI != null)
        healthUI.UpdateHealth(data.health);

    Debug.Log("Player healed. Health: " + data.health);
}


    public void SetFrozen(bool frozen)
    {
        if (data.currentState == PlayerState.Dead)
            return;

        data.currentState = frozen
            ? PlayerState.Frozen
            : PlayerState.Alive;
    }

    private void RestartScene()
{
    SceneManager.LoadScene(
        SceneManager.GetActiveScene().buildIndex
    );
}
}
