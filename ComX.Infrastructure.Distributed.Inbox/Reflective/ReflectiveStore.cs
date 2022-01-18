using System.Reflection;

namespace ComX.Infrastructure.Distributed.Inbox;

public class ReflectiveStore
{
    private readonly object store;
    private readonly MethodInfo _saveAsyncMethod;

    public ReflectiveStore(object store)
    {
        this.store = store;

        //TODO: this check does not look for parent types
        if (!store.GetType()
                .GetInterfaces()
                .Any(r=> r.IsGenericType && r.GetGenericTypeDefinition().Equals(typeof(IStore<>))))
        {
            throw new ArgumentException($"The provided store is not of type {typeof(IStore<>).FullName}");
        }

        _saveAsyncMethod = store
            .GetType()
            .GetMethod(nameof(IStore<object>.SaveAsync), BindingFlags.Public | BindingFlags.Instance)
            ?? throw new NullReferenceException($"Could not extract the method {nameof(IStore<object>)}.{nameof(IStore<object>.SaveAsync)}");
    }

    public async Task SaveAsync(object model)
    {
        await (Task)(_saveAsyncMethod.Invoke(store, new object[] { model }) 
            ?? throw new NullReferenceException("The method SaveAsync did not return a task"));
    }
}
