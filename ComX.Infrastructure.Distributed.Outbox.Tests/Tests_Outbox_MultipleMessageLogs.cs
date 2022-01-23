using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;
using URF.Core.Abstractions;

namespace ComX.Infrastructure.Distributed.Outbox.Tests
{
    public class Tests_Outbox_MultipleMessageLogs
    {
        public SqliteConnection DbConnection { get; set; }
        public ServiceProvider Services { get; set; }

        [SetUp]
        public void Setup()
        { // must pass an open connection because it's an in memory db
            // and if we let EF create it, it will close the connection often and data is lost
            DbConnection = new SqliteConnection(@"Data Source = :memory:");
            DbConnection.Open();

            string createLogTable(string tableName)
            {
                return $@"
                CREATE TABLE {tableName}(
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
            ";
            }

            SqliteCommand command1 = new SqliteCommand(createLogTable("MessageLogs"), DbConnection);
            SqliteCommand command2 = new SqliteCommand(createLogTable("MessageLogsEventOneAndTwo"), DbConnection);
            command1.ExecuteNonQuery();
            command2.ExecuteNonQuery();

            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddAutoMapper(typeof(Tests_Outbox_MultipleMessageLogs).Assembly);

            serviceCollection.AddDbContext<MultipleLogDataContext>(options =>
            {
                options.UseSqlite(DbConnection);
            }, ServiceLifetime.Scoped);

            serviceCollection.AddScoped<DbContext, MultipleLogDataContext>();
            serviceCollection.AddScoped<IUnitOfWork, MultipleLogUnitOfWork>();

            serviceCollection.AddOutboxService(cfg =>
            {
                cfg.ConfigureEvents(reg =>
                {
                    reg.RegisterMessage<IEventOne, CustomMessageLog>(Consts.EVENT_ONE_NAME);
                    reg.RegisterMessage<IEventTwo, CustomMessageLog>(Consts.EVENT_TWO_NAME);
                    // event tree will use the default IntegrationMessageLog
                    reg.RegisterMessage<IEventThree>(Consts.EVENT_THREE_NAME);
                });
                cfg.ConfigureTransforms(trCfg =>
                {
                    trCfg.Cfg.UseAutomapperTransformations();
                    trCfg.RegisterTransform<IEventOne, CustomMessageLog>();
                    trCfg.RegisterTransform<IEventTwo, CustomMessageLog>();
                });
                cfg.ConfigureStore(storeCfg =>
                {
                    storeCfg.UseUrfStore(efCfg =>
                    {
                        efCfg.UseRepository<IntegrationMessageLog, MultipleLogDataContext>();
                        efCfg.UseRepository<CustomMessageLog, MultipleLogDataContext>();
                    });
                });
                cfg.ConfigureSerializer(sCfg => sCfg.UseMassTransitSerializer());
            });

            Services = serviceCollection.BuildServiceProvider();
        }

        [TearDown]
        public void TearDown()
        {
            DbConnection?.Close();
        }
        [Test]
        public async Task OutboxService_Publish_ShouldSaveToDb_WithStatusPending()
        {
            IOutboxService outboxService
                = Services.GetService<IOutboxService>();

            IRepository<IntegrationMessageLog> repository
              = Services.GetService<IRepository<IntegrationMessageLog>>();

            IRepository<CustomMessageLog> repositoryCustomLog
              = Services.GetService<IRepository<CustomMessageLog>>();

            IEventSerializer eventSerializer
              = Services.GetService<IEventSerializer>();
            IUnitOfWork externalUOW
                = Services.GetService<IUnitOfWork>();

            Guid evone_documentId = Guid.NewGuid();
            Guid evone_referenceId = Guid.NewGuid();

            string evtwo_refType = Guid.NewGuid().ToString();
            Guid evtwo_referenceId = Guid.NewGuid();

            string evthreedata = Guid.NewGuid().ToString();

            IEventOne eventBody1 = new EventOne
            {
                DocumentId = evone_documentId,
                ReferenceId = evone_referenceId
            };

            IEventTwo eventBody2 = new EventTwo
            {
                ReferenceType = evtwo_refType,
                ReferenceId = evone_referenceId
            };

            IEventThree eventBody3 = new EventThree
            {
                Errors = new string[]
                {
                    evthreedata
                }
            };

            await outboxService.OutboxPublishAsync<IEventOne>(eventBody1);
            await outboxService.OutboxPublishAsync<IEventTwo>(eventBody2);
            await outboxService.OutboxPublishAsync<IEventThree>(eventBody3);
            await externalUOW.SaveChangesAsync();

            CustomMessageLog log_one
                = await repositoryCustomLog.Query().FirstOrDefaultAsync(r => r.MessageTypeName.Equals(Consts.EVENT_ONE_NAME));

            CustomMessageLog log_two
                = await repositoryCustomLog.Query().FirstOrDefaultAsync(r => r.MessageTypeName.Equals(Consts.EVENT_TWO_NAME));

            IntegrationMessageLog log_three
                = await repository.Query().FirstOrDefaultAsync(r => r.MessageTypeName.Equals(Consts.EVENT_THREE_NAME));


            Assert.IsNotNull(log_one);
            Assert.IsNotNull(log_two);
            Assert.IsNotNull(log_three);

            Assert.AreEqual(OutboxStatus.NotPublished, log_one.Status);
            Assert.AreEqual(OutboxStatus.NotPublished, log_two.Status);
            Assert.AreEqual(OutboxStatus.NotPublished, log_three.Status);
        }
    }
}