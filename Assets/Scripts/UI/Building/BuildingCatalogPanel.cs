using UnityEngine;
using TMPro;

public partial class BuildingCatalogPanel : MonoBehaviour
{
    [SerializeField] private MapModeController mapMode;

    [SerializeField] private GameDefinitionCatalog definitionCatalog;
    [SerializeField] private BuildingPlacementController buildingPlacement;

    [SerializeField] private Transform listRoot;
    [SerializeField] private BuildingCatalogItem itemPrefab;

    [SerializeField] private TMP_InputField searchInput;
    [SerializeField] private TMP_Dropdown categoryDropdown;
    [SerializeField] private GameObject emptyText;
    [SerializeField] private PanelController PanelController;

    private void Awake()
    {
        if (mapMode == null)
            mapMode = FindFirstObjectByType<MapModeController>();

        if (definitionCatalog == null)
            definitionCatalog = FindFirstObjectByType<GameDefinitionCatalog>();

        if (buildingPlacement == null)
            buildingPlacement =
            FindFirstObjectByType<BuildingPlacementController>();

        if (PanelController == null)
            PanelController = FindFirstObjectByType<PanelController>();

        if (searchInput != null)
        {
            searchInput.onValueChanged.AddListener(
                HandleSearchChanged
            );
        }

        if (categoryDropdown != null)
        {
            categoryDropdown.onValueChanged.AddListener(
                HandleCategoryChanged
            );
        }

        itemList = new ReusableUIList<BuildingCatalogItem>(itemPrefab, listRoot);

        if (definitionCatalog != null && definitionCatalog.BuildingCount == 0)
            definitionCatalog.Reload();
    }

    private void OnEnable()
    {
        if (buildingPlacement != null)
        {
            buildingPlacement.SelectedBuildingChanged +=
                HandleSelectedBuildingChanged;
        }

        BuildList();


    }

    private void OnDisable()
    {
        if (buildingPlacement != null)
        {
            buildingPlacement.SelectedBuildingChanged -=
                HandleSelectedBuildingChanged;
        }
    }

    private void OnDestroy()
    {
        if (searchInput != null)
        {
            searchInput.onValueChanged.RemoveListener(
                HandleSearchChanged
            );
        }

        if (categoryDropdown != null)
        {
            categoryDropdown.onValueChanged.RemoveListener(
                HandleCategoryChanged
            );
        }
    }

    private void HandleBuildingSelected(BuildingDefinition definition)
    {
        if (buildingPlacement == null ||
            definition == null || !definition.isEnabled)
            return;

        buildingPlacement.SelectBuilding(definition);

        if (PanelController != null) PanelController.CloseCurrentPanel();

        if (mapMode != null) mapMode.StartPlacementMode();
    }

}