using System;
using UnityEngine;

public sealed class BuildingSelector : MonoBehaviour
{
    [SerializeField] private MapModeController mapMode;
    [SerializeField] private PanelController panelController;
    [SerializeField] private BuildingInstance selectedBuilding;
    [SerializeField] private GameInput gameInput;

    private bool initialized;

    public BuildingInstance SelectedBuilding => selectedBuilding;
    public event Action<BuildingInstance> SelectionChanged;

    private bool HasRequiredReferences =>
        mapMode != null && panelController != null && gameInput != null;

    private void Awake()
    {
        if (!HasRequiredReferences)
        {
            Debug.LogError("BuildingSelector 缺少必要參考。", this);
            enabled = false;
            return;
        }

        initialized = true;
    }

    private void OnEnable()
    {
        if (!initialized)
            return;

        gameInput.PrimaryPressed += HandlePrimaryPressed;
    }

    private void OnDisable()
    {
        if (!initialized)
            return;

        gameInput.PrimaryPressed -= HandlePrimaryPressed;
    }

    private void HandlePrimaryPressed()
    {
        if (gameInput.IsPointerOverUI() || panelController.HasOpenPanel || mapMode.HasActiveMode)
            return;

        SelectBuilding(RaycastBuilding());
    }

    public void SelectBuilding(BuildingInstance building)
    {
        if (selectedBuilding == building)
            return;

        selectedBuilding = building;
        SelectionChanged?.Invoke(selectedBuilding);
    }

    public void ClearSelection()
    {
        SelectBuilding(null);
    }

    private BuildingInstance RaycastBuilding()
    {
        Camera camera = Camera.main;

        if (camera == null)
            return null;

        Ray ray = camera.ScreenPointToRay(gameInput.PointerPosition);

        if (!Physics.Raycast(ray, out RaycastHit hit))
            return null;

        return hit.collider.GetComponentInParent<BuildingInstance>();
    }
}