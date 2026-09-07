using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public sealed class BuildingDemolitionController : MonoBehaviour
{
    [SerializeField] private ProductionSystem productionSystem;
    [SerializeField] private TransportSystem transport;
    [SerializeField] private GameRules gameRules;

    [SerializeField] private BuildingPlacementController buildingPlacement;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private BuildingInfoPanel buildingInfoPanel;

    private bool isDemolitionMode;

    public bool IsDemolitionMode => isDemolitionMode;

    private void Awake()
    {
        if (buildingInfoPanel == null)
            buildingInfoPanel = FindFirstObjectByType<BuildingInfoPanel>();
        if (buildingPlacement == null)
            buildingPlacement = FindFirstObjectByType<BuildingPlacementController>();
        if (productionSystem == null)
            productionSystem = FindFirstObjectByType<ProductionSystem>();

        if (transport == null)
            transport = FindFirstObjectByType<TransportSystem>();

        if (gameRules == null)
            gameRules = FindFirstObjectByType<GameRules>();
    }

    private void Update()
    {
        if (!isDemolitionMode || Mouse.current == null)
            return;

        if (
            Mouse.current.rightButton.wasPressedThisFrame ||
            Keyboard.current.escapeKey.wasPressedThisFrame
        )
        {
            ExitDemolitionMode();
            return;
        }

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        TryDemolishClickedBuilding();
    }

    public void EnterDemolitionMode()
    {
        if (buildingPlacement != null && buildingPlacement.IsPlacementMode)
            buildingPlacement.ExitPlacementMode();

        isDemolitionMode = true;
        SetStatus("拆除模式：點選建築拆除，右鍵或 Esc 取消");
        Debug.Log("Demolition mode entered.");
    }

    public void ExitDemolitionMode()
    {
        isDemolitionMode = false;
        SetStatus("");
        Debug.Log("Demolition mode exited.");
    }
    private void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }

    public void ToggleDemolitionMode()
    {
        if (isDemolitionMode)
            ExitDemolitionMode();
        else
            EnterDemolitionMode();
    }

    private void TryDemolishClickedBuilding()
    {
        Camera camera = Camera.main;

        if (camera == null)
            return;

        Ray ray = camera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit))
            return;

        BuildingInstance building = hit.collider.GetComponent<BuildingInstance>();

        if (building == null)
            return;

        float demolitionCostRate =
            gameRules != null ? gameRules.DemolitionCostRate : 0.2f;

        if (
            productionSystem != null &&
            !productionSystem.TryPayDemolitionCost(building, demolitionCostRate)
        )
        {
            Debug.Log($"Cannot demolish: not enough resources for {building.InstanceId}");
            return;
        }

        if (transport != null)
            transport.RemoveRoutesForBuilding(building.InstanceId);

        if (productionSystem != null)
            productionSystem.Unregister(building);
        if (
buildingInfoPanel != null &&
buildingInfoPanel.IsShowing(building)
)
        {
            buildingInfoPanel.ClearSelection();
        }

        Destroy(building.gameObject);

        Debug.Log($"Building demolished: {building.InstanceId}");
        SetStatus("建築已拆除");
    }
}