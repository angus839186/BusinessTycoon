using System;
using UnityEditor;
using UnityEngine;

public class GameDataTableEditorPanel
{
    private const float ColumnWidth = 140f;

    private readonly Type definitionType;

    private GameDataTableSchema schema;
    private GameDataTsvTable table;
    private GameDataValidationResult validationResult;
    private GameDataImportResult importResult;

    private Vector2 scrollPosition;
    private string loadError;

    public bool IsLoaded => table != null;

    public bool IsValidated =>
        validationResult != null;

    public bool IsValid =>
        validationResult != null &&
        validationResult.IsValid;

    public int RowCount =>
        table?.Rows.Count ?? 0;

    public int ErrorCount =>
        validationResult?.ErrorCount ?? 0;

    public GameDataTableEditorPanel(Type definitionType)
    {
        this.definitionType = definitionType;
    }

    public void Validate()
    {
        if (table == null)
        {
            Reload();
        }

        if (table != null)
        {
            ValidateTable();
        }
    }

    public void Draw()
    {
        if (table == null && string.IsNullOrEmpty(loadError))
        {
            Reload();
        }

        DrawToolbar();

        if (!string.IsNullOrEmpty(loadError))
        {
            EditorGUILayout.HelpBox(
                loadError,
                MessageType.Error
            );

            return;
        }

        DrawStatus();
        DrawImportResult();
        DrawTable();
    }

    public void Reload()
    {
        LoadTable();
    }

    private void LoadTable()
    {
        try
        {
            schema =
                GameDataTableSchema.Create(definitionType);

            string sourcePath =
                $"Assets/GameData/Source/{schema.FileName}";

            table = GameDataTsvReader.Read(sourcePath);
            importResult = null;

            validationResult =
                GameDataTableValidator.Validate(schema, table);

            loadError = string.Empty;
        }
        catch (Exception exception)
        {
            table = null;
            validationResult = null;
            loadError = exception.Message;
        }
    }

    private void DrawToolbar()
    {
        bool hasTable = table != null;

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        if (GUILayout.Button(
            "Reload",
            EditorStyles.toolbarButton
        ))
        {
            Reload();
        }

        using (new EditorGUI.DisabledScope(!hasTable))
        {
            if (GUILayout.Button(
                "Validate",
                EditorStyles.toolbarButton
            ))
            {
                ValidateTable();
            }
        }

        bool canImport =
            hasTable &&
            validationResult != null &&
            validationResult.IsValid;

        using (new EditorGUI.DisabledScope(!canImport))
        {
            if (GUILayout.Button(
                "Import SO",
                EditorStyles.toolbarButton
            ))
            {
                TryImport(true);
            }
        }

        GUILayout.FlexibleSpace();

        if (hasTable)
        {
            GUILayout.Label(
                "Read Only",
                EditorStyles.miniLabel
            );

            GUILayout.Label(
                $"Rows: {table.Rows.Count}",
                EditorStyles.miniLabel
            );
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawStatus()
    {
        if (validationResult == null)
        {
            return;
        }

        if (validationResult.IsValid)
        {
            EditorGUILayout.HelpBox(
                "Validation passed.",
                MessageType.Info
            );

            return;
        }

        EditorGUILayout.HelpBox(
            $"Errors: {validationResult.ErrorCount}, " +
            $"Warnings: {validationResult.WarningCount}",
            MessageType.Error
        );

        foreach (GameDataValidationMessage message
                 in validationResult.Messages)
        {
            MessageType messageType =
                message.Severity ==
                GameDataValidationSeverity.Error
                    ? MessageType.Error
                    : MessageType.Warning;

            EditorGUILayout.HelpBox(
                message.ToString(),
                messageType
            );
        }
    }

    private void DrawTable()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(
            scrollPosition,
            true,
            true,
            GUILayout.Height(220f)
        );

        EditorGUILayout.BeginHorizontal(
            EditorStyles.toolbar
        );

        foreach (string header in table.Headers)
        {
            GUILayout.Label(
                header,
                GUILayout.Width(ColumnWidth)
            );
        }

        EditorGUILayout.EndHorizontal();

        for (int rowIndex = 0;
             rowIndex < table.Rows.Count;
             rowIndex++)
        {
            DrawRow(rowIndex);
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawRow(int rowIndex)
    {
        GameDataTsvRow row = table.Rows[rowIndex];

        EditorGUILayout.BeginHorizontal();

        foreach (string header in table.Headers)
        {
            EditorGUILayout.SelectableLabel(
                row.GetValue(header),
                EditorStyles.textField,
                GUILayout.Width(ColumnWidth),
                GUILayout.Height(
                    EditorGUIUtility.singleLineHeight
                )
            );
        }

        EditorGUILayout.EndHorizontal();
    }

    private void ValidateTable()
    {
        validationResult =
            GameDataTableValidator.Validate(schema, table);
    }


    public bool TryImport(bool requireConfirmation)
    {
        ValidateTable();

        if (!validationResult.IsValid)
        {
            EditorUtility.DisplayDialog(
                "Cannot Import",
                "Fix validation errors before importing.",
                "OK"
            );

            return false;
        }

        if (requireConfirmation)
        {
            bool confirmed = EditorUtility.DisplayDialog(
                "Import ScriptableObjects",
                $"Create or update {table.Rows.Count} " +
                $"{schema.DefinitionType.Name} assets?\n\n" +
                "Assets missing from the TSV will not be deleted.",
                "Import",
                "Cancel"
            );

            if (!confirmed)
            {
                return false;
            }
        }
        importResult =
            GameDataScriptableObjectImporter.Import(
                schema,
                table
            );

        if (importResult.IsSuccess)
        {
            Debug.Log(
                $"Import completed. Created: " +
                $"{importResult.CreatedCount}, Updated: " +
                $"{importResult.UpdatedCount}"
            );
        }
        return importResult.IsSuccess;
    }
    private void DrawImportResult()
    {
        if (importResult == null)
        {
            return;
        }

        MessageType summaryType;

        if (!importResult.IsSuccess)
        {
            summaryType = MessageType.Error;
        }
        else if (importResult.WarningCount > 0)
        {
            summaryType = MessageType.Warning;
        }
        else
        {
            summaryType = MessageType.Info;
        }

        EditorGUILayout.HelpBox(
    $"Created: {importResult.CreatedCount}, " +
    $"Updated: {importResult.UpdatedCount}, " +
    $"Warnings: {importResult.WarningCount}, " +
    $"Errors: {importResult.Errors.Count}",
    summaryType
);

        foreach (string message in importResult.Messages)
        {
            EditorGUILayout.HelpBox(
                message,
                MessageType.Info
            );
        }
        foreach (string warning in importResult.Warnings)
        {
            EditorGUILayout.HelpBox(
                warning,
                MessageType.Warning
            );
        }

        foreach (string error in importResult.Errors)
        {
            EditorGUILayout.HelpBox(
                error,
                MessageType.Error
            );
        }
    }
}