using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RecipeTablePanel
{
    private const string SourcePath =
        "Assets/GameData/Source/Recipes.tsv";

    private List<RecipeImportRow> rows =
        new List<RecipeImportRow>();

    private List<string> errors =
        new List<string>();

    private List<string> importMessages =
        new List<string>();

    private Vector2 scrollPosition;
    private bool hasValidated;

    public bool IsValidated => hasValidated;

    public bool IsValid =>
        hasValidated &&
        errors.Count == 0;

    public int RowCount => rows.Count;
    public int ErrorCount => errors.Count;

    public void Draw()
    {
        DrawToolbar();
        DrawStatus();

        if (!hasValidated)
        {
            return;
        }

        DrawImportMessages();

        if (errors.Count > 0)
        {
            DrawErrors();
            return;
        }

        DrawTable();
    }

    public void Validate()
    {
        importMessages.Clear();

        RecipeTsvValidator.Validate(
            SourcePath,
            out rows,
            out errors
        );

        hasValidated = true;
    }

    public bool TryImport(bool requireConfirmation)
    {
        Validate();

        if (!IsValid)
        {
            return false;
        }

        if (requireConfirmation)
        {
            bool confirmed = EditorUtility.DisplayDialog(
                "Import Recipes",
                $"Import {rows.Count} recipe rows?\n\n" +
                "Resource recipes will be replaced by TSV data.",
                "Import",
                "Cancel"
            );

            if (!confirmed)
            {
                return false;
            }
        }

        bool succeeded = RecipeDefinitionImporter.Import(
            rows,
            out importMessages,
            out List<string> importErrors
        );

        if (!succeeded)
        {
            errors.AddRange(importErrors);
        }

        return succeeded;
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(
            EditorStyles.toolbar
        );

        if (GUILayout.Button(
            "Validate",
            EditorStyles.toolbarButton
        ))
        {
            Validate();
        }

        using (new EditorGUI.DisabledScope(!IsValid))
        {
            if (GUILayout.Button(
                "Import Recipes",
                EditorStyles.toolbarButton
            ))
            {
                TryImport(true);
            }
        }

        GUILayout.FlexibleSpace();

        if (hasValidated)
        {
            GUILayout.Label(
                "Read Only",
                EditorStyles.miniLabel
            );

            GUILayout.Label(
                $"Rows: {rows.Count}",
                EditorStyles.miniLabel
            );
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawStatus()
    {
        if (!hasValidated)
        {
            EditorGUILayout.HelpBox(
                "Validate Recipes to load the TSV preview.",
                MessageType.Info
            );

            return;
        }

        if (errors.Count == 0)
        {
            EditorGUILayout.HelpBox(
                $"Validation passed. Rows: {rows.Count}",
                MessageType.Info
            );

            return;
        }

        EditorGUILayout.HelpBox(
            $"Validation failed. Errors: {errors.Count}",
            MessageType.Error
        );
    }

    private void DrawImportMessages()
    {
        foreach (string message in importMessages)
        {
            EditorGUILayout.HelpBox(
                message,
                MessageType.Info
            );
        }
    }

    private void DrawErrors()
    {
        scrollPosition =
            EditorGUILayout.BeginScrollView(
                scrollPosition,
                GUILayout.Height(220f)
            );

        foreach (string error in errors)
        {
            EditorGUILayout.HelpBox(
                error,
                MessageType.Error
            );
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawTable()
    {
        scrollPosition =
            EditorGUILayout.BeginScrollView(
                scrollPosition,
                true,
                true,
                GUILayout.Height(220f)
            );

        EditorGUILayout.BeginHorizontal(
            EditorStyles.toolbar
        );

        GUILayout.Label(
            "Owner Resource",
            GUILayout.Width(150f)
        );

        GUILayout.Label(
            "Type",
            GUILayout.Width(80f)
        );

        GUILayout.Label(
            "Resource",
            GUILayout.Width(150f)
        );

        GUILayout.Label(
            "Amount",
            GUILayout.Width(70f)
        );

        GUILayout.Label(
            "Enabled",
            GUILayout.Width(70f)
        );

        EditorGUILayout.EndHorizontal();

        foreach (RecipeImportRow row in rows)
        {
            DrawRow(row);
        }

        EditorGUILayout.EndScrollView();
    }

    private static void DrawRow(RecipeImportRow row)
    {
        EditorGUILayout.BeginHorizontal();

        DrawCell(row.OwnerResourceId, 150f);
        DrawCell(row.IoType.ToString(), 80f);
        DrawCell(row.ResourceId, 150f);
        DrawCell(row.Amount.ToString(), 70f);
        DrawCell(row.Enabled ? "Yes" : "No", 70f);

        EditorGUILayout.EndHorizontal();
    }

    private static void DrawCell(
        string value,
        float width
    )
    {
        EditorGUILayout.SelectableLabel(
            value,
            EditorStyles.textField,
            GUILayout.Width(width),
            GUILayout.Height(
                EditorGUIUtility.singleLineHeight
            )
        );
    }
}