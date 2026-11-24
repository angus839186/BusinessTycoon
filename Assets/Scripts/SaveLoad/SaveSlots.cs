using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

public static partial class SaveSlots
{
    public const string DefaultId = "slot_1";
    private const string FolderName = "Saves";
    public const string AutoSaveId = "autosave";

    private static readonly string[] slotIds =
    {
        "slot_1",
        "slot_2",
        "slot_3"
    };

    public static IReadOnlyList<string> Ids => slotIds;
    public static string FolderPath =>
        Path.Combine(Application.persistentDataPath, FolderName);

    public static string GetPath(string slotId)
    {
        if (!IsValidId(slotId))
        {
            throw new ArgumentException(
                $"Invalid save slot id: {slotId}",
                nameof(slotId)
            );
        }

        return Path.Combine(
            FolderPath,
            $"{slotId}.json"
        );
    }

    public static bool Exists(string slotId)
    {
        return File.Exists(GetPath(slotId));
    }

    public static bool TryDelete(
    string slotId,
    out string errorMessage
)
    {
        errorMessage = string.Empty;

        if (!IsKnownId(slotId))
        {
            errorMessage =
                $"未知的存檔槽：{slotId}";

            return false;
        }



        string path = GetPath(slotId);

        if (!File.Exists(path))
        {
            errorMessage = "存檔不存在。";
            return false;
        }

        try
        {
            File.Delete(path);

            if (File.Exists(path))
            {
                errorMessage = "存檔刪除失敗。";
                return false;
            }

            return true;
        }
        catch (Exception exception)
        {
            errorMessage = exception.Message;
            return false;
        }
    }

    public static void EnsureFolder()
    {
        Directory.CreateDirectory(FolderPath);
    }

    private static bool IsKnownId(string slotId)
    {
        if (string.IsNullOrWhiteSpace(slotId))
        {
            return false;
        }

        if (string.Equals(slotId, AutoSaveId, StringComparison.Ordinal))
        {
            return true;
        }

        foreach (string knownId in slotIds)
        {
            if (
                string.Equals(
                    knownId,
                    slotId,
                    StringComparison.Ordinal
                )
            )
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsValidId(string slotId)
    {
        if (string.IsNullOrWhiteSpace(slotId))
            return false;

        foreach (char character in slotId)
        {
            bool isLowercaseLetter =
                character >= 'a' && character <= 'z';

            bool isUppercaseLetter =
                character >= 'A' && character <= 'Z';

            bool isNumber =
                character >= '0' && character <= '9';

            bool isSeparator =
                character == '_' || character == '-';

            if (
                !isLowercaseLetter &&
                !isUppercaseLetter &&
                !isNumber &&
                !isSeparator
            )
            {
                return false;
            }
        }

        return true;
    }
}