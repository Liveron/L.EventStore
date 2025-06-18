namespace L.EventStore.Abstractions;

public class EventStoreEntry<TStreamIdentifier>
    where TStreamIdentifier : IEquatable<TStreamIdentifier>, 
    IComparable<TStreamIdentifier>
{
    public required TStreamIdentifier StreamId { get; init; }
    public required string StreamType { get; init; }
    public required string EventType { get; init; }
    public required string Event { get; init; }
    public long Version { get; init; }
    public DateTime CreatedAtUTC { get; } = DateTime.UtcNow;
}
