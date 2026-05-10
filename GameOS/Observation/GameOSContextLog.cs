namespace SkilmeAI.GameOS.Observation;

/// <summary>
/// Context-bound logger returned by <see cref="GameOSLog.For(string)" />.
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
