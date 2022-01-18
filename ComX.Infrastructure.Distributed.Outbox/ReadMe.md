<h1 style='color:darkcyan'> Outbox Service </h1>

The outbox pattern adds a resiliency layer on top of the messaging system by storing events in the database to be sent by a worker in a further stage.

With the outbox library we are listening saving events in the database which are later pushed by the worker to the broker. For the moment there are no options to delete the messages after they are pushed.

There are 2 projects in the outbox library
    - the outbox worker (with the purpose of reading the database and sending the events)
    - the outbox service (with the purpose of saving events to be sent in the database)

The outbox worker has implemented a read lock mechanism which will make sure that messages are pushed only once.

<h3 style='color:darkcyan'> IntegrationMessageLog </h3>

The events are serialized in the model Integration message log and for the moment there are no plans (reasons) to customize this model. It stores all required information about the event and the serialized message

The structure of the model is 
```cs
public class IntegrationMessageLog
{
    public Guid Id { get; set; }
    public string MessageBody { get; set; }
    public Status Status { get; set; }
    public string MessageTypeName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastAttemptDate { get; set; }
    public DateTime? LockUntil { get; set; }
    public int RetryCount { get; set; } = 0;
    public byte[] Timestamp { get; set; }
    public string LastError { get; set; }
    public IntegrationMessageLog()
    {
        CreatedAt = DateTime.UtcNow;
    }
}
```
```cs
 public class OutboxDataContext : DbContext
    {
        public OutboxDataContext(DbContextOptions<OutboxDataContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            new IntegrationMessageLogMap().Configure(
                    modelBuilder.Entity<IntegrationMessageLog>()
                );

            base.OnModelCreating(modelBuilder);
        }
    }
```
If you are using an external DbContext you must also register the mapping for this in EF. You can use the predefined `IntegrationMessageLogMap` in your DbContext
<h3 style='color:darkcyan'> Adding the service</h3>

```xml
<PackageReference Include="ComX.Infrastructure.Distributed.Outbox.Aspnet" Version="x.x.x">
```

First way of adding the outbox service will inject the service and all it's dependencies in the current service collection

```cs
services.AddOutboxService(cfg =>
{
   // code explained in other sections
});
```
If we want to add multiple outbox services that will publish the message in other repositories or collection we should use the isolated outbox service method. This will create a new IServiceCollection which will store all of the outbox service's dependencies. 

_Note:_ If you are using an external DbContext, the isolated outbox container will not be aware of it or download it automatically. You must explicitly specifiy this

```cs
services.AddDbContext<CustomDataContext>(options =>
{
    options.UseSqlite(DbConnection);
}, ServiceLifetime.Scoped);

// inject the external unit of work
services.AddScoped<IUnitOfWork, CustomUnitOfWork>();

//services.AddScoped<IRepository<IntegrationMessageLog>>(sp=>
//    new Repository<IntegrationMessageLog>(sp.GetRequiredService<CustomDataContext>()));
// or easier with the UrfRepository wrapper:
services.AddScoped<IRepository<IntegrationMessageLog>, UrfRepository<IntegrationMessageLog, CustomDataContext>>();

services.UseContainerOutboxService(OUTBOX_NAME, scfg =>
{
    scfg.UseExternalService<CustomDataContext>();

    scfg.Configure(cfg =>
    {
        cfg.RegisterEvents(reg =>
        {
            reg.RegisterMessage<IEventOne>(EVENT_ONE_NAME);
            reg.RegisterMessage<IEventTwo>(EVENT_TWO_NAME);
            reg.RegisterMessage<IEventThree>(EVENT_THREE_NAME);
        });
        cfg.ConfigureStore(storeCfg =>
        {
            storeCfg.UseUrfStore(efCfg =>
            {
                efCfg.UseRepositoryWithContext<CustomDataContext>();
            });
        });
        cfg.ConfigureSerializer(ser => ser.UseMassTransitSerializer());
    });
});

ServiceProvider = services.BuildServiceProvider();

IOutboxServiceProvider outboxProvider 
    = ServiceProvider.GetRequiredService<IOutboxServiceProvider>();

OutboxContainer = outboxProvider.GetServiceContainer(OUTBOX_NAME);
OutboxService = outboxProvider.GetService(OUTBOX_NAME);
```

<h3 style='color:darkcyan'> Registering the events </h3>

Each event must be registered with a name. This name is saved in the `IntegrationMessageLog`. The outbox service saves this name with it's serialized body. The outbox worker uses this name and matches it against it's registry to determine how to deserialize the event.

```cs
services.AddOutboxService(cfg =>
{
    cfg.RegisterEvents(reg =>
    {
        reg.RegisterMessage<IEventOne>("IEventDocumentStored");
        reg.RegisterMessage<IEventTwo>("IEventPersistDocumentsCompleted");
        reg.RegisterMessage<IEventThree>("IEventPrepareDocumentsFailed");
    });
   // other code commented to be explaine in other sections
});
```

<h3 style='color:darkcyan'> Configuring the store </h3>

We have multiple stores available and options to even use a custom repository (e.g. in memory repository). 

To better understand the flexibility it's good to know the underlying services. The main unit of storage is `IOutboxStorage`. It defines all the required operations needed by the message. The OutboxStorage usually uses `IOutboxRepository` and with EF it also requires the `IOutboxUnifOfWork`

<h3 style='color:darkcyan'> 1. Configuring the store: Use a custom repository</h3>

```xml
<PackageReference Include="ComX.Infrastructure.Distributed.Outbox.Aspnet" Version="x.x.x">
```

Create a custom repository that implements the repository:

```cs
public class InMemoryRepository : IOutboxRepository
{
    Task DeleteAsync(IntegrationMessageLog entity, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid entityId, CancellationToken cancellationToken = default);
    Task<IntegrationMessageLog> FindAsync(Guid entityId, CancellationToken cancellationToken = default);
    Task InsertAsync(IntegrationMessageLog entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(IntegrationMessageLog entity, CancellationToken cancellationToken = default);
    Task<List<IntegrationMessageLog>> FindAsync(FinderMessageLog findOptions,CancellationToken cancellationToken = default);
    Task<bool> LockAsync(IntegrationMessageLog entity, TimeSpan span);
    Task<bool> UnlockAsync(IntegrationMessageLog entity);
}
```

Register the service

```cs
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
```

<h3 style='color:darkcyan'> 2. Configuring the store: Use mongo with builtin repository </h3

```xml
<PackageReference Include="ComX.Infrastructure.Distributed.Outbox.Aspnet" Version="x.x.x">
<PackageReference Include="ComX.Infrastructure.Distributed.Outbox.Store.Mongo" Version="x.x.x">
```

Create the mongo settings
```cs
 public class ConfigurationMongo : IOutboxMongoSettings
    {
        public string ConnectionString { get; set;}

        public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromMinutes(1);

        public string DbName { get; set; }

        public string CollectionName { get; set; }

        public TestConfigurationMongo(
            string connectionString,
            string dbName,
            string collectionName)
        {
            ConnectionString = connectionString;
            DbName = dbName;
            CollectionName = collectionName;
        }
    }
```
Add the service using the builtin repository:

```cs
ConfigurationMongo configuration = new(
    MongoConnectionString,
    "outbox",
    "outbox");

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
        storeCfg.UseMongoStore(configuration, mCfg =>
        {
            mCfg.UseBuiltInRepository();
        });
    });
    cfg.ConfigureSerializer(sCfg => sCfg.UseMassTransitSerializer());
});
```

<h3 style='color:darkcyan'> 2. Configuring the store: Use mongo with external repository </h3

This is usefull when you want a custom implementation 
of the repository which leverages the builtin mongo manager

```xml
<PackageReference Include="ComX.Infrastructure.Distributed.Outbox.Aspnet" Version="x.x.x">
<PackageReference Include="ComX.Infrastructure.Distributed.Outbox.Store.Mongo" Version="x.x.x">
```

Create the mongo settings
```cs
 public class ConfigurationMongo : IOutboxMongoSettings
    {
        public string ConnectionString { get; set;}

        public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromMinutes(1);

        public string DbName { get; set; }

        public string CollectionName { get; set; }

        public TestConfigurationMongo(
            string connectionString,
            string dbName,
            string collectionName)
        {
            ConnectionString = connectionString;
            DbName = dbName;
            CollectionName = collectionName;
        }
    }
```

Create the mongo repository (you can check the outbox tests for the implementation of this example)

```cs
public class MongoExternalRepository : IOutboxRepository
{
    private readonly IMongoCollection<MongoIntegrationMessageLog> _collection;

    public MongoExternalRepository(
        OutobxMongoManager manager, 
        IOutboxMongoSettings mongoSettings)
    {
        _collection = manager.GetCollection<MongoIntegrationMessageLog>(
            mongoSettings.DbName, mongoSettings.CollectionName);
    }
```

Add the service
```cs
ConfigurationMongo configuration = new(
    Runner.ConnectionString,
    "outbox",
    "outbox");

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
        storeCfg.UseMongoStore(configuration, mCfg =>
        {
            mCfg.UseRepository<MongoExternalRepository>();
        });
    });
    cfg.ConfigureSerializer(sCfg => sCfg.UseMassTransitSerializer());
});
```

<h3 style='color:darkcyan'> 3. Configuring the store: Use sql with builtin context </h3

Note: Sql and Sqlite are similar. I will show only sql examples
```xml
<PackageReference Include="ComX.Infrastructure.Distributed.Outbox.Aspnet" Version="x.x.x">
<PackageReference Include="ComX.Infrastructure.Distributed.Outbox.Store.Sql" Version="x.x.x">
```
or
```xml
<PackageReference Include="ComX.Infrastructure.Distributed.Outbox.Aspnet" Version="x.x.x">
<PackageReference Include="ComX.Infrastructure.Distributed.Outbox.Store.Sqlite" Version="x.x.x">
```

This method will add a predefined `OutboxDataContext` and will use a custom UoW and Repository to persist data.
Transactions are not considered because the context is not used by the caller. Use this approach if you don't care about persisting data in a transactional commit with other entities from the caller app.

```cs
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
        storeCfg.UseSqliteStore(efCfg =>
        {
            efCfg.UseBuiltInContext(DbConnection);
        });
    });
    cfg.ConfigureSerializer(sCfg => sCfg.UseMassTransitSerializer());
});
```

The `UseBuiltinContext` has 2 overloads. One allows you to pass the DbConnection and the other the settings `ISqlSettings`. Passing the dbconection is usefull in unit tests:

```cs
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
        storeCfg.UseSqliteStore(efCfg =>
        {
            efCfg.UseBuiltInContext(DbConnection);
        });
    });
    cfg.ConfigureSerializer(sCfg => sCfg.UseMassTransitSerializer());
});

Services = serviceCollection.BuildServiceProvider();

```

<h3 style='color:darkcyan'> 4. Configuring urf: Use external db context and unit of work</h3>

This allows you to have transactional commits when using also the DbContext in your app:

serviceCollection.AddDbContext<CustomDataContext>(options =>
{
    options.UseSqlite(DbConnection);
}, ServiceLifetime.Scoped);

serviceCollection.AddScoped<DbContext, CustomDataContext>();
// inject the external unit of work
serviceCollection.AddScoped<IUnitOfWork, CustomUnitOfWork>();

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
        storeCfg.UseUrfStore(efCfg =>
        {
            // will inject if not present the IRepository<IntegrationMessageLog>
            efCfg.UseRepository<Repository<IntegrationMessageLog>>();
        });
    });
    cfg.ConfigureSerializer(ser => ser.UseMassTransitSerializer());
});

We have multiple overloads for UseRepository(). The default method will use the DbContext injected in services. We can also tell it to use the actual context
```
 efCfg.UseRepositoryWithContext<CustomDataContext>();
```

<h3 style='color:darkcyan'> Serializer </h3>

This serializer is required and it's not implicitly added. The only implemented one is leveraging 
mass transit dynamic serializer/deserializer implementation

```cs
serviceCollection.AddOutboxService(cfg =>
{
    // commented code
    cfg.ConfigureSerializer(ser => ser.UseMassTransitSerializer());
});
```


<h1 style='color:darkcyan'> Outbox Worker </h1>

THe worker configuration is similar with the publisher. All options are present(sql, sqlite, mongo, etc).
The difference is that the worker must have the publisher `cfg.ConfigurePublisher(pubCfg => pubCfg.UseMassTransitPublisher())`

Note that the MassTransitPublisher() does not allow you to configure masstransit in the worker registration, this has to be done
sepparately. The framework will only know that there is IBusControl present

```
services.AddMassTransit(cfg=>{
    // add mass transit yourself. The library uses IBusControl and assumes that it is added
})
```

Worker with broker publisher (IBusControl):
```cs
services.AddScoped<IConfigurationOutboxWorker>(
    _ => new Configuration(_workerPeriod, _timeBetweenRetries));

 services.AddOutboxWorker<Configuration>(cfg =>
 {
    cfg.RegisterEvents(reg =>
    {
        reg.RegisterMessage<IEventOne>(EVENT_ONE_NAME);
        reg.RegisterMessage<IEventTwo>(EVENT_TWO_NAME);
        reg.RegisterMessage<IEventThree>(EVENT_THREE_NAME);
    });

    cfg.ConfigureStore(storeCfg =>
        storeCfg.UseSqlStore(sqlCfg
            => sqlCfg.UseSqliteBuiltInContext(_dbConnection)));

    cfg.ConfigurePublisher(pubCfg => pubCfg.UseMassTransitPublisher());
    cfg.ConfigureSerializer(ser => ser.UseMassTransitSerializer());
});
```

Worker with mediator publisher (IMediator)
```cs
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
        storeCfg.UseSqlStore(sqlCfg =>
        {
            sqlCfg.UseSqliteBuiltInContext(con);
        });
    });

    cfg.ConfigurePublisher(pCfg => pCfg.UseMassTransitMediatorPublisher());
    cfg.ConfigureSerializer(ser => ser.UseMassTransitSerializer());
});
```
