using System;

namespace ComX.Infrastructure.Distributed.Workertimer
{
    public interface IConfigurationTimer
    {
        /// <summary>
        /// Timespan at which the worker is guaranteed to restart
        /// </summary>
        TimeSpan WorkerPeriod { get; }
    }
}