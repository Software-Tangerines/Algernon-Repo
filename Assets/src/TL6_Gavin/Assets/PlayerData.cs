
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public Vector3 position;
    public int health;
    public int maxHealth;
    public float moveSpeed;
    public PlayerState currentState;
    public Vector2 facingDirection;
    public int score;

    public PlayerData()
    {
        position = Vector3.zero;
        health = 100;
        maxHealth = 100;
        moveSpeed = 5f;
        currentState = PlayerState.Alive;
        facingDirection = Vector2.down;
        score = 0;
    }
}
