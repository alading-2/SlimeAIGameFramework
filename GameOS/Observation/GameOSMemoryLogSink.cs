namespace SkilmeAI.GameOS.Observation;

/// <summary>
/// In-memory sink for tests and artifact aggregation.
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
