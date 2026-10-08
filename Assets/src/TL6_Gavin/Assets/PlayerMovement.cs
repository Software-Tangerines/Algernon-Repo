using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Animator animator;

    [Header("Mobile")]
    public VirtualJoystick joystick; // optional, assign for touch controls

    private Rigidbody2D rb;
    private Vector2 movement;

    private InputTransformer currentTransformer = new NormalInputTransformer();

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        movement = Vector2.zero;

        // Keyboard (null on phones, so check first)
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.wKey.isPressed) movement.y += 1;
            if (keyboard.sKey.isPressed) movement.y -= 1;
            if (keyboard.aKey.isPressed) movement.x -= 1;
            if (keyboard.dKey.isPressed) movement.x += 1;

            // Space swaps between normal and inverted controls
            if (keyboard.spaceKey.wasPressedThisFrame)
                ToggleControls();
        }

        // Touch joystick
        if (joystick != null)
            movement += joystick.Direction;

        // Dynamic binding happens here
        movement = currentTransformer.TransformMovement(movement);

        // Cap at length 1 (keeps diagonals even, and lets the joystick be analog)
        movement = Vector2.ClampMagnitude(movement, 1f);

        animator.SetFloat("Speed", movement.magnitude);

        if (movement != Vector2.zero)
        {
            float angle = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    // Public so a UI Button can call it from its OnClick event
    public void ToggleControls()
    {
        if (currentTransformer is InvertedInputTransformer)
        {
            currentTransformer = new NormalInputTransformer();
            Debug.Log("Normal Movement");
        }
        else
        {
            currentTransformer = new InvertedInputTransformer();
            Debug.Log("Movement Inverted");
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
}


public class InputTransformer
{
    public virtual Vector2 TransformMovement(Vector2 movement)
    {
        return movement;
    }
}


public class NormalInputTransformer : InputTransformer
{
    public override Vector2 TransformMovement(Vector2 movement)
    {
        return movement;
    }
}


public class InvertedInputTransformer : InputTransformer
{
    public override Vector2 TransformMovement(Vector2 movement)
    {
        return -movement;
    }
}