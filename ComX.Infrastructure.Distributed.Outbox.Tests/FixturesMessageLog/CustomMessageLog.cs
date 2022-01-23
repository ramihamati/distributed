using System;

namespace ComX.Infrastructure.Distributed.Outbox.Tests
{
    public class CustomMessageLog : IIntegrationMessageLog
    {
        public Guid Id { get; set; }

        public string MessageBody { get; set; }

        public OutboxStatus Status { get; set; }

        public string MessageTypeName { get; set; }

        public DateTime CreatedAt { get;  set; }

        // In case the message failed to be published, set an offset for the next attempt to 
        // allow the system to recover and to pick up next messages
        public DateTime? LastAttemptDate { get; set; } = DateTime.UtcNow.Subtract(TimeSpan.FromDays(365));

        public int RetryCount { get; set; } = 0;

        public byte[] Timestamp { get; set; }

        public string LastError { get; set; }
        public DateTime? LockUntil { get; set; }

        public CustomMessageLog()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}
