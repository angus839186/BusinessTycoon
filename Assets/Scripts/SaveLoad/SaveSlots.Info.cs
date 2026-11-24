using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public static partial class SaveSlots
{
    public static SaveSlotInfo GetInfo(string slotId)
    {
        string path = GetPath(slotId);

        SaveSlotInfo info = new SaveSlotInfo
        {
            SlotId = slotId,
            FilePath = path,
            Exists = File.Exists(path),
            CanLoad = false
        };

        if (!info.Exists)
            return info;

        try
        {
            string json =
                File.ReadAllText(path, Encoding.UTF8);

            GameSaveData saveData =
                JsonUtility.FromJson<GameSaveData>(json);

            if (saveData == null)
            {
                info.ErrorMessage =
                    "Save data is empty.";

                return info;
            }

            info.SavedAt =
                File.GetLastWriteTime(path);

            info.TotalGameMinutes =
                Math.Max(0d, saveData.totalGameMinutes);

            info.SaveVersion =
                saveData.saveVersion;

            info.CanLoad =
                GameSaveSystem.CanLoadVersion(
                    saveData.saveVersion
                );

            if (!info.CanLoad)
            {
                info.ErrorMessage =
                    $"Unsupported save version: " +
                    $"{saveData.saveVersion}";
            }
        }
        catch (Exception exception)
        {
            info.ErrorMessage = exception.Message;
        }

        return info;
    }

    public static IReadOnlyList<SaveSlotInfo> GetInfos()
    {
        List<SaveSlotInfo> result =
            new List<SaveSlotInfo>(slotIds.Length);

        foreach (string slotId in slotIds)
        {
            result.Add(GetInfo(slotId));
        }

        return result;
    }

    public static IReadOnlyList<SaveSlotInfo>
    GetLoadInfos()
    {
        List<SaveSlotInfo> result =
            new List<SaveSlotInfo>(
                slotIds.Length + 1
            );

        result.Add(GetInfo(AutoSaveId));

        foreach (string slotId in slotIds)
        {
            result.Add(GetInfo(slotId));
        }

        return result;
    }

    public static bool HasLoadableSave()
    {
        foreach (SaveSlotInfo info in GetLoadInfos())
        {
            if (
                info != null &&
                info.Exists &&
                info.CanLoad &&
                string.IsNullOrWhiteSpace(
                    info.ErrorMessage
                )
            )
            {
                return true;
            }
        }

        return false;
    }
}