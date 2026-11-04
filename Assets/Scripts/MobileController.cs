using UnityEngine;
using UnityEngine.EventSystems;

public class MobileController : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IDragHandler
{
    public enum ControlType
    {
        Joystick,
        Jump,
        Shoot
    }

    [Header("References")]
    public PlayerMovement player;
    public ControlType controlType;

    [Header("Joystick")]
    public RectTransform handle;
    public float movementRange = 60f;

    private RectTransform rectTransform;
    private bool isPressed;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (player == null)
            player = FindAnyObjectByType<PlayerMovement>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (player == null || Time.timeScale == 0f)
            return;

        isPressed = true;

        switch (controlType)
        {
            case ControlType.Joystick:
                UpdateJoystick(eventData);
                break;

            case ControlType.Jump:
                player.MobileJump();
                break;

            case ControlType.Shoot:
                player.MobileShoot();
                break;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isPressed || player == null)
            return;

        if (controlType == ControlType.Joystick)
            UpdateJoystick(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ResetControl();
    }

    void OnDisable()
    {
        ResetControl();
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus) ResetControl();
    }

    public void ResetControl()
    {
        isPressed = false;

        if (controlType != ControlType.Joystick)
            return;

        if (player != null)
            player.SetMobileMove(0f);

        if (handle != null)
            handle.anchoredPosition = Vector2.zero;
    }

    void UpdateJoystick(PointerEventData eventData)
    {
        if (rectTransform == null || player == null)
            return;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint))
            return;

        float halfWidth = rectTransform.rect.width * 0.5f;
        float horizontal = Mathf.Clamp(
            localPoint.x / Mathf.Max(halfWidth, 1f),
            -1f,
            1f
        );

        player.SetMobileMove(horizontal);

        if (handle != null)
        {
            handle.anchoredPosition = new Vector2(
                horizontal * movementRange,
                0f
            );
        }
    }
}
