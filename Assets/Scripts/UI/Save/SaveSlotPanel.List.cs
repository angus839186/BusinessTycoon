using System.Collections.Generic;

public sealed partial class SaveSlotPanel
{
    private ReusableUIList<SaveSlotItem> itemList;
    private void Refresh()
    {
        if (itemList == null)
        {
            return;
        }

        if (titleText != null)
        {
            titleText.text =
                mode == SaveSlotMode.Load
                    ? "讀取遊戲"
                    : "儲存遊戲";
        }

        IReadOnlyList<SaveSlotInfo> infos =
    mode == SaveSlotMode.Load
        ? SaveSlots.GetLoadInfos()
        : SaveSlots.GetInfos();

        int index = 0;

        foreach (SaveSlotInfo info in infos)
        {
            SaveSlotItem item =
                itemList.GetOrCreate(index);

            if (item == null)
            {
                continue;
            }

            bool actionEnabled =
                CanUseSlot(info);

            bool deleteEnabled = info != null && info.Exists;

            item.Bind(info, mode == SaveSlotMode.Load ? "讀取" : "儲存",
            actionEnabled, HandleSlotAction, deleteEnabled, HandleDeleteRequested);

            index++;
        }

        itemList.HideFrom(index);
    }

    private bool CanUseSlot(SaveSlotInfo info)
    {
        if (info == null)
        {
            return false;
        }

        if (mode == SaveSlotMode.Save)
        {
            return gameSaveSystem != null;
        }

        return
            info.Exists &&
            info.CanLoad &&
            string.IsNullOrWhiteSpace(
                info.ErrorMessage
            );
    }
}