using System.Text.Encodings.Web;
using System.Text.Json;
using Godot;

namespace SkilmeAI.GameOS.Observation;

/// <summary>
/// 接收 GameOS 观测日志条目的 sink 协议。
/// </summary>
public interface IGameOSLogSink
{
    void Emit(GameOSLogEntry entry);
}

/// <summary>
/// 写入 Godot stdout；不可用时退回 Console。
/// </summary>
public sealed class GameOSDefaultLogSink : IGameOSLogSink
{
    private readonly bool useRichText;

    public GameOSDefaultLogSink(bool useRichText = true)
    {
        this.useRichText = useRichText;
    }

    public void Emit(GameOSLogEntry entry)
    {
        var text = entry.FormatText();
        try
        {
            if (useRichText)
            {
                GD.PrintRich($"[color={ColorFor(entry.Level)}]{text}[/color]");
            }
            else
            {
                GD.Print(text);
            }
        }
        catch (Exception)
        {
            Console.WriteLine(text);
        }
    }

    private static string ColorFor(GameOSLogLevel level)
    {
        return level switch
        {
            GameOSLogLevel.Pass => "green",
            GameOSLogLevel.Warn => "yellow",
            GameOSLogLevel.Error => "red",
            GameOSLogLevel.Fail => "red",
            GameOSLogLevel.Debug => "gray",
            GameOSLogLevel.Trace => "gray",
            _ => "white"
        };
    }
}

/// <summary>
/// 每行写入一个 JSON object 的日志 sink。
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
        writer = new StreamWriter(new System.IO.FileStream(path, System.IO.FileMode.Append, System.IO.FileAccess.Write, System.IO.FileShare.Read))
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

/// <summary>
/// 测试和 artifact 聚合使用的内存日志 sink。
/// </summary>
public sealed class GameOSMemoryLogSink : IGameOSLogSink
{
    private readonly List<GameOSLogEntry> entries = new();

    public IReadOnlyList<GameOSLogEntry> Entries => entries;

    public void Emit(GameOSLogEntry entry)
    {
        entries.Add(entry);
    }

    public void Clear()
    {
        entries.Clear();
    }
}
