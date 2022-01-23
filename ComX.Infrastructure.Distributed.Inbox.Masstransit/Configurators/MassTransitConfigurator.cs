using ComX.Common.MassTransitCommon;
using MassTransit;

namespace ComX.Infrastructure.Distributed.Inbox.Masstransit;

public class MassTransitConfigurator : IMassTransitConfigurator
{
    private IEndpointNameFormatter? _endpointNameFormatter;
    private readonly Dictionary<Type, ChannelNameAttribute> _preregistered;

    
    public IBusRegistrationConfigurator Cfg { get; }

    public MassTransitConfigurator(
        IBusRegistrationConfigurator busRegistrationConfigurator)
    {
        _preregistered = new Dictionary<Type, ChannelNameAttribute>();
        Cfg = busRegistrationConfigurator;
    }

    /// <summary>
    /// Registers channel name for <see cref="MassTransitConsumer{TEvent}"/>
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    public void RegisterChannelNameForConsumer<TEvent>(string channelName) where TEvent : class
    {
        if (_preregistered.Any(r => r.Key.Equals(typeof(MassTransitConsumer<TEvent>))))
        {
            throw new Exception($"The consumer channel type for MassTransitConsumer<{typeof(TEvent).Name}> was already registered");
        }

        if (_preregistered.Any(r => string.Equals(r.Value.Name, channelName, StringComparison.InvariantCultureIgnoreCase)))
        {
            throw new Exception($"The consumer channel name for MassTransitConsumer<{typeof(TEvent).Name}> was already registered");
        }

        _preregistered.Add(typeof(MassTransitConsumer<TEvent>), new ChannelNameAttribute(channelName));
    }

    public IEndpointNameFormatter GetEndpointNameFormatter()
    {
        // because we need the preregistered list after all registrations occured
        // this will ensure that when 
        return _endpointNameFormatter ??= new ComXEndpointNameFormatter(_preregistered);
    }

}
