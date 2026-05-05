namespace SkilmeAI.GameOS.Runtime.Event;

/// <summary>
/// Event subscriber priority. Larger values run earlier.
/// </summary>
public enum EventPriority
{
    /// <summary>Runs after normal gameplay handlers and is intended for monitoring.</summary>
    Monitor = -100,
    /// <summary>Very low priority.</summary>
    Lowest = -50,
    /// <summary>Low priority.</summary>
    Low = -10,
    /// <summary>Default priority.</summary>
    Normal = 0,
    /// <summary>High priority.</summary>
    High = 10,
    /// <summary>Very high priority.</summary>
    Highest = 50,
    /// <summary>Critical priority.</summary>
    Critical = 100
}
