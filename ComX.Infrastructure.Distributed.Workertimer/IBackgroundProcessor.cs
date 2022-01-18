using System;
using System.Threading.Tasks;

namespace ComX.Infrastructure.Distributed.Workertimer
{
    /// <summary>
    /// An abstraction of a configurable background worker.
    /// Use interface to define more timer types.
    /// <see cref="BackgroundTimerProcessor"/>
    /// 
    /// Note: This is not a worker the user must use, it's internal
    /// </summary>
    internal interface IBackgroundProcessor
    {
        public Task StartAsync(Func<Task> func);

        public Task StopAsync();

        public void Dispose();
    }
}
