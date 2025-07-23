using L.EventStore.Projector.Abstractions;
using L.EventStore.Projector.Configuration.EventHandling;
using Microsoft.Extensions.DependencyInjection;

namespace L.EventStore.Projector.DependencyInjection.Configuration;

public class DependencyInjectionEventStoreProjectorConfigurator(IServiceCollection services)
    : IDependencyInjectionEventStoreProjectorConfigurator
{
    public IServiceCollection Services => services;

    public void AddSubscription<TEvent, TEventHandler>()
        where TEventHandler : class, IEventHandler<TEvent>
    {
        Services.Configure<EventSubscriptionsInfo>(info =>
        {
            info.EventTypes[typeof(TEvent).Name] = typeof(TEvent);
            info.EventHandlers[typeof(TEvent)].Add(Activator.CreateInstance<TEventHandler>());
        });
    }
}
