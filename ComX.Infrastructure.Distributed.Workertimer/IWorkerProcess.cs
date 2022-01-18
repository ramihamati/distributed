using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ComX.Infrastructure.Distributed.Workertimer
{
    public interface IWorkerProcess : IAsyncDisposable
    {
        Task ProcessAsync();
    }
}
