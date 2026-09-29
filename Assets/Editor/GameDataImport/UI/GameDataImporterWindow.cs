using UnityEditor;
using UnityEngine;

public class GameDataImporterWindow : EditorWindow
{
    private static readonly string[] TabNames =
{
    "Resources",
    "Recipes",
    "Buildings"
};

    private int selectedTab;
    private const string ResourcesFilePath =
        "Assets/GameData/Source/Resources.tsv";
    private const string RecipesFilePath =
"Assets/GameData/Source/Recipes.tsv";
    private const string BuildingsFilePath =
        "Assets/GameData/Source/Buildings.tsv";

    private GameDataTableEditorPanel resourceTableEditor;

    private GameDataTableEditorPanel buildingTableEditor;

    private RecipeTablePanel recipeTablePanel;

    [MenuItem("Tools/Game Data Importer")]
    private static void OpenWindow()
    {
        GameDataImporterWindow window =
            GetWindow<GameDataImporterWindow>();

        window.titleContent = new GUIContent("Game Data Importer");
        window.minSize = new Vector2(620f, 420f);
    }

    private void OnEnable()
    {
        resourceTableEditor =
            new GameDataTableEditorPanel(
                typeof(ResourceDefinition)
            );

        recipeTablePanel =
            new RecipeTablePanel();

        buildingTableEditor =
            new GameDataTableEditorPanel(
                typeof(BuildingDefinition)
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
    resourceTableEditor == null ||
    !resourceTableEditor.IsValid ||
    recipeTablePanel == null ||
    !recipeTablePanel.IsValid ||
    buildingTableEditor == null ||
    !buildingTableEditor.IsValid
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

        selectedTab = GUILayout.Toolbar(
            selectedTab,
            TabNames,
            GUILayout.Height(26f)
        );

        EditorGUILayout.Space(8f);

        if (selectedTab == 0)
            DrawResourceTab();
        else if (selectedTab == 1)
            DrawRecipeSection();
        else
            DrawBuildingSection();


    }

    private void DrawValidationSummary()
    {
        EditorGUILayout.BeginHorizontal();

        DrawDataStatus(
    "Resources",
    resourceTableEditor != null &&
        resourceTableEditor.IsValidated,
    resourceTableEditor?.RowCount ?? 0,
    resourceTableEditor?.ErrorCount ?? 0
    );

        DrawDataStatus(
    "Recipes",
    recipeTablePanel != null &&
        recipeTablePanel.IsValidated,
    recipeTablePanel?.RowCount ?? 0,
    recipeTablePanel?.ErrorCount ?? 0
);

        DrawDataStatus(
    "Buildings",
    buildingTableEditor != null &&
        buildingTableEditor.IsValidated,
    buildingTableEditor?.RowCount ?? 0,
    buildingTableEditor?.ErrorCount ?? 0
    );

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

    private void ValidateAll()
    {
        resourceTableEditor.Validate();

        if (!resourceTableEditor.IsValid)
        {
            selectedTab = 0;
            return;
        }

        recipeTablePanel.Validate();
        buildingTableEditor.Validate();

        if (!recipeTablePanel.IsValid)
        {
            selectedTab = 1;
        }
        else if (!buildingTableEditor.IsValid)
        {
            selectedTab = 2;
        }
    }

    private void ImportAll()
    {
        ValidateAll();

        if (!resourceTableEditor.IsValid ||
    !recipeTablePanel.IsValid ||
    !buildingTableEditor.IsValid)
        {
            return;
        }

        bool confirmed = EditorUtility.DisplayDialog(
            "Import All Game Data",
            $"即將匯入：\n" +
            $"Resources：{resourceTableEditor.RowCount} 筆\n" +
            $"Recipes：{recipeTablePanel.RowCount} 筆\n" +
            $"Buildings：{buildingTableEditor.RowCount} 筆\n\n" +
            "順序：Resources → Recipes → Buildings",
            "Import All",
            "Cancel"
        );

        if (!confirmed)
        {
            return;
        }

        if (!resourceTableEditor.TryImport(false))
        {
            selectedTab = 0;
            Repaint();
            return;
        }

        if (!recipeTablePanel.TryImport(false))
        {
            selectedTab = 1;
            Repaint();
            return;
        }

        if (!buildingTableEditor.TryImport(false))
        {
            selectedTab = 2;
            Repaint();
            return;
        }

        AssetDatabase.SaveAssets();
        Repaint();
    }
    private void DrawResourceTab()
    {
        EditorGUILayout.LabelField(
            "Resource Data",
            EditorStyles.boldLabel
        );

        DrawSourceFileRow(ResourcesFilePath);

        EditorGUILayout.Space(8f);

        resourceTableEditor.Draw();
    }

    private void DrawRecipeSection()
    {
        EditorGUILayout.LabelField(
            "Recipe Data",
            EditorStyles.boldLabel
        );

        DrawSourceFileRow(RecipesFilePath);

        EditorGUILayout.Space(8f);

        recipeTablePanel.Draw();
    }

    private void DrawBuildingSection()
    {
        EditorGUILayout.LabelField(
            "Building Data",
            EditorStyles.boldLabel
        );

        DrawSourceFileRow(BuildingsFilePath);

        EditorGUILayout.Space(8f);

        buildingTableEditor.Draw();
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