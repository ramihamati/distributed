using Microsoft.Extensions.DependencyInjection;

namespace ComX.Infrastructure.Distributed.Inbox;

public class GenericConsumerFactory<TEvent> : IGenericConsumerFactory<TEvent> where TEvent: class
{
    private readonly IServiceProvider _services;

    public GenericConsumerFactory(IServiceProvider services)
    {
        _services = services;
    }

    public IGenericConsumer<TEvent> Create()
    {
        return ActivatorUtilities.CreateInstance<GenericConsumer<TEvent>>(_services);
    }
}