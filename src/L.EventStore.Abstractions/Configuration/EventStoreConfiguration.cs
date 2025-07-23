using L.EventStore.Abstractions.Configuration.StreamId;

namespace L.EventStore.Abstractions.Configuration;

public class EventStoreConfiguration
{
    public StreamIdType StreamIdType { get; set; } = new StreamIdTypes.Guid();
    public Type EventType { get; set; } = typeof(object);
}
