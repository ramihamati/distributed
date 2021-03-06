====================
Asp Net Registration
====================

.. role:: yaml(code)
   :language: yaml 

.. role:: cs(code)
   :language: cs 

Package
-----------
:yaml:`<PackageReference Include="ComX.Infrastructure.Distributed.Outbox.Aspnet" Version="x.x.x">`

Stand Alone
-----------

The outbox service and all dependencies are injected in the current :cs:`ServiceProvider`. You can configure
the outbox service to save the events using multiple repositories but you cannot configure multiple outbox services.


.. code-block:: cs

   services.AddOutboxService(cfg =>
   {
      // code explained in other sections
   });

Outbox Service Provider
-----------------------

You can register multiple outbox services, each service is named and has it's own :cs:`ServiceProvider` container. 
This makes all the dependencies for the specific service isolated (no overlapping configuration). Accessing the 
:cs:`OutboxService` is done using the :cs:`IOutboxServiceProvider`.

For this type of registry you have to explicitly specify what services have to be **downloaded** from the running 
:cs:`ServiceProvider` (services like :cs:`DbContext` or :cs:`IConfiguration`). This is done using the method
`UseExternalService`


Example:

.. code-block:: cs

   public class CustomDataContext: DbContext
   {
      public CustomDataContext(DbContextOptions<OutboxDataContext> options) :
            base(options){ }

      protected override void OnModelCreating(ModelBuilder modelBuilder)
      {
            new IntegrationMessageLogMap().Configure(
                        modelBuilder.Entity<IntegrationMessageLog>());
            base.OnModelCreating(modelBuilder);
      }
   }

   services.AddDbContext<CustomDataContext>(options =>
   {
      options.UseSqlite(DbConnection);
   }, ServiceLifetime.Scoped);

   services.AddScoped<IUnitOfWork, CustomUnitOfWork>();

   services.UseContainerOutboxService("service_name", scfg =>
   {
      scfg.UseExternalService<CustomDataContext>();

      scfg.Configure(cfg =>
      {
         cfg.ConfigureEvents(reg =>
         {
               reg.RegisterMessage<IEventOne>(EVENT_ONE_NAME);
               reg.RegisterMessage<IEventTwo>(EVENT_TWO_NAME);
               reg.RegisterMessage<IEventThree>(EVENT_THREE_NAME);
         });
         cfg.ConfigureStore(storeCfg =>
         {
               storeCfg.UseUrfStore(efCfg =>
               {
                  efCfg.UseDefaultRepository<CustomDataContext>();
               });
         });
         cfg.ConfigureSerializer(ser => ser.UseMassTransitSerializer());
      });
   });

To access this specific outbox service in code you have to use the :cs:`IOutboxServiceProvider`

.. code-block:: cs

   constructor(IOutboxServiceProvider provider){
      IOutboxServiceContainer outboxContainer = provider.GetServiceContainer("service_name");
      IOutboxService outboxService = provider.GetService("service_name");
   }

The outbox container is usefull when you want to access the inner :cs:`ServiceProvider`