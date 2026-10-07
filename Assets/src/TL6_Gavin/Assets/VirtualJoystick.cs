using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Tooltip("The joystick base. This script goes on this object.")]
    public RectTransform background;

    [Tooltip("The knob. Should be a child of the background.")]
    public RectTransform handle;

    [Range(0f, 1f)] public float deadZone = 0.1f;

    public Vector2 Direction { get; private set; }

    void Reset()
    {
        background = GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                background, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
            return;

        float radius = background.rect.width * 0.5f;
        Vector2 input = Vector2.ClampMagnitude(localPoint / radius, 1f);

        if (input.magnitude < deadZone)
            input = Vector2.zero;

        Direction = input;
        handle.anchoredPosition = input * radius;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Direction = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
    }
}