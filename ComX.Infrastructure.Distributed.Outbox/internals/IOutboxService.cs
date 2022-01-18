using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace ComX.Infrastructure.Distributed.Outbox
{

    /// <summary>
    /// Service that stores an event in a database
    /// </summary>
    public interface IOutboxService
    {
        Task OutboxPublishAsync<TMessage>(TMessage message);
    }
}
