
namespace ComX.Infrastructure.Distributed.Outbox
{
    public interface IIntegrationMessageLog
    {
        public DateTime CreatedAt { get; set; }
        public Guid Id { get; set; }
        public DateTime? LastAttemptDate { get; set; }
        public string LastError { get; set; }
        public DateTime? LockUntil { get; set; }
        public string MessageBody { get; set; }
        public string MessageTypeName { get; set; }
        public int RetryCount { get; set; }
        public OutboxStatus Status { get; set; }
        public byte[] Timestamp { get; set; }
    }
}