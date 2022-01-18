using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using URF.Core.Abstractions;
using URF.Core.EF;

namespace ComX.Infrastructure.Distributed.Outbox.Tests;

public class SampleStartup
{
    public void OutboxWorker_Mediator()
    {
        IServiceCollection services = new ServiceCollection();
        SqliteConnection con = new SqliteConnection(@"Data Source = :memory:");
        con.Open();

        services.AddScoped<IConfigurationOutboxWorker>(_ => new TestConfigurationOutboxWorker(
            TimeSpan.FromHours(1),
            TimeSpan.FromMinutes(1)));

        services.AddOutboxWorker<TestConfigurationOutboxWorker>(cfg =>
        {
            cfg.RegisterEvents(reg =>
            {
                reg.RegisterMessage<IEventOne>("IEventDocumentStored");
                reg.RegisterMessage<IEventTwo>("IEventPersistDocumentsCompleted");
                reg.RegisterMessage<IEventThree>("IEventPrepareDocumentsFailed");
            });
            cfg.ConfigureStore(storeCfg =>
            {
                storeCfg.UseSqliteStore(sqlCfg =>
                {
                    sqlCfg.UseBuiltInContext(con);
                });
            });

            cfg.ConfigurePublisher(pCfg => pCfg.UseMassTransitMediatorPublisher());
            cfg.ConfigureSerializer(ser => ser.UseMassTransitSerializer());
        });

        con.Close();
    }

    public void OutboxWorker_RegisterWithBuiltinContext()
    {
        IServiceCollection services = new ServiceCollection();
        SqliteConnection con = new SqliteConnection(@"Data Source = :memory:");
        con.Open();

        services.AddScoped<IConfigurationOutboxWorker>(_ => new TestConfigurationOutboxWorker(
            TimeSpan.FromHours(1),
            TimeSpan.FromMinutes(1)));

        services.AddOutboxWorker<TestConfigurationOutboxWorker>(cfg =>
        {
            cfg.RegisterEvents(reg =>
            {
                reg.RegisterMessage<IEventOne>("IEventDocumentStored");
                reg.RegisterMessage<IEventTwo>("IEventPersistDocumentsCompleted");
                reg.RegisterMessage<IEventThree>("IEventPrepareDocumentsFailed");
            });
            cfg.ConfigureStore(storeCfg =>
            {
                storeCfg.UseSqliteStore(sqlCfg =>
                {
                    sqlCfg.UseBuiltInContext(con);
                });
            });

            cfg.ConfigurePublisher(pCfg => pCfg.UseMassTransitPublisher());

            cfg.ConfigureSerializer(ser => ser.UseMassTransitSerializer());
        });

        con.Close();
    }

    public void Outbox_Mongo_RegisterWithBuiltinRepository()
    {
        IServiceCollection services = new ServiceCollection();

        services.AddOutboxService(cfg =>
        {
            cfg.RegisterEvents(reg =>
            {
                reg.RegisterMessage<IEventOne>("IEventDocumentStored");
                reg.RegisterMessage<IEventTwo>("IEventPersistDocumentsCompleted");
                reg.RegisterMessage<IEventThree>("IEventPrepareDocumentsFailed");
            });
            cfg.ConfigureStore(storeCfg =>
            {
                storeCfg.UseMongoStore<SampleMongoSettings>(mCfg =>
                {
                    mCfg.UseBuiltInRepository();
                });
            });

            cfg.ConfigureSerializer(ser => ser.UseMassTransitSerializer());
        });
    }

    public void Outbox_Mongo_RegisterWithExternalRepository()
    {
        IServiceCollection services = new ServiceCollection();

        services.AddOutboxService(cfg =>
        {
            cfg.RegisterEvents(reg =>
            {
                reg.RegisterMessage<IEventOne>("IEventDocumentStored");
                reg.RegisterMessage<IEventTwo>("IEventPersistDocumentsCompleted");
                reg.RegisterMessage<IEventThree>("IEventPrepareDocumentsFailed");
            });
            cfg.ConfigureStore(storeCfg =>
            {
                storeCfg.UseMongoStore<SampleMongoSettings>(mCfg =>
                {
                    // in this case the repository can be fully independent of anything,
                    // or it can leverage the MongoDbManager inserted. There is no real link between the user added
                    // repository and the framework added manager
                    mCfg.UseRepository<SampleOutboxRepository>();
                });
            });

            cfg.ConfigureSerializer(ser => ser.UseMassTransitSerializer());
        });
    }


    public void Outbox_Sql_RegisterWithBuiltinContext()
    {
        IServiceCollection services = new ServiceCollection();

        services.AddOutboxService(cfg =>
        {
            cfg.RegisterEvents(reg =>
            {
                reg.RegisterMessage<IEventOne>("IEventDocumentStored");
                reg.RegisterMessage<IEventTwo>("IEventPersistDocumentsCompleted");
                reg.RegisterMessage<IEventThree>("IEventPrepareDocumentsFailed");
            });

            cfg.ConfigureStore(storeCfg =>
            {
                storeCfg.UseSqliteStore(sqlCfg =>
                {
                    sqlCfg.UseBuiltInContext(new SampleSqlSettings());
                });
            });

            cfg.ConfigureSerializer(ser => ser.UseMassTransitSerializer());
        });
    }

    public void Outbox_Sql_RegisterWithExternalRepository()
    {
        // transactional commits, db context, repository are defined externally
        IServiceCollection services = new ServiceCollection();
        services.AddOutboxService(cfg =>
        {
            cfg.RegisterEvents(reg =>
            {
                reg.RegisterMessage<IEventOne>("IEventDocumentStored");
                reg.RegisterMessage<IEventTwo>("IEventPersistDocumentsCompleted");
                reg.RegisterMessage<IEventThree>("IEventPrepareDocumentsFailed");
            });
            cfg.ConfigureStore(storeCfg =>
            {
                storeCfg.UseRepository<SampleOutboxRepository>();
            });

            cfg.ConfigureSerializer(ser => ser.UseMassTransitSerializer());
        });

        Assert.Pass();
    }

    public void Outbox_Sql_RegisterWithExternalURFRepository()
    {
        // EXTERNALLY CREATING THE COTNEXT AND INJECTING THE IREPOSITORY
        IServiceCollection services = new ServiceCollection();

        services.AddDbContext<OutboxDataContext>(options =>
        {
            options.UseSqlServer("connection string",
                sqlServerOptionsAction: sqlOptions =>
                {
                });
        }, ServiceLifetime.Scoped);

        services.AddScoped<DbContext, CustomDataContext>();

        // create a unit of work used in the repository perhaps, or in a service

        // YOU HAVE TO DEFINE A UNIT OF WORK ALSO EXTERNALLY
        //_services.AddScoped<IUnitOfWork, MyUnitOfWork>();
        services.AddScoped<IRepository<IntegrationMessageLog>, Repository<IntegrationMessageLog>>();

        services.AddOutboxService(cfg =>
        {
            cfg.RegisterEvents(reg =>
            {
                reg.RegisterMessage<IEventOne>("IEventDocumentStored");
                reg.RegisterMessage<IEventTwo>("IEventPersistDocumentsCompleted");
                reg.RegisterMessage<IEventThree>("IEventPrepareDocumentsFailed");
            });
            cfg.ConfigureStore(storeCfg =>
            {
                storeCfg.UseUrfStore(sqlCfg =>
                {
                    sqlCfg.UseRepository<Repository<IntegrationMessageLog>>();
                });
            });

            cfg.ConfigureSerializer(ser => ser.UseMassTransitSerializer());
        });

        Assert.Pass();
    }
}
