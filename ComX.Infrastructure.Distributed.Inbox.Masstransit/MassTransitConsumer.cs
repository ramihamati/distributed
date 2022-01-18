using ComX.Infrastructure.Distributed.Inbox;
using MassTransit;

namespace ComX.Infrastructure.Distributed.Inbox.Masstransit;

// TODO : handle retry in case that the consumer fails to save
// TODO : metrics on failed rate
public class MassTransitConsumer<TEvent> : IConsumer<TEvent> where TEvent : class
{
    private readonly IGenericConsumerFactory<TEvent> _consumerFactory;

    public MassTransitConsumer(IGenericConsumerFactory<TEvent> consumerFactory)
    {
        _consumerFactory = consumerFactory;
    }

    public async Task Consume(ConsumeContext<TEvent> context)
    {
        try
        {
            IGenericConsumer<TEvent> genericConsumer = _consumerFactory.Create();
            await genericConsumer.Consume(context.Message);
        }
      
        catch (MassTransitException mex) when (mex.InnerException is Exception ex)
        {
            throw ex;
        }
        catch (Exception ex)
        {
            var a = 1;
        }
    }
}
