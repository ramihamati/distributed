using MassTransit.Mediator;

namespace ComX.Infrastructure.Distributed.Outbox;

public class OutboxMassTransitMediatorPublisher : IOutboxBrokerPublisher
{
    private readonly IMediator _mediator;

    public OutboxMassTransitMediatorPublisher(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
    {
        return _mediator.Publish<T>(message, cancellationToken);
    }
}