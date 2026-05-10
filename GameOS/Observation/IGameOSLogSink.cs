namespace SkilmeAI.GameOS.Observation;

/// <summary>
/// Receives GameOS observation log entries.
/// </summary>
public interface IGameOSLogSink
{
    void Emit(GameOSLogEntry entry);
}
