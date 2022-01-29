====================
Multiple Log Types
====================
.. role:: yaml(code)
   :language: yaml 

.. role:: cs(code)
   :language: cs 

By default the outbox service will chose the builtin entity `IntegrationMessageLog` to save the event into. This is attached
to one repository.
If we want to save each event in a different repository, we have to 
* define new message logs implementing `IIntegrationMessageLog` or extending `IntegrationMessageLog`
* define transforms (From event to log and backwards)
* register the repositories

Transforms with auto mapper:
------------------------------

Package:

.. code-block:: yaml

    <PackageReference Include="ComX.Infrastructure.Distributed.Outbox.AutoMapper" Version="x.x.x">

Usage:

The transforms service will use the automapper mapper, but it will not inject it. It's 
your responsability to do so

.. code-block:: cs

    {
        services.AddAutoMapper(typeof(Program).Assembly);
        services.AddOutboxService(cfg =>
        {
            // code omitted
            cfg.ConfigureTransforms(trCfg =>
            {
                trCfg.Cfg.UseAutomapperTransformations();
                trCfg.RegisterTransform<IEventOne, CustomMessageLog>();
            });
            // code omitted
        });
    }

Transforms Custom
-----------------

Create a service that implements `IOutboxTransformer`

.. code-block:: cs

    public class AutoMapperService : IOutboxTransformer
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public AutoMapperService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public TTransformed Transform<TSource, TTransformed>(TSource source)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            IMapper mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
            return mapper.Map<TSource, TTransformed>(source);
        }
    }

Create an extension method:

.. code-block:: cs

    public static class ExtensionsOutboxAutoMapper
    {
        public static void UseAutomapperTransformations(
            this IConfiguratorTransformer configurator)
        {
            configurator
                .Context
                .Services
                .AddScoped<IOutboxTransformer, AutoMapperService>();
        }
    }

Example Using multiple log types:
---------------------------------

.. code-block:: cs
   :caption: Create the new entity

    public class CustomMessageLog : IntegrationMessageLog
    {
    }

.. code-block:: cs
   :caption: Create the database context

    public class MultipleLogDataContext : DbContext
    {
        public MultipleLogDataContext(DbContextOptions<MultipleLogDataContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            new IntegrationMessageLogMap("TableOne")
                .Configure(modelBuilder.Entity<IntegrationMessageLog>());
            
            new IntegrationMessageLogMap<CustomMessageLog>("TableTwo")
                .Configure(modelBuilder.Entity<CustomMessageLog>());

            base.OnModelCreating(modelBuilder);
        }
    }

.. code-block:: cs
   :caption: Create the automapper profile

    public class ProfileCustomMessageLog : Profile
    {
        public ProfileCustomMessageLog()
        {
            CreateMap<IEventOne, CustomMessageLog>()
                .ConstructUsing((Func<IEventOne, ResolutionContext, CustomMessageLog>)((model, _) =>
                {
                    return new CustomMessageLog
                    {
                        Id = Guid.NewGuid(),
                        MessageTypeName = Consts.EVENT_ONE_NAME,
                        MessageBody = JsonConvert.SerializeObject(model),
                        Status = OutboxStatus.NotPublished
                    };
                }));

            CreateMap<IEventTwo, CustomMessageLog>()
                .ConstructUsing((Func<IEventTwo, ResolutionContext, CustomMessageLog>)((model, _) =>
                {
                    return new CustomMessageLog
                    {
                        Id = Guid.NewGuid(),
                        MessageTypeName = Consts.EVENT_TWO_NAME,
                        MessageBody = JsonConvert.SerializeObject(model),
                        Status = OutboxStatus.NotPublished
                    };
                }));
        }
    }

.. code-block:: cs
   :caption: Register the service
   
    {
        services.AddAutoMapper(typeof(Program).Assembly);

        services.AddDbContext<MultipleLogDataContext>(options =>
        {
            options.UseSqlite(DbConnection);
        }, ServiceLifetime.Scoped);

        services.AddScoped<DbContext, MultipleLogDataContext>();
        services.AddScoped<IUnitOfWork, MultipleLogUnitOfWork>();

        services.AddOutboxService(cfg =>
        {
            cfg.ConfigureEvents(reg =>
            {
                reg.RegisterMessage<IEventOne, CustomMessageLog>(Consts.EVENT_ONE_NAME);
                reg.RegisterMessage<IEventTwo, CustomMessageLog>(Consts.EVENT_TWO_NAME);
                // event tree will use the default IntegrationMessageLog
                reg.RegisterMessage<IEventThree>(Consts.EVENT_THREE_NAME);
            });
            cfg.ConfigureTransforms(trCfg =>
            {
                trCfg.Cfg.UseAutomapperTransformations();
                trCfg.RegisterTransform<IEventOne, CustomMessageLog>();
                trCfg.RegisterTransform<IEventTwo, CustomMessageLog>();
            });
            cfg.ConfigureStore(storeCfg =>
            {
                storeCfg.UseUrfStore(efCfg =>
                {
                    efCfg.UseRepository<IntegrationMessageLog, MultipleLogDataContext>();
                    efCfg.UseRepository<CustomMessageLog, MultipleLogDataContext>();
                });
            });
            cfg.ConfigureSerializer(sCfg => sCfg.UseMassTransitSerializer());
        });
    }