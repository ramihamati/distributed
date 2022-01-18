using MassTransit;

namespace ComX.Infrastructure.Distributed.Outbox;

public class OutboxMassTransitPublisher : IOutboxBrokerPublisher
{
    #region [ Fields ]
    private readonly IBusControl _busControl;
    #endregion

    #region [ Properties ]
    #endregion

    #region [ Constructor ]
    public OutboxMassTransitPublisher(
        IBusControl busControl)
    {
        _busControl = busControl;
    }
    #endregion

    #region [ Methods ]
    public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
    {
        return _busControl.Publish<T>(message, cancellationToken);
    }
    #endregion
}
