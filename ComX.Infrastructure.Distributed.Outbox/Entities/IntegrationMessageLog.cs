using System;

namespace ComX.Infrastructure.Distributed.Outbox;

public class IntegrationMessageLog
{
    public Guid Id { get; set; }
    public string MessageBody { get; set; }
    public OutboxStatus Status { get; set; }
    public string MessageTypeName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastAttemptDate { get; set; }
    public DateTime? LockUntil { get; set; }
    public int RetryCount { get; set; } = 0;
    public byte[] Timestamp { get; set; }
    public string LastError { get; set; }
    public IntegrationMessageLog()
    {
        CreatedAt = DateTime.UtcNow;
    }
}
