using L.EventStore.Abstractions;

namespace L.EventStore.Abstractions;

public interface IEventStoreRepository<TStreamIdentifier> : IDisposable, IAsyncDisposable
    where TStreamIdentifier : IComparable<TStreamIdentifier>, IEquatable<TStreamIdentifier>
{
    Task AddAsync(EventEntry<TStreamIdentifier> @event);
    void Add(EventEntry<TStreamIdentifier> @event);
    Task<long> GetStreamVersion(TStreamIdentifier streamId);
    Task AddManyAsync(IEnumerable<EventEntry<TStreamIdentifier>> events);
    Task<List<EventEntry<TStreamIdentifier>>> GetEventsAsync(
        TStreamIdentifier streamId);
    Task<List<EventEntry<TStreamIdentifier>>> GetEventsAsync(
        TStreamIdentifier streamId, string streamType);
    Task SaveChangesAsync(CancellationToken cancellation = default);
}