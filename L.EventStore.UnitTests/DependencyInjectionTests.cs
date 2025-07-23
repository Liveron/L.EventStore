using L.EventStore.Abstractions;
using L.EventStore.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace L.EventStore.UnitTests;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddEventStore_ShouldRegisterGuidEventStore_WhenStreamIdNotPresent()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var mockRepo = new Mock<IEventStoreRepository<Guid>>();
        services.AddScoped(_ => mockRepo.Object);
        services.AddEventStore();
        var provider = services.BuildServiceProvider();

        // Assert
        var service = provider.GetService<IEventStore<Guid>>();

        Assert.NotNull(service);
        Assert.IsType<EventStore<Guid>>(service);
    }

    [Fact]
    public void AddEventStore_ShouldRegisterTypedWithEventEventStore_WhenEventTypePresent()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var mockRepo = new Mock<IEventStoreRepository<Guid>>();
        services.AddScoped(_ => mockRepo.Object);
        services.AddEventStore(config => config.SetEventType<ITestEvent>());
        var provider = services.BuildServiceProvider();

        // Assert
        var service = provider.GetService<IEventStore<Guid>>();
        var service2 = provider.GetService<IEventStore<Guid, ITestEvent>>();

        Assert.NotNull(service);
        Assert.NotNull(service2);

        Assert.IsType<EventStore<Guid, ITestEvent>>(service);
        Assert.IsType<EventStore<Guid, ITestEvent>>(service2);

        Assert.Equivalent(service, service2);
    }

    [Fact]
    public void AddEventStore_ShouldRegisterRegisterTypesFromAssembly_WhenEventTypePresent()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockRepo = new Mock<IEventStoreRepository<Guid>>();
        services.AddScoped(_ => mockRepo.Object);

        // Act
        services.AddEventStore(config => config.SetEventType<ITestEvent>());
        var provider = services.BuildServiceProvider();

        // Assert
        var service = provider.GetService<IEventTypeRepository>();

        Assert.NotNull(service);
        
        var firstEventType = service.GetEventType(typeof(FirstTestEvent).Name);
        var secondEventType = service.GetEventType(typeof(SecondTestEvent).Name);

        Assert.Equal(typeof(FirstTestEvent), firstEventType);
        Assert.Equal(typeof(SecondTestEvent), secondEventType);
    }
}

public interface ITestEvent;

public sealed record FirstTestEvent : ITestEvent;

public sealed record SecondTestEvent : ITestEvent;
