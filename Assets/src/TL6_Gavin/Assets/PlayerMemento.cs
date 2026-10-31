public class PlayerMemento
{
    private readonly PlayerData savedData;

    public PlayerMemento(PlayerData data)
    {
        savedData = new PlayerData();

        savedData.position = data.position;
        savedData.health = data.health;
        savedData.maxHealth = data.maxHealth;
        savedData.moveSpeed = data.moveSpeed;
        savedData.currentState = data.currentState;
        savedData.facingDirection = data.facingDirection;
        savedData.score = data.score;
    }

    public PlayerData GetSavedData()
    {
        PlayerData copy = new PlayerData();

        copy.position = savedData.position;
        copy.health = savedData.health;
        copy.maxHealth = savedData.maxHealth;
        copy.moveSpeed = savedData.moveSpeed;
        copy.currentState = savedData.currentState;
        copy.facingDirection = savedData.facingDirection;
        copy.score = savedData.score;

        return copy;
    }
}
