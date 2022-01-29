======================================================
Sql/Sqlite with external db context and URF Repository
======================================================

.. role:: yaml(code)
   :language: yaml 

.. role:: cs(code)
   :language: cs 

Package
-----------
.. code-block:: yaml

    <PackageReference Include="ComX.Infrastructure.Distributed.Outbox.Aspnet" Version="x.x.x">
    <PackageReference Include="ComX.Infrastructure.Distributed.Outbox.Store.Sql" Version="x.x.x">
    <PackageReference Include="ComX.Infrastructure.Distributed.Outbox.Store.Sqlite" Version="x.x.x">

About
-----------

This allows you to have transactional commits when using also the DbContext in your app. You are providing the context and the unit of work. 
The outbox service will insert the entity to be saved but you are responsible to save the changes. This can be done in a transaction.

The default entity used is the :cs:`IntegrationMessageLog` and the db context will search for a tabled named `IntegrationMessageLogs`

The following map is used

.. code-block:: cs

    public  class IntegrationMessageLogMap<TMessageLog> : IEntityTypeConfiguration<TMessageLog>
        where TMessageLog : class, IIntegrationMessageLog
    {
        private readonly string _tableName;

        public IntegrationMessageLogMap(string tableName)
        {
            this._tableName = tableName;
        }

        public void Configure(EntityTypeBuilder<TMessageLog> builder)
        {
            builder.ToTable(_tableName);
            builder.HasKey(t => t.Id);
            builder.Property(x => x.Id) .IsRequired();
            builder.Property(x => x.MessageBody).IsRequired();
            builder.Property(x => x.Status) .IsRequired();
            builder.Property(x => x.MessageTypeName) .IsRequired();
            builder.Property(x => x.CreatedAt) .IsRequired().HasColumnType("datetime2");
            builder.Property(x => x.LastAttemptDate).HasColumnType("datetime2");
            builder.Property(x => x.LockUntil).HasColumnType("datetime2");
            builder.Property(x => x.RetryCount).IsRequired();
            builder.Property(x => x.LastError);
            builder.Property(x=> x.Timestamp).IsConcurrencyToken();
        }
    }

Register the service
--------------------

.. code-block:: cs

    serviceCollection.AddDbContext<CustomDataContext>(options =>
    {
        options.UseSqlite(DbConnection);
    }, ServiceLifetime.Scoped);

    serviceCollection.AddScoped<DbContext, CustomDataContext>();
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

**Note: if you are not specifying the context in method `UseRepository` it will automatically search for DbContext**

Overloads
---------

* :cs:`UrfStoreConfigurator UseRepository<TModel>() where TModel : class`

Automatically add the URF :cs:`IRepository<TModel>` with it's implementation :cs:`Repository<TModel>`

* :cs:`UrfStoreConfigurator UseRepository<TModel, TRepository>() where TModel : class where TRepository : class, IRepository<TModel>`

Adding the custom urf implementation of :cs:`IRepository<TModel>`

* :cs:`UrfStoreConfigurator UseRepositoryWithContext<TModel, TContext>() where TModel : class   where TContext : DbContext`

Automatically add the URF :cs:`IRepository<TModel>` with it's implementation :cs:`Repository<TModel>` built to use the specified `TContext`