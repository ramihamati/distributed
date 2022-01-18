using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;

namespace ComX.Infrastructure.Distributed.Outbox.Tests;

public class MyMongoOutboxDocument : IMongoOutboxDocument
{
    [BsonId(IdGenerator = typeof(GuidGenerator))]
    [BsonIgnoreIfDefault]
    public Guid Id { get; set; }

    [BsonRequired]
    [BsonElement("MessageBody")]
    public string MessageBody { get; set; }

    [BsonRequired]
    [BsonElement("Status")]
    public OutboxStatus Status { get; set; }

    [BsonRequired]
    [BsonElement("MessageTypeName")]
    public string MessageTypeName { get; set; }

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
    [BsonElement("Timestamp")]
    public long Timestamp { get; set; }

    [BsonRequired]
    [BsonElement("LastError")]
    public string LastError { get; set; }

    public MyMongoOutboxDocument()
    {
        CreatedAt = DateTime.UtcNow;
        Id = Guid.NewGuid();
        LastError = String.Empty;
    }

    public static MyMongoOutboxDocument ToMongoIntegrationMessageLog(IntegrationMessageLog message)
    {
        return new MyMongoOutboxDocument
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

    public static IntegrationMessageLog ToIntegrationMessageLog(MyMongoOutboxDocument message)
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
