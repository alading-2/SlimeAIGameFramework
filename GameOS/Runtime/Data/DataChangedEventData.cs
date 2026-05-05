namespace SkilmeAI.GameOS.Runtime.Data;

/// <summary>
/// Payload emitted when a runtime data value changes.
/// </summary>
public readonly record struct DataChangedEventData(string Key, object? OldValue, object? NewValue);

