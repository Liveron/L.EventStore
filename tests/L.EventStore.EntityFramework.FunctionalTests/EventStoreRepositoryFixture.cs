using L.EventStore.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace L.EventStore.EntityFramework.FunctionalTests;

public sealed class EventStoreRepositoryFixture : IDisposable
{
    private readonly TestDbContext _context;
    public IEventStoreRepository<Guid> Repository { get; private set; }

    public EventStoreRepositoryFixture()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase("EventStoreTestDb")
            .Options;

        _context = new TestDbContext(options);
        Repository = new EventStoreRepository<TestDbContext, Guid>(_context);
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        Repository.Dispose();
    }
}

public sealed class TestDbContext(DbContextOptions<TestDbContext> options) 
    : DbContext(options)
{    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EventStoreEntry<Guid>>(builder =>
        {
            builder.HasKey(e => new { e.StreamId, e.StreamType, e.Version });
        });
    }
}
