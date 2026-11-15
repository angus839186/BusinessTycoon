using System;
using System.IO;
using UnityEngine;

public static class SaveSlots
{
    public const string DefaultId = "slot_1";

    private const string FolderName = "Saves";

    public static string FolderPath =>
        Path.Combine(
            Application.persistentDataPath,
            FolderName
        );

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

    public static void EnsureFolder()
    {
        Directory.CreateDirectory(FolderPath);
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