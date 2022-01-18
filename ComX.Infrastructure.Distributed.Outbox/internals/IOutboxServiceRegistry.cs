namespace ComX.Infrastructure.Distributed.Outbox
{
    /// <summary>
    /// Service that binds a name to an event. Required for 
    /// serialization/deserialization
    /// </summary>
    public interface IOutboxServiceRegistry
    {
        RegistryMessageInfo GetInfoFor<TMessage>();
        RegistryMessageInfo GetInfoFor(string name);
    }
}
