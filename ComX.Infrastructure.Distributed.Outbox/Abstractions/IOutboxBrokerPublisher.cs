using System.Threading;
using System.Threading.Tasks;

namespace ComX.Infrastructure.Distributed.Outbox
{
    public interface IOutboxBrokerPublisher
    {
        /// <summary>
        /// Method used to publish the message to an actual broker
        /// </summary>
        /// <typeparam name="T">The message type</typeparam>
        /// <param name="message">The message payload</param>
        /// <param name="cancellationToken">The cancellation token</param>
        Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class;
    }
}
