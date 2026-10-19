using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class DataImporterWindow : EditorWindow
{
    private int selectedTab;
    private readonly List<IDataPanel> panels =
    new List<IDataPanel>();

    private string[] tabNames =
    new string[0];

    [MenuItem("Tools/Game Data Importer")]
    private static void OpenWindow()
    {
        DataImporterWindow window =
            GetWindow<DataImporterWindow>();

        window.titleContent = new GUIContent("Game Data Importer");
        window.minSize = new Vector2(620f, 420f);
    }

    private void OnEnable()
    {
        panels.Clear();

        foreach (
            System.Type definitionType
            in DefinitionRegistry.FindAll()
        )
        {
            panels.Add(
                new DataTablePanel(definitionType)
            );
        }

        panels.Add(
            new RecipePanel()
        );

        panels.Sort(ComparePanels);

        tabNames =
            new string[panels.Count];

        for (int index = 0;
             index < panels.Count;
             index++)
        {
            tabNames[index] =
                panels[index].DisplayName;
        }

        selectedTab = Mathf.Clamp(
            selectedTab,
            0,
            Mathf.Max(0, panels.Count - 1)
        );
    }

    private static int ComparePanels(
    IDataPanel left,
    IDataPanel right
)
    {
        int orderComparison =
            left.ImportOrder.CompareTo(
                right.ImportOrder
            );

        if (orderComparison != 0)
        {
            return orderComparison;
        }

        return string.Compare(
            left.DisplayName,
            right.DisplayName,
            System.StringComparison.OrdinalIgnoreCase
        );
    }
    private void OnGUI()
    {
        EditorGUILayout.LabelField(
            "Game Data Importer",
            EditorStyles.boldLabel
        );

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button(
            "Validate All",
            GUILayout.Height(32f)
        ))
        {
            ValidateAll();
        }

        using (new EditorGUI.DisabledScope(
    !AreAllPanelsValid()
))
        {
            if (GUILayout.Button(
                "Import All",
                GUILayout.Height(32f)
            ))
            {
                ImportAll();
            }
        }

        EditorGUILayout.EndHorizontal();

        DrawValidationSummary();

        EditorGUILayout.Space(8f);

        if (panels.Count == 0)
        {
            return;
        }

        selectedTab = Mathf.Clamp(
            selectedTab,
            0,
            panels.Count - 1
        );

        selectedTab = GUILayout.Toolbar(
            selectedTab,
            tabNames,
            GUILayout.Height(26f)
        );

        EditorGUILayout.Space(8f);

        DrawSelectedPanel();


    }

    private void DrawSelectedPanel()
    {
        if (selectedTab < 0 ||
            selectedTab >= panels.Count)
        {
            return;
        }

        IDataPanel panel =
            panels[selectedTab];

        EditorGUILayout.LabelField(
            $"{panel.DisplayName} Data",
            EditorStyles.boldLabel
        );

        DrawSourceFileRow(
            panel.FilePath
        );

        EditorGUILayout.Space(8f);

        panel.Draw();
    }



    private void DrawValidationSummary()
    {
        if (panels.Count == 0)
        {
            EditorGUILayout.HelpBox(
                "No game data definitions were found.",
                MessageType.Warning
            );

            return;
        }

        EditorGUILayout.BeginHorizontal();

        foreach (IDataPanel panel in panels)
        {
            DrawDataStatus(
                panel.DisplayName,
                panel.IsValidated,
                panel.RowCount,
                panel.ErrorCount
            );
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawDataStatus(
        string label,
        bool validated,
        int rowCount,
        int errorCount
    )
    {
        string status;

        if (!validated)
            status = "Not validated";
        else if (errorCount > 0)
            status = $"Errors: {errorCount}";
        else
            status = $"Valid: {rowCount}";

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        EditorGUILayout.LabelField(status);
        EditorGUILayout.EndVertical();
    }

    private bool AreAllPanelsValid()
    {
        if (panels.Count == 0)
        {
            return false;
        }

        foreach (IDataPanel panel in panels)
        {
            if (!panel.IsValid)
            {
                return false;
            }
        }

        return true;
    }

    private void ValidateAll()
    {
        int firstInvalidPanel = -1;

        for (int index = 0;
             index < panels.Count;
             index++)
        {
            IDataPanel panel = panels[index];

            panel.Validate();

            if (!panel.IsValid &&
                firstInvalidPanel < 0)
            {
                firstInvalidPanel = index;
            }
        }

        if (firstInvalidPanel >= 0)
        {
            selectedTab = firstInvalidPanel;
        }

        Repaint();
    }

    private void ImportAll()
    {
        ValidateAll();

        if (!AreAllPanelsValid())
        {
            return;
        }

        string confirmationMessage =
            BuildImportConfirmationMessage();

        bool confirmed =
            EditorUtility.DisplayDialog(
                "Import All Game Data",
                confirmationMessage,
                "Import All",
                "Cancel"
            );

        if (!confirmed)
        {
            return;
        }

        for (int index = 0;
             index < panels.Count;
             index++)
        {
            IDataPanel panel = panels[index];

            if (panel.TryImport(false))
            {
                continue;
            }

            selectedTab = index;
            Repaint();
            return;
        }

        AssetDatabase.SaveAssets();
        Repaint();
    }

    private string BuildImportConfirmationMessage()
    {
        System.Text.StringBuilder message =
            new System.Text.StringBuilder();

        message.AppendLine("即將匯入：");

        foreach (IDataPanel panel in panels)
        {
            message.AppendLine(
                $"{panel.DisplayName}：" +
                $"{panel.RowCount} 筆"
            );
        }

        message.AppendLine();
        message.Append("順序：");

        for (int index = 0;
             index < panels.Count;
             index++)
        {
            if (index > 0)
            {
                message.Append(" → ");
            }

            message.Append(
                panels[index].DisplayName
            );
        }

        return message.ToString();
    }
    private void DrawSourceFileRow(string assetPath)
    {
        EditorGUILayout.LabelField("Source File");

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.SelectableLabel(
            assetPath,
            EditorStyles.textField,
            GUILayout.Height(EditorGUIUtility.singleLineHeight)
        );

        if (GUILayout.Button("Locate", GUILayout.Width(80f)))
            LocateFile(assetPath);

        EditorGUILayout.EndHorizontal();
    }

    private void LocateFile(string assetPath)
    {
        Object asset = AssetDatabase.LoadMainAssetAtPath(assetPath);

        if (asset == null)
        {
            ShowNotification(
                new GUIContent($"File not found: {assetPath}")
            );

            return;
        }

        Selection.activeObject = asset;
        EditorGUIUtility.PingObject(asset);
    }

}