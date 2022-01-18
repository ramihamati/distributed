using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using URF.Core.Abstractions;
using URF.Core.EF;

namespace ComX.Infrastructure.Distributed.Outbox.store.sql;

public class ConfiguratorSqlStore
{
    private readonly IServiceCollection _services;
    internal bool ConnectionInitializedOnce { get; private set; } = false;

    public ConfiguratorSqlStore(IServiceCollection services)
    {
        this._services = services;
    }

    /// <summary>
    /// Using an internal built in DbContext, repository and unit of work
    /// </summary>
    public ConfiguratorSqlStore UseBuiltInContext(
        ISqlSettings sqlSettings,
        Action<SqlServerDbContextOptionsBuilder> builder = null)
    {
        if (ConnectionInitializedOnce)
        {
            throw new Exception("The store was already configured");
        }
        _services.AddDbContext<OutboxDataContext>(options =>
        {
            options.UseSqlServer(
                sqlSettings.ConnectionString,
                sqlOptions => builder?.Invoke(sqlOptions));
        }, ServiceLifetime.Scoped);

        _services.AddScoped<IOutboxUnitOfWork, OutboxUowEntityFramework<OutboxDataContext>>();
        // IOutboxRepository is a wrapper over URF implementation which is used in IOutboxStorage
        // It allows us to quicklt change the underlying repository if required and not depend on URF
        _services.AddScoped<IOutboxRepository, OutboxRepositoryEntityFramework>();

        _services.AddScoped<IRepository<IntegrationMessageLog>, Repository<IntegrationMessageLog>>(
                sp => new Repository<IntegrationMessageLog>(
                        sp.GetRequiredService<OutboxDataContext>()));

        ConnectionInitializedOnce = true;
        return this;
    }

    
}
