using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using System.Text;
using System.Data;

namespace ComX.Infrastructure.Distributed.Outbox;

public class OutboxWorkerService<TMessageLog> : IOutboxWorkerService<TMessageLog>
    where TMessageLog : class, IIntegrationMessageLog
{
    private readonly IOutboxServiceRegistry _outboxServiceRegistry;
    private readonly IEventSerializer _eventSerializer;
    private readonly IConfigurationOutboxWorker _configuration;
    private readonly IOutboxStorage<TMessageLog> _outboxStorage;
    private readonly IServiceProvider _serviceProvider;
    private readonly IOutboxBrokerPublisher _outboxPublisher;
    private readonly ILogger<IOutboxService> _logger;
    private const string errMsgPublisherNotReg = "The outbox publisher service is not registered";

    public OutboxWorkerService(
        IOutboxStorage<TMessageLog> outboxStorage,
        IServiceProvider serviceProvider,
        ILogger<IOutboxService> logger,
        IOutboxServiceRegistry outboxServiceRegistry,
        IEventSerializer eventSerializer,
        IConfigurationOutboxWorker configuration)
    {
        _outboxServiceRegistry = outboxServiceRegistry;
        _eventSerializer = eventSerializer;
        _configuration = configuration;
        _outboxStorage = outboxStorage;
        _serviceProvider = serviceProvider;
        _outboxPublisher = serviceProvider.GetService<IOutboxBrokerPublisher>();
        _logger = logger;
    }

    #region [ Methods ]
    public Task<List<TMessageLog>> FindAsync(
        FinderMessageLog finder,
        CancellationToken cancellationToken = default)
    {
        return _outboxStorage.FindAsync(finder, cancellationToken);
    }

    public async Task PublishAsync(TMessageLog message, CancellationToken cancellationToken = default)
    {
        if (_outboxPublisher is null)
        {
            throw new Exception("The outbox publisher service is not registered");
        }

        RegistryMessageInfo messageInfo = _outboxServiceRegistry.GetInfoFor(message.MessageTypeName);

        if (messageInfo is null)
        {
            throw new Exception($"Could not find an outbox message info for type \'{message.MessageTypeName}\'");
        }

        if (message.Status != OutboxStatus.NotPublished)
        {
            throw new Exception($"Found the message log {message.Id} with status = {message.Status}. It is not publisheable");
        }

        bool isLocked = false;

        try
        {
            isLocked = await _outboxStorage.LockAsync(message, TimeSpan.FromHours(1));
        }
        catch (OutboxConcurrencyException)
        {
            isLocked = false;
        }
        catch (DBConcurrencyException)
        {
            isLocked = false;
        }
        catch (Exception)
        {
            throw;
        }

        if (!isLocked)
        {
            return;
        }

        try
        {
            object messageBody
                = _eventSerializer.Deserialize(message.MessageBody, messageInfo.MessageType);
            /**
             * Because we are not in a generic method but we need to call a generic method, we have to use reflection to make that happen
             * TODO: make mInfo static for performance reasons
             */
            MethodInfo mInfo = _outboxPublisher.GetType().GetMethod(nameof(IOutboxBrokerPublisher.PublishAsync), BindingFlags.Public | BindingFlags.Instance);
            mInfo = mInfo.MakeGenericMethod(new Type[] { messageInfo.MessageType });

            Task publishTask = (Task)mInfo.Invoke(_outboxPublisher, new object[] { messageBody, cancellationToken });
            await publishTask;

            message.Status = OutboxStatus.Published;
            await _outboxStorage.UpdateAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Could not publish message {message.Id}");

            if (message.RetryCount == _configuration.EnterErrorStateAfterNoOfRetries)
            {
                message.Status = OutboxStatus.ErrorState;
            }
            else
            {
                message.Status = OutboxStatus.NotPublished;
                message.RetryCount++;
                message.LastAttemptDate = DateTime.UtcNow;
            }

            StringBuilder sb = new();
            message.LastError = ExtractError(ex);
            await _outboxStorage.UpdateAsync(message);
            await _outboxStorage.UnlockAsync(message);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string ExtractError(Exception exception)
    {
        StringBuilder sb = new();
        ExtractError(exception, sb);
        return sb.ToString();
    }

    private static void ExtractError(Exception exception, StringBuilder sb)
    {
        sb.AppendLine(exception.Message);
        if (exception.InnerException is not null)
        {
            ExtractError(exception.InnerException, sb);
        }
    }

    #endregion
}
