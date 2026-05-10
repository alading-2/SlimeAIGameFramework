using Godot;

namespace SkilmeAI.GameOS.Observation;

/// <summary>
/// Writes observation logs to Godot stdout when available, otherwise to Console.
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
