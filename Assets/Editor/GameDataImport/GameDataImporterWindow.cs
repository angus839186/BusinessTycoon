using System.Collections.Generic;
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

    private List<ResourceImportRow> rows =
        new List<ResourceImportRow>();

    private List<string> errors =
        new List<string>();

    private List<string> importMessages =
new List<string>();

    private List<string> recipeImportMessages =
        new List<string>();
    private List<RecipeImportRow> recipeRows =
        new List<RecipeImportRow>();

    private List<string> recipeErrors =
        new List<string>();

    private List<BuildingImportRow> buildingRows =
new List<BuildingImportRow>();

    private List<string> buildingErrors =
        new List<string>();

    private List<string> buildingImportMessages =
new List<string>();

    private Vector2 buildingScrollPosition;
    private bool hasValidatedBuildings;

    private Vector2 recipeScrollPosition;
    private bool hasValidatedRecipes;

    private Vector2 scrollPosition;
    private bool hasValidated;

    [MenuItem("Tools/Game Data Importer")]
    private static void OpenWindow()
    {
        GameDataImporterWindow window =
            GetWindow<GameDataImporterWindow>();

        window.titleContent = new GUIContent("Game Data Importer");
        window.minSize = new Vector2(620f, 420f);
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
            !hasValidated ||
            errors.Count > 0 ||
            !hasValidatedRecipes ||
            recipeErrors.Count > 0
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
            hasValidated,
            rows.Count,
            errors.Count
        );

        DrawDataStatus(
            "Recipes",
            hasValidatedRecipes,
            recipeRows.Count,
            recipeErrors.Count
        );

        DrawDataStatus(
    "Buildings",
    hasValidatedBuildings,
    buildingRows.Count,
    buildingErrors.Count
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
        ValidateResources();

        if (errors.Count > 0)
        {
            selectedTab = 0;
            return;
        }

        ValidateRecipes();

        ValidateBuildings();

        if (recipeErrors.Count > 0)
            selectedTab = 1;
        else if (buildingErrors.Count > 0)
            selectedTab = 2;
    }

    private void ImportAll()
    {
        // 匯入前重新驗證兩份檔案。
        ValidateAll();

        if (errors.Count > 0 || recipeErrors.Count > 0)
            return;

        bool confirmed = EditorUtility.DisplayDialog(
            "Import All Game Data",
            $"即將匯入：\n" +
            $"Resources：{rows.Count} 筆\n" +
            $"Recipes：{recipeRows.Count} 筆\n\n" +
            "Resources 會先匯入，接著重建 Recipes。",
            "Import All",
            "Cancel"
        );

        if (!confirmed)
            return;

        bool resourcesSucceeded =
            ResourceDefinitionImporter.Import(
                rows,
                out importMessages,
                out List<string> resourceImportErrors
            );

        if (!resourcesSucceeded)
        {
            errors.AddRange(resourceImportErrors);
            selectedTab = 0;
            Repaint();
            return;
        }

        bool recipesSucceeded =
            RecipeDefinitionImporter.Import(
                recipeRows,
                out recipeImportMessages,
                out List<string> recipeImportErrors
            );

        if (!recipesSucceeded)
        {
            recipeErrors.AddRange(recipeImportErrors);
            selectedTab = 1;
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

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button(
            "Validate Resources",
            GUILayout.Height(30f)
        ))
        {
            ValidateResources();
        }

        using (new EditorGUI.DisabledScope(
            !hasValidated || errors.Count > 0
        ))
        {
            if (GUILayout.Button(
                "Import Resources",
                GUILayout.Height(30f)
            ))
            {
                ImportResources();
            }
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(8f);

        if (!hasValidated)
        {
            EditorGUILayout.HelpBox(
                "按下 Validate Resources 檢查 TSV，不會修改任何資產。",
                MessageType.Info
            );

            return;
        }

        if (errors.Count == 0)
        {
            EditorGUILayout.HelpBox(
                $"驗證成功，共讀取 {rows.Count} 筆資源。",
                MessageType.Info
            );
        }
        else
        {
            EditorGUILayout.HelpBox(
                $"驗證失敗，共有 {errors.Count} 個錯誤。",
                MessageType.Error
            );
        }

        scrollPosition = EditorGUILayout.BeginScrollView(
    scrollPosition,
    GUILayout.ExpandHeight(true)
);

        if (errors.Count > 0)
            DrawErrors();
        else
            DrawRows();

        EditorGUILayout.EndScrollView();
    }

    private void ValidateResources()
    {
        ResourceTsvValidator.Validate(
            ResourcesFilePath,
            out rows,
            out errors
        );

        importMessages.Clear();

        recipeRows.Clear();
        recipeErrors.Clear();
        recipeImportMessages.Clear();
        hasValidatedRecipes = false;

        buildingRows.Clear();
        buildingErrors.Clear();
        hasValidatedBuildings = false;

        hasValidated = true;
        Repaint();
    }
    private void DrawRecipeSection()
    {
        EditorGUILayout.Space(16f);

        EditorGUILayout.LabelField(
            "Recipe Data",
            EditorStyles.boldLabel
        );

        DrawSourceFileRow(RecipesFilePath);

        EditorGUILayout.BeginHorizontal();

        using (new EditorGUI.DisabledScope(
            !hasValidated || errors.Count > 0
        ))
        {
            if (GUILayout.Button(
                "Validate Recipes",
                GUILayout.Height(30f)
            ))
            {
                ValidateRecipes();
            }
        }

        using (new EditorGUI.DisabledScope(
            !hasValidatedRecipes || recipeErrors.Count > 0
        ))
        {
            if (GUILayout.Button(
                "Import Recipes",
                GUILayout.Height(30f)
            ))
            {
                ImportRecipes();
            }
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(8f);

        if (!hasValidatedRecipes)
        {
            EditorGUILayout.HelpBox(
                "請先驗證 Resources，再驗證 Recipes。",
                MessageType.Info
            );

            return;
        }

        if (recipeErrors.Count == 0)
        {
            EditorGUILayout.HelpBox(
                $"Recipe 驗證成功，共讀取 {recipeRows.Count} 筆。",
                MessageType.Info
            );
        }
        else
        {
            EditorGUILayout.HelpBox(
                $"Recipe 驗證失敗，共有 {recipeErrors.Count} 個錯誤。",
                MessageType.Error
            );
        }

        recipeScrollPosition = EditorGUILayout.BeginScrollView(
            recipeScrollPosition,
            GUILayout.ExpandHeight(true)
        );

        if (recipeErrors.Count > 0)
        {
            foreach (string error in recipeErrors)
            {
                EditorGUILayout.HelpBox(
                    error,
                    MessageType.Error
                );
            }
        }
        else
        {
            DrawRecipeRows();
        }

        EditorGUILayout.EndScrollView();
    }

    private void ImportRecipes()
    {
        ResourceTsvValidator.Validate(
            ResourcesFilePath,
            out rows,
            out errors
        );

        hasValidated = true;
        recipeImportMessages.Clear();

        if (errors.Count > 0)
        {
            hasValidatedRecipes = false;
            selectedTab = 0;
            Repaint();
            return;
        }

        RecipeTsvValidator.Validate(
            RecipesFilePath,
            rows,
            out recipeRows,
            out recipeErrors
        );

        hasValidatedRecipes = true;

        if (recipeErrors.Count > 0)
        {
            Repaint();
            return;
        }

        bool confirmed = EditorUtility.DisplayDialog(
            "Import Recipes",
            $"即將匯入 {recipeRows.Count} 筆配方資料。\n" +
            "TSV 中出現的資源配方會被重建。",
            "Import",
            "Cancel"
        );

        if (!confirmed)
            return;

        bool succeeded = RecipeDefinitionImporter.Import(
            recipeRows,
            out recipeImportMessages,
            out List<string> importErrors
        );

        if (!succeeded)
            recipeErrors.AddRange(importErrors);

        Repaint();
    }
    private void ValidateRecipes()
    {
        recipeImportMessages.Clear();
        RecipeTsvValidator.Validate(
            RecipesFilePath,
            rows,
            out recipeRows,
            out recipeErrors
        );

        hasValidatedRecipes = true;
        Repaint();
    }

    private void ValidateBuildings()
    {
        buildingImportMessages.Clear();
        BuildingTsvValidator.Validate(
            BuildingsFilePath,
            rows,
            out buildingRows,
            out buildingErrors
        );

        hasValidatedBuildings = true;
        Repaint();
    }

    private void ImportResources()
    {
        // 匯入前重新讀取，避免使用舊的驗證結果。
        ResourceTsvValidator.Validate(
            ResourcesFilePath,
            out rows,
            out errors
        );

        importMessages.Clear();
        hasValidated = true;

        if (errors.Count > 0)
        {
            Repaint();
            return;
        }

        bool confirmed = EditorUtility.DisplayDialog(
            "Import Resources",
            $"即將匯入 {rows.Count} 筆資源。\n" +
            "相同 ID 會更新，不存在的 ID 會新增。",
            "Import",
            "Cancel"
        );

        if (!confirmed)
            return;

        bool succeeded = ResourceDefinitionImporter.Import(
            rows,
            out importMessages,
            out List<string> importErrors
        );



        if (!succeeded)
            errors.AddRange(importErrors);

        if (succeeded)
        {
            recipeRows.Clear();
            recipeErrors.Clear();
            recipeImportMessages.Clear();
            hasValidatedRecipes = false;
        }

        Repaint();
    }

    private void DrawErrors()
    {
        foreach (string error in errors)
        {
            EditorGUILayout.HelpBox(error, MessageType.Error);
        }
    }

    private void DrawRows()
    {
        foreach (string message in importMessages)
            EditorGUILayout.HelpBox(message, MessageType.Info);

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("Resource ID", GUILayout.Width(150f));
        GUILayout.Label("Display Name");
        GUILayout.Label("Base Price", GUILayout.Width(90f));
        GUILayout.Label("Enabled", GUILayout.Width(70f));
        EditorGUILayout.EndHorizontal();

        foreach (ResourceImportRow row in rows)
        {
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(
                row.ResourceId,
                GUILayout.Width(150f)
            );

            GUILayout.Label(row.DisplayName);

            GUILayout.Label(
                row.BasePrice.ToString(),
                GUILayout.Width(90f)
            );

            GUILayout.Label(
                row.Enabled ? "Yes" : "No",
                GUILayout.Width(70f)
            );

            EditorGUILayout.EndHorizontal();
        }
    }

    private void DrawRecipeRows()
    {
        foreach (string message in recipeImportMessages)
            EditorGUILayout.HelpBox(message, MessageType.Info);
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("Owner Resource", GUILayout.Width(150f));
        GUILayout.Label("Type", GUILayout.Width(80f));
        GUILayout.Label("Resource");
        GUILayout.Label("Amount", GUILayout.Width(70f));
        GUILayout.Label("Enabled", GUILayout.Width(70f));
        EditorGUILayout.EndHorizontal();

        foreach (RecipeImportRow row in recipeRows)
        {
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(
                row.OwnerResourceId,
                GUILayout.Width(150f)
            );

            GUILayout.Label(
                row.IoType.ToString(),
                GUILayout.Width(80f)
            );

            GUILayout.Label(row.ResourceId);

            GUILayout.Label(
                row.Amount.ToString(),
                GUILayout.Width(70f)
            );

            GUILayout.Label(
                row.Enabled ? "Yes" : "No",
                GUILayout.Width(70f)
            );

            EditorGUILayout.EndHorizontal();
        }
    }

    private void DrawBuildingSection()
    {
        EditorGUILayout.LabelField(
            "Building Data",
            EditorStyles.boldLabel
        );

        DrawSourceFileRow(BuildingsFilePath);

        EditorGUILayout.BeginHorizontal();

        using (new EditorGUI.DisabledScope(
            !hasValidated || errors.Count > 0
        ))
        {
            if (GUILayout.Button(
                "Validate Buildings",
                GUILayout.Height(30f)
            ))
            {
                ValidateBuildings();
            }
        }

        using (new EditorGUI.DisabledScope(
            !hasValidatedBuildings || buildingErrors.Count > 0
        ))
        {
            if (GUILayout.Button(
                "Import Buildings",
                GUILayout.Height(30f)
            ))
            {
                ImportBuildings();
            }
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(8f);

        if (!hasValidatedBuildings)
        {
            EditorGUILayout.HelpBox(
                "請先驗證 Resources，再驗證 Buildings。",
                MessageType.Info
            );

            return;
        }

        MessageType messageType =
            buildingErrors.Count == 0
                ? MessageType.Info
                : MessageType.Error;

        EditorGUILayout.HelpBox(
            buildingErrors.Count == 0
                ? $"Building 驗證成功，共讀取 {buildingRows.Count} 筆。"
                : $"Building 驗證失敗，共有 {buildingErrors.Count} 個錯誤。",
            messageType
        );

        buildingScrollPosition = EditorGUILayout.BeginScrollView(
            buildingScrollPosition,
            GUILayout.ExpandHeight(true)
        );

        if (buildingErrors.Count > 0)
        {
            foreach (string error in buildingErrors)
                EditorGUILayout.HelpBox(error, MessageType.Error);
        }
        else
        {
            foreach (string message in buildingImportMessages)
            {
                EditorGUILayout.HelpBox(
                    message,
                    MessageType.Info
                );
            }

            foreach (BuildingImportRow row in buildingRows)
            {
                EditorGUILayout.LabelField(
                    $"{row.BuildingId} | {row.Category} | " +
                    $"{row.ProducedResourceId} | " +
                    $"Speed {row.WorkSpeedMultiplier}"
                );
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private void ImportBuildings()
    {
        ResourceTsvValidator.Validate(
            ResourcesFilePath,
            out rows,
            out errors
        );

        hasValidated = true;
        buildingImportMessages.Clear();

        if (errors.Count > 0)
        {
            hasValidatedBuildings = false;
            selectedTab = 0;
            Repaint();
            return;
        }

        BuildingTsvValidator.Validate(
            BuildingsFilePath,
            rows,
            out buildingRows,
            out buildingErrors
        );

        hasValidatedBuildings = true;

        if (buildingErrors.Count > 0)
        {
            Repaint();
            return;
        }

        bool confirmed = EditorUtility.DisplayDialog(
            "Import Buildings",
            $"即將匯入 {buildingRows.Count} 筆建築資料。\n" +
            "相同 ID 會更新，不存在的 ID 會新增。",
            "Import",
            "Cancel"
        );

        if (!confirmed)
            return;

        bool succeeded = BuildingDefinitionImporter.Import(
            buildingRows,
            out buildingImportMessages,
            out List<string> importErrors
        );

        if (!succeeded)
            buildingErrors.AddRange(importErrors);

        Repaint();
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