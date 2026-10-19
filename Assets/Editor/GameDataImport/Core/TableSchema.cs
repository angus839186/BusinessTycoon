using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class TableSchema
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

    public IReadOnlyList<ColumnSchema> Columns =>
columns;

    private readonly Dictionary<string, string> columnAliases;

    private readonly List<ColumnSchema> columns;

    private TableSchema(
    Type definitionType,
    string fileName,
    string assetFolder,
    string idField,
    Dictionary<string, string> columnAliases,
    List<ColumnSchema> columns
)
    {
        DefinitionType = definitionType;
        FileName = fileName;
        AssetFolder = assetFolder;
        IdField = idField;
        this.columnAliases = columnAliases;
        this.columns = columns;
    }

    public string ResolveFieldPath(string header)
    {
        if (columnAliases.TryGetValue(header, out string fieldPath))
        {
            return fieldPath;
        }

        return header;
    }

    public static TableSchema Create(Type definitionType)
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

        List<ColumnSchema> columns =
    BuildColumns(
        definitionType,
        aliases
    );

        bool idIsImportable =
        columns.Exists(
            column => string.Equals(
                column.FieldPath,
                tableAttribute.IdField,
                StringComparison.OrdinalIgnoreCase
            )
        );

        if (!idIsImportable)
        {
            throw new InvalidOperationException(
                $"ID field '{tableAttribute.IdField}' " +
                $"on {definitionType.Name} must have " +
                $"GameDataColumnAttribute."
            );
        }



        return new TableSchema(
            definitionType,
            tableAttribute.FileName,
            tableAttribute.AssetFolder,
            tableAttribute.IdField,
            aliases,
            columns
        );
    }

    private static List<ColumnSchema> BuildColumns(
    Type definitionType,
    Dictionary<string, string> aliases
)
    {
        List<ColumnSchema> columns =
            new List<ColumnSchema>();

        HashSet<string> addedFieldPaths =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase
            );

        FieldInfo[] rootFields =
            definitionType.GetFields(FieldFlags);

        foreach (FieldInfo field in rootFields)
        {
            GameDataColumnAttribute attribute =
                field.GetCustomAttribute<
                    GameDataColumnAttribute
                >();

            if (attribute == null)
            {
                continue;
            }

            string fieldPath = field.Name;
            string header =
                FindHeader(fieldPath, aliases);

            columns.Add(
                new ColumnSchema(
                    header,
                    fieldPath,
                    field.FieldType,
                    attribute
                )
            );

            addedFieldPaths.Add(fieldPath);
        }

        foreach (
            KeyValuePair<string, string> alias
            in aliases
        )
        {
            if (addedFieldPaths.Contains(alias.Value))
            {
                continue;
            }

            if (!TryGetField(
                definitionType,
                alias.Value,
                out FieldInfo field
            ))
            {
                continue;
            }

            GameDataColumnAttribute attribute =
                field.GetCustomAttribute<
                    GameDataColumnAttribute
                >();

            if (attribute == null)
            {
                continue;
            }

            columns.Add(
                new ColumnSchema(
                    alias.Key,
                    alias.Value,
                    field.FieldType,
                    attribute
                )
            );

            addedFieldPaths.Add(alias.Value);
        }

        return columns;
    }

    private static string FindHeader(
        string fieldPath,
        Dictionary<string, string> aliases
    )
    {
        foreach (
            KeyValuePair<string, string> alias
            in aliases
        )
        {
            if (string.Equals(
                alias.Value,
                fieldPath,
                StringComparison.OrdinalIgnoreCase
            ))
            {
                return alias.Key;
            }
        }

        return fieldPath;
    }

    private static bool HasFieldPath(
    Type rootType,
    string fieldPath
)
    {
        return TryGetField(
            rootType,
            fieldPath,
            out _
        );
    }

    private static bool TryGetField(
        Type rootType,
        string fieldPath,
        out FieldInfo result
    )
    {
        result = null;

        if (rootType == null ||
            string.IsNullOrWhiteSpace(fieldPath))
        {
            return false;
        }

        Type currentType = rootType;
        string[] fieldNames = fieldPath.Split('.');

        foreach (string fieldName in fieldNames)
        {
            result = currentType.GetField(
                fieldName,
                FieldFlags
            );

            if (result == null)
            {
                return false;
            }

            currentType = result.FieldType;
        }

        return true;
    }
}