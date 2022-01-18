namespace ComX.Infrastructure.Distributed.Outbox;

public static class MessageConverter
{
    public static MongoOutboxDocument ToMongoIntegrationMessageLog(IntegrationMessageLog message)
    {
        return new MongoOutboxDocument
        {
            Id = message.Id,
            MessageBody = message.MessageBody,
            Status = message.Status,
            MessageTypeName = message.MessageTypeName,
            CreatedAt = message.CreatedAt,
            LastAttemptDate = message.LastAttemptDate,
            LockUntil = message.LockUntil,
            RetryCount = message.RetryCount,
            Timestamp = message.Timestamp is null ? DateTime.UtcNow.Ticks : BitConverter.ToInt64(message.Timestamp),
            LastError = message.LastError
        };
    }

    public static IntegrationMessageLog ToIntegrationMessageLog(MongoOutboxDocument message)
    {
        return new IntegrationMessageLog
        {
            Id = message.Id,
            MessageBody = message.MessageBody,
            Status = message.Status,
            MessageTypeName = message.MessageTypeName,
            CreatedAt = message.CreatedAt,
            LastAttemptDate = message.LastAttemptDate,
            LockUntil = message.LockUntil,
            RetryCount = message.RetryCount,
            Timestamp = BitConverter.GetBytes(message.Timestamp),
            LastError = message.LastError
        };
    }
}
