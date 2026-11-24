using UnityEngine;
using TMPro;
using UnityEngine.UI;

public sealed partial class TransportReportPanel : MonoBehaviour
{
    [SerializeField] private TransportSystem transport;

    [SerializeField] private PanelController panelController;
    [SerializeField] private MapModeController mapMode;
    [SerializeField] private ProductionSystem productionSystem;
    [SerializeField] private Transform routeListRoot;
    [SerializeField] private TransportRouteItem routeItemPrefab;
    [SerializeField] private GameObject emptyText;

    [SerializeField] private TMP_InputField searchInput;
    [SerializeField] private TMP_Dropdown statusDropdown;
    [SerializeField] private Button createRouteButton;




    private void Awake()
    {
        if (panelController == null)
        {
            panelController = FindFirstObjectByType<PanelController>();
        }

        if (mapMode == null)
        {
            mapMode = FindFirstObjectByType<MapModeController>();
        }

        if (productionSystem == null)
            productionSystem = FindFirstObjectByType<ProductionSystem>();

        if (transport == null)
            transport = FindFirstObjectByType<TransportSystem>();

        if (routeItemPrefab != null && routeListRoot != null)
        {
            routeItemList =
                new ReusableUIList<TransportRouteItem>(
                    routeItemPrefab,
                    routeListRoot
                );
        }
        if (searchInput != null)
        {
            searchInput.onValueChanged.AddListener(HandleSearchChanged);
        }

        if (statusDropdown != null)
        {
            statusDropdown.onValueChanged.AddListener(HandleStatusChanged);
        }
        if (createRouteButton != null)
        {
            createRouteButton.onClick.AddListener(
                HandleCreateRouteClicked
            );
        }

    }
    private void OnEnable()
    {
        if (transport == null)
            transport = FindFirstObjectByType<TransportSystem>();

        if (transport != null)
            transport.RoutesChanged += Refresh;
        if (productionSystem != null)
            productionSystem.DataChanged += RefreshRouteListNow;

        if (transport != null)
            transport.TransportChanged += RefreshRouteListNow;

        lastRouteVersion = -1;
        Refresh();
    }

    private void OnDisable()
    {
        if (transport != null)
            transport.RoutesChanged -= Refresh;
        if (productionSystem != null)
            productionSystem.DataChanged -= RefreshRouteListNow;
        if (transport != null)
            transport.TransportChanged -= RefreshRouteListNow;
    }

    private void OnDestroy()
    {
        if (searchInput != null)
        {
            searchInput.onValueChanged.RemoveListener(
                HandleSearchChanged
            );
        }

        if (statusDropdown != null)
        {
            statusDropdown.onValueChanged.RemoveListener(
                HandleStatusChanged
            );
        }
        if (createRouteButton != null)
        {
            createRouteButton.onClick.RemoveListener(
                HandleCreateRouteClicked
            );
        }
    }


}