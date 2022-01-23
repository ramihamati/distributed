namespace ComX.Infrastructure.Distributed.Outbox;

/// <summary>
/// Used to link the event type to a discriminator name. Usefull for serialization/deserialization
/// </summary>
public class RegistryMessageInfo
{
    /// <summary>
    /// The message contract
    /// </summary>
    public Type MessageType { get; }

    /// <summary>
    /// The name of the registry information
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The type of the repository used when handling this particular event. If null
    /// the default type is <see cref="IntegrationMessageLog"/>
    /// </summary>
    public Type MessageLogType { get; }

    /// <summary>
    /// Information binding the <see cref="MessageLogType"/> which lets the outbox know what repostory to use
    /// when saving / reading the message and <see cref="MessageType"/> which is the event we are working with
    /// </summary>
    /// <param name="messageType">The type of the event we are handling</param>
    /// <param name="name">The name of the registry information</param>
    /// <param name="messageLogType">The type of the repository used when handling this particular event. If null
    /// the default type is <see cref="IntegrationMessageLog"/></param>
    public RegistryMessageInfo(
        Type messageType,
        string name,
        Type messageLogType)
    {
        MessageType = messageType;
        Name = name;
        MessageLogType = messageLogType;
    }
}
