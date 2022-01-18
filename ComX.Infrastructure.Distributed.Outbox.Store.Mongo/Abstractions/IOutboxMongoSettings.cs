namespace ComX.Infrastructure.Distributed.Outbox;

public interface IOutboxMongoSettings
{
    public string ConnectionString { get; }
    public TimeSpan ConnectionTimeout { get; }
    public string DbName { get; }
    public string CollectionName { get; }
}
