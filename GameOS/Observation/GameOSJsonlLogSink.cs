using System.Text.Encodings.Web;
using System.Text.Json;

namespace SkilmeAI.GameOS.Observation;

/// <summary>
/// Writes one JSON object per observation log line.
/// </summary>
public sealed class GameOSJsonlLogSink : IGameOSLogSink, IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly object sync = new();
    private readonly StreamWriter writer;

    public GameOSJsonlLogSink(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path) ?? ".");
        writer = new StreamWriter(new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read))
        {
            AutoFlush = true
        };
        Path = path;
    }

    public string Path { get; }

    public void Emit(GameOSLogEntry entry)
    {
        var payload = new
        {
            timestamp = entry.Timestamp.ToString("O"),
            level = entry.Level.ToString().ToUpperInvariant(),
            context = entry.Context,
            message = entry.Message,
            values = entry.Values,
            text = entry.FormatText()
        };
        var json = JsonSerializer.Serialize(payload, JsonOptions);
        lock (sync)
        {
            writer.WriteLine(json);
        }
    }

    public void Dispose()
    {
        lock (sync)
        {
            writer.Dispose();
        }
    }
}
