using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public sealed class MapCameraController : MonoBehaviour
{
    [SerializeField] private GameInput gameInput;
    [SerializeField] private PanelController panelController;
    [SerializeField] private float minCameraSize = 1f;
    [SerializeField] private float maxCameraSize = 20f;
    [SerializeField] private float zoomSpeed = 0.1f;
    [SerializeField] private float panSpeed = 1f;

    [SerializeField, Min(0.05f)]
    private float focusDuration = 0.35f;

    private Coroutine focusRoutine;


    private bool isInputBlocked;

    private void Awake()
    {
        if (gameInput == null)
        {
            gameInput = FindFirstObjectByType<GameInput>();
        }
        if (panelController == null)
        {
            panelController = FindFirstObjectByType<PanelController>();
        }
    }

    private void OnEnable()
    {
        if (panelController == null)
            return;

        panelController.PanelStateChanged +=
            HandlePanelStateChanged;

        HandlePanelStateChanged(
            panelController.HasOpenPanel
        );
    }

    private void OnDisable()
    {
        if (panelController != null)
        {
            panelController.PanelStateChanged -=
                HandlePanelStateChanged;
        }
    }

    private void HandlePanelStateChanged(bool isOpen)
    {
        isInputBlocked = isOpen;
    }
    public void FocusOn(Vector3 worldPosition)
    {
        if (focusRoutine != null)
        {
            StopCoroutine(focusRoutine);
        }

        focusRoutine = StartCoroutine(
            FocusRoutine(worldPosition)
        );
    }

    private IEnumerator FocusRoutine(
    Vector3 worldPosition
)
    {
        Camera camera = Camera.main;

        if (camera == null)
        {
            focusRoutine = null;
            yield break;
        }

        Vector3 startPosition =
            camera.transform.position;

        Vector3 targetPosition = new Vector3(
            worldPosition.x,
            worldPosition.y,
            startPosition.z
        );

        float elapsedTime = 0f;

        while (elapsedTime < focusDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(
                elapsedTime / focusDuration
            );

            float smoothProgress =
                Mathf.SmoothStep(0f, 1f, progress);

            camera.transform.position =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    smoothProgress
                );

            yield return null;
        }

        camera.transform.position = targetPosition;
        focusRoutine = null;
    }

    private void Update()
    {
        if (
            gameInput == null ||
            isInputBlocked ||
            !gameInput.IsMapEnabled
        )
        {
            return;
        }

        if (
            EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject()
        )
        {
            return;
        }

        Camera camera = Camera.main;

        if (camera == null)
            return;

        if (!camera.orthographic)
            camera.orthographic = true;

        Vector2 scroll =
            gameInput.ZoomDelta;

        if (Mathf.Abs(scroll.y) > 0.01f)
        {
            float zoomDelta =
                -scroll.y *
                zoomSpeed *
                camera.orthographicSize;

            camera.orthographicSize = Mathf.Clamp(
                camera.orthographicSize + zoomDelta,
                minCameraSize,
                maxCameraSize
            );
        }

        if (
            gameInput.IsPanHeld ||
            gameInput.IsSecondaryHeld
        )
        {
            Vector2 delta =
                gameInput.PanDelta;

            float unitsPerPixel =
                camera.orthographicSize *
                2f /
                Screen.height;

            Vector3 move = new Vector3(
                -delta.x * unitsPerPixel * panSpeed,
                -delta.y * unitsPerPixel * panSpeed,
                0f
            );

            camera.transform.position += move;
        }
    }
}