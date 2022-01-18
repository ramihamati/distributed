using Microsoft.Extensions.DependencyInjection;

namespace ComX.Infrastructure.Distributed.Inbox;

public class GenericConsumerAbstractFactory : IGenericConsumerAbstractFactory
{
    private readonly IServiceProvider _services;

    public GenericConsumerAbstractFactory(
        IServiceProvider services)
    {
        _services = services;
    }

    public IGenericConsumerFactory<TEvent> Create<TEvent>() where TEvent : class
    {
        return ActivatorUtilities.CreateInstance<GenericConsumerFactory<TEvent>>(_services);
    }
}
