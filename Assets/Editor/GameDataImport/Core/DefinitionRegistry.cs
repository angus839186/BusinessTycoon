using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class DefinitionRegistry
{
    public static IReadOnlyList<Type> FindAll()
    {
        List<Type> definitionTypes =
            new List<Type>();

        foreach (
            Type type in
            TypeCache.GetTypesWithAttribute<
                GameDataTableAttribute
            >()
        )
        {
            if (type.IsAbstract)
            {
                continue;
            }

            if (!typeof(ScriptableObject)
                .IsAssignableFrom(type))
            {
                continue;
            }

            definitionTypes.Add(type);
        }

        definitionTypes.Sort(CompareTypes);

        return definitionTypes;
    }

    private static int CompareTypes(
        Type left,
        Type right
    )
    {
        GameDataTableAttribute leftAttribute =
            left.GetCustomAttribute<
                GameDataTableAttribute
            >();

        GameDataTableAttribute rightAttribute =
            right.GetCustomAttribute<
                GameDataTableAttribute
            >();

        int orderComparison =
            leftAttribute.ImportOrder.CompareTo(
                rightAttribute.ImportOrder
            );

        if (orderComparison != 0)
        {
            return orderComparison;
        }

        string leftName = GetDisplayName(
            left,
            leftAttribute
        );

        string rightName = GetDisplayName(
            right,
            rightAttribute
        );

        return string.Compare(
            leftName,
            rightName,
            StringComparison.OrdinalIgnoreCase
        );
    }

    private static string GetDisplayName(
        Type type,
        GameDataTableAttribute attribute
    )
    {
        if (!string.IsNullOrWhiteSpace(
            attribute.DisplayName
        ))
        {
            return attribute.DisplayName;
        }

        return type.Name;
    }
}