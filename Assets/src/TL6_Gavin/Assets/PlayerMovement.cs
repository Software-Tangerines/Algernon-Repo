using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Animator animator;
    public AudioManager audioManager;

    [Header("Mobile")]
    public VirtualJoystick joystick; // optional, assign for touch controls

    private Rigidbody2D rb;
    private Vector2 movement;

    private InputTransformer currentTransformer = new NormalInputTransformer();

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Find AudioManager on this player if it wasn't assigned in Inspector
        if (audioManager == null)
            audioManager = GetComponent<AudioManager>();

        // The joystick lives in the gameplay scene, not on the player prefab.
        if (joystick == null)
            joystick = FindAnyObjectByType<VirtualJoystick>();
    }

    void Update()
    {
        movement = Vector2.zero;

        // Keyboard
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.wKey.isPressed) movement.y += 1;
            if (keyboard.sKey.isPressed) movement.y -= 1;
            if (keyboard.aKey.isPressed) movement.x -= 1;
            if (keyboard.dKey.isPressed) movement.x += 1;

            if (keyboard.spaceKey.wasPressedThisFrame)
                ToggleControls();
        }

        // Touch joystick
        if (joystick != null)
            movement += joystick.Direction;

        // Apply current input transformer
        movement = currentTransformer.TransformMovement(movement);

        // Keep diagonal movement capped
        movement = Vector2.ClampMagnitude(movement, 1f);

        // Update walking sound using the final movement value
        if (audioManager != null)
            audioManager.PlayWalkingSound(movement);

        animator.SetFloat("Speed", movement.magnitude);

        if (movement != Vector2.zero)
        {
            float angle = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

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