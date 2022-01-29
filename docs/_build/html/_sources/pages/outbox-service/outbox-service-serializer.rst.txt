====================
Serializer
====================


The serializer configuration is important, there is no implicit using of it. There is a mass transit serializer implementation
which is used both in serialization and deserialization. The reason for using this particular implementation is that it uses their
green pipes library making it possible to use an interface and dynamically build the object

Adding a custom serializer
--------------------------

You have to implement the interface `IEventSerializer` and create an extension method which you can use in your configuration.

.. code-block:: cs

   public interface IEventSerializer
    {
        public string Serialize<T>(T @event);
        public object Deserialize(string body, Type eventType);
    }

Example

.. code-block:: cs

    public class MySerializer : IEventSerializer
    {
        public object Deserialize(string body, Type eventType)
        {
            // implementation
        }

        public string Serialize<T>(T @event)
        {
            // implementation
        }
    }

.. code-block:: cs

    public static class ExtensionsEventSerializerConfigurator
    {
        public static void UseMySerializer(
            this IConfiguratorEventSerializer serializerConfigurator)
        {
            serializerConfigurator.Context.Services.TryAddScoped<IEventSerializer, MySerializer>();
        }

        public static void UseMySerializer(
            this IConfiguratorWorkerEventSerializer serializerConfigurator)
        {
            serializerConfigurator.Context.ContainerServices.TryAddScoped<IEventSerializer, MySerializer>();
        }
    }

.. code-block:: cs

    services.AddOutboxService(cfg =>
    {
        cfg.ConfigureEvents(reg =>
        {
            reg.RegisterMessage<IEventOne>("IEventDocumentStored");
            reg.RegisterMessage<IEventTwo>("IEventPersistDocumentsCompleted");
            reg.RegisterMessage<IEventThree>("IEventPrepareDocumentsFailed");
        });
        cfg.ConfigureStore(storeCfg =>
        {
            storeCfg.UseUrfStore(sqlCfg =>
            {
                sqlCfg.UseRepository<Repository<IntegrationMessageLog>>();
            });
        });

        cfg.ConfigureSerializer(ser => ser.UseMySerializer());
    });

Using built-in serializer
-------------------------

.. code-block:: yaml

    <PackageReference Include="ComX.Infrastructure.Distributed.Outbox.MassTransit" Version="x.x.x">

.. code-block:: cs

    services.AddOutboxService(cfg =>
    {
        cfg.ConfigureEvents(reg =>
        {
            reg.RegisterMessage<IEventOne>("IEventDocumentStored");
            reg.RegisterMessage<IEventTwo>("IEventPersistDocumentsCompleted");
            reg.RegisterMessage<IEventThree>("IEventPrepareDocumentsFailed");
        });
        cfg.ConfigureStore(storeCfg =>
        {
            storeCfg.UseUrfStore(sqlCfg =>
            {
                sqlCfg.UseRepository<Repository<IntegrationMessageLog>>();
            });
        });

        cfg.ConfigureSerializer(ser => ser.UseMassTransitSerializer());
    });
