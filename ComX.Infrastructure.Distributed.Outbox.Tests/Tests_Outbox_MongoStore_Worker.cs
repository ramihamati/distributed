using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Mongo2Go;
using MongoDB.Driver;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using URF.Core.Abstractions;

namespace ComX.Infrastructure.Distributed.Outbox.Tests
{
    /*
     * Database context, unit of work and URF Repository for INtegrationMessageLog
     * are registered using builtin methods.
     * Transactional commits are handled by the internal repository 
     */
    public class Tests_Outbox_MongoStore_Worker
    {
        private MongoDbRunner Runner;
        public TestConfigurationMongo MongoConfiguration { get; set; }
        public ServiceProvider Services { get; set; }

        public const string EVENT_ONE_NAME = "EventOne";
        public const string EVENT_TWO_NAME = "EventTwo";
        public const string EVENT_THREE_NAME = "EventThree";

        [SetUp]
        public void Setup()
        {
            Runner = MongoDbRunner.StartForDebugging();
            CleanDatabase();

            MongoConfiguration = new TestConfigurationMongo(
                Runner.ConnectionString,
                "outbox",
                "outbox");

            IServiceCollection serviceCollection = new ServiceCollection();

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
                    storeCfg.UseMongoStore(MongoConfiguration, mCfg =>
                    {
                        mCfg.UseBuiltInRepository();
                    });
                });

                cfg.ConfigureSerializer(ser => ser.UseMassTransitSerializer());
            });

            Services = serviceCollection.BuildServiceProvider();
        }

        [TearDown]
        public void TearDown()
        {
            Runner?.Dispose();
        }

        [Test]
        public async Task Worker_ShouldPublish_LogsWithPendingStatus()
        {
            IOutboxService outboxService = Services.GetService<IOutboxService>();

            Guid documentId = Guid.NewGuid();
            Guid referenceId = Guid.NewGuid();

            IEventOne eventBody = new EventOne
            {
                DocumentId = documentId,
                FileExtension = "pdf",
                FileName = "file",
                ReferenceId = referenceId,
            };

            await outboxService.OutboxPublishAsync<IEventOne>(eventBody);

            /*
             * Creating a worker using the host builder
             */
            Mock<IOutboxBrokerPublisher> brokerPublisher = new();
            IEventOne eventOne = null;

            brokerPublisher
                .Setup(r => r.PublishAsync<It.IsAnyType>(It.IsAny<It.IsAnyType>(), It.IsAny<CancellationToken>()))
                .Callback(new Action<object, CancellationToken>((item, _) =>
                {
                    // this callback catches the arguments passed to PublishAsync before the method
                    // is returned
                    eventOne = (IEventOne)item;
                }))
                .Returns(Task.CompletedTask);

            IHostBuilder workerHost =
                Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Worker_Mongo_Startup>(_ =>
                    {
                        return new Worker_Mongo_Startup(
                            configuration: MongoConfiguration,
                            outboxBrokerPublisher: brokerPublisher.Object,
                            workerPeriod: TimeSpan.FromSeconds(10),
                            timeBetweenRetries: TimeSpan.FromMinutes(1));
                    });
                });

            IHost host = workerHost.Start();

            // block until event is retrieved or timeout is reached
            // NOTE: IF YOU ARE DEBUGGIN INCREASWE THE TIMEOUT. OR THE WORKER HOST WILL CLOSE
            SpinWait.SpinUntil(() => eventOne is not null, TimeSpan.FromSeconds(60));


            Assert.IsNotNull(eventOne);
            Assert.AreEqual(documentId, eventOne.DocumentId);
            Assert.AreEqual(referenceId, eventOne.ReferenceId);

            /*
             * Checking the database for the log status
             */
            // Note : Use host.Services and not publisherServices because we are using SqlLite
            //        and after save the context is not synced

            IOutboxRepository<IntegrationMessageLog> outboxRepository =
                 host.Services
                     .GetService<OutboxPublisherWorker<IntegrationMessageLog>>()
                     .IsolatedServiceProvider
                     .GetService<IOutboxRepository<IntegrationMessageLog>>();

            var log = (await outboxRepository.FindAsync(FinderMessageLog.New(FilterMessageLog.Empty, 1))).FirstOrDefault();
            Assert.IsNotNull(log);
            Assert.AreEqual(OutboxStatus.Published, log.Status);

            await host.StopAsync();
        }

        [Test]
        public async Task Worker_ShouldSetLog_ToErrorState_AfterMaxNoOfRetries()
        {
            IOutboxService outboxService = Services.GetService<IOutboxService>();
            Guid documentId = Guid.NewGuid();
            Guid referenceId = Guid.NewGuid();

            IEventOne eventBody = new EventOne
            {
                DocumentId = documentId,
                FileExtension = "pdf",
                FileName = "file",
                ReferenceId = referenceId,
            };

            await outboxService.OutboxPublishAsync<IEventOne>(eventBody);

            /*
             * Creating a worker using the host builder
             */
            Mock<IOutboxBrokerPublisher> brokerPublisher = new Mock<IOutboxBrokerPublisher>();

            int exceptionsThrow = 0;

            brokerPublisher
                .Setup(r => r.PublishAsync<It.IsAnyType>(It.IsAny<It.IsAnyType>(), It.IsAny<CancellationToken>()))
                .Returns(() =>
                {
                    exceptionsThrow++;
                    throw new Exception("This is an error");
                });

            IHostBuilder workerHost =
                Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Worker_Mongo_Startup>(_ =>
                    {
                        return new Worker_Mongo_Startup(
                            configuration: MongoConfiguration,
                            outboxBrokerPublisher: brokerPublisher.Object,
                            workerPeriod: TimeSpan.FromSeconds(10),
                            timeBetweenRetries: TimeSpan.FromSeconds(0));
                    });
                });

            IHost host = workerHost.Start();
            // Note : Use host.Services and not publisherServices because we are using SqlLite
            //        and after save the context is not synced
            IOutboxRepository<IntegrationMessageLog> outboxRepository =
                host.Services
                  .GetService<OutboxPublisherWorker<IntegrationMessageLog>>()
                  .IsolatedServiceProvider
                  .GetService<IOutboxRepository<IntegrationMessageLog>>();

            IConfigurationOutboxWorker configuration =
                host.Services
                  .GetService<OutboxPublisherWorker<IntegrationMessageLog>>()
                  .IsolatedServiceProvider
                  .GetService<IConfigurationOutboxWorker>();

            // block until event is retrieved or timeout is reached
            // NOTE: IF YOU ARE DEBUGGIN INCREASWE THE TIMEOUT. OR THE WORKER HOST WILL CLOSE

            SpinWait.SpinUntil(
                () => exceptionsThrow == configuration.EnterErrorStateAfterNoOfRetries + 1,
                TimeSpan.FromSeconds(30));

            /*
             * Checking the database for the log status
             */

            var log = (await outboxRepository.FindAsync(FinderMessageLog.New(FilterMessageLog.Empty, 1))).FirstOrDefault();
            Assert.IsNotNull(log);
            Assert.AreEqual(OutboxStatus.ErrorState, log.Status);

            await host.StopAsync();
        }

        private void CleanDatabase()
        {
            MongoClient client = new(Runner.ConnectionString);

            foreach (string dbName in client.ListDatabaseNames().ToList())
            {
#pragma warning disable RCS1075 // Avoid empty catch clause that catches System.Exception.
                try
                {
                    client.DropDatabase(dbName);
                }
                catch (Exception)
                {
                }
#pragma warning restore RCS1075 // Avoid empty catch clause that catches System.Exception.
            }
        }
    }
}