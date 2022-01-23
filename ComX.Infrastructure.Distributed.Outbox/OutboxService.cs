using Microsoft.Extensions.Logging;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace ComX.Infrastructure.Distributed.Outbox;

public class OutboxService : IOutboxService
{
    private readonly IOutboxServiceRegistry _outboxServiceRegistry;
    private readonly IEventSerializer _eventSerializer;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<IOutboxService> _logger;
    private static MethodInfo reflectivePublish
        = typeof(OutboxService).GetMethod(nameof(ReflectiveOutboxPublishAsync), BindingFlags.Instance | BindingFlags.NonPublic);
    private Lazy<IOutboxTransformerService> _transformerService;

    public OutboxService(
        IServiceProvider serviceProvider,
        ILogger<IOutboxService> logger,
        IOutboxServiceRegistry outboxServiceRegistry,
        IEventSerializer eventSerializer)
    {
        _outboxServiceRegistry = outboxServiceRegistry;
        _eventSerializer = eventSerializer;
        _serviceProvider = serviceProvider;
        _logger = logger;

        // the transformer service is not required if we always map to IntegrationMessageLog instead of derivates
        _transformerService = new Lazy<IOutboxTransformerService>(() =>
        {
            return _serviceProvider.GetRequiredService<IOutboxTransformerService>();
        });
    }

    /// <summary>
    /// Saving the message (event) in the database.
    /// </summary>
    /// <typeparam name="TMessage">The type of saved message</typeparam>
    /// <param name="message">The message saving in the database</param>
    public Task OutboxPublishAsync<TMessage>(TMessage message)
    {
        RegistryMessageInfo messageInfo = _outboxServiceRegistry.GetInfoFor<TMessage>();

        if (messageInfo is null)
        {
            string exMsg = $"Could not find an outbox message info for type \'{typeof(TMessage).FullName}\'";
            _logger?.LogError(exMsg);
            throw new Exception(exMsg);
        }

        if (messageInfo.MessageLogType.Equals(typeof(IntegrationMessageLog)))
        {
            IOutboxStorage<IntegrationMessageLog> outboxStorage
                = _serviceProvider.GetService<IOutboxStorage<IntegrationMessageLog>>();

            if (outboxStorage is null)
            {
                throw new Exception($"Could not find the outbox storage for default entity {nameof(IntegrationMessageLog)}");
            }
            IntegrationMessageLog messageLog = new()
            {
                MessageBody = _eventSerializer.Serialize(message),
                MessageTypeName = messageInfo.Name,
                Status = OutboxStatus.NotPublished,
                Id = Guid.NewGuid()
            };

            _logger?.LogTrace($"Trying to store message {messageLog.Id}");
            return outboxStorage.InsertAsync(messageLog);

        }
        else
        {
            /// user defined a custom repository for this message type <see cref="IIntegrationMessageLog"/>
            return (Task)reflectivePublish
                .MakeGenericMethod(messageInfo.MessageType, messageInfo.MessageLogType)
                .Invoke(this, new object[] { message });
        }
    }

    private Task ReflectiveOutboxPublishAsync<TMessage, TMessageLog>(TMessage message)
        where TMessageLog : class, IIntegrationMessageLog
    {
        IOutboxStorage<TMessageLog> outboxStorage = _serviceProvider.GetService<IOutboxStorage<TMessageLog>>();

        if (outboxStorage is null)
        {
            throw new Exception($"Could not find the outbox storage for {typeof(TMessageLog).FullName}");
        }

        /// since this is not <see cref="IntegrationMessageLog"/> but a derived type 
        /// of <see cref="IIntegrationMessageLog"/>m user must provide a transform between the event
        /// and the message we save in the database
        TMessageLog messageLog = _transformerService.Value.Transform<TMessage, TMessageLog>(message);

        // these 2 values are required for the worker to pick the message
        messageLog.Status = OutboxStatus.NotPublished;
        messageLog.Id = messageLog.Id == Guid.Empty ? Guid.NewGuid() : messageLog.Id;
        messageLog.RetryCount = 0;
        messageLog.LastError = String.Empty;
        messageLog.LastAttemptDate = null;
        messageLog.LockUntil = null;
        _logger?.LogTrace($"Trying to store message {messageLog.Id}");
        return outboxStorage.InsertAsync(messageLog);
    }
}
