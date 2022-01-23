using Microsoft.EntityFrameworkCore;
using URF.Core.EF;

namespace ComX.Infrastructure.Distributed.Outbox;

public class OutboxUowEntityFramework<TContext> : UnitOfWork, IOutboxUnitOfWork
    where TContext : DbContext
{
    private readonly TContext _context;

    public OutboxUowEntityFramework(TContext dbContext) : base(dbContext)
    {
        _context = dbContext;
    }

    Task IOutboxUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
    {
        return base.SaveChangesAsync(cancellationToken);
    }
}
