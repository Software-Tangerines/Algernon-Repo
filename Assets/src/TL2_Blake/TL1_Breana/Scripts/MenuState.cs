using UnityEngine;
using UnityEngine.InputSystem;

public class MenuState : GameState
{
    public override void Enter()
    {
        Debug.Log("Entered menustate");
    }

    public override void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Debug.Log("Start pressed");
            GameManager.Instance.StartGame();
        }
    }

    public override void Exit()
    {
        Debug.Log("Exiting menustate");
    }
}
