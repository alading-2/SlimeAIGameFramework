using SkilmeAI.GameOS.Runtime.Data;

namespace SkilmeAI.GameOS.Runtime.Event;

/// <summary>
/// Bridges Data notifications into an EventBus.
/// </summary>
public sealed class EventDataChangeSink : IDataChangeSink
{
    private readonly EventBus events;

    /// <summary>
    /// Creates a Data change sink for the target event bus.
    /// </summary>
    /// <param name="events">Event bus that receives Data change events.</param>
    public EventDataChangeSink(EventBus events)
    {
        this.events = events;
    }

    /// <inheritdoc />
    public void OnDataChanged(DataChangedEventData change)
    {
        events.Emit(GameEventType.Data.PropertyChanged, new GameEventType.Data.PropertyChangedEventData(change));
    }
}
