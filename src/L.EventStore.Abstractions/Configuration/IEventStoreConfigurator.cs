using L.EventStore.Abstractions.Configuration.StreamId;

namespace L.EventStore.Abstractions.Configuration;

public interface IEventStoreConfigurator
{
    public EventStoreConfiguration Configuration { get; init; }
    void SetEventStreamIdType<TStreamIdentifier>()
        where TStreamIdentifier : StreamIdType, new();
    void SetEventType<TEvent>() where TEvent : notnull;
}

