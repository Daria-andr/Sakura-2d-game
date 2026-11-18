
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour
{
    private RectTransform panel;
    private Rect lastSafeArea;
    private Vector2Int lastScreenSize;

    void Awake()
    {
        panel = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    void Update()
    {
        if (VisibleSafeArea() != lastSafeArea ||
            Screen.width != lastScreenSize.x ||
            Screen.height != lastScreenSize.y)
        {
            ApplySafeArea();
        }
    }

    static Rect VisibleSafeArea()
    {
        Rect area = Screen.safeArea;
        if (!Application.isMobilePlatform || area.width <= 0 || area.height <= 0 ||
            area.xMin < 0 || area.yMin < 0 || area.xMax > Screen.width || area.yMax > Screen.height)
            return new Rect(0, 0, Screen.width, Screen.height);
        return area;
    }

    void ApplySafeArea()
    {
        if (Screen.width <= 0 || Screen.height <= 0)
            return;

        Rect safeArea = VisibleSafeArea();

        lastSafeArea = safeArea;
        lastScreenSize = new Vector2Int(
            Screen.width,
            Screen.height
        );

        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;

        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        panel.anchorMin = anchorMin;
        panel.anchorMax = anchorMax;

        panel.offsetMin = Vector2.zero;
        panel.offsetMax = Vector2.zero;
    }
}

