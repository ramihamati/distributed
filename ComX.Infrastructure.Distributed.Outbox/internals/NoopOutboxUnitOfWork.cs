using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ComX.Infrastructure.Distributed.Outbox
{
    /// <summary>
    /// NoOperation UnitOfWork required to override the built in unit of work. This class
    /// is used when we are working with external DbContexts instead of an internal one, and transactional
    /// commits are handled by the caller.
    /// </summary>
    public class NoopOutboxUnitOfWork : IOutboxUnitOfWork
    {
        public void BeginTransaction()
        {
        }

        public void CommitTransaction()
        {
        }

        public Task<int> ExecuteSqlCommandAsync(
            string sql,
            IEnumerable<object> parameters,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(0);
        }

        public void RollbackTransaction()
        {
        }

        public bool TransactionExists()
        {
            return false;
        }

        Task IOutboxUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
