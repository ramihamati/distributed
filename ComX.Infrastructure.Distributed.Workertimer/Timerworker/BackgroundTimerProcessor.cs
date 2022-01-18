using System;
using System.Threading;
using System.Threading.Tasks;

namespace ComX.Infrastructure.Distributed.Workertimer
{
    internal sealed class BackgroundTimerProcessor : IBackgroundProcessor
    {
        #region [ Fields ]
        private readonly IConfigurationTimer _configurationWorker;
        private Func<Task> _workerAction;
        private Timer _timer;
        private const int FALSE = 0;
        private const int TRUE = 1;
        private int _preventReentry = FALSE;
        private int _requestForExecution = 0;
        #endregion

        #region [ Properties ]
        /// <summary>
        /// When a previous execution has not finished, a new execution is prevented and a <see cref="RequestForExecution"/>
        /// is set. After the stop of the previous execution, a new execution will start.
        /// </summary>
        public bool PreventReentry
        {
            get { return Interlocked.CompareExchange(ref _preventReentry, TRUE, TRUE) == TRUE; }
            set
            {
                if (value) Interlocked.CompareExchange(ref _preventReentry, TRUE, FALSE);
                else Interlocked.CompareExchange(ref _preventReentry, FALSE, TRUE);
            }
        }

        public bool RequestForExecution
        {
            get { return Interlocked.CompareExchange(ref _requestForExecution, 1, 1) == 1; }
            set
            {
                if (value) Interlocked.CompareExchange(ref _requestForExecution, 1, 0);
                else Interlocked.CompareExchange(ref _requestForExecution, 0, 1);
            }
        }
        #endregion

        #region [ Constructor ]
        public BackgroundTimerProcessor(IConfigurationTimer configurationWorker)
        {
            _configurationWorker = configurationWorker;
        }
        #endregion

        #region [ Methods ]
        public void Dispose()
        {
            _timer?.Dispose();
        }

        public Task StartAsync(Func<Task> workerAction)
        {
            _workerAction = workerAction;

            _timer = new Timer(async (_) => await ExecuteAsync().ConfigureAwait(false),
            null,
            TimeSpan.Zero,
            _configurationWorker.WorkerPeriod);

            return Task.CompletedTask;
        }

        private async Task ExecuteAsync()
        {
            if (IfEntrySetLock())
            {
                // making sure another thread of the same process will not trigger
                // this is valid only for a single instance (does not affect scalability)
                // we are only avoiding the process to start (by the timer event) if the existing process 
                // has not finished
                PreventReentry = true;
                await _workerAction().ConfigureAwait(false);

                if (RequestForExecution)
                {
                    RequestForExecution = false;
                    PreventReentry = false;
                    await ExecuteAsync().ConfigureAwait(false);
                }

                PreventReentry = false;
            }
            else
            {
                RequestForExecution = true;
            }
        }

        public bool IfEntrySetLock()
        {
            // if value is false, atomically set it to true
            // it returns the old value. If it was false, we return true to enter the execution
            return Interlocked.CompareExchange(ref _preventReentry, TRUE, FALSE) == FALSE;
        }

        public Task StopAsync()
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }
        #endregion
    }
}
