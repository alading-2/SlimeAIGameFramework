namespace SkilmeAI.GameOS.Runtime.Data;

/// <summary>
/// Payload emitted when a runtime data value changes.
/// </summary>
public readonly record struct DataChangedEventData(IDataKey Key, object? OldValue, object? NewValue)
{
    /// <summary>稳定键名，用于 observation 和事件调试。</summary>
    public string StableKey => Key.StableKey;
}
