using L.EventStore.Configuration.StreamId;

namespace L.EventStore.Configuration;

public interface IEventBusConfigurator
{
    public EventBusConfiguration Configuration { get; init; }
    void SetEventStreamIdType<TStreamIdentifier>()
        where TStreamIdentifier : StreamIdType;
    void SetEventType<TEvent>() where TEvent: notnull;
}
