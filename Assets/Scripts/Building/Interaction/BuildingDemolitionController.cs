using TMPro;
using UnityEngine;

public sealed class BuildingDemolitionController : MonoBehaviour
{
    [SerializeField] private GameInput gameInput;
    [SerializeField] private ProductionSystem productionSystem;
    [SerializeField] private TransportSystem transport;
    [SerializeField] private BuildingSelector buildingSelector;
    [SerializeField] private GameRules gameRules;
    [SerializeField] private BuildingPlacementController buildingPlacement;
    [SerializeField] private TextMeshProUGUI statusText;

    private bool isDemolitionMode;
    private bool initialized;

    public bool IsDemolitionMode => isDemolitionMode;

    private bool HasRequiredReferences =>
        gameInput != null && productionSystem != null && transport != null &&
        buildingSelector != null && gameRules != null &&
        buildingPlacement != null && statusText != null;

    private void Awake()
    {
        if (!HasRequiredReferences)
        {
            Debug.LogError("BuildingDemolitionController 缺少必要參考。", this);
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
        if (!isDemolitionMode || gameInput.IsPointerOverUI())
            return;

        TryDemolishClickedBuilding();
    }

    public void EnterDemolitionMode()
    {
        if (buildingPlacement.IsPlacementMode)
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

    public void ToggleDemolitionMode()
    {
        if (isDemolitionMode)
            ExitDemolitionMode();
        else
            EnterDemolitionMode();
    }

    public bool TryDemolish(BuildingInstance building)
    {
        if (building == null)
            return false;

        if (!productionSystem.TryPayDemolitionCost(building, gameRules.DemolitionCostRate))
        {
            Debug.Log($"Cannot demolish: not enough resources for {building.InstanceId}");
            return false;
        }

        string buildingId = building.InstanceId;

        transport.RemoveRoutesForBuilding(buildingId);
        productionSystem.Unregister(building);

        if (buildingSelector.SelectedBuilding == building)
            buildingSelector.ClearSelection();

        Destroy(building.gameObject);

        Debug.Log($"Building demolished: {buildingId}");
        SetStatus("建築已拆除");
        return true;
    }

    private void TryDemolishClickedBuilding()
    {
        Camera camera = Camera.main;

        if (camera == null)
            return;

        Ray ray = camera.ScreenPointToRay(gameInput.PointerPosition);

        if (!Physics.Raycast(ray, out RaycastHit hit))
            return;

        TryDemolish(hit.collider.GetComponentInParent<BuildingInstance>());
    }

    private void SetStatus(string message)
    {
        statusText.text = message;
    }
}