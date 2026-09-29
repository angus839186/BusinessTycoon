using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class GameDefinitionCatalog : MonoBehaviour
{
    [SerializeField] private string buildingResourcesFolder = "Buildings";
    [SerializeField] private string resourceResourcesFolder = "Resource";

    private readonly Dictionary<string, BuildingDefinition> buildingsById =
        new Dictionary<string, BuildingDefinition>(StringComparer.Ordinal);

    public IEnumerable<BuildingDefinition> BuildingDefinitions =>
buildingsById.Values;

    private readonly Dictionary<string, ResourceDefinition> resourcesById =
        new Dictionary<string, ResourceDefinition>(StringComparer.Ordinal);
    public IEnumerable<ResourceDefinition> ResourceDefinitions =>
    resourcesById.Values;

    public int BuildingCount => buildingsById.Count;
    public int ResourceCount => resourcesById.Count;

    private void Awake()
    {
        Reload();
    }

    public void Reload()
    {
        buildingsById.Clear();
        resourcesById.Clear();

        BuildingDefinition[] buildings =
            Resources.LoadAll<BuildingDefinition>(buildingResourcesFolder);

        foreach (BuildingDefinition building in buildings)
        {
            if (building == null || string.IsNullOrWhiteSpace(building.buildingId))
                continue;

            string id = building.buildingId.Trim();

            if (!buildingsById.TryAdd(id, building))
                Debug.LogError($"Duplicate buildingId: {id}");
        }

        ResourceDefinition[] resources =
            Resources.LoadAll<ResourceDefinition>(resourceResourcesFolder);

        foreach (ResourceDefinition resource in resources)
        {
            if (resource == null || string.IsNullOrWhiteSpace(resource.resourceId))
                continue;

            string id = resource.resourceId.Trim();

            if (!resourcesById.TryAdd(id, resource))
                Debug.LogError($"Duplicate resourceId: {id}");
        }
    }

    public BuildingDefinition FindBuilding(string buildingId)
    {
        if (string.IsNullOrWhiteSpace(buildingId))
            return null;

        buildingsById.TryGetValue(buildingId.Trim(), out BuildingDefinition result);
        return result;
    }

    public ResourceDefinition FindResource(string resourceId)
    {
        if (string.IsNullOrWhiteSpace(resourceId))
            return null;

        resourcesById.TryGetValue(resourceId.Trim(), out ResourceDefinition result);
        return result;
    }

    [ContextMenu("Validate Definitions")]
    private void ValidateDefinitions()
    {
        Reload();

        Debug.Log(
            $"Definitions validated: " +
            $"Buildings={BuildingCount}, Resources={ResourceCount}"
        );
    }
}