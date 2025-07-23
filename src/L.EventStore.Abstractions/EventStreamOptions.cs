namespace L.EventStore.Abstractions;

public sealed class EventStreamOptions<TStreamIdentifier>
    where TStreamIdentifier : IComparable<TStreamIdentifier>, IEquatable<TStreamIdentifier>
{
    public required TStreamIdentifier StreamId { get; init; } 
    public required string StreamType { get; init; }
    public long ExpectedVersion { get; init; } = 0;
}
