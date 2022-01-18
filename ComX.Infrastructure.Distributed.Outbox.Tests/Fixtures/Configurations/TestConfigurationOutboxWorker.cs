using System;

namespace ComX.Infrastructure.Distributed.Outbox.Tests
{
    public class TestConfigurationOutboxWorker : IConfigurationOutboxWorker
    {
        public TimeSpan WorkerPeriod { get; }
        public int EnterErrorStateAfterNoOfRetries { get; } = 5;
        public TimeSpan TimeBetweenRetries { get; }

        public TestConfigurationOutboxWorker(
            TimeSpan workerPeriod,
            TimeSpan timeBetweenRetries)
        {
            WorkerPeriod = workerPeriod;
            TimeBetweenRetries = timeBetweenRetries;
        }
    }
}