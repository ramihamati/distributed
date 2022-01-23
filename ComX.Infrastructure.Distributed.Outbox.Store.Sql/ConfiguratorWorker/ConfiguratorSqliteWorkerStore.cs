using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Data.Common;
using URF.Core.Abstractions;
using URF.Core.EF;

namespace ComX.Infrastructure.Distributed.Outbox;

public class ConfiguratorSqliteWorkerStore
{
    private readonly ConfiguratorWorkerContext _context;

    public ConfiguratorSqliteWorkerStore(ConfiguratorWorkerContext context)
    {
        _context = context;
    }

    public ConfiguratorSqliteWorkerStore UseBuiltInContext(
        ISqlSettings sqlSettings,
        Action<SqliteDbContextOptionsBuilder> builder = null)
    {
        _context.ContainerServices.AddDbContext<OutboxDataContext>(options =>
        {
            options.UseSqlite(
                sqlSettings.ConnectionString,
                sqlOptions => builder?.Invoke(sqlOptions));

        }, ServiceLifetime.Scoped);

        InternalUseBuiltinContext();
        return this;
    }

    public ConfiguratorSqliteWorkerStore UseBuiltInContext(
       DbConnection dbConnection,
       Action<SqliteDbContextOptionsBuilder> builder = null)
    {
        _context.ContainerServices.AddDbContext<OutboxDataContext>(options =>
        {
            options.UseSqlite(
                dbConnection,
                sqlOptions => builder?.Invoke(sqlOptions));

        }, ServiceLifetime.Scoped);
        InternalUseBuiltinContext();
        return this;
    }

    private void InternalUseBuiltinContext()
    {
        _context.ContainerServices.TryAddScoped<IOutboxStorage<IntegrationMessageLog>, OutboxStorageEntityFramework<IntegrationMessageLog>>();
        _context.ContainerServices.AddScoped<IOutboxUnitOfWork, OutboxUowEntityFramework<OutboxDataContext>>();
        // IOutboxRepository is a wrapper over URF implementation which is used in IOutboxStorage
        // It allows us to quicklt change the underlying repository if required and not depend on URF
        _context.ContainerServices.AddScoped<IOutboxRepository<IntegrationMessageLog>, OutboxRepositoryEntityFramework<IntegrationMessageLog>>();
        _context.ContainerServices.AddScoped<IRepository<IntegrationMessageLog>, Repository<IntegrationMessageLog>>(
             sp => new Repository<IntegrationMessageLog>(
                        sp.GetRequiredService<OutboxDataContext>()));
    }
}
