using MassTransit;

namespace ComX.Infrastructure.Distributed.Outbox;

public class OutboxMassTransitPublisher : IOutboxBrokerPublisher
{
    private readonly IBusControl _busControl;

    public OutboxMassTransitPublisher(
        IBusControl busControl)
    {
        _busControl = busControl;
    }

    public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
    {
        return _busControl.Publish<T>(message, cancellationToken);
    }
}
