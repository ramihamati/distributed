==============================
Mongo with external repository
==============================

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

Use this method when you want to leverage the builtin mongo manager but provide your custom
repository implementation.


Usage
-----

Creating the repository:

.. code-block:: cs

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
    }

Adding the service:

.. code-block:: cs

    services.AddOutboxService(cfg =>
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