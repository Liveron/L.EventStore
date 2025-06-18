using L.EventStore.Abstractions;
using L.EventStore.EntityFramework.PostgreSQL.DataModel;
using Microsoft.EntityFrameworkCore;

namespace L.EventStore.EntityFramework.PostgreSQL.FunctionalTests;

[CollectionDefinition("PostgreSQL EventStore Collection")]
public sealed class PostgreEventStoreCollection : ICollectionFixture<PostgreEventStoreFixture>;

public sealed class PostgreEventStoreFixture : IDisposable
{
    private readonly string _connectionString =
        "Host=localhost;Port=5432;Database=EventStoreTest;Username=postgres;Password=postgres";

    public TestDbContext DbContext { get; }
    public IEventStoreRepository<Guid> Repository { get; }
    public IEventStore<Guid> EventStore { get; }

    public PostgreEventStoreFixture()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        DbContext = new TestDbContext(options);
        DbContext.Database.EnsureCreated();

        Repository = new EventStoreRepository<TestDbContext, Guid>(DbContext);
        EventStore = new EventStore<Guid>(Repository);
    }

    public void Dispose()
    {
        DbContext.Database.EnsureDeleted();
        EventStore.Dispose();
    }
}

public sealed class TestDbContext(DbContextOptions<TestDbContext> options)
    : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AddEventStore<Guid>("event_store_test");
    }
}

