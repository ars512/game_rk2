using UnityEngine;

// Подгоняет RectTransform под Screen.safeArea (вырез, скругления, жесты).
// Фон остаётся на весь экран, а кнопки и текст лежат внутри этой панели.
[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour
{
    // Нужен для скриншотов без реального симулятора: подменяет safeArea.
    public static Rect? Override;
    public static Vector2Int OverrideScreen;

    Rect applied;

    void Update() { Apply(); }

    public void Apply()
    {
        Rect area = Override ?? Screen.safeArea;
        Vector2 size = Override.HasValue ? (Vector2)OverrideScreen : new Vector2(Screen.width, Screen.height);
        if (area == applied || size.x <= 0 || size.y <= 0) return;
        applied = area;

        var rt = (RectTransform)transform;
        rt.anchorMin = new Vector2(area.xMin / size.x, area.yMin / size.y);
        rt.anchorMax = new Vector2(area.xMax / size.x, area.yMax / size.y);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }
}
