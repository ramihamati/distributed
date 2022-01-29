=============================
Mongo with builtin repository
=============================

.. role:: yaml(code)
   :language: yaml 

.. role:: cs(code)
   :language: cs 


Package
-----------
.. code-block:: yaml

    <PackageReference Include="ComX.Infrastructure.Distributed.Outbox.Aspnet" Version="x.x.x">
    <PackageReference Include="ComX.Infrastructure.Distributed.Outbox.Store.Mongo" Version="x.x.x">

About
-----------

Using this method will allow you to only specify the mongo connection details leaving 
the outbox service to deal with the mongo manager and repositories

Usage
-----

.. code-block:: cs

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