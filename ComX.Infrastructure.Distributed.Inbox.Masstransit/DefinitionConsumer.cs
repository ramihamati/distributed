using GreenPipes;
using MassTransit;
using MassTransit.ConsumeConfigurators;
using MassTransit.Definition;

namespace ComX.Infrastructure.Distributed.Inbox.Masstransit;

public class DefinitionConsumer<TConsumer> : ConsumerDefinition<TConsumer>
      where TConsumer : class, IConsumer
{
    #region [ Fields ]
    #endregion

    #region [ Properties ]
    #endregion

    #region [ Constructor ]
    #endregion

    #region [ Methods ]
    protected override void ConfigureConsumer(
     IReceiveEndpointConfigurator endpointConfigurator,
     IConsumerConfigurator<TConsumer> consumerConfigurator)
    {
        endpointConfigurator.UseMessageRetry(
            r => r.Intervals(
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10)));
    }
    #endregion
}
