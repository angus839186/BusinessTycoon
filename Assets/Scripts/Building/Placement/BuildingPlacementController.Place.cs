using UnityEngine;

public sealed partial class BuildingPlacementController
{
    public void PlaceBuilding(Vector3 worldPosition, double longitude, double latitude)
    {
        if (selectedBuilding == null)
        {
            const string message = "No building selected.";
            Debug.Log(message);
            PlacementFailed?.Invoke(message);
            return;
        }

        ResourceAmount[] paidConstructionCosts = selectedBuilding.constructionCosts;

        if (!productionSystem.TryPayConstructionCost(selectedBuilding, worldPosition, longitude, latitude, out paidConstructionCosts))
        {
            string message = $"Cannot Build: not enough resources for {selectedBuilding.displayName}";
            Debug.Log(message);
            PlacementFailed?.Invoke(message);
            return;
        }

        string instanceId = $"{selectedBuilding.buildingId}_{nextBuildingInstanceIndex}";

        nextBuildingInstanceIndex++;

        BuildingInstance instance =
            CreateBuildingInstance(
                selectedBuilding,
                instanceId,
                worldPosition,
                longitude,
                latitude,
                paidConstructionCosts
            );

        BuildingPlaced?.Invoke(instance);

        Debug.Log(
            $"Building Placed: Building={selectedBuilding.displayName}, " +
            $"Lon={longitude:F6}, Lat={latitude:F6}, " +
            $"Total={productionSystem.Buildings.Count}"
        );
    }

    private BuildingInstance CreateBuildingInstance(
        BuildingDefinition definition,
        string instanceId,
        Vector3 worldPosition,
        double longitude,
        double latitude,
        ResourceAmount[] constructionCosts
    )
    {
        GameObject buildingObject =
            GameObject.CreatePrimitive(PrimitiveType.Quad);

        buildingObject.transform.position =
            new Vector3(worldPosition.x, worldPosition.y, -0.5f);

        buildingObject.name = $"Building_{instanceId}";
        buildingObject.transform.localScale = Vector3.one * 0.15f;

        Renderer renderer = buildingObject.GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.material.color = definition.markerColor;
        renderer.sortingOrder = 200;

        BuildingInstance instance =
            buildingObject.AddComponent<BuildingInstance>();

        instance.Initialize(definition, instanceId);
        instance.SetLocation(worldPosition, longitude, latitude);
        instance.SetConstructionCostSnapshot(constructionCosts);

        productionSystem.Register(instance);

        CreateProgressBar(buildingObject.transform, instance);

        return instance;
    }
}