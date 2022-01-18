using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ComX.Infrastructure.Distributed.Workertimer.Tests
{
    public class TestIncrementalWorker : IWorkerProcess
    {
        public event Action Increment;

        private readonly TimeSpan? processDelay;

        public TestIncrementalWorker(TimeSpan? processDelay = null)
        {
            this.processDelay = processDelay;
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }

        public async Task ProcessAsync()
        {
            Increment?.Invoke();
            if (processDelay.HasValue)
            {
                await Task.Delay(processDelay.Value);
            }
        }
    }
}