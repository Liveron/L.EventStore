using L.EventStore.Abstractions;

namespace L.EventStore.EntityFramework.PostgreSQL.FunctionalTests;

[Collection("Guid EventStore Collection")]
public sealed class GuidEventStoreTests(GuidEventStoreFixture fixture) 
{
    private readonly IEventStore<Guid, IEvent> _eventStore = fixture.EventStore;

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

    private static TestEvent CreateTestEvent(string data, long version = 1)
    {
        return new TestEvent(version, data);
    }
}

public sealed record TestEvent(long Version, string Data) : IEvent;
