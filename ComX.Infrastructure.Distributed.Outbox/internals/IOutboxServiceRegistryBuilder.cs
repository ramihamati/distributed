namespace ComX.Infrastructure.Distributed.Outbox
{
    public interface IOutboxServiceRegistryBuilder
    {
        void RegisterMessage<TMessageType>(string name);
        void RegisterMessage<TMessageType, TMessageLog>(string name) where TMessageLog : class, IIntegrationMessageLog;
    }
}
