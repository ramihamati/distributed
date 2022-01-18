# Inbox

The purpose of the Inbox library is to configure a generic service which listens to events and stores them in the database. This maintains consistency and helps with ordering events. Another functionality is the ability to replay the events on the system.

- ## Configure the outbox service

#### Adding the service
```xml 
    <PackageReference Include="ComX.Infrastructure.Distributed.Inbox.Aspnet" Version="x.x.x" />
```
```cs
Services.AddInboxService(cfg =>{
    // code explained in following sections
})
```

#### Register events
We must register the events and for each event the library will create a consumer (which is set up in the following section).
The library will listen on the registered events and will process them
```cs
Services.AddInboxService(cfg =>{
        cfg.RegisterEvents(evtCfg =>
        {
            // what are we listening for?
            evtCfg.RegisterEventType<IEventDocumentStored>("IEventDocumentStored");
            evtCfg.RegisterEventType<IEventPersistDocumentsCompleted>("IEventPersistDocumentsCompleted");
            evtCfg.RegisterEventType<IEventPersistDocumentsFailed>("IEventPersistDocumentsFailed");
            evtCfg.RegisterEventType<IEventPrepareDocumentsFailed>("IEventPrepareDocumentsFailed");
            evtCfg.RegisterEventType<IEventPrepareDocumentsCompleted>("IEventPrepareDocumentsCompleted");
        });
    // other code explained in following sections
})
```

#### Configure worker
This section only registers a worker which will be used to start the service. It has nothing to do with the masstransit broker, it only uses the masstransit hosted service
```xml 
    <PackageReference Include="ComX.Infrastructure.Distributed.inbox.masstransit" Version="x.x.x" />
```
```cs
Services.AddInboxService(cfg =>{
    // previous code 
      cfg.ConfigureWorker(wrkCfg =>
        {
            wrkCfg.UseMassTransitHostedService();
        });
    // other code explained in following sections
})
```
#### Configure the broker
This is where we setup the broker which has the consumer definition. We are using masstransit and for each registered event it will automatically create a consumer. 
Note: Because the EndPointNameFormatter (which creates names based on event types) requires the `ChannelNameAttribute` we have to manually register this attribute for each consumer (of course you can create an automatic method with reflection or other cool ways :)
```xml 
    <PackageReference Include="ComX.Infrastructure.Distributed.inbox.masstransit" Version="x.x.x" />
```
```cs
Services.AddInboxService(cfg =>{
    // previous code 
    cfg.ConfigureBroker(brkCfg =>
    {
        // what consumers are we creating for the listened events
        brkCfg.UseMassTransit(mtCfg =>
        {
            ConfigurationMassTransit settingsMassTransit = new(builder.Configuration);
            mtCfg.RegisterChannelNameForConsumer<IEventDocumentStored>("consumer-event-document-stored");
            mtCfg.RegisterChannelNameForConsumer<IEventPersistDocumentsCompleted>("consumer-event-persist-documents-completed");
            mtCfg.RegisterChannelNameForConsumer<IEventPersistDocumentsFailed>("consumer-event-persist-documents-failed");
            mtCfg.RegisterChannelNameForConsumer<IEventPrepareDocumentsCompleted>("consumer-event-prepare-documents-completed");
            mtCfg.RegisterChannelNameForConsumer<IEventPrepareDocumentsFailed>("consumer-event-prepare-documents-failed");

            ConfigureBroker(mtCfg, settingsMassTransit);
        });
    });
    // other code explained in following sections
})

void ConfigureBroker(IMassTransitConfigurator mtCfg, ConfigurationMassTransit settingsMassTransit)
{
    if (settingsMassTransit.StateBroker == StateBroker.RabbitMQ)
    {
        mtCfg.Cfg.UsingRabbitMq((ctx, r) =>
        {
            r.Host(settingsMassTransit.RabbitMQHostName, (ushort)settingsMassTransit.RabbitMQPort, "/", h =>
            {
                h.Username(settingsMassTransit.RabbitMQUsername);
                h.Password(settingsMassTransit.RabbitMQPassword);
            });
            r.MessageTopology.SetEntityNameFormatter(new EntityNameFormatter(r.MessageTopology.EntityNameFormatter, "suite"));
            r.ConfigureEndpoints(ctx, mtCfg.GetEndpointNameFormatter("suite"));
        });
    }
    else
    {
        mtCfg.Cfg.UsingAzureServiceBus((ctx, r) =>
        {
            r.Host(settingsMassTransit.SBConnectionString);

            r.MaxConcurrentCalls = 100;
            r.PrefetchCount = 100;

            r.MessageTopology.SetEntityNameFormatter(new EntityNameFormatter(r.MessageTopology.EntityNameFormatter, "suite"));
            r.ConfigureEndpoints(ctx, mtCfg.GetEndpointNameFormatter("suite"));
            // To use the mass transit default names, comment the 2 lines above and uncomment the one below
            // r.ConfigureEndpoints(ctx);
        });
    }
}
```
where 
```cs
 public class ConfigurationMassTransit
    {
        public string? SBConnectionString { get; }
        public string? RabbitMQHostName { get; }
        public int RabbitMQPort { get; }
        public string? RabbitMQUsername { get; }
        public string? RabbitMQPassword { get; }
        public StateBroker StateBroker { get; }

        public ConfigurationMassTransit(IConfiguration configuration)
        {
            StateBroker = configuration.GetRequiredValue<StateBroker>("MassTransit:StateBroker");

            if (StateBroker == StateBroker.RabbitMQ)
            {
                RabbitMQHostName = configuration.GetRequiredValue<string>("MassTransit:RabbitMQ:HostName");
                RabbitMQPort = configuration.GetRequiredValue<int>("MassTransit:RabbitMQ:Port");
                RabbitMQUsername = configuration.GetRequiredValue<string>("MassTransit:RabbitMQ:Username");
                RabbitMQPassword = configuration.GetRequiredValue<string>("MassTransit:RabbitMQ:Password");
            }
            else
            {
                SBConnectionString = configuration.GetRequiredValue<string>("MassTransit:AzureServiceBusConnectionString");
            }
        }
    }
```

#### Configure transforms
This section is not required. The transformer service will convert the received events to the transformed object, if there is a map. When no transform is registered this is skipped. The library will then save either the transformed object or the original event in the store.
```xml 
    <PackageReference Include="ComX.Infrastructure.Distributed.inbox.automapper" Version="x.x.x" />
```
```cs
Services.AddInboxService(cfg =>{
    // previous code 
        cfg.ConfigureTransforms(trCfg =>
        {
            // setting the transformer to use IMapper
            trCfg.Cfg.UseAutomapperTransformations();

            // letting framework know that we want these events transformed
            // before storing them
            trCfg.RegisterTransform<IEventDocumentStored, InboxMessageLog>();
            trCfg.RegisterTransform<IEventPersistDocumentsCompleted, InboxMessageLog>();
            trCfg.RegisterTransform<IEventPersistDocumentsFailed, InboxMessageLog>();
            trCfg.RegisterTransform<IEventPrepareDocumentsCompleted, InboxMessageLog>();
            trCfg.RegisterTransform<IEventPrepareDocumentsFailed, InboxMessageLog>();
        });
    // other code explained in following sections
})
```
You have the responsability to add automapper and register the maps. The library will assume that the service is registered
```cs
services.AddAutoMapper(typeof(Program));
```
In the above example the map is located in our main solution
```cs
public class ProfileEventDocumentStored : AutoMapper.Profile
{
    public ProfileEventDocumentStored()
    {
        CreateMap<IEventDocumentStored, InboxMessageLog>()
            .ConstructUsing(evt =>
                 new InboxMessageLog
                 {
                     UniqueId = Guid.NewGuid(),
                     MessageBody = JsonConvert.SerializeObject(evt),
                     MessageTypeName = typeof(IEventDocumentStored).Name
                 }
            );
    }
}
```
#### Configure the store
The store handles saving the event or the transformed event into the database. With urf you connect to sql/postgresql using entity framework.

- **Configuration for URF**
```xml 
    <PackageReference Include="ComX.Infrastructure.Distributed.inbox.urf" Version="x.x.x" />
```
```cs
Services.AddInboxService(cfg =>{
    // previous code 
    cfg.ConfigureStore(sCfg =>
        {
            sCfg.UseUrfRepository(urfCfg =>
            {
                urfCfg.UseRepository<InboxMessageLog>();
            });
        });
})
```
- **Configuration for mongo**
```xml 
    <PackageReference Include="ComX.Infrastructure.Distributed.inbox.mongo" Version="x.x.x" />
```
```cs
Services.AddInboxService(cfg =>{
    // previous code 
    cfg.ConfigureStore(sCfg =>
        {
             sCfg.UseMongoRepository(mngCfg =>
            {
                mngCfg.UseConnection<MongoConfiguration>();
                mngCfg.UseCollection<InboxMessageLog>("Cqrs", "InboxMessageLog");
            });
        });
})
```
The settings class for mongo is
```cs
public class MongoConfiguration : IMongoConfiguration
{
    public string ConnectionString { get; }
    public TimeSpan ConnectionTimeout { get; }

    public MongoConfiguration(IConfiguration configuration)
    {
        ConnectionString = configuration["Mongo:ConnectionString"]
            ?? throw new Exception("Could not find mongo connection string");

        ConnectionTimeout = TimeSpan.FromSeconds(10);
    }
}
```
You have the responsability to register the database context when working with sql. With mongo this is not required, as a mongo manager is present when registering the store
- **Adding Sql context**

```xml 
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="6.0.0" />
    <PackageReference Include="Npgsql.EntityFrameworkCore.SqlServer" Version="6.0.0" />
```
```cs
    services.AddDbContext<InboxDbContext>(options =>
    {
        options.UseSqlServer(
            configurationDb.ConnectionString,
            sqlOptions => { sqlOptions.EnableRetryOnFailure(); });
    }, ServiceLifetime.Scoped);
    services.AddScoped<DbContext, InboxDbContext>();
```
- **Adding Postgresql context**
```xml 
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="6.0.0" />
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="6.0.1" />
```
```cs
services.AddDbContext<InboxDbContext>(options =>
{
    options.UseNpgsql(
        configurationDb.ConnectionString,
        sqlOptions => { sqlOptions.EnableRetryOnFailure(); });
}, ServiceLifetime.Scoped);
services.AddScoped<DbContext, InboxDbContext>();
```

Example of the db context
```cs

public class InboxDbContext : DbContext
{
    public InboxDbContext(DbContextOptions<InboxDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
public class InboxMessageLog
{
    public Guid UniqueId { get; set; }
    public string MessageBody { get; set; } = String.Empty;
    public string MessageTypeName { get; set; } = String.Empty;
    public DateTime CreatedAt { get; }
    public byte[] Timestamp { get; set; } = Array.Empty<byte>();
    public InboxMessageLog()
    {
        CreatedAt = DateTime.UtcNow;
    }
}
```