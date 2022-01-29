==============================
Mongo with wrapped repository
==============================

.. role:: yaml(code)
   :language: yaml 

.. role:: cs(code)
   :language: cs 

Package
-----------
.. code-block:: yaml

    <PackageReference Include="ComX.Infrastructure.Distributed.Outbox.Aspnet" Version="x.x.x">
    <PackageReference Include="ComX.Infrastructure.Distributed.Outbox.Store.Mongo" Version="x.x.x">

About
-----

Similar to creating a custom repository, but leveraging the built-in implementation if needed


Create the new entity
---------------------

.. code-block:: cs

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

Create the wrapper repository
-----------------------------

.. code-block:: cs

    public class MongoWrappedRepository : IOutboxRepository
    {
        private readonly OutboxMongoRepository<MyMongoOutboxDocument> _innerRepository;

        public MongoWrappedRepository(
            IOutboxMongoSettings outboxMongoSettings,
            OutboxMongoManager mongoManager)
        {
            _innerRepository = new OutboxMongoRepository<MyMongoOutboxDocument>(
                mongoManager.GetCollection<MyMongoOutboxDocument>(outboxMongoSettings.DbName, outboxMongoSettings.ConnectionString),
                MyMongoOutboxDocument.ToMongoIntegrationMessageLog,
                MyMongoOutboxDocument.ToIntegrationMessageLog);
        }

        public Task DeleteAsync(IntegrationMessageLog entity, CancellationToken cancellationToken = default)
        {
            return _innerRepository.DeleteAsync(entity, cancellationToken);
        }

        public Task<bool> ExistsAsync(Guid entityId, CancellationToken cancellationToken = default)
        {
            return _innerRepository.ExistsAsync(entityId, cancellationToken);
        }

        public Task<IntegrationMessageLog> FindAsync(
            Guid entityId,
            CancellationToken cancellationToken = default)
        {
            return _innerRepository.FindAsync(entityId, cancellationToken);
        }

        public Task<List<IntegrationMessageLog>> FindAsync(
            FinderMessageLog finder,
            CancellationToken cancellationToken = default)
        {
            return _innerRepository.FindAsync(finder, cancellationToken);
        }

        public Task InsertAsync(IntegrationMessageLog entity, CancellationToken cancellationToken = default)
        {
            return _innerRepository.InsertAsync(entity, cancellationToken);
        }

        public Task UpdateAsync(IntegrationMessageLog entity, CancellationToken cancellationToken = default)
        {
            return _innerRepository.UpdateAsync(entity);
        }

        public Task<bool> UnlockAsync(IntegrationMessageLog entity)
        {
            return _innerRepository.UnlockAsync(entity);
        }

        public Task<bool> LockAsync(IntegrationMessageLog entity, TimeSpan span)
        {
            return _innerRepository.LockAsync(entity, span);
        }
    }

Register the service
--------------------

.. code-block:: cs

    services.AddOutboxService(cfg =>
    {
        cfg.RegisterEvents(reg =>
        {
            reg.RegisterMessage<IEventOne>(EVENT_ONE_NAME);
            reg.RegisterMessage<IEventTwo>(EVENT_TWO_NAME);
            reg.RegisterMessage<IEventThree>(EVENT_THREE_NAME);
        });
        cfg.ConfigureStore(storeCfg =>
        {
            storeCfg.UseMongoStore(configuration, mCfg =>
            {
                mCfg.UseRepository<MongoWrappedRepository>();
            });
        });
        cfg.ConfigureSerializer(sCfg => sCfg.UseMassTransitSerializer());
    });