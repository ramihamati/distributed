using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Data.Common;
using URF.Core.Abstractions;
using URF.Core.EF;

namespace ComX.Infrastructure.Distributed.Outbox;

public class ConfiguratorSqlStore
{
    private readonly ConfiguratorContext _context;

    public ConfiguratorSqlStore(ConfiguratorContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Using an internal built in DbContext, repository and unit of work
    /// </summary>
    public ConfiguratorSqlStore UseBuiltInContext(
        ISqlSettings sqlSettings,
        Action<SqlServerDbContextOptionsBuilder> builder = null)
    {
        _context.Services.AddDbContext<OutboxDataContext>(options =>
        {
            options.UseSqlServer(
                sqlSettings.ConnectionString,
                sqlOptions => builder?.Invoke(sqlOptions));
        }, ServiceLifetime.Scoped);

        InternalUseBuiltinContext();
        return this;
    }

    public ConfiguratorSqlStore UseBuiltInContext(
       DbConnection dbConnection,
       Action<SqlServerDbContextOptionsBuilder> builder = null)
    {
        _context.Services.AddDbContext<OutboxDataContext>(options =>
        {
            options.UseSqlServer(
                dbConnection,
                sqlOptions => builder?.Invoke(sqlOptions));

        }, ServiceLifetime.Scoped);
        InternalUseBuiltinContext();
        return this;
    }

    private void InternalUseBuiltinContext()
    {
        _context.Services.TryAddScoped<IOutboxStorage<IntegrationMessageLog>, OutboxStorageEntityFramework<IntegrationMessageLog>>();
        _context.Services.AddScoped<IOutboxUnitOfWork, OutboxUowEntityFramework<OutboxDataContext>>();
        // IOutboxRepository is a wrapper over URF implementation which is used in IOutboxStorage
        // It allows us to quicklt change the underlying repository if required and not depend on URF
        _context.Services.AddScoped<IOutboxRepository<IntegrationMessageLog>, OutboxRepositoryEntityFramework<IntegrationMessageLog>>();
        _context.Services.AddScoped<IRepository<IntegrationMessageLog>, Repository<IntegrationMessageLog>>(
             sp => new Repository<IntegrationMessageLog>(
                        sp.GetRequiredService<OutboxDataContext>()));
    }
}
