using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ComX.Infrastructure.Distributed.Outbox.Tests
{

    /*
     * Database context, unit of work and Repository for INtegrationMessageLog
     * are registered externally.
     * Transactional commits are handled by the caller
     * 
     * This method does not require the use of URF
     */
    public class Tests_Outbox_External_Repository
    {
        public ServiceProvider Services { get; set; }

        public const string EVENT_ONE_NAME = "EventOne";
        public const string EVENT_TWO_NAME = "EventTwo";
        public const string EVENT_THREE_NAME = "EventThree";

        [SetUp]
        public void Setup()
        {
            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddOutboxService(cfg =>
            {
                cfg.RegisterEvents(reg =>
                {
                    reg.RegisterMessage<IEventOne>(EVENT_ONE_NAME);
                    reg.RegisterMessage<IEventTwo>(EVENT_TWO_NAME);
                    reg.RegisterMessage<IEventThree>(EVENT_THREE_NAME);
                });
                cfg.ConfigureStore(storeCfg =>
                {
                    storeCfg.UseRepository<InMemoryRepository>();
                });
                cfg.ConfigureSerializer(ser => ser.UseMassTransitSerializer());
            });

            Services = serviceCollection.BuildServiceProvider();
        }


        #region [URF REPOSITORY]

        [Test]
        public async Task Repository_Insert_ShouldWork()
        {
            IOutboxRepository repository
                = Services.GetService<IOutboxRepository>();

            Guid entityId = Guid.NewGuid();

            await repository.InsertAsync(new IntegrationMessageLog
            {
                MessageTypeName = "test",
                Id = Guid.NewGuid(),
                MessageBody = "",
                LastAttemptDate = DateTime.UtcNow,
                RetryCount = 1,
                Status = OutboxStatus.NotPublished,
                Timestamp = null
            });

            var finder = FinderMessageLog.New(FilterMessageLog.Empty.SetMessageTypeName("test"));

            IntegrationMessageLog log = (await repository
                .FindAsync(finder))
                .FirstOrDefault();

            Assert.IsNotNull(log);
            Assert.AreEqual("test", log.MessageTypeName);
            Assert.AreEqual(OutboxStatus.NotPublished, log.Status);
        }

        #endregion

        #region [ OUTBOX STORAGE ]

        [Test]
        public async Task OutboxStorage_Save_ShouldWork()
        {
            IOutboxStorage outboxStorage
                = Services.GetService<IOutboxStorage>();
            IOutboxRepository repository
                = Services.GetService<IOutboxRepository>();

            await outboxStorage.InsertAsync(new IntegrationMessageLog
            {
                MessageTypeName = "test",
                Id = Guid.NewGuid(),
                MessageBody = "",
                LastAttemptDate = DateTime.UtcNow,
                RetryCount = 1,
                Status = OutboxStatus.NotPublished,
                Timestamp = null
            });

            var finder = FinderMessageLog.New(FilterMessageLog.Empty.SetMessageTypeName("test"));

            IntegrationMessageLog log = (await repository
                .FindAsync(finder))
                .FirstOrDefault();

            Assert.IsNotNull(log);
            Assert.AreEqual("test", log.MessageTypeName);
            Assert.AreEqual(OutboxStatus.NotPublished, log.Status);
        }

        [Test]
        public async Task OutboxStorage_Update_ShouldWork()
        {
            IOutboxStorage outboxStorage
               = Services.GetService<IOutboxStorage>();
            IOutboxRepository repository
               = Services.GetService<IOutboxRepository>();

            Guid messageId = Guid.NewGuid();

            await outboxStorage.InsertAsync(new IntegrationMessageLog
            {
                MessageTypeName = "test",
                Id = messageId,
                MessageBody = "",
                LastAttemptDate = DateTime.UtcNow,
                RetryCount = 1,
                Status = OutboxStatus.NotPublished,
                Timestamp = null
            });

            IntegrationMessageLog log = await repository.FindAsync(messageId);

            log.Status = OutboxStatus.Published;
            log.MessageTypeName = "newname";

            await outboxStorage.UpdateAsync(log);

            log = await repository.FindAsync(messageId);

            Assert.IsNotNull(log);
            Assert.AreEqual("newname", log.MessageTypeName);
            Assert.AreEqual(OutboxStatus.Published, log.Status);

        }

        [Test]
        public async Task OutboxStorage_Delete_ShouldWork()
        {
            IOutboxStorage outboxStorage
               = Services.GetService<IOutboxStorage>();
            IOutboxRepository repository
               = Services.GetService<IOutboxRepository>();

            Guid messageId = Guid.NewGuid();

            await outboxStorage.InsertAsync(new IntegrationMessageLog
            {
                MessageTypeName = "test",
                Id = messageId,
                MessageBody = "",
                LastAttemptDate = DateTime.UtcNow,
                RetryCount = 1,
                Status = OutboxStatus.NotPublished,
                Timestamp = null
            });

            IntegrationMessageLog log = await repository.FindAsync(messageId);

            log.Status = OutboxStatus.Published;
            log.MessageTypeName = "newname";

            await outboxStorage.DeleteAsync(log);

            log = await repository.FindAsync(messageId);

            Assert.IsNull(log);
        }

        [Test]
        public async Task OutboxStorage_Find_ShouldWork()
        {
            IOutboxStorage outboxStorage
               = Services.GetService<IOutboxStorage>();
            IOutboxRepository repository
               = Services.GetService<IOutboxRepository>();

            Guid messageId = Guid.NewGuid();

            await outboxStorage.InsertAsync(new IntegrationMessageLog
            {
                MessageTypeName = "test",
                Id = messageId,
                MessageBody = "",
                LastAttemptDate = DateTime.UtcNow,
                RetryCount = 1,
                Status = OutboxStatus.NotPublished,
                Timestamp = null
            });

            IntegrationMessageLog log = await outboxStorage.FindAsync(messageId);
            Assert.IsNotNull(log);
            Assert.AreEqual("test", log.MessageTypeName);
            Assert.AreEqual(OutboxStatus.NotPublished, log.Status);
        }

        [Test]
        public async Task OutboxStorage_GetBatch_ShouldReturn_BatchSize_InDateOrder()
        {
            IOutboxStorage outboxStorage
                 = Services.GetService<IOutboxStorage>();

            static IntegrationMessageLog createMessage(Guid id, string name) => new IntegrationMessageLog
            {
                MessageTypeName = name,
                Id = id,
                MessageBody = "",
                LastAttemptDate = DateTime.UtcNow,
                RetryCount = 1,
                Status = OutboxStatus.NotPublished,
                Timestamp = null
            };

            var statuses = new List<OutboxStatus>
            {
                OutboxStatus.NotPublished, //0
                OutboxStatus.NotPublished, //1
                OutboxStatus.Published, //2
                OutboxStatus.NotPublished, //3
                OutboxStatus.Published, //4
                OutboxStatus.NotPublished, //5
                OutboxStatus.NotPublished, //6
                OutboxStatus.NotPublished, //7
                OutboxStatus.NotPublished, //8
                OutboxStatus.NotPublished, //9
            };

            for (int i = 0; i < 10; i++)
            {
                IntegrationMessageLog message = createMessage(Guid.NewGuid(), i.ToString());
                message.Status = statuses[i];
                await outboxStorage.InsertAsync(message);
            }

            FinderMessageLog finder = FinderMessageLog.New(
              FilterMessageLog.Empty
                  .SetStatus(OutboxStatus.NotPublished)
                  .SetLastAttemptOffset(TimeSpan.FromSeconds(0)),
              5);

            List<IntegrationMessageLog> messages =
                await outboxStorage.FindAsync(finder);

            Assert.AreEqual(5, messages.Count);
            Assert.AreEqual("0", messages[0].MessageTypeName);
            Assert.AreEqual("1", messages[1].MessageTypeName);
            Assert.AreEqual("3", messages[2].MessageTypeName);
            Assert.AreEqual("5", messages[3].MessageTypeName);
            Assert.AreEqual("6", messages[4].MessageTypeName);
        }

        #endregion

        [Test]
        public async Task OutboxService_Publish_ShouldSaveToDb_WithStatusPending()
        {
            IOutboxService outboxService
                = Services.GetService<IOutboxService>();
            IOutboxRepository repository
              = Services.GetService<IOutboxRepository>();
            IEventSerializer eventSerializer
              = Services.GetService<IEventSerializer>();

            Guid documentId = Guid.NewGuid();
            Guid referenceId = Guid.NewGuid();

            IEventOne eventBody = new EventOne
            {
                DocumentId = documentId,
                FileExtension = "pdf",
                FileName = "file",
                ReferenceId = referenceId
            };

            await outboxService.OutboxPublishAsync<IEventOne>(eventBody);

            IntegrationMessageLog log =
                (await repository.FindAsync(FinderMessageLog.Empty))
                .FirstOrDefault();

            Assert.IsNotNull(log);
            Assert.AreEqual(OutboxStatus.NotPublished, log.Status);
            Assert.AreEqual(EVENT_ONE_NAME, log.MessageTypeName);

            Assert.AreEqual(log.MessageBody, eventSerializer.Serialize(eventBody));
        }
    }
}