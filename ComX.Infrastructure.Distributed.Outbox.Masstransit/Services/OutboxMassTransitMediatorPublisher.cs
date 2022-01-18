using MassTransit.Mediator;

namespace ComX.Infrastructure.Distributed.Outbox;

public class OutboxMassTransitMediatorPublisher : IOutboxBrokerPublisher
{
    #region [ Fields ]
    private readonly IMediator _mediator;
    #endregion

    #region [ Properties ]
    #endregion

    #region [ Constructor ]
    public OutboxMassTransitMediatorPublisher(
        IMediator mediator)
    {
        _mediator = mediator;
    }
    #endregion

    #region [ Methods ]
    public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
    {
        return _mediator.Publish<T>(message, cancellationToken);
    }
    #endregion
}