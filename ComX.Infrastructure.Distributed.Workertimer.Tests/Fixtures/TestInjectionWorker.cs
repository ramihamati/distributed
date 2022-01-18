using System.Threading.Tasks;

namespace ComX.Infrastructure.Distributed.Workertimer.Tests
{
    public class TestInjectionWorker : IWorkerProcess
    {
        public ServiceInjected Service { get; }
        
        public TestInjectionWorker(ServiceInjected service)
        {
            Service = service;
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }

        public Task ProcessAsync()
        {
            return Task.CompletedTask;
        }
    }
}