namespace SkilmeAI.GameOS.Runtime.Data;

/// <summary>
/// Minimal bridge used by Data before the Event runtime is migrated.
/// </summary>
public interface IDataChangeSink
{
    /// <summary>
    /// Receives a runtime data change notification.
    /// </summary>
    /// <param name="change">Data change payload.</param>
    void OnDataChanged(DataChangedEventData change);
}

