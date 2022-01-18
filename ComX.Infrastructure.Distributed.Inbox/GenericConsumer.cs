namespace ComX.Infrastructure.Distributed.Inbox;

public class GenericConsumer<TEvent> : IGenericConsumer<TEvent> where TEvent : class
{
    private readonly ITransformerService _transformerService;
    private readonly IStoreProvider _storeProvider;
    private readonly ReflectiveStoreProvider _reflectiveStoreProvider;

    public GenericConsumer(
        ITransformerService transformerService,
        IStoreProvider storeProvider)
    {
        _transformerService = transformerService;
        _storeProvider = storeProvider;
        _reflectiveStoreProvider = new (_storeProvider);
    }

    public async Task Consume(TEvent message)
    {
        if (_transformerService.HasMap<TEvent>())
        {
            // look for a store with the transformed type
            object transformed = _transformerService.Transform(message);
            Type type = transformed.GetType();
            ReflectiveStore reflectiveStore = _reflectiveStoreProvider.GetStore(type)
                ?? throw new NullReferenceException($"Could not find a store for the type {type.FullName}");
            await reflectiveStore.SaveAsync(transformed);
        }
        else
        {
            IStore<TEvent> store = _storeProvider.GetStore<TEvent>()
                ?? throw new NullReferenceException($"Could not find a store for the type {typeof(TEvent).FullName}");
            await store.SaveAsync(message);
        }
    }
}
