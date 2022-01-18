using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using URF.Core.Abstractions;
using URF.Core.EF;

namespace ComX.Infrastructure.Distributed.Outbox.store.sql;

public class ConfiguratorUrfStore
{
    private readonly IServiceCollection _services;
    internal bool ConnectionInitializedOnce { get; private set; } = false;

    public ConfiguratorUrfStore(IServiceCollection services)
    {
        this._services = services;
    }

    /// <summary>
    /// Automatically adds <see cref="IRepository{IntegrationMessageLog}"/> as <see cref="Repository{IntegrationMessageLog}"/>
    ///<para>User will externally register the <see cref="DbContext"/>. The urf framework uses the DbContext in the Repository</para>
    /// </summary>
    public ConfiguratorUrfStore UseDefaultRepository()
    {
        if (ConnectionInitializedOnce)
        {
            throw new Exception($"You already registered a {typeof(IRepository<IntegrationMessageLog>).FullName}");
        }
        // we don't have a unit of work internally because we are not creating the context.
        // the user manages that externally
        _services.TryAddScoped<IOutboxUnitOfWork, NoopOutboxUnitOfWork>();
        // IOutboxRepository is a wrapper over URF implementation which is used in IOutboxStorage
        // It allows us to quicklt change the underlying repository if required and not depend on URF
        _services.AddScoped<IOutboxRepository, OutboxRepositoryEntityFramework>();
        _services.TryAddScoped<IRepository<IntegrationMessageLog>, Repository<IntegrationMessageLog>>();
        ConnectionInitializedOnce = true;
        return this;
    }

    /// <summary>
    /// Automatically adds <see cref="IRepository{IntegrationMessageLog}"/> as <see cref="Repository{IntegrationMessageLog}"/>
    ///<para>User will externally register the <see cref="DbContext"/>. The urf framework uses the DbContext in the Repository</para>
    /// </summary>
    public ConfiguratorUrfStore UseDefaultRepository<TContext>()
         where TContext : DbContext
    {
        if (ConnectionInitializedOnce)
        {
            throw new Exception($"You already registered a {typeof(IRepository<IntegrationMessageLog>).FullName}");
        }
        // we don't have a unit of work internally because we are not creating the context.
        // the user manages that externally
        _services.TryAddScoped<IOutboxUnitOfWork, NoopOutboxUnitOfWork>();
        // IOutboxRepository is a wrapper over URF implementation which is used in IOutboxStorage
        // It allows us to quicklt change the underlying repository if required and not depend on URF
        _services.AddScoped<IOutboxRepository, OutboxRepositoryEntityFramework>();
        _services.TryAddScoped<IRepository<IntegrationMessageLog>>(
            sp => new Repository<IntegrationMessageLog>(sp.GetRequiredService<TContext>()));
        ConnectionInitializedOnce = true;
        return this;
    }

    /// <summary>
    /// Automatically adds <see cref="IRepository{IntegrationMessageLog}"/> as <see cref="Repository{IntegrationMessageLog}"/>
    ///<para>User will externally register the <see cref="DbContext"/>. The urf framework uses the DbContext in the Repository</para>
    ///<para>Intstructing URF to use the provided TContext instead of default DbContext</para>
    /// </summary>
    public ConfiguratorUrfStore UseRepository<TRepository, TContext>()
        where TContext : DbContext
        where TRepository : class, IRepository<IntegrationMessageLog>
    {
        if (ConnectionInitializedOnce)
        {
            throw new Exception($"You already registered a {typeof(IRepository<IntegrationMessageLog>).FullName}");
        }
        // we don't have a unit of work internally because we are not creating the context.
        // the user manages that externally
        _services.TryAddScoped<IOutboxUnitOfWork, NoopOutboxUnitOfWork>();
        // IOutboxRepository is a wrapper over URF implementation which is used in IOutboxStorage
        // It allows us to quicklt change the underlying repository if required and not depend on URF
        _services.AddScoped<IOutboxRepository, OutboxRepositoryEntityFramework>();
        _services.TryAddScoped<IRepository<IntegrationMessageLog>>(
            sp => new Repository<IntegrationMessageLog>(sp.GetRequiredService<TContext>()));
        ConnectionInitializedOnce = true;
        return this;
    }

    /// <summary>
    /// User will externally register the <see cref="DbContext"/>. The urf framework uses the DbContext in the Repository
    /// </summary>
    public ConfiguratorUrfStore UseRepository<TRepository>()
        where TRepository : class, IRepository<IntegrationMessageLog>
    {
        if (ConnectionInitializedOnce)
        {
            throw new Exception($"You already registered a {typeof(IRepository<IntegrationMessageLog>).FullName}");
        }
        // we don't have a unit of work internally because we are not creating the context.
        // the user manages that externally
        _services.TryAddScoped<IOutboxUnitOfWork, NoopOutboxUnitOfWork>();
        // IOutboxRepository is a wrapper over URF implementation which is used in IOutboxStorage
        // It allows us to quicklt change the underlying repository if required and not depend on URF
        _services.AddScoped<IOutboxRepository, OutboxRepositoryEntityFramework>();
        _services.TryAddScoped<IRepository<IntegrationMessageLog>, TRepository>();
        ConnectionInitializedOnce = true;
        return this;
    }
}
