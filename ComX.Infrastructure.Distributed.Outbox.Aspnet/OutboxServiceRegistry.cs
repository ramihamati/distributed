using System;
using System.Collections.Generic;

namespace ComX.Infrastructure.Distributed.Outbox;

internal class OutboxServiceRegistry : IOutboxServiceRegistry
{
    private List<RegistryMessageInfo> MessageInfos { get; }

    public OutboxServiceRegistry(List<RegistryMessageInfo> messages)
    {
        MessageInfos = messages ?? new List<RegistryMessageInfo>();
    }

    public RegistryMessageInfo GetInfoFor<TMessage>()
    {
        Type type = typeof(TMessage);
        return MessageInfos.Find(r => r.MessageType.Equals(type));
    }

    public RegistryMessageInfo GetInfoFor(string name)
    {
        return MessageInfos.Find(r => r.Name.Equals(name));
    }
}
