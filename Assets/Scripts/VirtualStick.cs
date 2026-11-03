using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// Экранный стик: нужна только ось X (бег влево/вправо). На ПК работают A/D и стрелки.
public class VirtualStick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public RectTransform knob;
    public static float Horizontal { get; private set; }

    float touch;
    RectTransform rt;

    void Awake() { rt = (RectTransform)transform; }

    void OnDisable() { touch = 0; Horizontal = 0; }

    public void OnPointerDown(PointerEventData e) { OnDrag(e); }

    public void OnDrag(PointerEventData e)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, e.position, e.pressEventCamera, out var p);
        float r = rt.rect.width * 0.5f;
        Vector2 v = Vector2.ClampMagnitude(p / r, 1f);
        touch = Mathf.Abs(v.x) < 0.15f ? 0 : v.x;
        if (knob) knob.anchoredPosition = v * r * 0.6f;
    }

    public void OnPointerUp(PointerEventData e)
    {
        touch = 0;
        if (knob) knob.anchoredPosition = Vector2.zero;
    }

    void Update()
    {
        float key = 0;
        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.leftArrowKey.isPressed || kb.aKey.isPressed) key -= 1;
            if (kb.rightArrowKey.isPressed || kb.dKey.isPressed) key += 1;
        }
        Horizontal = Mathf.Abs(key) > 0 ? key : touch;
    }
}
