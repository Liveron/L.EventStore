using L.EventStore.Projector.Abstractions;

namespace L.EventStore.Projector.Configuration.EventHandling;

public interface IProjectorSubscriptionsConfigurator
{
    public void AddSubscription<TEvent, TEventHandler>()
        where TEventHandler : class, IEventHandler<TEvent>;
}
