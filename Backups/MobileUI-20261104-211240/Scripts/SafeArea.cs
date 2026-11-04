using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour
{
    RectTransform panel;
    Rect last;

    void Awake() { panel = GetComponent<RectTransform>(); Apply(); }
    void Update() { if (Screen.safeArea != last) Apply(); }

    void Apply()
    {
        last = Screen.safeArea;
        Vector2 min = last.position;
        Vector2 max = last.position + last.size;
        min.x /= Screen.width;  min.y /= Screen.height;
        max.x /= Screen.width;  max.y /= Screen.height;
        panel.anchorMin = min;   // the panel's corners follow
        panel.anchorMax = max;   // the visible part of the screen
    }
}