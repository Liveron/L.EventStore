using L.EventStore.Abstractions;
using L.EventStore.Exceptions;

namespace L.EventStore.EntityFramework.PostgreSQL.FunctionalTests;

[Collection("Guid EventStore Collection")]
public sealed class GuidEventStoreTests(GuidEventStoreFixture fixture) 
{
    private readonly IEventTypeRepository _eventTypeRepository = fixture.EventTypeRepository;
    private readonly IEventStore<Guid> _eventStore = fixture.EventStore;

    [Fact]
    public async Task SaveEventsAsync_ShouldPersistEvents()
    {
        // Arrange
        var streamId = Guid.NewGuid();
        var events = new List<object> { CreateTestEvent("Test Event 1") };
        var eventStreamOptions = new EventStreamOptions<Guid>
        {
            StreamId = streamId,
            StreamType = "TestStream"
        };

        // Act
        _eventTypeRepository.AddEventTypeIfNotExist(typeof(TestEvent).Name, typeof(TestEvent));
        await _eventStore.SaveEventsAsync(events, eventStreamOptions);
        var loadedEvents = await _eventStore.GetEventStreamAsync(streamId);

        // Assert
        Assert.Single(loadedEvents);
        var firstEvent = Assert.IsType<TestEvent>(loadedEvents[0]);
        Assert.Equal(1, firstEvent.Version);
        Assert.Equal("Test Event 1", firstEvent.Data);
    }

    [Fact]
    public async Task SaveEventsAsync_ShouldThrowConcurrencyException_WhenVersionNotCorrect()
    {
        // Arrange 
        var streamId = Guid.NewGuid();
        var events = new List<object> { CreateTestEvent("Test Event 1") };
        var eventStreamOptions = new EventStreamOptions<Guid>
        {
            StreamId = streamId,
            StreamType = "TestStream"
        };
        await _eventStore.SaveEventsAsync(events, eventStreamOptions);

        // Act & Assert
        var newEvents = new List<object> { CreateTestEvent("Test Event 2") };
        var newEventStreamOptions = new EventStreamOptions<Guid>
        {
            StreamId = streamId,
            StreamType = "TestStream",
            ExpectedVersion = 2
        };

        await Assert.ThrowsAsync<ConcurrencyException>(async () => 
            await _eventStore.SaveEventsAsync(newEvents, newEventStreamOptions));
    }

    private static TestEvent CreateTestEvent(string data, long version = 1)
    {
        return new TestEvent(version, data);
    }
}

public sealed record TestEvent(long Version, string Data);
