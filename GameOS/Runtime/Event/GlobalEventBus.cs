namespace SkilmeAI.GameOS.Runtime.Event;

/// <summary>
/// Process-wide event bus for system-level events.
/// </summary>
public static class GlobalEventBus
{
    /// <summary>
    /// Global runtime event bus.
    /// </summary>
    public static EventBus Global { get; } = new();
}
