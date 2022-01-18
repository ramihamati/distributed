
namespace ComX.Infrastructure.Distributed.Inbox.Store.Mongo
{
    public interface IMongoConfiguration
    {
        string ConnectionString { get; }
        TimeSpan ConnectionTimeout { get; }
    }
}