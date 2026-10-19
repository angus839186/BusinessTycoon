public interface IDataPanel
{
    string DisplayName { get; }
    int ImportOrder { get; }
    string FilePath { get; }

    bool IsValidated { get; }
    bool IsValid { get; }

    int RowCount { get; }
    int ErrorCount { get; }

    void Validate();
    bool TryImport(bool requireConfirmation);
    void Draw();
}