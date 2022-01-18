using MongoDB.Driver;

namespace ComX.Infrastructure.Distributed.Outbox.Store.MongoBridge;

public interface IOutboxSessionWrapper
{
    IClientSessionHandle? ActiveSession { get; }
}
