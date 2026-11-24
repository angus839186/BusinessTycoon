using UnityEngine;

public sealed partial class TransportRouteBuilder
{
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
    private BuildingInstance RaycastBuilding()
    {
        Camera camera = Camera.main;

        if (camera == null) return null;

        Ray ray = camera.ScreenPointToRay(gameInput.PointerPosition);

        if (!Physics.Raycast(ray, out RaycastHit hit)) return null;

        return hit.collider.GetComponentInParent<BuildingInstance>();
    }
}