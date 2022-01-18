using System;
using System.Threading.Tasks;

namespace ComX.Infrastructure.Distributed.Workertimer.Tests
{
    public class Configuration : IConfigurationTimer
    {
        private readonly TimeSpan workerPeriod;
        public TimeSpan WorkerPeriod => workerPeriod;

        public Configuration(TimeSpan timeSpan)
        {
            this.workerPeriod = timeSpan;
        }
    }
}