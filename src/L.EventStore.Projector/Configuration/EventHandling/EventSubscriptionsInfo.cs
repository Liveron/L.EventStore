using L.EventStore.Projector.Abstractions;

namespace L.EventStore.Projector.Configuration.EventHandling;

public sealed class EventSubscriptionsInfo
{
    public Dictionary<string, Type> EventTypes { get; } = [];
    public Dictionary<Type, List<IEventHandler>> EventHandlers { get; } = [];
}
