====================
Store
====================

.. toctree::
   :maxdepth: 2
   :titlesonly:
   :hidden:

   stores/outbox-service-custom-repository
   stores/outbox-service-mongo-builtin
   stores/outbox-service-mongo-externalrepo
   stores/outbox-service-mongo-wrapperrepo
   stores/outbox-service-sql-builtin
   stores/outbox-service-external-db-context-urf

.. role:: yaml(code)
   :language: yaml 

.. role:: cs(code)
   :language: cs 

Internals
---------

To understand how a store is configured, it's important to know about the internals.

The outbox service will use the :cs:`IOutboxStorage<TIntegrationMessageLog>` which contains all 
methods that the service and the worker actually needs. The outbox storage is does not contain 
information about how the data is persisted, it uses internally an :cs:`IOutboxRepository` to do that.

.. code-block:: cs

    public interface IOutboxStorage<TMessage>
        where TMessage : class, IIntegrationMessageLog
    {
        Task InsertAsync(TMessage item, CancellationToken cancellationToken = default);
        Task UpdateAsync(TMessage item, CancellationToken cancellationToken = default);
        Task DeleteAsync(TMessage item, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<TMessage> FindAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<TMessage>> FindAsync(FinderMessageLog finder, CancellationToken cancellationToken = default);
        Task<bool> LockAsync(TMessage entity, TimeSpan span);
        Task<bool> UnlockAsync(TMessage entity);
    }

Both the outbox storage and outbox repository have different implementation depending on the persistance medium (sql, inmemory, custom, mongo).

.. code-block:: cs

    public interface IOutboxRepository<TMessage>
        where TMessage : class, IIntegrationMessageLog
    {
        Task DeleteAsync(TMessage entity, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid entityId, CancellationToken cancellationToken = default);
        Task<TMessage> FindAsync(Guid entityId, CancellationToken cancellationToken = default);
        Task InsertAsync(TMessage entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(TMessage entity, CancellationToken cancellationToken = default);
        Task<List<TMessage>> FindAsync(
            FinderMessageLog findOptions,
            CancellationToken cancellationToken = default);
    }