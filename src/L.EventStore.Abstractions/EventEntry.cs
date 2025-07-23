namespace L.EventStore.Abstractions;

public class EventEntry<TStreamIdentifier> : EventEntry
    where TStreamIdentifier : IEquatable<TStreamIdentifier>, 
    IComparable<TStreamIdentifier>
{
    public required TStreamIdentifier StreamId { get; init; }
}

public abstract class EventEntry
{
    public long Id { get; init; }
    public required string StreamType { get; init; }
    public required string EventType { get; init; }
    public required string Event { get; init; }
    public long Version { get; init; }
    public DateTime CreatedAt { get; init; }
}