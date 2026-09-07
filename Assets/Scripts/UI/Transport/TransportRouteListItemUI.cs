using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public sealed class TransportRouteListItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI routeText;
    [SerializeField] private Button toggleButton;
    [SerializeField] private Button removeButton;
    [SerializeField] private TextMeshProUGUI toggleButtonText;

    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private Button decreaseAmountButton;
    [SerializeField] private Button increaseAmountButton;

    [SerializeField] private TMP_Dropdown resourceDropdown;

    private readonly List<ResourceDefinition> resources =
        new List<ResourceDefinition>();
    private int amountPerTransfer;

    private TransportReportPanel owner;
    private string routeId;
    private bool routeEnabled;

    public void Initialize(
        TransportReportPanel panel,
        TransportRouteDefinition route
    )
    {
        owner = panel;

        if (route == null)
            return;

        routeId = route.routeId;
        routeEnabled = route.enabled;
        amountPerTransfer = Mathf.Max(1, route.amountPerTransfer);

        string routeName = string.IsNullOrWhiteSpace(route.displayName)
            ? route.routeId
            : route.displayName;

        string resourceName = route.resource != null
            ? route.resource.displayName
            : "No Resource";

        if (routeText != null)
        {
            routeText.text =
                $"{route.routeId} | {routeName} | {resourceName} x{route.amountPerTransfer}";
        }

        if (amountText != null)
            amountText.text = amountPerTransfer.ToString();

        if (toggleButtonText != null)
            toggleButtonText.text = routeEnabled ? "停用" : "啟用";

        if (toggleButton != null)
        {
            toggleButton.onClick.RemoveAllListeners();
            toggleButton.onClick.AddListener(OnToggleClicked);
        }

        if (removeButton != null)
        {
            removeButton.onClick.RemoveAllListeners();
            removeButton.onClick.AddListener(OnRemoveClicked);
        }
        if (decreaseAmountButton != null)
        {
            decreaseAmountButton.onClick.RemoveAllListeners();
            decreaseAmountButton.onClick.AddListener(OnDecreaseAmountClicked);
        }

        if (increaseAmountButton != null)
        {
            increaseAmountButton.onClick.RemoveAllListeners();
            increaseAmountButton.onClick.AddListener(OnIncreaseAmountClicked);
        }
        SetupResourceDropdown(route);
    }

    private void OnToggleClicked()
    {
        if (owner == null || string.IsNullOrWhiteSpace(routeId))
            return;

        if (routeEnabled)
            owner.DisableRoute(routeId);
        else
            owner.EnableRoute(routeId);
    }

    private void OnRemoveClicked()
    {
        if (owner == null || string.IsNullOrWhiteSpace(routeId))
            return;

        owner.RemoveRoute(routeId);
    }
    private void OnDecreaseAmountClicked()
    {
        if (owner == null || string.IsNullOrWhiteSpace(routeId))
            return;

        owner.SetRouteAmount(routeId, amountPerTransfer - 1);
    }

    private void OnIncreaseAmountClicked()
    {
        if (owner == null || string.IsNullOrWhiteSpace(routeId))
            return;

        owner.SetRouteAmount(routeId, amountPerTransfer + 1);
    }
    private void SetupResourceDropdown(TransportRouteDefinition route)
    {
        if (resourceDropdown == null || owner == null || route == null)
            return;

        resources.Clear();
        resourceDropdown.ClearOptions();
        resourceDropdown.onValueChanged.RemoveAllListeners();

        resources.AddRange(owner.GetAvailableRouteResources(route));

        if (route.resource != null && !resources.Contains(route.resource))
            resources.Insert(0, route.resource);

        List<string> labels =
            new List<string>();

        int selectedIndex = 0;

        for (int i = 0; i < resources.Count; i++)
        {
            ResourceDefinition resource = resources[i];

            if (resource == null)
                continue;

            if (resource == route.resource)
                selectedIndex = i;

            labels.Add(string.IsNullOrWhiteSpace(resource.displayName)
                ? resource.resourceId
                : resource.displayName);
        }

        resourceDropdown.AddOptions(labels);
        resourceDropdown.value = selectedIndex;
        resourceDropdown.RefreshShownValue();
        resourceDropdown.onValueChanged.AddListener(OnResourceChanged);
    }

    private void OnResourceChanged(int index)
    {
        if (
            owner == null ||
            string.IsNullOrWhiteSpace(routeId) ||
            index < 0 ||
            index >= resources.Count
        )
            return;

        owner.SetRouteResource(routeId, resources[index]);
    }
}