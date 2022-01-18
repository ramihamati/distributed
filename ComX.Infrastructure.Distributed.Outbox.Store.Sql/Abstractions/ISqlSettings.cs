namespace ComX.Infrastructure.Distributed.Outbox;

public interface ISqlSettings
{
    public string ConnectionString { get; }
    public int MaxRetryCount { get; }
    public TimeSpan MaxRetryDelay { get; }
}
