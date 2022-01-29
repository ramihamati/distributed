================
Publisher Custom
================

.. role:: yaml(code)
   :language: yaml 

.. role:: cs(code)
   :language: cs 

Package
-----------
.. code-block:: yaml

    <PackageReference Include="ComX.Infrastructure.Distributed.Outbox.Aspnet" Version="x.x.x">

To create a custom publisher you have to:

* Create a service implementing `IOutboxBrokerPublisher`

.. code-block:: cs
   
   public class MyPublisher : IOutboxBrokerPublisher
   {
      private readonly IMediator _mediator;

      public MyPublisher(
         IMediator mediator)
      {
         _mediator = mediator;
      }

      public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
      {
         return _mediator.Publish<T>(message, cancellationToken);
      }
   }

* Create an extension method

.. code-block:: cs

   public static class ExtensionsMyPublisher
   {
      public static void UseMyPublisher(
         this IConfiguratorWorkerPublisher brokerConfigurator)
      {
         brokerConfigurator.Context.ContainerServices.TryAddScoped<IOutboxBrokerPublisher, MyPublisher>();
      }
   }

* Register the service

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
               pubCfg.UseMyPublisher();
            });
            cfg.ConfigureSerializer(ser => ser.UseMassTransitSerializer());
      });
   }
