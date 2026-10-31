
using UnityEngine;

public class PuzzleVisual : MonoBehaviour
{
    private PuzzleElement element;
    private SpriteRenderer spriteRenderer;

    private Color originalColor;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(
        PuzzleElement puzzleElement,
        string objectName,
        Color color)
    {
        element = puzzleElement;
        gameObject.name = objectName;

        originalColor = color;
        spriteRenderer.color = color;

        transform.localScale = new Vector3(0.7f, 0.7f, 1f);
    }

    public void Refresh()
    {
        if (element == null)
            return;

        // A collected key disappears.
        if (element is KeyElement)
        {
            gameObject.SetActive(!element.IsResolved);
            return;
        }

        // A resolved pressure plate or lever turns green.
        if (element is PressurePlateElement ||
            element is LeverElement)
        {
            spriteRenderer.color = element.IsResolved
                ? Color.green
                : originalColor;
        }

        // A door turns green when open.
        if (element is DoorElement door)
        {
            spriteRenderer.color = door.IsOpen
                ? Color.green
                : originalColor;
        }

        // A disabled hazard turns gray.
        if (element is HazardElement hazard)
        {
            spriteRenderer.color = hazard.IsDisabled
                ? Color.gray
                : originalColor;
        }

        // The exit turns green when unlocked.
        if (element is ExitElement exit)
        {
            spriteRenderer.color = exit.IsOpen
                ? Color.green
                : originalColor;
        }
    }
}