using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using URF.Core.Abstractions;
using URF.Core.EF;

namespace ComX.Infrastructure.Distributed.Outbox.Tests
{
    /*
     * Database context, unit of work and URF Repository for INtegrationMessageLog
     * are registered externally.
     * Transactional commits are handled by the caller
     */
    public class Tests_Outbox_UrfStore
    {
        public SqliteConnection DbConnection { get; set; }

        public ServiceProvider Services { get; set; }

        public const string EVENT_ONE_NAME = "EventOne";
        public const string EVENT_TWO_NAME = "EventTwo";
        public const string EVENT_THREE_NAME = "EventThree";

        [SetUp]
        public void Setup()
        {
            // must pass an open connection because it's an in memory db
            // and if we let EF create it, it will close the connection often and data is lost
            DbConnection = new SqliteConnection(@"Data Source = :memory:");
            DbConnection.Open();

            SqliteCommand command = new SqliteCommand(@"
                CREATE TABLE IntegrationMessageLogs(
	                Id text NOT NULL PRIMARY KEY,
	                MessageBody text NOT NULL,
                    Status int NOT NULL,
	                MessageTypeName text NOT NULL,
	                CreatedAt DATETIME NOT NULL,
	                LastAttemptDate DATETIME NULL,
                    LockUntil DATETIME NULL,
	                RetryCount int NOT NULL,
                    Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
                    LastError text)
            ", DbConnection);

            command.ExecuteNonQuery();

            IServiceCollection serviceCollection = new ServiceCollection();

            // create the database context
            serviceCollection.AddDbContext<CustomDataContext>(options =>
            {
                options.UseSqlite(DbConnection);
            }, ServiceLifetime.Scoped);

            // If you want to use DbContext with Repository<> the uncomment below
            // serviceCollection.AddScoped<DbContext>(sp => sp.GetRequiredService<CustomDataContext>());
            // Note: serviceCollection.AddScoped<DbContext, CustomDataContext>() is bad because now we have 2 contexts and 
            //       if CustomUnitOfWork() uses CUstomDbCOntext while Repository<> uses DbContext whey will not sync

            serviceCollection.AddScoped<IUnitOfWork, CustomUnitOfWork>();

            serviceCollection.AddOutboxService(cfg =>
            {
                cfg.ConfigureEvents(reg =>
                {
                    reg.RegisterMessage<IEventOne>(EVENT_ONE_NAME);
                    reg.RegisterMessage<IEventTwo>(EVENT_TWO_NAME);
                    reg.RegisterMessage<IEventThree>(EVENT_THREE_NAME);
                });
                cfg.ConfigureStore(storeCfg =>
                {
                    storeCfg.UseUrfStore(efCfg =>
                    {
                        // will inject if not present the IRepository<IntegrationMessageLog>
                        efCfg.UseRepository<IntegrationMessageLog, CustomDataContext>();
                    });
                });
                cfg.ConfigureSerializer(ser => ser.UseMassTransitSerializer());
            });

            Services = serviceCollection.BuildServiceProvider();
        }

        [TearDown]
        public void TearDown()
        {
            DbConnection?.Close();
        }

        #region [URF REPOSITORY]

        [Test]
        public async Task Repository_Insert_ShouldWork()
        {
            IRepository<IntegrationMessageLog> repository
                = Services.GetService<IRepository<IntegrationMessageLog>>();
            IUnitOfWork externalUnitOfWork
                = Services.GetService<IUnitOfWork>();

            repository.Insert(new IntegrationMessageLog
            {
                MessageTypeName = "test",
                Id = Guid.NewGuid(),
                MessageBody = "",
                LastAttemptDate = DateTime.UtcNow,
                RetryCount = 1,
                Status = OutboxStatus.NotPublished,
                Timestamp = null
            });
            await externalUnitOfWork.SaveChangesAsync();

            IntegrationMessageLog log = await repository
                .Query()
                .FirstOrDefaultAsync(r => r.MessageTypeName == "test");

            Assert.IsNotNull(log);
            Assert.AreEqual("test", log.MessageTypeName);
            Assert.AreEqual(OutboxStatus.NotPublished, log.Status);
        }

        #endregion

        #region [ OUTBOX STORAGE ]

        [Test]
        public async Task OutboxStorage_Save_ShouldWork()
        {
            IOutboxStorage<IntegrationMessageLog> outboxStorage
                = Services.GetService<IOutboxStorage<IntegrationMessageLog>>();
            IRepository<IntegrationMessageLog> repository
               = Services.GetService<IRepository<IntegrationMessageLog>>();
            IUnitOfWork externalUOW
               = Services.GetService<IUnitOfWork>();

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

            await externalUOW.SaveChangesAsync();

            IntegrationMessageLog log = await repository
                .Queryable()
                .FirstOrDefaultAsync(r => r.MessageTypeName == "test");

            Assert.IsNotNull(log);
            Assert.AreEqual("test", log.MessageTypeName);
            Assert.AreEqual(OutboxStatus.NotPublished, log.Status);
        }

        [Test]
        public async Task OutboxStorage_Update_ShouldWork()
        {
            IOutboxStorage<IntegrationMessageLog> outboxStorage
               = Services.GetService<IOutboxStorage<IntegrationMessageLog>>();
            IRepository<IntegrationMessageLog> repository
               = Services.GetService<IRepository<IntegrationMessageLog>>();
            IUnitOfWork externalUOW
               = Services.GetService<IUnitOfWork>();

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

            await externalUOW.SaveChangesAsync();
            
            IntegrationMessageLog log = await repository.FindAsync(messageId);
            log.Status = OutboxStatus.Published;
            log.MessageTypeName = "newname";

            await outboxStorage.UpdateAsync(log);
            await externalUOW.SaveChangesAsync();

            log = await repository.FindAsync(messageId);

            Assert.IsNotNull(log);
            Assert.AreEqual("newname", log.MessageTypeName);
            Assert.AreEqual(OutboxStatus.Published, log.Status);

        }

        [Test]
        public async Task OutboxStorage_Delete_ShouldWork()
        {
            IOutboxStorage<IntegrationMessageLog> outboxStorage
               = Services.GetService<IOutboxStorage<IntegrationMessageLog>>();
            IRepository<IntegrationMessageLog> repository
               = Services.GetService<IRepository<IntegrationMessageLog>>();
            IUnitOfWork externalUOW
               = Services.GetService<IUnitOfWork>();

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
            await externalUOW.SaveChangesAsync();

            IntegrationMessageLog log = await repository.FindAsync(messageId);

            log.Status = OutboxStatus.Published;
            log.MessageTypeName = "newname";

            await outboxStorage.DeleteAsync(log);
            await externalUOW.SaveChangesAsync();

            log = await repository.FindAsync(messageId);

            Assert.IsNull(log);
        }

        [Test]
        public async Task OutboxStorage_Find_ShouldWork()
        {
            IOutboxStorage<IntegrationMessageLog> outboxStorage
               = Services.GetService<IOutboxStorage<IntegrationMessageLog>>();
            IRepository<IntegrationMessageLog> repository
               = Services.GetService<IRepository<IntegrationMessageLog>>();
            IUnitOfWork externalUOW
               = Services.GetService<IUnitOfWork>();

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
            await externalUOW.SaveChangesAsync();

            IntegrationMessageLog log = await outboxStorage.FindAsync(messageId);
            Assert.IsNotNull(log);
            Assert.AreEqual("test", log.MessageTypeName);
            Assert.AreEqual(OutboxStatus.NotPublished, log.Status);
        }

        [Test]
        public async Task OutboxStorage_GetBatch_ShouldReturn_BatchSize_InDateOrder()
        {
            IOutboxStorage<IntegrationMessageLog> outboxStorage
                 = Services.GetService<IOutboxStorage<IntegrationMessageLog>>();
            IUnitOfWork externalUOW
               = Services.GetService<IUnitOfWork>();

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

            await externalUOW.SaveChangesAsync();

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
            IRepository<IntegrationMessageLog> repository
              = Services.GetService<IRepository<IntegrationMessageLog>>();
            IEventSerializer eventSerializer
              = Services.GetService<IEventSerializer>();
            IUnitOfWork externalUOW
               = Services.GetService<IUnitOfWork>();

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
            await externalUOW.SaveChangesAsync();

            IntegrationMessageLog log
                = await repository.Query().FirstOrDefaultAsync();

            Assert.IsNotNull(log);
            Assert.AreEqual(OutboxStatus.NotPublished, log.Status);
            Assert.AreEqual(EVENT_ONE_NAME, log.MessageTypeName);

            Assert.AreEqual(log.MessageBody, eventSerializer.Serialize(eventBody));
        }
    }
}