using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class GameDataSerializedPropertyWriter
{
    public static bool TryWrite(
        SerializedObject serializedObject,
        GameDataTableSchema schema,
        string header,
        string rawValue,
        out string error
    )
    {
        error = string.Empty;

        string fieldPath =
            schema.ResolveFieldPath(header);

        if (!GameDataFieldPathResolver.TryResolve(
            schema.DefinitionType,
            fieldPath,
            out FieldInfo field,
            out error
        ))
        {
            return false;
        }

        SerializedProperty property =
            serializedObject.FindProperty(fieldPath);

        if (property == null)
        {
            error =
                $"Serialized property '{fieldPath}' " +
                "was not found.";

            return false;
        }

        if (field.FieldType == typeof(Sprite) &&
    string.IsNullOrWhiteSpace(rawValue))
        {
            // Blank icon paths preserve existing icons.
            return true;
        }

        if (!GameDataValueConverter.TryConvert(
            rawValue,
            field.FieldType,
            out object convertedValue,
            out error
        ))
        {
            return false;
        }

        return TryAssign(
            property,
            field.FieldType,
            convertedValue,
            out error
        );
    }

    private static bool TryAssign(
        SerializedProperty property,
        Type fieldType,
        object value,
        out string error
    )
    {
        error = string.Empty;

        switch (property.propertyType)
        {
            case SerializedPropertyType.String:
                property.stringValue = (string)value;
                return true;

            case SerializedPropertyType.Integer:
                property.intValue = Convert.ToInt32(value);
                return true;

            case SerializedPropertyType.Boolean:
                property.boolValue = (bool)value;
                return true;

            case SerializedPropertyType.Float:
                property.floatValue = Convert.ToSingle(value);
                return true;

            case SerializedPropertyType.Color:
                property.colorValue = (Color)value;
                return true;

            case SerializedPropertyType.Enum:
                return TryAssignEnum(
                    property,
                    fieldType,
                    value,
                    out error
                );

            case SerializedPropertyType.ObjectReference:
                property.objectReferenceValue =
                    value as UnityEngine.Object;

                return true;

            default:
                error =
                    $"Serialized property type " +
                    $"'{property.propertyType}' is not supported.";

                return false;
        }
    }

    private static bool TryAssignEnum(
        SerializedProperty property,
        Type enumType,
        object value,
        out string error
    )
    {
        Array enumValues = Enum.GetValues(enumType);
        int index = Array.IndexOf(enumValues, value);

        if (index < 0)
        {
            error =
                $"Enum value '{value}' was not found " +
                $"in {enumType.Name}.";

            return false;
        }

        property.enumValueIndex = index;
        error = string.Empty;

        return true;
    }
}