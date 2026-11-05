using UnityEngine;

// Основа — SafeArea из лекции 8–9.
[RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour
{
    RectTransform panel;
    Rect last;

    void Awake()
    {
        panel = GetComponent<RectTransform>();
        Apply();
    }

    void Update()
    {
        if (Screen.safeArea != last) Apply();
    }

    void Apply()
    {
        if (Screen.width == 0 || Screen.height == 0) return;

        last = Screen.safeArea;
        Vector2 min = last.position;
        Vector2 max = last.position + last.size;
        min.x /= Screen.width;
        min.y /= Screen.height;
        max.x /= Screen.width;
        max.y /= Screen.height;
        panel.anchorMin = min;
        panel.anchorMax = max;

        panel.offsetMin = Vector2.zero;
        panel.offsetMax = Vector2.zero;
    }
}
