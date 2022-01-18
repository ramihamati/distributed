using System;
using System.Collections.Generic;
using System.Linq;

namespace ComX.Infrastructure.Distributed.Outbox
{
    internal class OutboxServiceRegistryBuilder : IOutboxServiceRegistryBuilder
    {
        #region [ Fields ]
        private List<RegistryMessageInfo> MessageInfos { get; }
        #endregion

        #region [ Properties ]

        #endregion

        #region [ Constructor ]
        public OutboxServiceRegistryBuilder()
        {
            MessageInfos = new List<RegistryMessageInfo>();
        }
        #endregion

        #region [ Methods ]
        public void RegisterMessage<TMessageType>(string name)
        {
            if (MessageInfos.Any(r => r.Name == name))
            {
                throw new Exception($"A message with the outbox name {name} is already registered");
            }

            MessageInfos.Add(new RegistryMessageInfo(typeof(TMessageType), name));
        }
            
        public IOutboxServiceRegistry Build()
        {
            return new OutboxServiceRegistry(MessageInfos);
        }
        #endregion
    }
}
