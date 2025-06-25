namespace L.EventStore.Abstractions;

public interface IEventStoreRepository<TStreamIdentifier> : IDisposable, IAsyncDisposable
    where TStreamIdentifier : IComparable<TStreamIdentifier>, IEquatable<TStreamIdentifier>
{
    Task AddAsync(EventStoreEntry<TStreamIdentifier> @event);
    void Add(EventStoreEntry<TStreamIdentifier> @event);
    Task<long> GetStreamVersion(TStreamIdentifier streamId);
    Task AddManyAsync(IEnumerable<EventStoreEntry<TStreamIdentifier>> events);
    Task<List<EventStoreEntry<TStreamIdentifier>>> GetEventsAsync(
        TStreamIdentifier streamId);
    Task<List<EventStoreEntry<TStreamIdentifier>>> GetEventsAsync(
        TStreamIdentifier streamId, string streamType);
    Task SaveChangesAsync(CancellationToken cancellation = default);
}
