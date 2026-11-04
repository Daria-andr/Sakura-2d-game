
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Player")]
    public Transform target;

    [Header("Camera Settings")]
    public float smoothTime = 0.2f;
    public Vector3 offset = new Vector3(0, 2, -10);

    [Header("Camera Bounds")]
    public bool useBounds = true;

    public float minX = -10f;
    public float maxX = 30f;

    public float minY = -5f;
    public float maxY = 10f;

    private Vector3 velocity = Vector3.zero;
    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = new Vector3(
    target.position.x + offset.x,
    transform.position.y,
    offset.z
);

        Vector3 smoothPosition = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime
        );

        if (useBounds && cam != null && cam.orthographic)
        {
            float cameraHeight = cam.orthographicSize;
            float cameraWidth = cameraHeight * cam.aspect;

            float left = minX + cameraWidth;
            float right = maxX - cameraWidth;

            float bottom = minY + cameraHeight;
            float top = maxY - cameraHeight;

            if (left <= right)
            {
                smoothPosition.x = Mathf.Clamp(
                    smoothPosition.x,
                    left,
                    right
                );
            }
            else
            {
                smoothPosition.x = (minX + maxX) / 2f;
            }

            if (bottom <= top)
            {
                smoothPosition.y = Mathf.Clamp(
                    smoothPosition.y,
                    bottom,
                    top
                );
            }
            else
            {
                smoothPosition.y = (minY + maxY) / 2f;
            }
        }

        transform.position = smoothPosition;
    }
}
