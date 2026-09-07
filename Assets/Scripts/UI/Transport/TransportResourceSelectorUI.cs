using System.Collections.Generic;
using TMPro;
using UnityEngine;

public sealed class TransportResourceSelectorUI : MonoBehaviour
{
    [SerializeField] private TransportRouteBuilder routeBuilder;
    [SerializeField] private TMP_Dropdown dropdown;
    [SerializeField] private string resourcesFolder = "Resource";

    private readonly List<ResourceDefinition> resources = new List<ResourceDefinition>();

    private void Start()
    {
        LoadOptions();
    }

    private void LoadOptions()
    {
        if (routeBuilder == null || dropdown == null)
        {
            Debug.LogError("TransportResourceSelectorUI missing references.");
            return;
        }

        ResourceDefinition[] loadedResources =
            Resources.LoadAll<ResourceDefinition>(resourcesFolder);

        resources.Clear();
        dropdown.ClearOptions();

        List<string> labels = new List<string>();

        foreach (ResourceDefinition resource in loadedResources)
        {
            if (resource == null)
                continue;

            resources.Add(resource);
            labels.Add(string.IsNullOrWhiteSpace(resource.displayName)
                ? resource.name
                : resource.displayName);
        }

        dropdown.AddOptions(labels);
        dropdown.onValueChanged.AddListener(OnDropdownChanged);

        if (resources.Count > 0)
            routeBuilder.SetSelectedResource(resources[dropdown.value]);
    }

    private void OnDropdownChanged(int index)
    {
        if (index < 0 || index >= resources.Count)
            return;

        routeBuilder.SetSelectedResource(resources[index]);
    }
}