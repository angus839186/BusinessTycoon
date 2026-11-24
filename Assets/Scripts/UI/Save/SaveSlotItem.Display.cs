using System;

public sealed partial class SaveSlotItem
{
    private void RefreshInfo(SaveSlotInfo info)
    {
        if (info == null)
        {
            SetStatus(
                "無法取得存檔資料",
                string.Empty
            );

            return;
        }

        if (!info.Exists)
        {
            SetStatus(
                "空白存檔",
                "尚未建立存檔"
            );

            return;
        }

        if (!string.IsNullOrWhiteSpace(
            info.ErrorMessage
        ))
        {
            SetStatus(
                "存檔異常",
                info.ErrorMessage
            );

            return;
        }

        string details =
            $"遊戲進度：{info.TotalGameMinutes:0} 分鐘\n" +
            $"儲存時間：{info.SavedAt:yyyy/MM/dd HH:mm}\n" +
            $"版本：{info.SaveVersion}";

        SetStatus(
            info.CanLoad
                ? "可以讀取"
                : "版本不相容",
            details
        );
    }
    private void SetStatus(string status, string details)
    {
        if (statusText != null)
        {
            statusText.text = status;
        }

        if (detailText != null)
        {
            detailText.text = details;
        }
    }
    private string GetDisplayName(string id)
    {
        if (string.Equals(
        id, SaveSlots.AutoSaveId, StringComparison.Ordinal))
        {
            return "自動存檔";
        }
        const string prefix = "slot_";

        if (!string.IsNullOrWhiteSpace(id) && id.StartsWith(prefix, StringComparison.Ordinal))
        {
            string number = id.Substring(prefix.Length);
            return $"存檔 {number}";
        }

        return string.IsNullOrWhiteSpace(id) ? "未知存檔" : id;
    }
}