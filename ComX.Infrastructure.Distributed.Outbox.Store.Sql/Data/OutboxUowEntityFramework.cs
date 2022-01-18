using Microsoft.EntityFrameworkCore;
using URF.Core.Abstractions;

namespace ComX.Infrastructure.Distributed.Outbox;

public class OutboxUowEntityFramework<TContext> : URF.Core.EF.UnitOfWork, IOutboxUnitOfWork
    where TContext : DbContext
{
    #region [ Fields ]
    private readonly TContext _context;
    #endregion

    #region [ Properties ]
    public IRepository<IntegrationMessageLog> IntegrationMessageLogRepository { get; set; }
    #endregion

    #region [ Constructor ]
    public OutboxUowEntityFramework(
        TContext dbContext,
        IRepository<IntegrationMessageLog> integrationMessageLogRepository) : base(dbContext)
    {
        _context = dbContext;
        IntegrationMessageLogRepository = integrationMessageLogRepository;
    }

    #endregion

    #region [ Methods ]
    public void BeginTransaction()
    {
        _context.Database.BeginTransaction();
    }

    public void CommitTransaction()
    {
        _context.Database.CommitTransaction();
    }

    public void RollbackTransaction()
    {
        _context.Database.RollbackTransaction();
    }

    public bool TransactionExists()
    {
        return _context.Database.CurrentTransaction != null;
    }

    Task IOutboxUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
    }
    #endregion
}
