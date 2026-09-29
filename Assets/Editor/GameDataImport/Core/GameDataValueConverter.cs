using System;
using System.Globalization;
using UnityEditor;
using UnityEngine;

public static class GameDataValueConverter
{
    public static bool TryConvert(
        string rawValue,
        Type targetType,
        out object convertedValue,
        out string error
    )
    {
        convertedValue = null;
        error = string.Empty;

        if (targetType == typeof(string))
        {
            convertedValue = rawValue;
            return true;
        }

        if (targetType == typeof(int))
        {
            if (int.TryParse(
                rawValue,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out int intValue
            ))
            {
                convertedValue = intValue;
                return true;
            }

            error = $"'{rawValue}' is not a valid integer.";
            return false;
        }

        if (targetType == typeof(float))
        {
            if (float.TryParse(
                rawValue,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out float floatValue
            ))
            {
                convertedValue = floatValue;
                return true;
            }

            error = $"'{rawValue}' is not a valid float.";
            return false;
        }

        if (targetType == typeof(bool))
        {
            if (bool.TryParse(rawValue, out bool boolValue))
            {
                convertedValue = boolValue;
                return true;
            }

            if (rawValue == "1" || rawValue == "0")
            {
                convertedValue = rawValue == "1";
                return true;
            }

            error = $"'{rawValue}' is not a valid boolean.";
            return false;
        }

        if (targetType.IsEnum)
        {
            if (Enum.TryParse(
                targetType,
                rawValue,
                true,
                out object enumValue
            ))
            {
                convertedValue = enumValue;
                return true;
            }

            error =
                $"'{rawValue}' is not valid for " +
                $"{targetType.Name}.";

            return false;
        }

        if (targetType == typeof(Color))
        {
            if (ColorUtility.TryParseHtmlString(
                rawValue,
                out Color color
            ))
            {
                convertedValue = color;
                return true;
            }

            error = $"'{rawValue}' is not a valid HTML color.";
            return false;
        }

        if (targetType == typeof(Sprite))
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                convertedValue = null;
                return true;
            }

            Sprite sprite =
                AssetDatabase.LoadAssetAtPath<Sprite>(rawValue);

            if (sprite != null)
            {
                convertedValue = sprite;
                return true;
            }

            error = $"Sprite was not found at '{rawValue}'.";
            return false;
        }

        if (typeof(ScriptableObject).IsAssignableFrom(targetType))
        {
            if (GameDataObjectReferenceResolver.TryResolve(
                targetType,
                rawValue,
                out UnityEngine.Object asset,
                out error
            ))
            {
                convertedValue = asset;
                return true;
            }

            return false;
        }

        error =
            $"Type '{targetType.Name}' is not supported.";

        return false;
    }
}