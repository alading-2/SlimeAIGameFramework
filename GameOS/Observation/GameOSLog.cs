namespace SkilmeAI.GameOS.Observation;

/// <summary>
/// Global entry point for GameOS observation logging.
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
