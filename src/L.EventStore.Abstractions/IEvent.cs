namespace L.EventStore.Abstractions;

public interface IEvent
{
    long Version { get; init; }
}
