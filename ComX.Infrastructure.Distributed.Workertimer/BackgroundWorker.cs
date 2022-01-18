using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ComX.Infrastructure.Distributed.Workertimer
{
    internal sealed class BackgroundWorker<T> : IHostedService, IDisposable
        where T : class, IWorkerProcess
    {
        #region [ Fields ]
        private readonly IBackgroundProcessor _workerProgramability;
        private readonly ILogger<T> _logger;
        private readonly Type workerType;
        private CancellationTokenSource _cancellationTokenSource;
        /// <summary>
        /// The actual process running inside the current worker
        /// </summary>
        private T _workerProcess;
        private readonly Func<T> _processFactory;
        #endregion

        #region [ Properties ]
        public CancellationToken CancellationToken { get; private set; }
        #endregion

        #region [ Constructor ]
        public BackgroundWorker(
            IBackgroundProcessor backgroundWorker,
            ILoggerFactory loggerFactory,
            Func<T> processFactory)
        {
            _workerProgramability = backgroundWorker;
            _logger = loggerFactory?.CreateLogger<T>();
            workerType = typeof(T);
            _processFactory = processFactory;
            EnsureProcessLoaded();
        }

        ~BackgroundWorker()
        {
            _logger?.LogTrace($"Worker \'{workerType.FullName}\' desstructor was called.");
            _workerProgramability.Dispose();
            _cancellationTokenSource.Dispose();
        }
        #endregion

        #region [ Methods ]
        public void Dispose()
        {
            _workerProgramability?.Dispose();
            _cancellationTokenSource?.Dispose();
            GC.SuppressFinalize(this);
            _logger?.LogTrace($"Worker \'{workerType.FullName}\' dispose() was called");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger?.LogTrace($"Worker \'{workerType.FullName}\' start was called.");
            if (_cancellationTokenSource is not null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
            }
            _cancellationTokenSource = new CancellationTokenSource();
            CancellationToken = _cancellationTokenSource.Token;

            return _workerProgramability.StartAsync(_workerProcess.ProcessAsync);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger?.LogTrace($"Worker \'{workerType.FullName}\' stop was called.");
            _cancellationTokenSource.Cancel();
            await _workerProgramability.StopAsync();
            _workerProgramability.Dispose();
            await _workerProcess?.DisposeAsync().AsTask();
        }

        private void EnsureProcessLoaded()
        {
            _workerProcess = _processFactory();

            if (_workerProcess is null)
            {
                throw new ApplicationException($"Cannot load the service {typeof(T).FullName}. Did you register it in services?");
            }
        }
        #endregion
    }
}
