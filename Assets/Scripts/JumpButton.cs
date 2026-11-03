using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class JumpButton : MonoBehaviour, IPointerDownHandler
{
    // Нажатие «съедается» героем в FixedUpdate.
    public static bool Pressed;

    public void OnPointerDown(PointerEventData e) { Pressed = true; }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb != null && (kb.spaceKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame))
            Pressed = true;
    }
}
