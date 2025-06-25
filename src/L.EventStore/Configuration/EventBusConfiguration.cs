using L.EventStore.Abstractions;
using L.EventStore.Configuration.StreamId;

namespace L.EventStore.Configuration;

public abstract class EventBusConfiguration
{
    public Type StreamIdType { get; set; } = typeof(StreamIdTypes.Guid);
    public Type EventType { get; set; } = typeof(IEvent);
}
