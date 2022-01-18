namespace ComX.Infrastructure.Distributed.Outbox
{
    public interface IOutboxServiceRegistryBuilder
    {
        void RegisterMessage<TMessageType>(string name);
    }
}
