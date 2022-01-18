using System.Threading;
using System.Threading.Tasks;

namespace ComX.Infrastructure.Distributed.Outbox.Tests
{
    public class TestPublisher : IOutboxBrokerPublisher
    {
        public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
        {
            return Task.CompletedTask;
        }
    }
}
