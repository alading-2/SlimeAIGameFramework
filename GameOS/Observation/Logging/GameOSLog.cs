namespace SkilmeAI.GameOS.Observation;

/// <summary>
/// GameOS 观测日志的全局入口。
/// </summary>
public static class GameOSLog
{
    private static readonly object Sync = new();
    private static readonly List<IGameOSLogSink> Sinks = new();
    private static GameOSLogOptions options = new();
    private static bool defaultSinkConfigured;

    public static GameOSContextLog For(string context)
    {
        EnsureDefaultSink();
        return new GameOSContextLog(context);
    }

    public static void Configure(GameOSLogOptions nextOptions)
    {
        ArgumentNullException.ThrowIfNull(nextOptions);

        lock (Sync)
        {
            options = nextOptions;
            Sinks.RemoveAll(sink => sink is GameOSDefaultLogSink);
            defaultSinkConfigured = false;
        }

        EnsureDefaultSink();
    }

    public static void AddSink(IGameOSLogSink sink)
    {
        ArgumentNullException.ThrowIfNull(sink);
        lock (Sync)
        {
            Sinks.Add(sink);
        }
    }

    public static void RemoveSink(IGameOSLogSink sink)
    {
        ArgumentNullException.ThrowIfNull(sink);
        lock (Sync)
        {
            Sinks.Remove(sink);
        }
    }

    public static void Reset(GameOSLogOptions? resetOptions = null)
    {
        lock (Sync)
        {
            foreach (var sink in Sinks.OfType<IDisposable>())
            {
                sink.Dispose();
            }

            Sinks.Clear();
            options = resetOptions ?? new GameOSLogOptions();
            defaultSinkConfigured = false;
        }
    }

    internal static void Emit(GameOSLogEntry entry)
    {
        IGameOSLogSink[] currentSinks;
        lock (Sync)
        {
            if (entry.Level < options.MinimumLevel)
            {
                return;
            }

            currentSinks = Sinks.ToArray();
        }

        foreach (var sink in currentSinks)
        {
            sink.Emit(entry);
        }
    }

    internal static GameOSLogOptions Options
    {
        get
        {
            lock (Sync)
            {
                return options;
            }
        }
    }

    private static void EnsureDefaultSink()
    {
        lock (Sync)
        {
            if (defaultSinkConfigured)
            {
                return;
            }

            if (options.EnableStdout)
            {
                Sinks.Add(new GameOSDefaultLogSink(options.EnableGodotRichText));
            }

            defaultSinkConfigured = true;
        }
    }
}

/// <summary>
/// 绑定 context 的日志器。
/// </summary>
public sealed class GameOSContextLog
{
    internal GameOSContextLog(string context)
    {
        Context = string.IsNullOrWhiteSpace(context) ? "GameOS" : context;
    }

    public string Context { get; }

    public void Trace(string message, IReadOnlyDictionary<string, object?>? values = null) => Emit(GameOSLogLevel.Trace, message, values);

    public void Debug(string message, IReadOnlyDictionary<string, object?>? values = null) => Emit(GameOSLogLevel.Debug, message, values);

    public void Info(string message, IReadOnlyDictionary<string, object?>? values = null) => Emit(GameOSLogLevel.Info, message, values);

    public void Pass(string message, IReadOnlyDictionary<string, object?>? values = null) => Emit(GameOSLogLevel.Pass, message, values);

    public void Warn(string message, IReadOnlyDictionary<string, object?>? values = null) => Emit(GameOSLogLevel.Warn, message, values);

    public void Error(string message, IReadOnlyDictionary<string, object?>? values = null) => Emit(GameOSLogLevel.Error, message, values);

    public void Fail(string message, IReadOnlyDictionary<string, object?>? values = null) => Emit(GameOSLogLevel.Fail, message, values);

    public void Emit(GameOSLogLevel level, string message, IReadOnlyDictionary<string, object?>? values = null)
    {
        GameOSLog.Emit(new GameOSLogEntry(level, Context, message, values));
    }
}

/// <summary>
/// GameOS 观测日志等级。
/// </summary>
public enum GameOSLogLevel
{
    Trace = 0,
    Debug = 1,
    Info = 2,
    Pass = 3,
    Warn = 4,
    Error = 5,
    Fail = 6
}

/// <summary>
/// GameOS 观测日志全局配置。
/// </summary>
public sealed class GameOSLogOptions
{
    public GameOSLogLevel MinimumLevel { get; init; } = GameOSLogLevel.Trace;

    public bool EnableStdout { get; init; } = true;

    public bool EnableJsonl { get; init; } = true;

    public bool EnableGodotRichText { get; init; } = true;
}
