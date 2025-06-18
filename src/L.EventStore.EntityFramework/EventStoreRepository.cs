using L.EventStore.Abstractions;
using L.EventStore.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace L.EventStore.EntityFramework;

public sealed class EventStoreRepository<TContext, TEventStreamId>(TContext context) 
    : IEventStoreRepository<TEventStreamId>
    where TEventStreamId : IEquatable<TEventStreamId>, 
    IComparable<TEventStreamId>
    where TContext : DbContext
{
    private readonly TContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public void Add(EventStoreEntry<TEventStreamId> @event)
    {

         _context.Set<EventStoreEntry<TEventStreamId>>()
             .Add(@event);
    }

    public async Task AddAsync(EventStoreEntry<TEventStreamId> @event)
    {
         await _context.Set<EventStoreEntry<TEventStreamId>>()
             .AddAsync(@event);
    }

    public async Task AddManyAsync(IEnumerable<EventStoreEntry<TEventStreamId>> events)
    {      
        await _context.Set<EventStoreEntry<TEventStreamId>>()
            .AddRangeAsync(events);
    }

    public async Task<List<EventStoreEntry<TEventStreamId>>> GetEventsAsync(
        TEventStreamId streamId, string streamType)
    {
        return await _context.Set<EventStoreEntry<TEventStreamId>>()
            .AsNoTracking()
            .Where(e => e.StreamId.Equals(streamId) && e.StreamType.Equals(streamType))
            .OrderBy(e => e.Version)
            .ToListAsync();
    }

    public async Task<List<EventStoreEntry<TEventStreamId>>> GetEventsAsync(
        TEventStreamId streamId)
    {
        return await _context.Set<EventStoreEntry<TEventStreamId>>()
            .AsNoTracking()
            .Where(e => e.StreamId.Equals(streamId))
            .OrderBy(e => e.Version)
            .ToListAsync();
    }

    public async Task SaveChangesAsync(CancellationToken cancellation = default)
    {
        try
        {
            await _context.SaveChangesAsync(cancellation);
        }
        catch (DbUpdateException)
        {
            _context.ChangeTracker.Clear();
            throw new ConcurrencyException();
        }
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
    }
}
