using L.EventStore.Abstractions;

namespace L.EventStore.EntityFramework.PostgreSQL.FunctionalTests;

[Collection("PostgreSQL EventStore Collection")]
public sealed class EventStoreTests(PostgreEventStoreFixture fixture) 
{
    private readonly IEventStore<Guid> _eventStore = fixture.EventStore;

    [Fact]
    public async Task SaveEventsAsync_ShouldPersistEvents()
    {
        // Arrange
        var streamId = Guid.NewGuid();
        var events = new List<IEvent> { CreateTestEvent("Test Event 1") };

        // Act
        await _eventStore.SaveEventsAsync(events, streamId, "TestStream");
        var loadedEvents = await _eventStore.GetEventStreamAsync(streamId);

        // Assert
        Assert.Single(loadedEvents);
        var firstEvent = Assert.IsType<TestEvent>(loadedEvents[0]);
        Assert.Equal(1, firstEvent.Version);
        Assert.Equal("Test Event 1", firstEvent.Data);
    }

    [Fact]
    public async Task SaveEvents_ShouldMaintainEventOrder()
    {
        // Arrange
        var streamId = Guid.NewGuid();
        var events = new IEvent[]
        {
            CreateTestEvent("Test Event 1", 1),
            CreateTestEvent("Test Event 2", 2),
            CreateTestEvent("Test Event 3", 3)
        };

        // Act
        await _eventStore.SaveEventsAsync(events, streamId, "TestStream");
        var loadedEvents = await _eventStore.GetEventStreamAsync(streamId);

        // Assert
        Assert.Collection(loadedEvents,
            e => Assert.Equal(1, e.Version),
            e => Assert.Equal(2, e.Version),
            e => Assert.Equal(3, e.Version));
    }

    private static TestEvent CreateTestEvent(string data, long version = 1)
    {
        return new TestEvent(version, data);
    }
}

public sealed record TestEvent(long Version, string Data) : IEvent;
