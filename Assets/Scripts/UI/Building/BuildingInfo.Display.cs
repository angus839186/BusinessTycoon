using System.Text;

public sealed partial class BuildingInfo
{
    private readonly StringBuilder builder = new StringBuilder();

    private void Refresh()
    {
        if (selectedBuilding == null || panelOpen)
        {
            transition.Hide();
            return;
        }

        BuildingDefinition definition = selectedBuilding.Definition;

        if (definition == null)
        {
            transition.Hide();
            return;
        }

        nameText.text = definition.displayName;

        categoryText.text =
            GetCategoryText(
                definition.category
            );

        statusText.text = selectedBuilding.IsOperational ? "運作中" : "已停用";

        iconImage.sprite = definition.icon;

        iconImage.enabled = definition.icon != null;

        RefreshProduction(definition);
        RefreshInventory();
        RefreshLocation();

        transition.Show();
    }

    private string GetCategoryText(
    BuildingCategory category
)
    {
        return category switch
        {
            BuildingCategory.Resource => "資源建築",
            BuildingCategory.Processing => "加工建築",
            BuildingCategory.Commercial => "商業建築",
            BuildingCategory.Storage => "倉庫",
            _ => "其他"
        };
    }

    private void RefreshProduction(
    BuildingDefinition definition)
    {
        if (definition.category == BuildingCategory.Storage
        )
        {
            productionText.text =
                "此建築不進行生產";

            progressRoot.SetActive(false);
            return;
        }

        progressRoot.SetActive(true);

        if (definition.category == BuildingCategory.Commercial)
        {
            productionText.text =
                definition.saleResource != null
                    ? $"販售：{definition.saleResource.displayName} × {definition.saleAmount}"
                    : "尚未設定販售商品";
        }
        else
        {
            productionText.text =
                definition.producedResource != null
                    ? $"產出：{definition.producedResource.displayName}"
                    : "尚未設定產出資源";
        }

        float progress = 0f;
        string progressLabel;

        if (!selectedBuilding.IsOperational)
        {
            progressLabel = "已停止";
        }
        else if (
            productionSystem.IsWaitingForInput(selectedBuilding)
        )
        {
            progressLabel = "等待輸入資源";
        }
        else
        {
            progress = productionSystem.GetProcessProgress(selectedBuilding);

            progressLabel = $"{progress:P0}";
        }

        progressSlider.value = progress;
        progressText.text = progressLabel;
    }

    private void RefreshInventory()
    {
        builder.Clear();
        int shown = 0;

        foreach (ResourceStack stack in selectedBuilding.Inventory.Resources)
        {
            if (stack == null || stack.resource == null || stack.amount <= 0)
            {
                continue;
            }

            if (shown > 0)
            {
                builder.AppendLine();
            }

            builder.Append($"{stack.resource.displayName} × {stack.amount}");

            shown++;

            if (shown >= maxInventoryLines)
            {
                break;
            }
        }

        inventoryText.text = shown > 0 ? builder.ToString() : "目前沒有庫存";
    }

    private void RefreshLocation()
    {
        LocationValueEvaluation evaluation =
            productionSystem.GetEconomyEvaluation(selectedBuilding);

        bool hasArea = !string.IsNullOrWhiteSpace(evaluation.areaName) &&
        evaluation.areaName != "None";

        locationText.text = hasArea ? $"地區：{evaluation.areaName}" : "地區：尚無資料";
    }
}