using Microsoft.Extensions.DependencyInjection;

namespace ComX.Infrastructure.Distributed.Inbox;

public class InboxService
{
    private readonly IServiceCollection _serviceCollection;
    private readonly IEventTypeRegistry _eventTypes;
    private readonly ReflectiveIInboxBusConfigurator _reflectiveIInboxBusConfigurator;

    public InboxService(
        IEventTypeRegistry eventTypeRegistry,
        IInboxBusConfigurator inboxBusConfigurator,
        IServiceCollection serviceCollection)
    {
        _eventTypes = eventTypeRegistry;
        _serviceCollection = serviceCollection;
        _reflectiveIInboxBusConfigurator = new ReflectiveIInboxBusConfigurator(inboxBusConfigurator);
    }

    public void Register()
    {
        foreach (EventTypeInfo eventInfo in _eventTypes.GetEventTypes())
        {
            // register the consumers
            _reflectiveIInboxBusConfigurator.RegisterConsumer(eventInfo.EventType);

            /// <see cref="IGenericConsumerFactory{TEvent}"/> is required by 
            /// <see cref="ComX.Infrastructure.Distributed.Inbox.MassTransitConsumer"/>
            Type genericConsumerFactoryType = typeof(IGenericConsumerFactory<>).MakeGenericType(eventInfo.EventType);

            _serviceCollection.AddScoped(
                genericConsumerFactoryType,
                sp =>
                {
                    IGenericConsumerAbstractFactory consumerAbstractFactory
                        = sp.GetRequiredService<IGenericConsumerAbstractFactory>();

                    ReflectiveGenericConsumerAbstractFactory reflective = new(consumerAbstractFactory);
                    return reflective.Create(eventInfo.EventType);
                });
        }
    }
}