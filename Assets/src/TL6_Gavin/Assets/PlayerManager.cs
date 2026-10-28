using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour, IPlayerCharacter
{
    public PlayerData data = new PlayerData();

    public bool IsAlive => data.currentState != PlayerState.Dead;
    public Vector2Int CurrentCell => Vector2Int.RoundToInt(transform.position);
    public void SpawnAt(Vector3 worldPosition) => transform.position = worldPosition;
    public void Kill(DeathCause cause) => data.currentState = PlayerState.Dead;
    public void EnableInput(bool enabled) => SetFrozen(!enabled);

    private PlayerMemento savedState;

    

private void Update()
{
    if (Keyboard.current == null)
        return;

    if (Keyboard.current.pKey.wasPressedThisFrame)
        SaveState();

    if (Keyboard.current.oKey.wasPressedThisFrame)
        LoadState();

    if (Keyboard.current.hKey.wasPressedThisFrame)
        TakeDamage(25);

    if (Keyboard.current.fKey.wasPressedThisFrame)
        SetFrozen(data.currentState != PlayerState.Frozen);

    if (Keyboard.current.jKey.wasPressedThisFrame)
        Heal(25);
}



    private void Awake()
    {
        data.position = transform.position;
    }

    public void SaveState()
    {
        data.position = transform.position;

        savedState = new PlayerMemento(data);

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

        Debug.Log("Player state restored!");
    }

 
public void TakeDamage(int amount)
{
    if (data.currentState != PlayerState.Alive)
        return;

    if (amount <= 0)
        return;

    data.health = Mathf.Max(0, data.health - amount);

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
