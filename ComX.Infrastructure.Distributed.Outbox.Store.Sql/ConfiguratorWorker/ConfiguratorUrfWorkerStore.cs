using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using URF.Core.Abstractions;
using URF.Core.EF;

namespace ComX.Infrastructure.Distributed.Outbox.store.sql;

public class ConfiguratorUrfWorkerStore
{
    private readonly ConfiguratorWorkerContext _context;

    public ConfiguratorUrfWorkerStore(ConfiguratorWorkerContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Automatically adds <see cref="IRepository{IntegrationMessageLog}"/> as <see cref="Repository{IntegrationMessageLog}"/>
    /// <para>User will externally register the <see cref="DbContext"/>. The urf framework uses the DbContext in the Repository</para>
    /// <para>The worker is responsable for using the <see cref="IOutboxUnitOfWork"/> Save method</para>
    /// </summary>
    public ConfiguratorUrfWorkerStore UseDefaultRepository()
    {
        _context.ContainerServices.TryAddScoped<IOutboxStorage<IntegrationMessageLog>, OutboxStorageEntityFramework<IntegrationMessageLog>>();
        // the user manages that externally
        _context.ContainerServices.TryAddScoped<IOutboxUnitOfWork, OutboxUowEntityFramework<DbContext>>();
        // IOutboxRepository is a wrapper over URF implementation which is used in IOutboxStorage
        // It allows us to quicklt change the underlying repository if required and not depend on URF
        _context.ContainerServices.AddScoped<IOutboxRepository<IntegrationMessageLog>, OutboxRepositoryEntityFramework<IntegrationMessageLog>>();
        _context.ContainerServices.TryAddScoped<IRepository<IntegrationMessageLog>>(sp =>
        {
            // worker runs in an isolated container. DbContext is external to it
            return new Repository<IntegrationMessageLog>(_context.AppServices.GetRequiredService<DbContext>());
        });
        return this;
    }

    /// <summary>
    /// Automatically adds <see cref="IRepository{IntegrationMessageLog}"/> as <see cref="Repository{IntegrationMessageLog}"/>
    /// <para> User will externally register the <typeparamref name="TContext"/>. The urf framework uses the DbContext in the Repository</para>
    /// <para> The worker is responsable for using the <see cref="IOutboxUnitOfWork"/> Save method</para>
    /// </summary>
    public ConfiguratorUrfWorkerStore UseDefaultRepository<TContext>()
         where TContext : DbContext
    {
        _context.ContainerServices.TryAddScoped<IOutboxStorage<IntegrationMessageLog>, OutboxStorageEntityFramework<IntegrationMessageLog>>();
        // we don't have a unit of work internally because we are not creating the context.
        // the user manages that externally
        _context.ContainerServices.TryAddScoped<IOutboxUnitOfWork, OutboxUowEntityFramework<TContext>>();
        // IOutboxRepository is a wrapper over URF implementation which is used in IOutboxStorage
        // It allows us to quicklt change the underlying repository if required and not depend on URF
        _context.ContainerServices.AddScoped<IOutboxRepository<IntegrationMessageLog>, OutboxRepositoryEntityFramework<IntegrationMessageLog>>();
        _context.ContainerServices.TryAddScoped<IRepository<IntegrationMessageLog>>(
            sp =>
            {
                // worker runs in an isolated container. DbContext is external to it
                return new Repository<IntegrationMessageLog>(_context.AppServices.GetRequiredService<TContext>());
            });
        return this;
    }

    /// <summary>
    /// User will externally register the <see cref="DbContext"/>. The urf framework uses the DbContext in the Repository
    /// <para>This method will register an <see cref="IRepository{IntegrationMessageLog}"/> that uses an <see cref="DbContext"/></para>
    /// <para> The worker is responsable for using the <see cref="IOutboxUnitOfWork"/> Save method</para>
    /// </summary>
    public ConfiguratorUrfWorkerStore UseRepository<TRepository>()
        where TRepository : class, IRepository<IntegrationMessageLog>
    {
        _context.ContainerServices.TryAddScoped<IOutboxStorage<IntegrationMessageLog>, OutboxStorageEntityFramework<IntegrationMessageLog>>();
        // we don't have a unit of work internally because we are not creating the context.
        // the user manages that externally
        _context.ContainerServices.TryAddScoped<IOutboxUnitOfWork, OutboxUowEntityFramework<DbContext>>();
        // IOutboxRepository is a wrapper over URF implementation which is used in IOutboxStorage
        // It allows us to quicklt change the underlying repository if required and not depend on URF
        _context.ContainerServices.AddScoped<IOutboxRepository<IntegrationMessageLog>, OutboxRepositoryEntityFramework<IntegrationMessageLog>>();
        
        _context.ContainerServices.TryAddScoped<IRepository<IntegrationMessageLog>>(sp =>
        {
            // worker runs in an isolated container. DbContext is external to it
            return _context.AppServices.GetService<IRepository<IntegrationMessageLog>>()
                ?? _context.AppServices.GetService<TRepository>()
                ?? ActivatorUtilities.CreateInstance<TRepository>(_context.AppServices, _context.AppServices.GetRequiredService<DbContext>());
        });

        return this;
    }

    /// <summary>
    /// Automatically adds <see cref="IRepository{IntegrationMessageLog}"/> as <see cref="Repository{IntegrationMessageLog}"/>
    /// <para>User will externally register the <typeparamref name="TContext"/>. The urf framework uses the DbContext in the Repository</para>
    /// <para>Intstructing URF to use the provided TContext instead of default DbContext</para>
    /// <para> The worker is responsable for using the <see cref="IOutboxUnitOfWork"/> Save method</para>
    /// </summary>
    public ConfiguratorUrfWorkerStore UseRepository<TRepository, TContext>()
        where TContext : DbContext
        where TRepository : class, IRepository<IntegrationMessageLog>
    {
        _context.ContainerServices.TryAddScoped<IOutboxStorage<IntegrationMessageLog>, OutboxStorageEntityFramework<IntegrationMessageLog>>();
        // we don't have a unit of work internally because we are not creating the context.
        // the user manages that externally
        _context.ContainerServices.TryAddScoped<IOutboxUnitOfWork, OutboxUowEntityFramework<TContext>>();
        // IOutboxRepository is a wrapper over URF implementation which is used in IOutboxStorage
        // It allows us to quicklt change the underlying repository if required and not depend on URF
        _context.ContainerServices.AddScoped<IOutboxRepository<IntegrationMessageLog>, OutboxRepositoryEntityFramework<IntegrationMessageLog>>();
        _context.ContainerServices.TryAddScoped<IRepository<IntegrationMessageLog>>(sp =>
        {
            // worker runs in an isolated container. DbContext is external to it
            return new Repository<IntegrationMessageLog>(_context.AppServices.GetRequiredService<TContext>());
        });
        return this;
    }

    /// <summary>
    /// Automatically adds <see cref="IRepository{IntegrationMessageLog}"/> as <see cref="Repository{IntegrationMessageLog}"/>
    /// <para>User will externally register the <typeparamref name="TContext"/>. The urf framework uses the DbContext in the Repository</para>
    /// <para>Intstructing URF to use the provided TContext instead of default DbContext</para>
    /// <para> The worker is responsable for using the <see cref="IOutboxUnitOfWork"/> Save method</para>
    /// </summary>
    public ConfiguratorUrfWorkerStore UseRepository<TMessageLog, TRepository, TContext>()
        where TMessageLog : class, IIntegrationMessageLog
        where TContext : DbContext
        where TRepository : class, IRepository<TMessageLog>
    {
        _context.ContainerServices.TryAddScoped<IOutboxStorage<TMessageLog>, OutboxStorageEntityFramework<TMessageLog>>();
        // we don't have a unit of work internally because we are not creating the context.
        // the user manages that externally
        _context.ContainerServices.TryAddScoped<IOutboxUnitOfWork, OutboxUowEntityFramework<TContext>>();
        // IOutboxRepository is a wrapper over URF implementation which is used in IOutboxStorage
        // It allows us to quicklt change the underlying repository if required and not depend on URF
        _context.ContainerServices.AddScoped<IOutboxRepository<TMessageLog>, OutboxRepositoryEntityFramework<TMessageLog>>();
        _context.ContainerServices.TryAddScoped<IRepository<TMessageLog>>(sp =>
        {
            // worker runs in an isolated container. DbContext is external to it
            return new Repository<TMessageLog>(_context.AppServices.GetRequiredService<TContext>());
        });
        return this;
    }
}
