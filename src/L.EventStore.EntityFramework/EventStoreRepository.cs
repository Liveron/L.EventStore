using L.EventStore.Abstractions;
using L.EventStore.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace L.EventStore.EntityFramework;

public sealed class EventStoreRepository<TStreamIdentifier, TContext>(TContext context) 
    : IEventStoreRepository<TStreamIdentifier> where TContext : DbContext
    where TStreamIdentifier : IComparable<TStreamIdentifier>, IEquatable<TStreamIdentifier>
{
    private readonly TContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public void Add(EventEntry<TStreamIdentifier> @event)
    {
        _context.Set<EventEntry<TStreamIdentifier>>()
            .Add(@event);
    }

    public async Task AddAsync(EventEntry<TStreamIdentifier> @event)
    {
        await _context.Set<EventEntry<TStreamIdentifier>>()
            .AddAsync(@event);
    }

    public async Task AddManyAsync(IEnumerable<EventEntry<TStreamIdentifier>> events)
    {
        await _context.Set<EventEntry<TStreamIdentifier>>()
            .AddRangeAsync(events);
    }

    public async Task<List<EventEntry<TStreamIdentifier>>> GetEventsAsync(TStreamIdentifier streamId)
    {
        return await _context.Set<EventEntry<TStreamIdentifier>>()
            .AsNoTracking()
            .Where(e => e.StreamId.Equals(streamId))
            .OrderBy(e => e.Version)
            .ToListAsync();
    }

    public async Task<List<EventEntry<TStreamIdentifier>>> GetEventsAsync(TStreamIdentifier streamId, string streamType)
    {
        return await _context.Set<EventEntry<TStreamIdentifier>>()
            .AsNoTracking()
            .Where(e => e.StreamId.Equals(streamId) && e.StreamType == streamType)
            .OrderBy(e => e.Version)
            .ToListAsync();
    }

    public async Task<long> GetStreamVersion(TStreamIdentifier streamId)
    {
        return await _context.Set<EventEntry<TStreamIdentifier>>()
            .AsNoTracking()
            .Where(e => e.StreamId.Equals(streamId))
            .MaxAsync(e => (long?)e.Version) ?? 0;
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
