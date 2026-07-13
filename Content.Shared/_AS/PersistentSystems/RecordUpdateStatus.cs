namespace Content.Shared._AS.PersistentSystems;

public struct RecordUpdateResult()
{
    public RecordUpdateStatus Status = RecordUpdateStatus.NoChange;
    public List<Edit> Edits = [];
}

public enum RecordUpdateStatus
{
    Failed, // Generic failure.
    NotFound, // The record was not found.
    Prohibited, // The user is not allowed to edit this record.
    NoChange, // Successful update, but no changes were made.
    Updated, // Successful update, changes applied.
}

public struct Edit(string field, int editId, object? oldValue, object? newValue)
{
    public string Field = field;
    public int UpdateId = editId;
    public object? OldValue = oldValue;
    public object? NewValue = newValue;
}
