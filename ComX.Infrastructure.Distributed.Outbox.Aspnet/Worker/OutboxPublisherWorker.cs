using ComX.Infrastructure.Distributed.Workertimer;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ComX.Infrastructure.Distributed.Outbox;

public class OutboxPublisherWorker<TMessageLog> : IWorkerProcess
    where TMessageLog : class, IIntegrationMessageLog
{
    private readonly IOutboxWorkerService<TMessageLog> _outboxWorkerService;
    private readonly IConfigurationOutboxWorker _configuration;
    private readonly ILogger<OutboxPublisherWorker<TMessageLog>> _logger;
    public IServiceProvider IsolatedServiceProvider { get; }

    public OutboxPublisherWorker(
        IServiceProvider boxProvider,
        IOutboxWorkerService<TMessageLog> outboxWorkerService,
        IConfigurationOutboxWorker configuration,
        ILogger<OutboxPublisherWorker<TMessageLog>> logger)
    {
        IsolatedServiceProvider = boxProvider;
        _outboxWorkerService = outboxWorkerService;
        _configuration = configuration;
        _logger = logger;
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public async Task ProcessAsync()
    {
        bool keepDoing = true;
        do
        {
            FinderMessageLog finder = FinderMessageLog.New(
                FilterMessageLog
                    .Empty
                    .SetStatus(OutboxStatus.NotPublished)
                    .SetLastAttemptOffset(_configuration.TimeBetweenRetries)
                    .SetUnlocked(true),
                10);

            List<TMessageLog> messages = await _outboxWorkerService.FindAsync(finder);

            if (messages is null || messages.Count == 0)
            {
                keepDoing = false;
            }
            else
            {
                foreach (TMessageLog msgLog in messages)
                {
                    _logger.LogDebug($"Worker: processing pending message {msgLog.Id} to be published:");
                    // locking and unlocking inside the service
                    await _outboxWorkerService.PublishAsync(msgLog);
                }
            }

        } while (keepDoing);
    }
}
