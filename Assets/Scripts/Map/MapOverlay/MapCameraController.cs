using UnityEngine;
using UnityEngine.InputSystem;

public sealed class MapCameraController : MonoBehaviour
{
    [SerializeField] private float minCameraSize = 1f;
    [SerializeField] private float maxCameraSize = 20f;
    [SerializeField] private float zoomSpeed = 0.1f;
    [SerializeField] private float panSpeed = 1f;

    private Vector2 lastMousePosition;

    private void Update()
    {
        if (Mouse.current == null)
            return;

        Camera camera = Camera.main;

        if (camera == null)
            return;

        if (!camera.orthographic)
            camera.orthographic = true;

        Vector2 mousePosition = Mouse.current.position.ReadValue();

        Vector2 scroll = Mouse.current.scroll.ReadValue();

        if (Mathf.Abs(scroll.y) > 0.01f)
        {
            float zoomDelta =
                -scroll.y * zoomSpeed * camera.orthographicSize;

            camera.orthographicSize = Mathf.Clamp(
                camera.orthographicSize + zoomDelta,
                minCameraSize,
                maxCameraSize
            );
        }

        if (
            Mouse.current.middleButton.isPressed ||
            Mouse.current.rightButton.isPressed
        )
        {
            Vector2 delta = mousePosition - lastMousePosition;

            float unitsPerPixel =
                camera.orthographicSize * 2f / Screen.height;

            Vector3 move = new Vector3(
                -delta.x * unitsPerPixel * panSpeed,
                -delta.y * unitsPerPixel * panSpeed,
                0f
            );

            camera.transform.position += move;
        }

        lastMousePosition = mousePosition;
    }
}