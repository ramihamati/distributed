using System;
using System.Collections.Generic;
using System.Linq;

namespace ComX.Infrastructure.Distributed.Outbox
{
    internal class OutboxServiceRegistryBuilder : IOutboxServiceRegistryBuilder
    {
        private List<RegistryMessageInfo> MessageInfos { get; }

        public OutboxServiceRegistryBuilder()
        {
            MessageInfos = new List<RegistryMessageInfo>();
        }

        public void RegisterMessage<TMessageType>(string name)
        {
            if (MessageInfos.Any(r => r.Name == name))
            {
                throw new Exception($"A message with the outbox name {name} is already registered");
            }

            MessageInfos.Add(new RegistryMessageInfo(typeof(TMessageType), name, typeof(IntegrationMessageLog)));
        }

        public void RegisterMessage<TMessageType, TMessageLog>(string name)
            where TMessageLog : class, IIntegrationMessageLog
        {
            if (MessageInfos.Any(r => r.Name == name))
            {
                throw new Exception($"A message with the outbox name {name} is already registered");
            }

            MessageInfos.Add(new RegistryMessageInfo(typeof(TMessageType), name, typeof(TMessageLog)));
        }

        public IOutboxServiceRegistry Build()
        {
            return new OutboxServiceRegistry(MessageInfos);
        }
    }
}
