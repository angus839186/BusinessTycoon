using UnityEngine;
using System.Collections.Generic;

public sealed partial class BuildingPlacementController : MonoBehaviour
{
    public string ExportPlacedBuildingsJson()
    {
        PlacedBuildingSaveData saveData = new PlacedBuildingSaveData();

        foreach (PlacedBuildingData placedBuilding in placedBuildings)
            saveData.buildings.Add(placedBuilding);

        return JsonUtility.ToJson(saveData, true);
    }
    public void DebugPrintPlacedBuildingsJson()
    {
        string json = ExportPlacedBuildingsJson();
        Debug.Log(json);
    }
    private string GetSavePath()
    {
        return System.IO.Path.Combine(
            Application.persistentDataPath,
            saveFileName
        );
    }
    public void SavePlacedBuildings()
    {
        PlacedBuildingSaveData saveData = new PlacedBuildingSaveData();

        foreach (PlacedBuildingData placedBuilding in placedBuildings)
            saveData.buildings.Add(placedBuilding);

        string json = JsonUtility.ToJson(saveData, true);
        string savePath = GetSavePath();

        System.IO.File.WriteAllText(savePath, json);

        Debug.Log($"Placed buildings saved: {savePath}");
    }
    public bool CanPlaceSelectedBuilding()
    {
        return isPlacementMode && selectedBuilding != null;
    }

}