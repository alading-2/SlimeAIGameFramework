namespace SkilmeAI.GameOS.Observation;

/// <summary>
/// Global options for GameOS observation logging.
/// </summary>
public sealed class GameOSLogOptions
{
    public GameOSLogLevel MinimumLevel { get; init; } = GameOSLogLevel.Trace;

    public bool EnableStdout { get; init; } = true;

    public bool EnableJsonl { get; init; } = true;

    public bool EnableGodotRichText { get; init; } = true;
}
