using MassTransit;

namespace ComX.Infrastructure.Distributed.Inbox.Masstransit;

public interface IMassTransitConfigurator
{
    IBusRegistrationConfigurator Cfg { get; }

    /// <summary>
    /// Uses the EndPoitnNameFOrmatter and adds channel names for the 
    /// <see cref="MassTransitConsumer{TEvent}"/>
    /// </summary>
    IEndpointNameFormatter GetEndpointNameFormatter();


    /// <summary>
    /// Registers the channel names for <see cref="MassTransitConsumer{TEvent}"/>
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    /// <param name="channelName"></param>
    void RegisterChannelNameForConsumer<TEvent>(string channelName) where TEvent : class;
}
