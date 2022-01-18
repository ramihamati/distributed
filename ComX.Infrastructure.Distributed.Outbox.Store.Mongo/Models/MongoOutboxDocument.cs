using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace ComX.Infrastructure.Distributed.Outbox
{
    public class MongoOutboxDocument : IMongoOutboxDocument
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

        public MongoOutboxDocument()
        {
            CreatedAt = DateTime.UtcNow;
            Id = Guid.NewGuid();
            LastError = String.Empty;
        }
    }
}