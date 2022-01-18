using System.Reflection;

namespace ComX.Infrastructure.Distributed.Inbox;

public class ReflectiveStoreProvider
{
    private readonly IStoreProvider _storeProvider;
    private readonly MethodInfo _getStoreMethod;

    public ReflectiveStoreProvider(
        IStoreProvider storeProvider)
    {
        _storeProvider = storeProvider;
        _getStoreMethod = _storeProvider
              .GetType()
              .GetMethod(nameof(IStoreProvider.GetStore), BindingFlags.Public | BindingFlags.Instance)
              ?? throw new NullReferenceException($"Could not get method {nameof(IStoreProvider)}.{nameof(IStoreProvider.GetStore)}");
    }

    public ReflectiveStore? GetStore(Type modelType)
    {
        object store = _getStoreMethod
               .MakeGenericMethod(modelType)
               .Invoke(_storeProvider, Array.Empty<object>());

        if (store is null)
        {
            return null;
        }

        return new ReflectiveStore(store);
    }
}
