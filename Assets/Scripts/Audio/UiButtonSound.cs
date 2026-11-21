using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public sealed class UiButtonSound :
    MonoBehaviour
{
    [SerializeField] private InputActionReference pointAction;

    [SerializeField] private InputActionReference clickAction;

    private readonly List<RaycastResult> raycastResults = new List<RaycastResult>();

    private bool ownsPointAction;
    private bool ownsClickAction;

    private bool HasRequiredActions =>
    pointAction != null &&
    pointAction.action != null &&
    clickAction != null &&
    clickAction.action != null;

    private void OnEnable()
    {
        if (!HasRequiredActions)
        {
            Debug.LogError(
                "UiButtonSound 尚未設定 UI Point 或 Click Action。",
                this
            );

            enabled = false;
            return;
        }

        if (!pointAction.action.enabled)
        {
            pointAction.action.Enable();
            ownsPointAction = true;
        }

        if (!clickAction.action.enabled)
        {
            clickAction.action.Enable();
            ownsClickAction = true;
        }

        clickAction.action.performed += HandleClick;
    }

    private void OnDisable()
    {
        if (!HasRequiredActions)
        {
            ownsPointAction = false;
            ownsClickAction = false;
            return;
        }

        clickAction.action.performed -= HandleClick;

        if (ownsClickAction)
        {
            clickAction.action.Disable();
        }

        if (ownsPointAction)
        {
            pointAction.action.Disable();
        }

        ownsPointAction = false;
        ownsClickAction = false;
    }

    private void HandleClick(InputAction.CallbackContext context)
    {
        EventSystem eventSystem = EventSystem.current;

        if (eventSystem == null)
        {
            return;
        }

        Vector2 pointerPosition = pointAction.action.ReadValue<Vector2>();

        PointerEventData pointerData = new PointerEventData(eventSystem)
        {
            position = pointerPosition
        };

        raycastResults.Clear();

        EventSystem.current.RaycastAll(pointerData, raycastResults);

        foreach (RaycastResult result in raycastResults)
        {
            Button button = result.gameObject.GetComponentInParent<Button>();

            if (button == null ||
            !button.isActiveAndEnabled || !button.IsInteractable())
            {
                continue;
            }

            AudioSystem audioSystem = AudioSystem.Instance;

            if (audioSystem != null)
            {
                audioSystem.PlaySfx(SfxId.UiClick);
            }

            return;
        }
    }
}