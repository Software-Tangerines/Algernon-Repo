using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Animator animator;

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

        if (Keyboard.current.wKey.isPressed)
            movement.y += 1;

        if (Keyboard.current.sKey.isPressed)
            movement.y -= 1;

        if (Keyboard.current.aKey.isPressed)
            movement.x -= 1;

        if (Keyboard.current.dKey.isPressed)
            movement.x += 1;

        // Space swaps between normal and inverted controls
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
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

        // Dynamic binding happens here
        movement = currentTransformer.TransformMovement(movement);

        movement = movement.normalized;

        animator.SetFloat("Speed", movement.magnitude);

        if (movement != Vector2.zero)
        {
            float angle = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(
            rb.position + movement * moveSpeed * Time.fixedDeltaTime
        );
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