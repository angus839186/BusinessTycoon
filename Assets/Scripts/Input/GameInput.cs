using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public sealed class GameInput : MonoBehaviour
{
    [SerializeField] private InputActionAsset actions;
    private InputActionMap mapActions;
    private InputActionMap globalActions;
    private InputAction point;
    private InputAction primaryClick;
    private InputAction secondaryClick;
    private InputAction zoom;
    private InputAction pan;
    private InputAction panHold;
    private InputAction cancel;
    private InputAction speedUp;
    private InputAction speedDown;
    private InputAction toggleMenu;
    private bool initialized;
    private readonly List<RaycastResult> uiRaycastResults = new List<RaycastResult>();

    public bool IsSecondaryHeld => secondaryClick?.IsPressed() ?? false;
    public Vector2 PointerPosition => point?.ReadValue<Vector2>() ?? Vector2.zero;
    public Vector2 ZoomDelta => zoom?.ReadValue<Vector2>() ?? Vector2.zero;
    public Vector2 PanDelta => pan?.ReadValue<Vector2>() ?? Vector2.zero;
    public bool IsPanHeld => panHold?.IsPressed() ?? false;
    public bool IsMapEnabled => mapActions?.enabled ?? false;
    public event Action PrimaryPressed;
    public event Action SecondaryPressed;
    public event Action CancelPressed;
    public event Action SpeedUpPressed;
    public event Action SpeedDownPressed;
    public event Action MenuTogglePressed;

    private void Awake()
    {
        Initialize();
    }

    private void OnEnable()
    {
        if (!initialized)
            return;

        primaryClick.performed += HandlePrimary;
        secondaryClick.performed += HandleSecondary;
        cancel.performed += HandleCancel;
        speedUp.performed += HandleSpeedUp;
        speedDown.performed += HandleSpeedDown;
        toggleMenu.performed += HandleMenuToggle;

        mapActions.Enable();
        globalActions.Enable();
    }

    private void OnDisable()
    {
        if (!initialized) return;

        primaryClick.performed -= HandlePrimary;
        secondaryClick.performed -= HandleSecondary;

        cancel.performed -= HandleCancel;
        speedUp.performed -= HandleSpeedUp;
        speedDown.performed -= HandleSpeedDown;
        toggleMenu.performed -= HandleMenuToggle;

        mapActions.Disable();
        globalActions.Disable();
    }

    public void SetMapEnabled(bool enabled)
    {
        if (!initialized)
            return;

        if (enabled)
            mapActions.Enable();
        else
            mapActions.Disable();
    }

    private void Initialize()
    {
        if (actions == null)
        {
            Debug.LogError("GameInput 尚未設定 Input Action Asset。", this);
            enabled = false;
            return;
        }

        mapActions = actions.FindActionMap("Map", true);

        globalActions = actions.FindActionMap("Global", true);

        point = mapActions.FindAction("Point", true);
        primaryClick = mapActions.FindAction("PrimaryClick", true);
        secondaryClick = mapActions.FindAction("SecondaryClick", true);
        zoom = mapActions.FindAction("Zoom", true);
        pan = mapActions.FindAction("Pan", true);
        panHold = mapActions.FindAction("PanHold", true);

        cancel = globalActions.FindAction("Cancel", true);
        speedUp = globalActions.FindAction("SpeedUp", true);
        speedDown = globalActions.FindAction("SpeedDown", true);
        toggleMenu = globalActions.FindAction("ToggleMenu", true);

        initialized = true;
    }

    private void HandlePrimary(InputAction.CallbackContext context) => PrimaryPressed?.Invoke();
    private void HandleSecondary(InputAction.CallbackContext context) => SecondaryPressed?.Invoke();
    private void HandleCancel(InputAction.CallbackContext context) => CancelPressed?.Invoke();
    private void HandleSpeedUp(InputAction.CallbackContext context) => SpeedUpPressed?.Invoke();
    private void HandleSpeedDown(InputAction.CallbackContext context) => SpeedDownPressed?.Invoke();
    private void HandleMenuToggle(InputAction.CallbackContext context) => MenuTogglePressed?.Invoke();
    public bool IsPointerOverUI()
    {
        EventSystem eventSystem = EventSystem.current;

        if (eventSystem == null)
            return false;

        PointerEventData pointerData = new PointerEventData(eventSystem) { position = PointerPosition };

        uiRaycastResults.Clear();
        eventSystem.RaycastAll(pointerData, uiRaycastResults);

        return uiRaycastResults.Count > 0;
    }
}