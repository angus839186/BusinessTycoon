using UnityEngine;
using TMPro;
using System;

public sealed class TransportRouteBuilder : MonoBehaviour
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

    private void HandlePrimaryPressed()
    {
        if (
            !isCreatingRoute ||
            gameInput == null ||
            gameInput.IsPointerOverUI()
        )
        {
            return;
        }

        BuildingInstance clickedBuilding =
            RaycastBuilding();

        if (clickedBuilding == null)
            return;

        if (clickedBuilding.Definition == null)
        {
            ReportFailure("建築資料無效。");
            return;
        }

        if (selectedSource == null)
        {
            if (transport.HasOutgoingRoute(
                    clickedBuilding.InstanceId
                ))
            {
                ReportFailure(
                $"{clickedBuilding.Definition.displayName} " +
                "已經有輸出路線。"
            );

                return;
            }

            selectedSource = clickedBuilding;

            SetStatus(
                $"已選擇起點：" +
                $"{selectedSource.Definition.displayName}，" +
                "請選擇終點建築。"
            );

            Debug.Log(
                $"Route Source Selected: " +
                $"{selectedSource.InstanceId}"
            );

            return;
        }

        if (clickedBuilding == selectedSource)
        {
            ReportFailure("起點與終點不能是同一棟建築。");

            return;
        }

        if (!transport.TryAddDefinedRoute(
                selectedSource,
                clickedBuilding,
                out string errorMessage
            ))
        {
            ReportFailure(errorMessage);
            return;
        }

        SetStatus("路線已建立。");
        StopCreateRoute();
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

    private BuildingInstance RaycastBuilding()
    {
        Camera camera = Camera.main;

        if (camera == null) return null;

        Ray ray = camera.ScreenPointToRay(gameInput.PointerPosition);

        if (!Physics.Raycast(ray, out RaycastHit hit)) return null;

        return hit.collider.GetComponentInParent<BuildingInstance>();
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