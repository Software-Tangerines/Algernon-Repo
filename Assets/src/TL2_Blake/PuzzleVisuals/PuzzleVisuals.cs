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
        if (element == null) return;

        if (element is KeyElement || element is CheeseElement)
        {
            spriteRenderer.enabled = !element.IsResolved;
            return;
        }

        if (resolvedSprite != null)
        {
            spriteRenderer.sprite =
                element.IsResolved ? resolvedSprite : originalSprite;
        }

        if (element is DoorElement door)
        {
            spriteRenderer.enabled = !door.IsOpen;

            if (blockingCollider != null)
                blockingCollider.enabled = !door.IsOpen;
        }

        if (element is HazardElement hazard)
        {
            if (blockingCollider != null)
                blockingCollider.enabled = !hazard.IsDisabled;

            spriteRenderer.enabled = !hazard.IsDisabled;
        }
    }
}