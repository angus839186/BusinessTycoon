using UnityEngine;
using TMPro;

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

    public bool IsDemolitionMode => isDemolitionMode;

    private void Awake()
    {
        if (gameInput == null)
            gameInput = FindFirstObjectByType<GameInput>();
        if (buildingPlacement == null)
            buildingPlacement = FindFirstObjectByType<BuildingPlacementController>();
        if (productionSystem == null)
            productionSystem = FindFirstObjectByType<ProductionSystem>();

        if (transport == null)
            transport = FindFirstObjectByType<TransportSystem>();

        if (gameRules == null)
            gameRules = FindFirstObjectByType<GameRules>();
        if (buildingSelector == null)
        {
            buildingSelector = FindFirstObjectByType<BuildingSelector>();
        }
    }

    private void OnEnable()
    {
        if (gameInput != null)
        {
            gameInput.PrimaryPressed +=
                HandlePrimaryPressed;
        }
    }

    private void OnDisable()
    {
        if (gameInput != null)
        {
            gameInput.PrimaryPressed -=
                HandlePrimaryPressed;
        }
    }

    private void HandlePrimaryPressed()
    {
        if (
            !isDemolitionMode ||
            gameInput == null ||
            gameInput.IsPointerOverUI()
        )
        {
            return;
        }

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
    public bool TryDemolish(BuildingInstance building)
    {
        if (building == null) return false;

        float demolitionCostRate = gameRules != null ? gameRules.DemolitionCostRate
                : 0.2f;

        if (productionSystem != null &&
            !productionSystem.TryPayDemolitionCost(building, demolitionCostRate)
        )
        {
            Debug.Log(
                $"Cannot demolish: not enough resources " +
                $"for {building.InstanceId}"
            );

            return false;
        }

        if (transport != null)
        {
            transport.RemoveRoutesForBuilding(building.InstanceId);
        }

        if (productionSystem != null)
        {
            productionSystem.Unregister(building);
        }

        if (buildingSelector != null && buildingSelector.SelectedBuilding == building
        )
        {
            buildingSelector.ClearSelection();
        }

        Destroy(building.gameObject);

        Debug.Log(
            $"Building demolished: {building.InstanceId}"
        );

        SetStatus("建築已拆除");
        return true;
    }

    private void TryDemolishClickedBuilding()
    {
        Camera camera = Camera.main;

        if (camera == null || gameInput == null)
            return;

        Ray ray = camera.ScreenPointToRay(gameInput.PointerPosition);

        if (!Physics.Raycast(ray, out RaycastHit hit))
            return;

        BuildingInstance building =
            hit.collider
                .GetComponentInParent<BuildingInstance>();

        TryDemolish(building);
    }
}