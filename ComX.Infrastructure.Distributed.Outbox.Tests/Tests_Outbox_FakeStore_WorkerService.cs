using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace ComX.Infrastructure.Distributed.Outbox.Tests
{
    public class Tests_Outbox_FakeStore_WorkerService
    {
        public ServiceProvider ServiceProvider { get; set; }
        public IConfigurationOutboxWorker ConfigurationWorker { get; set; }

        public const string EVENT_ONE_NAME = "EventOne";
        public const string EVENT_TWO_NAME = "EventTwo";
        public const string EVENT_THREE_NAME = "EventThree";

        [SetUp]
        public void Setup()
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();

            ConfigurationWorker = new TestConfigurationOutboxWorker(
                workerPeriod: TimeSpan.FromHours(1),
                timeBetweenRetries: TimeSpan.FromHours(1));

            serviceCollection.AddOutboxWorker(ConfigurationWorker, cfg =>
            {
                cfg.ConfigureEvents(reg =>
                {
                    reg.RegisterMessage<IEventOne>(EVENT_ONE_NAME);
                    reg.RegisterMessage<IEventTwo>(EVENT_TWO_NAME);
                    reg.RegisterMessage<IEventThree>(EVENT_THREE_NAME);
                });

                cfg.ConfigureStore(storeCfg =>
                {
                    storeCfg.UseRepository<InMemoryRepository, IntegrationMessageLog>();
                });

                cfg.ConfigurePublisher(pCfg =>
                {
                    pCfg.UseTestPublisher();
                });

                cfg.ConfigureSerializer(ser => ser.UseMassTransitSerializer());
            });

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        [Test]
        public async Task Worker_WillNotPublish_LockedMessage()
        {
            IServiceProvider containerSP = ServiceProvider
                  .GetService<OutboxPublisherWorker<IntegrationMessageLog>>()
                  .IsolatedServiceProvider;

            IOutboxStorage<IntegrationMessageLog> outboxStorage =
                containerSP
                .GetService<IOutboxStorage<IntegrationMessageLog>>();

            IntegrationMessageLog log1 = new()
            {
                CreatedAt = DateTime.UtcNow,
                LastAttemptDate = null,
                MessageBody = String.Empty,
                MessageTypeName = EVENT_ONE_NAME,
                Status = OutboxStatus.NotPublished
            };

            IntegrationMessageLog log2 = new()
            {
                CreatedAt = DateTime.UtcNow,
                LastAttemptDate = null,
                MessageBody = String.Empty,
                MessageTypeName = EVENT_ONE_NAME,
                Status = OutboxStatus.NotPublished
            };

            await outboxStorage.InsertAsync(log1);
            await outboxStorage.InsertAsync(log2);

            // because we are locking log1, the worker will not read it
            await outboxStorage.LockAsync(log1, TimeSpan.FromHours(1));

            OutboxPublisherWorker<IntegrationMessageLog> workerInstance
                = ActivatorUtilities.CreateInstance<OutboxPublisherWorker<IntegrationMessageLog>>(containerSP);

            await workerInstance.ProcessAsync();

            log1 = await outboxStorage.FindAsync(log1.Id);
            log2 = await outboxStorage.FindAsync(log2.Id);

            Assert.AreEqual(OutboxStatus.NotPublished, log1.Status);
            Assert.AreEqual(OutboxStatus.Published, log2.Status);
        }
    }
}