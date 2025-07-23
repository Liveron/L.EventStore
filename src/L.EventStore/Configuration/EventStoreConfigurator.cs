using L.EventStore.Abstractions.Configuration;
using L.EventStore.Abstractions.Configuration.StreamId;

namespace L.EventStore.Configuration;

public class EventStoreConfigurator : IEventStoreConfigurator
{
    public EventStoreConfiguration Configuration { get; init; } = new();

    public void SetEventStreamIdType<TStreamIdentifier>() where TStreamIdentifier : StreamIdType, new()
    {
        Configuration.StreamIdType = new TStreamIdentifier();
    }

    public void SetEventType<TEvent>() where TEvent : notnull
    {
        var eventType = typeof(TEvent);
        if (!eventType.IsInterface)
            throw new ArgumentException($"Event type {eventType} is not interface");

        Configuration.EventType = typeof(TEvent);
    }
}
