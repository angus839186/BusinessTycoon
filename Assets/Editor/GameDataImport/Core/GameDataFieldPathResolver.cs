using System;
using System.Reflection;

public static class GameDataFieldPathResolver
{
    private const BindingFlags FieldFlags =
        BindingFlags.Instance |
        BindingFlags.Public |
        BindingFlags.NonPublic;

    public static bool TryResolve(
        Type rootType,
        string fieldPath,
        out FieldInfo field,
        out string error
    )
    {
        field = null;
        error = string.Empty;

        if (rootType == null)
        {
            error = "Root type is null.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(fieldPath))
        {
            error = "Field path is empty.";
            return false;
        }

        Type currentType = rootType;
        string[] fieldNames = fieldPath.Split('.');

        foreach (string fieldName in fieldNames)
        {
            field = currentType.GetField(
                fieldName,
                FieldFlags
            );

            if (field == null)
            {
                error =
                    $"Field '{fieldName}' was not found on " +
                    $"{currentType.Name}.";

                return false;
            }

            currentType = field.FieldType;
        }

        return true;
    }
}