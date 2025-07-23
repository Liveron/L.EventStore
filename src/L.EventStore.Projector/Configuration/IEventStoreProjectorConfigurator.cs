using L.EventStore.Abstractions.Configuration.StreamId;

namespace L.EventStore.Projector.Configuration;

public interface IEventStoreProjectorConfigurator
{
    EventStoreProjectorConfiguration Configuration { get; }
    void SetEventStreamIdType<TStreamIdentifier>()
        where TStreamIdentifier : StreamIdType, new();
}
