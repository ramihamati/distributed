using ComX.Common.MongoBase;
using MongoDB.Bson.Serialization.Attributes;

namespace ComX.Infrastructure.Distributed.Outbox.Store.MongoBridge;

public class IntegrationMessageEntity
    : BaseDocument<IntegrationMessageEntity>, IMongoOutboxDocument
{
    [BsonRequired]
    [BsonElement("MessageBody")]
    public string MessageBody { get; set; } = String.Empty;

    [BsonRequired]
    [BsonElement("Status")]
    public OutboxStatus Status { get; set; }

    [BsonRequired]
    [BsonElement("MessageTypeName")]
    public string MessageTypeName { get; set; } = String.Empty;

    [BsonRequired]
    [BsonElement("CreatedAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("LastAttemptDate")]
    public DateTime? LastAttemptDate { get; set; }

    [BsonElement("LockUntil")]
    public DateTime? LockUntil { get; set; }

    [BsonRequired]
    [BsonElement("RetryCount")]
    public int RetryCount { get; set; } = 0;

    [BsonRequired]
    [BsonElement("LastError")]
    public string LastError { get; set; }

    public IntegrationMessageEntity()
    {
        CreatedAt = DateTime.UtcNow;
        Id = Guid.NewGuid();
        LastError = String.Empty;
    }

    public static IntegrationMessageEntity ToEntity(IntegrationMessageLog message)
    {
        return new IntegrationMessageEntity
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

    public static IntegrationMessageLog ToLog(IntegrationMessageEntity message)
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
