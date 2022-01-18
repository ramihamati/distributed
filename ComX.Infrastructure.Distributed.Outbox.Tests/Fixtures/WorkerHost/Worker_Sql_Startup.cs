using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data.Common;

namespace ComX.Infrastructure.Distributed.Outbox.Tests
{
    public class Worker_Sql_Startup
    {
        public const string EVENT_ONE_NAME = "EventOne";
        public const string EVENT_TWO_NAME = "EventTwo";
        public const string EVENT_THREE_NAME = "EventThree";
        
        private readonly DbConnection _dbConnection;
        private readonly IOutboxBrokerPublisher _outboxBrokerPublisher;
        private readonly TimeSpan _workerPeriod;
        private readonly TimeSpan _timeBetweenRetries;

        public Worker_Sql_Startup(
            DbConnection dbConnection,
            IOutboxBrokerPublisher outboxBrokerPublisher,
            TimeSpan workerPeriod,
            TimeSpan timeBetweenRetries)
        {
            _dbConnection = dbConnection;
            _outboxBrokerPublisher = outboxBrokerPublisher;
            _workerPeriod = workerPeriod;
            _timeBetweenRetries = timeBetweenRetries;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IConfigurationOutboxWorker>(
                           _ => new TestConfigurationOutboxWorker(_workerPeriod, _timeBetweenRetries));
            
            services.AddLogging();

            // MASS TRANSIT MUST BE REGISTERED FOR THE IBusControl
            // OF THE PUBLISHER

            // FOR TESTING I WILL BE USING A MOCK
            //services.AddMassTransit(cfg =>
            //{

            //});

            // services.UseMassTransitPublisher() is with TryAddScoped
            services.AddScoped<IOutboxBrokerPublisher>(_ => _outboxBrokerPublisher);
            
            services.AddOutboxWorker<TestConfigurationOutboxWorker>(cfg =>
            {
                cfg.RegisterEvents(reg =>
                {
                    reg.RegisterMessage<IEventOne>(EVENT_ONE_NAME);
                    reg.RegisterMessage<IEventTwo>(EVENT_TWO_NAME);
                    reg.RegisterMessage<IEventThree>(EVENT_THREE_NAME);
                });
                
                cfg.ConfigureStore(storeCfg =>
                    storeCfg.UseSqliteStore(sqlCfg
                        => sqlCfg.UseBuiltInContext(_dbConnection)));

                //cfg.ConfigurePublisher(pubCfg => pubCfg.UseMassTransitPublisher());
                cfg.ConfigureSerializer(ser => ser.UseMassTransitSerializer());
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

        }
    }
}