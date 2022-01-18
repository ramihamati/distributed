using System;

namespace ComX.Infrastructure.Distributed.Outbox
{
    /// <summary>
    /// Used to link the event type to a discriminator name. Usefull for serialization/deserialization
    /// </summary>
    public class RegistryMessageInfo
    {
        /// <summary>
        /// The message contract
        /// </summary>
        public Type MessageType { get; }

        public string Name { get; }

        public RegistryMessageInfo(Type messageType,  string name)
        {
            MessageType = messageType;
            Name = name;
        }
    }
}
