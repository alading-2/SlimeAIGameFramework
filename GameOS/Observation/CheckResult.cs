namespace SkilmeAI.GameOS.Observation;

/// <summary>
/// Result returned by a scene validation check.
/// </summary>
public sealed record CheckResult(bool Success, string Message, IReadOnlyDictionary<string, object?> Details)
{
    public static CheckResult Pass(string message, IReadOnlyDictionary<string, object?>? details = null)
    {
        return new CheckResult(true, message, details ?? new Dictionary<string, object?>());
    }

    public static CheckResult Fail(string message, IReadOnlyDictionary<string, object?>? details = null)
    {
        return new CheckResult(false, message, details ?? new Dictionary<string, object?>());
    }

    public static CheckResult From(bool success, string message, IReadOnlyDictionary<string, object?>? details = null)
    {
        return success ? Pass(message, details) : Fail(message, details);
    }
}
