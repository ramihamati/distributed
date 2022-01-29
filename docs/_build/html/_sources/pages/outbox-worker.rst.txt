==============
Outbox Worker
==============

.. toctree::
   :maxdepth: 2
   :titlesonly:
   :hidden:
   
   outbox-worker/publisher-masstransit-broker
   outbox-worker/publisher-masstransit-mediator
   outbox-worker/publisher-custom


The outbox worker has the responsibility of reading events from the database and publishes them. 
The design of the worker is made so that
* it runs in an isolated container (ServiceProvider), automatically taking all external dependencies (repositories, context) from the parent container. With this setup we can have as many outboxworkers as we want in our backgroundhost and each will run in it's own configuration.
* it has the same configuration of the store as the `IOutboxService`
* it has the same configuration of the events registration as the `IOutboxService`

The worker uses the :ref:`Worker Process <workerprocess>` which will ensure that the worker will trigger on a period basis, but
multiple threads will be spawned. If the worker is still busy while the timer is triggered, the intent of re-processing is stored and current
activity is continued.

Because the worker has to publish, we have to register the publisher in our worker.

Example

.. code-block:: cs

   public void ConfigureServices(IServiceCollection services)
   {
      services.AddScoped<ConfigurationOutboxWorker>(
                     _ => new ConfigurationOutboxWorker(_workerPeriod, _timeBetweenRetries));

      services.AddOutboxWorker<ConfigurationOutboxWorker>(cfg =>
      {
            cfg.ConfigureEvents(reg =>
            {
               reg.RegisterMessage<IEventOne>(EVENT_ONE_NAME);
               reg.RegisterMessage<IEventTwo>(EVENT_TWO_NAME);
               reg.RegisterMessage<IEventThree>(EVENT_THREE_NAME);
            });

            cfg.ConfigureStore(storeCfg =>
            {
               storeCfg.UseMongoStore(_configuration, mCfg =>
               {
                  mCfg.UseBuiltInRepository();
               });
            });
            cfg.ConfigurePublisher(pubCfg =>
            {
               pubCfg.UseMassTransitPublisher();
            });
            cfg.ConfigureSerializer(ser => ser.UseMassTransitSerializer());
      });
   }

You can register the outbox worker multiple times in the same application. It runs on an isolated container and will be 
a different background worker.