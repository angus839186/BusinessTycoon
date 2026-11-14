using System;
using UnityEngine;

public sealed class BuildingSelector : MonoBehaviour
{
    [SerializeField] private MapModeController mapMode;
    [SerializeField] private PanelController panelController;
    [SerializeField] private BuildingInstance selectedBuilding;
    [SerializeField] private GameInput gameInput;

    public BuildingInstance SelectedBuilding => selectedBuilding;
    public event Action<BuildingInstance> SelectionChanged;

    private void Awake()
    {
        if (mapMode == null)
            mapMode = FindFirstObjectByType<MapModeController>();

        if (panelController == null)
            panelController = FindFirstObjectByType<PanelController>();

        if (gameInput == null)
            gameInput = FindFirstObjectByType<GameInput>();
    }

    private void OnEnable()
    {
        if (gameInput != null)
        {
            gameInput.PrimaryPressed += HandlePrimaryPressed;
        }
    }

    private void OnDisable()
    {
        if (gameInput != null)
        {
            gameInput.PrimaryPressed -= HandlePrimaryPressed;
        }
    }

    private void HandlePrimaryPressed()
    {
        if (gameInput != null && gameInput.IsPointerOverUI()) return;
        if (panelController != null && panelController.HasOpenPanel) return;
        if (mapMode != null && mapMode.HasActiveMode) return;

        SelectBuilding(RaycastBuilding());
    }

    public void SelectBuilding(BuildingInstance building)
    {
        if (selectedBuilding == building) return;

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
        if (camera == null) return null;
        Ray ray = camera.ScreenPointToRay(gameInput.PointerPosition);

        if (!Physics.Raycast(ray, out RaycastHit hit)) return null;
        return hit.collider.GetComponentInParent<BuildingInstance>();
    }
}