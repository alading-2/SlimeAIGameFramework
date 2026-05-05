namespace SkilmeAI.GameOS.Runtime.Event;

/// <summary>
/// Context object for request-response events.
/// </summary>
public class EventContext
{
    private object? result;

    /// <summary>
    /// True once any subscriber has handled the event.
    /// </summary>
    public bool IsHandled { get; protected set; }

    /// <summary>
    /// True when later subscribers should be skipped.
    /// </summary>
    public bool IsPropagationStopped { get; private set; }

    /// <summary>
    /// Operation result for check or command events.
    /// </summary>
    public bool Success { get; protected set; } = true;

    /// <summary>
    /// First failure reason reported by a subscriber.
    /// </summary>
    public string? FailReason { get; protected set; }

    /// <summary>
    /// True when a typed result has been stored.
    /// </summary>
    public bool HasResult => result != null;

    /// <summary>
    /// Stops dispatch to lower-priority subscribers.
    /// </summary>
    public void StopPropagation()
    {
        IsPropagationStopped = true;
    }

    /// <summary>
    /// Marks the event as failed.
    /// </summary>
    /// <param name="reason">Human-readable failure reason.</param>
    public void SetFailed(string reason)
    {
        Success = false;
        IsHandled = true;
        FailReason ??= reason;
    }

    /// <summary>
    /// Stores a typed result.
    /// </summary>
    /// <param name="value">Result value written by a subscriber.</param>
    public void SetResult<T>(T value)
    {
        result = value;
        IsHandled = true;
    }

    /// <summary>
    /// Reads a typed result.
    /// </summary>
    public T? GetResult<T>()
    {
        return result is T typedResult ? typedResult : default;
    }
}
