using L.EventStore.Abstractions;
using L.EventStore.Projector.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace L.EventStore.Projector.EntityFramework.PostgreSQL.Strategies;

internal sealed class GetUnprocessedEventEntriesTimeLagStrategy<TStreamIdentifier>(DbContext context)
    : IGetUnprocessedEventEntriesStrategy<TStreamIdentifier> where TStreamIdentifier
    : IEquatable<TStreamIdentifier>, IComparable<TStreamIdentifier>
{
    private readonly DbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<List<EventEntry<TStreamIdentifier>>> GetUnprocessedEventEntriesAsync()
    {
        var idCounter = await GetOrCreateIdCounterAsync();

        return await GetUnprocessedEventEntriesWithTimeLagAsync(idCounter.LastProcessedId);
    }

    private async Task<IdCounterEntity> GetOrCreateIdCounterAsync()
    {
        var idCounter = await GetIdCounterAsync();

        if (idCounter is null)
        {
            var firstIdCounter = new IdCounterEntity();
            CreateIdCounter(firstIdCounter);
            return firstIdCounter;
        }

        return idCounter;
    }

    private async Task<IdCounterEntity?> GetIdCounterAsync()
    {
        return await _context.Set<IdCounterEntity>()
            .FirstOrDefaultAsync();
    }

    private void CreateIdCounter(IdCounterEntity idCounter)
    {
        _context.Set<IdCounterEntity>()
            .Add(idCounter);
    }

    private async Task<List<EventEntry<TStreamIdentifier>>> GetUnprocessedEventEntriesWithTimeLagAsync(
        long lastProcessedId)
    {
        return await _context.Set<EventEntry<TStreamIdentifier>>()
            .AsNoTracking()
            .Where(e => e.Id > lastProcessedId && e.CreatedAt < DateTime.UtcNow.AddSeconds(-5))
            .OrderBy(e => e.Id)
            .ToListAsync();
    }
}
