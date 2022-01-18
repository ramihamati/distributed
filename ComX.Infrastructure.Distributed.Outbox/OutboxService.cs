using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace ComX.Infrastructure.Distributed.Outbox
{
    public class OutboxService : IOutboxService
    {
        #region [ Fields ]
        private readonly IOutboxServiceRegistry _outboxServiceRegistry;
        private readonly IEventSerializer _eventSerializer;
        private readonly IOutboxStorage _outboxStorage;
        private readonly ILogger<IOutboxService> _logger;
        #endregion

        #region [ Properties ]
        #endregion

        #region [ Constructor ]
        public OutboxService(
            IOutboxStorage outboxStorage,
            ILogger<IOutboxService> logger,
            IOutboxServiceRegistry outboxServiceRegistry,
            IEventSerializer eventSerializer)
        {
            _outboxServiceRegistry = outboxServiceRegistry;
            _eventSerializer = eventSerializer;
            _outboxStorage = outboxStorage;
            _logger = logger;
        }
        #endregion

        #region [ Methods ]
        public Task OutboxPublishAsync<TMessage>(TMessage message)
        {
            RegistryMessageInfo messageInfo = _outboxServiceRegistry.GetInfoFor<TMessage>();

            if (messageInfo is null)
            {
                string exMsg = $"Could not find an outbox message info for type \'{typeof(TMessage).FullName}\'";
                _logger?.LogError(exMsg);
                throw new Exception(exMsg);
            }

            IntegrationMessageLog messageLog = new()
            {
                MessageBody = _eventSerializer.Serialize(message),
                MessageTypeName = messageInfo.Name,
                Status = OutboxStatus.NotPublished,
                Id = Guid.NewGuid()
            };

            _logger?.LogTrace($"Trying to store message {messageLog.Id}");
            return _outboxStorage.InsertAsync(messageLog);
        }

        #endregion
    }
}
