using ComX.Infrastructure.Distributed.Workertimer;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ComX.Infrastructure.Distributed.Outbox
{
    public class OutboxPublisherWorker : IWorkerProcess
    {
        #region [ Fields ]
        private readonly IOutboxWorkerService _outboxWorkerService;
        private readonly IConfigurationOutboxWorker _configuration;
        private readonly ILogger<OutboxPublisherWorker> _logger;
        #endregion

        #region [ Properties ]
        #endregion

        #region [ Constructor ]
        public OutboxPublisherWorker(
            IOutboxWorkerService outboxWorkerService,
            IConfigurationOutboxWorker configuration,
            ILogger<OutboxPublisherWorker> logger)
        {
            _outboxWorkerService = outboxWorkerService;
            _configuration = configuration;
            _logger = logger;
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
        #endregion

        #region [ Methods ]
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

                List<IntegrationMessageLog> messages = await _outboxWorkerService.FindAsync(finder);

                if (messages is null || messages.Count == 0)
                {
                    keepDoing = false;
                }
                else
                {
                    foreach (IntegrationMessageLog msgLog in messages)
                    {
                        _logger.LogDebug($"Worker: processing pending message {msgLog.Id} to be published:");
                        // locking and unlocking inside the service
                        await _outboxWorkerService.PublishAsync(msgLog);
                    }
                }

            } while (keepDoing);
        }
        #endregion
    }
}
