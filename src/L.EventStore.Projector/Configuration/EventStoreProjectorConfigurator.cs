using L.EventStore.Abstractions.Configuration.StreamId;

namespace L.EventStore.Projector.Configuration;

public class EventStoreProjectorConfigurator
    : IEventStoreProjectorConfigurator
{
    public EventStoreProjectorConfiguration Configuration { get; init; } = new();

    public void SetEventStreamIdType<TStreamIdentifier>() where TStreamIdentifier : StreamIdType, new()
    {
        Configuration.StreamIdType = new TStreamIdentifier();
    }
}
