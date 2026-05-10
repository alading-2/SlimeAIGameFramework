using System.Collections.ObjectModel;
using System.Text;

namespace SkilmeAI.GameOS.Observation;

/// <summary>
/// A single structured GameOS observation log entry.
/// </summary>
public sealed class GameOSLogEntry
{
    public GameOSLogEntry(
        GameOSLogLevel level,
        string context,
        string message,
        IReadOnlyDictionary<string, object?>? values = null,
        DateTimeOffset? timestamp = null)
    {
        Level = level;
        Context = string.IsNullOrWhiteSpace(context) ? "GameOS" : context;
        Message = message ?? string.Empty;
        Values = new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?>(values ?? new Dictionary<string, object?>(), StringComparer.Ordinal));
        Timestamp = timestamp ?? DateTimeOffset.UtcNow;
    }

    public DateTimeOffset Timestamp { get; }

    public GameOSLogLevel Level { get; }

    public string Context { get; }

    public string Message { get; }

    public IReadOnlyDictionary<string, object?> Values { get; }

    public string FormatText()
    {
        var builder = new StringBuilder();
        builder.Append('[').Append(Level.ToString().ToUpperInvariant()).Append(']');
        builder.Append('[').Append(Context).Append("] ");
        builder.Append(Message);

        foreach (var value in Values)
        {
            builder.Append(' ')
                .Append(value.Key)
                .Append('=')
                .Append(FormatValue(value.Value));
        }

        return builder.ToString();
    }

    private static string FormatValue(object? value)
    {
        return value switch
        {
            null => "null",
            string text when text.Length == 0 => "\"\"",
            string text when text.Contains(' ', StringComparison.Ordinal) => $"\"{text.Replace("\"", "\\\"", StringComparison.Ordinal)}\"",
            bool flag => flag ? "true" : "false",
            IFormattable formattable => formattable.ToString(null, System.Globalization.CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty
        };
    }
}
