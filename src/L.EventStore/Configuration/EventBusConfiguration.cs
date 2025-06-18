namespace L.EventStore.Configuration;

public abstract class EventBusConfiguration
{
    public Type StreamIdType { get; set; } = typeof(Guid);
}
