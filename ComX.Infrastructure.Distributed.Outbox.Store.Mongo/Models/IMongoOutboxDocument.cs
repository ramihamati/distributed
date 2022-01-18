namespace ComX.Infrastructure.Distributed.Outbox;

public interface IMongoOutboxDocument
{
    public Guid Id { get; set; }
    public OutboxStatus Status { get; set; }
    public string MessageTypeName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastAttemptDate { get; set; }
    public DateTime? LockUntil { get; set; }
    public int RetryCount { get; set; }
    public long Timestamp { get; set; }
    public string LastError { get; set; }
}
