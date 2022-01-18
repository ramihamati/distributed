using ComX.Infrastructure.Distributed.Workertimer;
using System;

namespace ComX.Infrastructure.Distributed.Outbox
{
    public interface IConfigurationOutboxWorker : IConfigurationTimer
    {
        /// <summary>
        /// After x number of retries the state is set to <see cref="OutboxStatus.ErrorState"/>
        /// and no other retires are allowed
        /// </summary>
        int EnterErrorStateAfterNoOfRetries { get; }

        /// <summary>
        /// Worker will let cool off time between retries
        /// </summary>
        TimeSpan TimeBetweenRetries { get; }
    }
}
