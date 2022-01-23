using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using URF.Core.Abstractions;
using URF.Core.EF;

namespace ComX.Infrastructure.Distributed.Outbox.store.sql;

public class ConfiguratorUrfStore
{
    private readonly ConfiguratorContext _context;

    public ConfiguratorUrfStore(ConfiguratorContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Automatically adds <see cref="IRepository{IntegrationMessageLog}"/> as <see cref="Repository{IntegrationMessageLog}"/>
    /// <para>User will externally register the <see cref="DbContext"/>. The urf framework uses the DbContext in the Repository</para>
    /// <para>IMPORTANT: The caller is responsable for using the <see cref="IUnitOfWork"/> Save method</para>
    /// </summary>
    public ConfiguratorUrfStore UseDefaultRepository()
    {
        _context.Services.TryAddScoped<IOutboxStorage<IntegrationMessageLog>, OutboxStorageEntityFramework<IntegrationMessageLog>>();
        // the user manages that externally
        _context.Services.TryAddScoped<IOutboxUnitOfWork, NoopOutboxUnitOfWork>();
        // IOutboxRepository is a wrapper over URF implementation which is used in IOutboxStorage
        // It allows us to quicklt change the underlying repository if required and not depend on URF
        _context.Services.AddScoped<IOutboxRepository<IntegrationMessageLog>, OutboxRepositoryEntityFramework<IntegrationMessageLog>>();
        _context.Services.TryAddScoped<IRepository<IntegrationMessageLog>, Repository<IntegrationMessageLog>>();
        return this;
    }

    /// <summary>
    /// Automatically adds <see cref="IRepository{IntegrationMessageLog}"/> as <see cref="Repository{IntegrationMessageLog}"/>
    /// <para>User will externally register the <typeparamref name="TContext"/>. The urf framework uses the DbContext in the Repository</para>
    /// <para>IMPORTANT: The caller is responsable for using the <see cref="IUnitOfWork"/> Save method</para>
    /// </summary>
    public ConfiguratorUrfStore UseDefaultRepository<TContext>()
         where TContext : DbContext
    {
        _context.Services.TryAddScoped<IOutboxStorage<IntegrationMessageLog>, OutboxStorageEntityFramework<IntegrationMessageLog>>();
        // we don't have a unit of work internally because we are not creating the context.
        // the user manages that externally
        _context.Services.TryAddScoped<IOutboxUnitOfWork, NoopOutboxUnitOfWork>();
        // IOutboxRepository is a wrapper over URF implementation which is used in IOutboxStorage
        // It allows us to quicklt change the underlying repository if required and not depend on URF
        _context.Services.AddScoped<IOutboxRepository<IntegrationMessageLog>, OutboxRepositoryEntityFramework<IntegrationMessageLog>>();
        _context.Services.TryAddScoped<IRepository<IntegrationMessageLog>>(
            sp => new Repository<IntegrationMessageLog>(sp.GetRequiredService<TContext>()));
        return this;
    }

    /// <summary>
    /// User will externally register the <see cref="DbContext"/>. The urf framework uses the DbContext in the Repository
    /// <para>This method will register an <see cref="IRepository{IntegrationMessageLog}"/> that uses an <see cref="DbContext"/></para>
    /// <para>IMPORTANT: The caller is responsable for using the <see cref="IUnitOfWork"/> Save method</para>
    /// </summary>
    public ConfiguratorUrfStore UseRepository<TRepository>()
        where TRepository : class, IRepository<IntegrationMessageLog>
    {
        _context.Services.TryAddScoped<IOutboxStorage<IntegrationMessageLog>, OutboxStorageEntityFramework<IntegrationMessageLog>>();
        // we don't have a unit of work internally because we are not creating the context.
        // the user manages that externally
        _context.Services.TryAddScoped<IOutboxUnitOfWork, NoopOutboxUnitOfWork>();
        // IOutboxRepository is a wrapper over URF implementation which is used in IOutboxStorage
        // It allows us to quicklt change the underlying repository if required and not depend on URF
        _context.Services.AddScoped<IOutboxRepository<IntegrationMessageLog>, OutboxRepositoryEntityFramework<IntegrationMessageLog>>();
        _context.Services.TryAddScoped<IRepository<IntegrationMessageLog>, TRepository>();
        return this;
    }

    /// <summary>
    /// Automatically adds <see cref="IRepository{IntegrationMessageLog}"/> as <see cref="Repository{IntegrationMessageLog}"/>
    /// <para>User will externally register the <typeparamref name="TContext"/>. The urf framework uses the DbContext in the Repository</para>
    /// <para>Intstructing URF to use the provided TContext instead of default DbContext</para>
    /// <para>IMPORTANT: The caller is responsable for using the <see cref="IUnitOfWork"/> Save method</para>
    /// </summary>
    //public ConfiguratorUrfStore UseRepository<TRepository, TContext>()
    //    where TContext : DbContext
    //    where TRepository : class, IRepository<IntegrationMessageLog>
    //{
    //    _context.Services.TryAddScoped<IOutboxStorage<IntegrationMessageLog>, OutboxStorageEntityFramework<IntegrationMessageLog>>();
    //    // we don't have a unit of work internally because we are not creating the context.
    //    // the user manages that externally
    //    _context.Services.TryAddScoped<IOutboxUnitOfWork, NoopOutboxUnitOfWork>();
    //    // IOutboxRepository is a wrapper over URF implementation which is used in IOutboxStorage
    //    // It allows us to quicklt change the underlying repository if required and not depend on URF
    //    _context.Services.AddScoped<IOutboxRepository<IntegrationMessageLog>, OutboxRepositoryEntityFramework<IntegrationMessageLog>>();
    //    _context.Services.TryAddScoped<IRepository<IntegrationMessageLog>>(
    //        sp => new Repository<IntegrationMessageLog>(sp.GetRequiredService<TContext>()));
    //    return this;
    //}

    /// <summary>
    /// Automatically adds <see cref="IRepository{IntegrationMessageLog}"/> as <see cref="Repository{IntegrationMessageLog}"/>
    /// <para>User will externally register the <typeparamref name="TContext"/>. The urf framework uses the DbContext in the Repository</para>
    /// <para>Intstructing URF to use the provided TContext instead of default DbContext</para>
    /// <para>IMPORTANT: The caller is responsable for using the <see cref="IUnitOfWork"/> Save method</para>
    /// </summary>
    public ConfiguratorUrfStore UseRepository<TMessageLog, TContext>()
        where TMessageLog : class, IIntegrationMessageLog
        where TContext : DbContext
    {
        _context.Services.TryAddScoped<IOutboxStorage<TMessageLog>, OutboxStorageEntityFramework<TMessageLog>>();
        // we don't have a unit of work internally because we are not creating the context.
        // the user manages that externally
        _context.Services.TryAddScoped<IOutboxUnitOfWork, NoopOutboxUnitOfWork>();
        // IOutboxRepository is a wrapper over URF implementation which is used in IOutboxStorage
        // It allows us to quicklt change the underlying repository if required and not depend on URF
        _context.Services.AddScoped<IOutboxRepository<TMessageLog>, OutboxRepositoryEntityFramework<TMessageLog>>();
        _context.Services.TryAddScoped<IRepository<TMessageLog>>(
            sp => new Repository<TMessageLog>(sp.GetRequiredService<TContext>()));
        return this;
    }
}
