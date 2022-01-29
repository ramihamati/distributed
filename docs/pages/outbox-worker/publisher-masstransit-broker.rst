============================
Publisher MassTransit Broker
============================

.. role:: yaml(code)
   :language: yaml 

.. role:: cs(code)
   :language: cs 

Package
-----------
.. code-block:: yaml

    <PackageReference Include="ComX.Infrastructure.Distributed.Outbox.Aspnet" Version="x.x.x">
    <PackageReference Include="ComX.Infrastructure.Distributed.Outbox.MassTransit" Version="x.x.x">

This publisher will use the `IBusControl` to publish the messages. You have to configure mass transit because
the outbox worker will only use the `IBusControl` service

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
