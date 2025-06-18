namespace L.EventStore.Configuration;

public interface IEventBusConfigurator
{
    public EventBusConfiguration Configuration { get; init; }

    void SetEventStreamIdType<TEventStreamId>() where TEventStreamId 
        : IEquatable<TEventStreamId>, IComparable<TEventStreamId>;
}
