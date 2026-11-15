using System;

public sealed class SaveSlotInfo
{
    public string SlotId { get; set; }
    public string FilePath { get; set; }

    public bool Exists { get; set; }
    public bool CanLoad { get; set; }

    public DateTime SavedAt { get; set; }
    public double TotalGameMinutes { get; set; }
    public int SaveVersion { get; set; }

    public string ErrorMessage { get; set; }
}