using L.EventStore.Abstractions;
using L.EventStore.Projector.Abstractions;

namespace L.EventStore.Projector.Services;

internal sealed class EventEntriesService<TStreamIdentifier>(
    IGetUnprocessedEventEntriesStrategy<TStreamIdentifier> getUnprocessedEventsStrategy)
    : IEventEntriesService<TStreamIdentifier>
    where TStreamIdentifier : IComparable<TStreamIdentifier>, IEquatable<TStreamIdentifier>
{
    private readonly IGetUnprocessedEventEntriesStrategy<TStreamIdentifier> _getUnprocessedEventsStrategy
        = getUnprocessedEventsStrategy ?? throw new ArgumentNullException(nameof(getUnprocessedEventsStrategy));

    public async Task<List<EventEntry<TStreamIdentifier>>> GetUnprocessedEventEntriesAsync()
    {
        return await _getUnprocessedEventsStrategy.GetUnprocessedEventEntriesAsync();
    }
}
