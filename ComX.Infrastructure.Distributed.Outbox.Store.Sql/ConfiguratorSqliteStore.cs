using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;
using URF.Core.Abstractions;
using URF.Core.EF;

namespace ComX.Infrastructure.Distributed.Outbox.store.sql;
public class ConfiguratorSqliteStore
{
    private readonly IServiceCollection _services;
    internal bool ConnectionInitializedOnce { get; private set; } = false;

    public ConfiguratorSqliteStore(IServiceCollection services)
    {
        this._services = services;
    }

    public ConfiguratorSqliteStore UseBuiltInContext(
        ISqlSettings sqlSettings,
        Action<SqliteDbContextOptionsBuilder> builder = null)
    {
        if (ConnectionInitializedOnce)
        {
            throw new Exception("The sql connection was already configured");
        }
        _services.AddDbContext<OutboxDataContext>(options =>
        {
            options.UseSqlite(
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

    public ConfiguratorSqliteStore UseBuiltInContext(
       DbConnection dbConnection,
       Action<SqliteDbContextOptionsBuilder> builder = null)
    {
        if (ConnectionInitializedOnce)
        {
            throw new Exception("The sql connection was already configured");
        }
        _services.AddDbContext<OutboxDataContext>(options =>
        {
            options.UseSqlite(
                dbConnection,
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
