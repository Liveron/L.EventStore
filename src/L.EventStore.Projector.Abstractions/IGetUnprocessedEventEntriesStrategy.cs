using L.EventStore.Abstractions;

namespace L.EventStore.Projector.Abstractions;

public interface IGetUnprocessedEventEntriesStrategy<TStreamIdentifier>
    where TStreamIdentifier : IEquatable<TStreamIdentifier>, IComparable<TStreamIdentifier>
{
    Task<List<EventEntry<TStreamIdentifier>>> GetUnprocessedEventEntriesAsync();
}
