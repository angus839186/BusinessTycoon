using UnityEngine;
using TMPro;
using System;

public sealed partial class TransportRouteBuilder : MonoBehaviour
{
    [SerializeField] private GameInput gameInput;
    [SerializeField] private BuildingPlacementController buildingPlacement;

    [SerializeField] private BuildingDemolitionController demolitionController;
    [SerializeField] private TransportSystem transport;
    [SerializeField] private TextMeshProUGUI statusText;

    private bool isCreatingRoute;
    public bool IsCreatingRoute => isCreatingRoute;
    private BuildingInstance selectedSource;

    public event Action<string> RouteFailed;

    private void Awake()
    {
        if (gameInput == null) gameInput = FindFirstObjectByType<GameInput>();
        if (buildingPlacement == null)
        {
            buildingPlacement =
            FindFirstObjectByType<BuildingPlacementController>();
        }

        if (demolitionController == null)
        {
            demolitionController = FindFirstObjectByType<BuildingDemolitionController>();
        }
        if (transport == null) transport = FindFirstObjectByType<TransportSystem>();
    }

    private void OnEnable()
    {
        if (gameInput != null)
            gameInput.PrimaryPressed += HandlePrimaryPressed;
    }

    private void OnDisable()
    {
        if (gameInput != null)
            gameInput.PrimaryPressed -= HandlePrimaryPressed;
    }



    public void StartCreateRoute()
    {
        if (transport == null)
        {
            Debug.LogError("TransportSystem missing.");
            return;
        }

        if (buildingPlacement != null && buildingPlacement.IsPlacementMode)
        {
            buildingPlacement.ExitPlacementMode();
        }

        if (demolitionController != null && demolitionController.IsDemolitionMode
        )
        {
            demolitionController.ExitDemolitionMode();
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


    private void ReportFailure(string message)
    {
        SetStatus(message);
        RouteFailed?.Invoke(message);
    }
    private void SetStatus(string message)
    {
        if (statusText != null) statusText.text = message;
    }
}