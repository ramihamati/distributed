namespace ComX.Infrastructure.Distributed.Outbox;

public interface IOutboxServiceProvider
{
    IOutboxService GetService(string name);
    IOutboxServiceContainer GetServiceContainer(string name);
}
