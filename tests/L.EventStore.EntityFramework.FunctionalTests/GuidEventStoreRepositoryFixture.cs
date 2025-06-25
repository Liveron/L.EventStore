using L.EventStore.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace L.EventStore.EntityFramework.FunctionalTests;

public sealed class GuidEventStoreRepositoryFixture : IDisposable
{
    public TestDbContext Context { get; private set; }
    public IEventStoreRepository<Guid> Repository { get; private set; }

    public GuidEventStoreRepositoryFixture()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase("EventStoreTestDb")
            .Options;

        Context = new TestDbContext(options);
        Repository = new EventStoreRepository<Guid, TestDbContext>(Context);
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context.Database.EnsureDeleted();
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
            builder.HasKey(e => new { e.StreamId, e.Version });
        });
    }
}
