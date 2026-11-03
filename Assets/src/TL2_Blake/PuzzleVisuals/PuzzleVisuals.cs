using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PuzzleVisual : MonoBehaviour
{
    [Header("Appearance")]
    [SerializeField] private Sprite resolvedSprite;

    private PuzzleElement element;
    private SpriteRenderer spriteRenderer;
    private Collider2D blockingCollider;

    private Sprite originalSprite;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        blockingCollider = GetComponent<Collider2D>();
        originalSprite = spriteRenderer.sprite;
    }

    public void Initialize(
        PuzzleElement puzzleElement,
        string objectName,
        Color color)
    {
        element = puzzleElement;
        gameObject.name = objectName;

        spriteRenderer.color = color;
        Refresh();
    }

    public void Refresh()
    {
        if (element == null)
            return;

        // A collected key disappears.
        if (element is KeyElement)
        {
            spriteRenderer.enabled = !element.IsResolved;
            return;
        }

        // Use a different sprite when resolved, if provided.
        if (resolvedSprite != null)
        {
            spriteRenderer.sprite = element.IsResolved
                ? resolvedSprite
                : originalSprite;
        }

        // A door physically blocks movement while closed.
        if (element is DoorElement door)
        {
            if (blockingCollider != null)
                blockingCollider.enabled = !door.IsOpen;
        }

        // A disabled hazard is still visible,
        // but its color indicates that it is safe.
        if (element is HazardElement hazard)
        {
            spriteRenderer.sprite = element.IsResolved
                ? resolvedSprite
                : originalSprite;
        }
    }
}