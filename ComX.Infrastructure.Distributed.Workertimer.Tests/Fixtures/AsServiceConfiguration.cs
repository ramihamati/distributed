using System;
using System.Threading.Tasks;

namespace ComX.Infrastructure.Distributed.Workertimer.Tests
{
    public class AsServiceConfiguration : IConfigurationTimer
    {
        // CONFIGURATION WITH PARAMETERLESS CONSTRUCTOR
        // OR INJECTED AS A SERVICE
        public TimeSpan WorkerPeriod => TimeSpan.FromSeconds(1);
    }
}