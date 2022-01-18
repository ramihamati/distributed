using Microsoft.EntityFrameworkCore;
using URF.Core.EF;

namespace ComX.Infrastructure.Distributed.Outbox.store.sql;

/// <summary>
/// Wrapper arround UrfRepository to use a custom DbContext instead of the actual DbContext
/// </summary>
public class UrfRepository<TEntity, TContext> : Repository<TEntity>
    where TEntity : class
    where TContext : DbContext
{
    public UrfRepository(TContext context) : base(context)
    {

    }
}
