using System.Collections;
using UnityEngine;

public sealed partial class MapCameraController
{
    private Coroutine focusRoutine;

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
}