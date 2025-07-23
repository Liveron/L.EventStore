using dotenv.net;
using L.EventStore.Abstractions;
using L.EventStore.EntityFramework.PostgreSQL.DataModel;
using Microsoft.EntityFrameworkCore;

namespace L.EventStore.EntityFramework.PostgreSQL.FunctionalTests;

[CollectionDefinition("Guid EventStore Collection")]
public sealed class GuidEventStoreCollection : ICollectionFixture<GuidEventStoreFixture>;

public sealed class GuidEventStoreFixture : IDisposable
{
    private const string _environmentEVN = "DOTNET_ENVIRONMENT";
    private const string _connectionStringEVN = "POSTGRESQL_DB";

    public TestDbContext DbContext { get; }
    public IEventStoreRepository<Guid> EventStoreRepository { get; }
    public IEventTypeRepository EventTypeRepository { get; }
    public IEventStore<Guid> EventStore { get; }

    public GuidEventStoreFixture()
    {
        var environment = Environment.GetEnvironmentVariable(_environmentEVN)
            ?? throw new InvalidOperationException($"Environment variable {_environmentEVN} isn't set");

        if (environment == "Development")
            DotEnv.Load();

        var connectionString = Environment.GetEnvironmentVariable(_connectionStringEVN)
            ?? throw new InvalidOperationException($"Environment variable {_connectionStringEVN} isn't set");

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        DbContext = new TestDbContext(options);
        DbContext.Database.EnsureCreated();

        EventStoreRepository = new EventStoreRepository<Guid, TestDbContext>(DbContext);
        EventTypeRepository = new EventTypeRepository();
        EventStore = new EventStore<Guid>(EventStoreRepository, EventTypeRepository);
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

