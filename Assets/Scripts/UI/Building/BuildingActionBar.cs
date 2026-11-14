using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class BuildingActionBar : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject barRoot;

    [SerializeField] private TextMeshProUGUI nameText;

    [SerializeField] private Button operationalButton;

    [SerializeField] private TextMeshProUGUI operationalButtonText;

    [SerializeField] private Button demolitionButton;

    [SerializeField] private Button upgradeButton;

    [Header("Systems")]
    [SerializeField] private BuildingSelector selector;

    [SerializeField] private ProductionSystem productionSystem;

    [SerializeField] private BuildingDemolitionController demolition;

    [SerializeField] private PanelController panelController;

    private BuildingInstance selectedBuilding;
    private bool panelOpen;

    private void Awake()
    {
        if (selector == null)
        {
            selector = FindFirstObjectByType<BuildingSelector>();
        }

        if (productionSystem == null)
        {
            productionSystem = FindFirstObjectByType<ProductionSystem>();
        }

        if (demolition == null)
        {
            demolition =
                FindFirstObjectByType<BuildingDemolitionController>();
        }

        if (panelController == null)
        {
            panelController = FindFirstObjectByType<PanelController>();
        }

        if (operationalButton != null)
        {
            operationalButton.onClick.AddListener(HandleOperationalClicked);
        }

        if (demolitionButton != null)
        {
            demolitionButton.onClick.AddListener(HandleDemolitionClicked);
        }

        if (upgradeButton != null)
        {
            upgradeButton.interactable = false;
        }

        if (barRoot != null)
        {
            barRoot.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (selector != null)
        {
            selector.SelectionChanged += HandleSelectionChanged;
        }

        if (panelController != null)
        {
            panelController.PanelStateChanged += HandlePanelStateChanged;

            panelOpen = panelController.HasOpenPanel;
        }

        HandleSelectionChanged(selector != null ? selector.SelectedBuilding : null);
    }

    private void OnDisable()
    {
        if (selector != null)
        {
            selector.SelectionChanged -= HandleSelectionChanged;
        }

        if (panelController != null)
        {
            panelController.PanelStateChanged -= HandlePanelStateChanged;
        }

        UnsubscribeSelectedBuilding();
    }

    private void OnDestroy()
    {
        if (operationalButton != null)
        {
            operationalButton.onClick.RemoveListener(HandleOperationalClicked);
        }

        if (demolitionButton != null)
        {
            demolitionButton.onClick.RemoveListener(HandleDemolitionClicked);
        }
    }

    private void HandleSelectionChanged(BuildingInstance building)
    {
        UnsubscribeSelectedBuilding();
        selectedBuilding = building;

        if (selectedBuilding != null)
        {
            selectedBuilding.OperationalStateChanged += HandleOperationalStateChanged;
        }

        Refresh();
    }

    private void UnsubscribeSelectedBuilding()
    {
        if (selectedBuilding != null)
        {
            selectedBuilding.OperationalStateChanged -= HandleOperationalStateChanged;
        }
    }

    private void HandleOperationalStateChanged(bool operational)
    {
        Refresh();
    }

    private void HandlePanelStateChanged(bool isOpen
    )
    {
        panelOpen = isOpen;
        Refresh();
    }

    private void HandleOperationalClicked()
    {
        if (selectedBuilding == null || productionSystem == null
        )
        {
            return;
        }

        productionSystem.SetBuildingOperational(selectedBuilding, !selectedBuilding.IsOperational);
    }

    private void HandleDemolitionClicked()
    {
        if (selectedBuilding == null || demolition == null)
        {
            return;
        }

        demolition.TryDemolish(selectedBuilding);
    }

    private void Refresh()
    {
        bool visible = selectedBuilding != null && !panelOpen;

        if (barRoot != null)
        {
            barRoot.SetActive(visible);
        }

        if (!visible) return;

        BuildingDefinition definition = selectedBuilding.Definition;

        if (nameText != null)
        {
            nameText.text =
                definition != null ? definition.displayName : selectedBuilding.InstanceId;
        }

        if (operationalButtonText != null)
        {
            operationalButtonText.text =
                selectedBuilding.IsOperational ? "停用" : "啟用";
        }

        if (operationalButton != null)
        {
            operationalButton.interactable = productionSystem != null;
        }

        if (demolitionButton != null)
        {
            demolitionButton.interactable = demolition != null;
        }

        if (upgradeButton != null)
        {
            upgradeButton.interactable = false;
        }
    }
}