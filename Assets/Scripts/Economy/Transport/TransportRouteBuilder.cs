using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;

public sealed class TransportRouteBuilder : MonoBehaviour
{
    [SerializeField] private TransportSystem transport;
    [SerializeField] private ResourceDefinition selectedResource;
    [SerializeField] private int amountPerTransfer = 1;
    [SerializeField] private TextMeshProUGUI statusText;

    private bool isCreatingRoute;
    public bool IsCreatingRoute => isCreatingRoute;
    private BuildingInstance selectedSource;

    private void Awake()
    {
        if (transport == null)
            transport = FindFirstObjectByType<TransportSystem>();
    }

    private void Update()
    {
        if (!isCreatingRoute || Mouse.current == null)
            return;

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            CancelCreateRoute();
            return;
        }

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        // if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        //     return;

        BuildingInstance clickedBuilding = RaycastBuilding();

        if (clickedBuilding == null)
            return;

        if (selectedSource == null)
        {
            selectedSource = clickedBuilding;
            SetStatus($"來源：{selectedSource.Definition.displayName}，請選擇目標建築");
            Debug.Log($"Route Source Selected: {selectedSource.InstanceId}");
            return;
        }

        if (clickedBuilding == selectedSource)
            return;

        transport.AddDefinedRoute(
    selectedSource,
    clickedBuilding,
    selectedResource,
    amountPerTransfer
);

        SetStatus("路線已建立");
        StopCreateRoute();
    }

    public void StartCreateRoute()
    {
        if (selectedResource == null)
        {
            Debug.Log("Cannot create route: no resource selected.");
            return;
        }

        isCreatingRoute = true;
        selectedSource = null;
        SetStatus("請選擇來源建築");
        Debug.Log("Route creation started. Click source building.");
    }

    public void CancelCreateRoute()
    {
        isCreatingRoute = false;
        selectedSource = null;
        SetStatus("");
        Debug.Log("Route creation cancelled.");
    }

    private void StopCreateRoute()
    {
        isCreatingRoute = false;
        selectedSource = null;
    }

    private BuildingInstance RaycastBuilding()
    {
        Camera camera = Camera.main;

        if (camera == null)
            return null;

        Ray ray = camera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit))
            return null;

        return hit.collider.GetComponent<BuildingInstance>();
    }

    public void SetSelectedResource(ResourceDefinition resource)
    {
        selectedResource = resource;

        if (selectedResource == null)
        {
            Debug.Log("Route resource selected: none");
            return;
        }

        Debug.Log($"Route resource selected: {selectedResource.displayName}");
    }

    public void SetAmountPerTransfer(int amount)
    {
        amountPerTransfer = Mathf.Max(1, amount);
    }
    private void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }
}