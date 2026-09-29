using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class GameDataTableSchema
{
    private const BindingFlags FieldFlags =
        BindingFlags.Instance |
        BindingFlags.Public |
        BindingFlags.NonPublic;

    public Type DefinitionType { get; }
    public string FileName { get; }
    public string AssetFolder { get; }
    public string IdField { get; }

    public IReadOnlyDictionary<string, string> ColumnAliases =>
        columnAliases;

    private readonly Dictionary<string, string> columnAliases;

    private GameDataTableSchema(
        Type definitionType,
        string fileName,
        string assetFolder,
        string idField,
        Dictionary<string, string> columnAliases
    )
    {
        DefinitionType = definitionType;
        FileName = fileName;
        AssetFolder = assetFolder;
        IdField = idField;
        this.columnAliases = columnAliases;
    }

    public string ResolveFieldPath(string header)
    {
        if (columnAliases.TryGetValue(header, out string fieldPath))
        {
            return fieldPath;
        }

        return header;
    }

    public static GameDataTableSchema Create(Type definitionType)
    {
        if (definitionType == null)
        {
            throw new ArgumentNullException(nameof(definitionType));
        }

        if (!typeof(ScriptableObject).IsAssignableFrom(definitionType))
        {
            throw new InvalidOperationException(
                $"{definitionType.Name} is not a ScriptableObject."
            );
        }

        GameDataTableAttribute tableAttribute =
            definitionType.GetCustomAttribute<GameDataTableAttribute>();

        if (tableAttribute == null)
        {
            throw new InvalidOperationException(
                $"{definitionType.Name} does not have GameDataTableAttribute."
            );
        }

        if (!HasFieldPath(definitionType, tableAttribute.IdField))
        {
            throw new InvalidOperationException(
                $"ID field '{tableAttribute.IdField}' was not found " +
                $"on {definitionType.Name}."
            );
        }

        Dictionary<string, string> aliases =
            new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase
            );

        IEnumerable<GameDataColumnAliasAttribute> aliasAttributes =
    definitionType.GetCustomAttributes<
        GameDataColumnAliasAttribute
    >();

        foreach (GameDataColumnAliasAttribute alias in aliasAttributes)
        {
            if (!HasFieldPath(definitionType, alias.FieldPath))
            {
                throw new InvalidOperationException(
                    $"Field path '{alias.FieldPath}' was not found " +
                    $"on {definitionType.Name}."
                );
            }

            if (!aliases.TryAdd(alias.Header, alias.FieldPath))
            {
                throw new InvalidOperationException(
                    $"Duplicate TSV header alias '{alias.Header}' " +
                    $"on {definitionType.Name}."
                );
            }
        }

        return new GameDataTableSchema(
            definitionType,
            tableAttribute.FileName,
            tableAttribute.AssetFolder,
            tableAttribute.IdField,
            aliases
        );
    }

    private static bool HasFieldPath(
        Type rootType,
        string fieldPath
    )
    {
        if (string.IsNullOrWhiteSpace(fieldPath))
        {
            return false;
        }

        Type currentType = rootType;
        string[] fieldNames = fieldPath.Split('.');

        foreach (string fieldName in fieldNames)
        {
            FieldInfo field = currentType.GetField(
                fieldName,
                FieldFlags
            );

            if (field == null)
            {
                return false;
            }

            currentType = field.FieldType;
        }

        return true;
    }
}