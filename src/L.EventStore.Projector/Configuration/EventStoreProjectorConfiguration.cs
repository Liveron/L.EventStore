using L.EventStore.Abstractions.Configuration.StreamId;

namespace L.EventStore.Projector.Configuration;

public class EventStoreProjectorConfiguration
{
    public StreamIdType StreamIdType { get; set; } = new StreamIdTypes.Guid();
}
