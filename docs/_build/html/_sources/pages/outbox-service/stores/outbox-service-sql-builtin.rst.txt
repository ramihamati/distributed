===============================
Sql/Sqlite with builtin context
===============================

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

This method will add a predefined OutboxDataContext and will use a custom UoW and Repository to persist data.
Transactions are not considered because the context is not used by the caller.
Use this approach if you don't care about persisting data in a transactional commit with other entities from the caller app.

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

Overloads
---------

* :cs:`ConfiguratorSqlStore UseBuiltInContext(ISqlSettings sqlSettings, Action<SqlServerDbContextOptionsBuilder> builder = null);`

This method configures the sql using a connection string

.. code-block:: cs

    public interface ISqlSettings
    {
        public string ConnectionString { get; }
        public int MaxRetryCount { get; }
        public TimeSpan MaxRetryDelay { get; }
    }

* :cs:`public ConfiguratorSqlStore UseBuiltInContext(DbConnection dbConnection, Action<SqlServerDbContextOptionsBuilder> builder = null)`

This method is usefull for unit tests:

Example:

.. code-block:: cs

    SqliteConnection DbConnection = new SqliteConnection(@"Data Source = :memory:");
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
        cfg.ConfigureEvents(reg =>
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

    ServiceProvider Services = serviceCollection.BuildServiceProvider();