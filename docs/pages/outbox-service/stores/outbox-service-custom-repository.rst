===========================
Custom IOutboxRepository<T>
===========================

.. role:: yaml(code)
   :language: yaml 

.. role:: cs(code)
   :language: cs 

We can set up the outbox service to use a specific storage which will call our custom repository, the outbox
service is not aware of the type of persistance medium, as this will be provided by the custom repository.

.. code-block:: cs

    public class InMemoryRepository : IOutboxRepository<IntegrationMessageLog>
    {
        Task DeleteAsync(IntegrationMessageLog entity, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid entityId, CancellationToken cancellationToken = default);
        Task<IntegrationMessageLog> FindAsync(Guid entityId, CancellationToken cancellationToken = default);
        Task InsertAsync(IntegrationMessageLog entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(IntegrationMessageLog entity, CancellationToken cancellationToken = default);
        Task<List<IntegrationMessageLog>> FindAsync(FinderMessageLog findOptions,CancellationToken cancellationToken = default);
        Task<bool> LockAsync(IntegrationMessageLog entity, TimeSpan span);
        Task<bool> UnlockAsync(IntegrationMessageLog entity);
    }

Registering the repository

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
            storeCfg.UseRepository<InMemoryRepository>();
        });
        cfg.ConfigureSerializer(ser => ser.UseMassTransitSerializer());
    });