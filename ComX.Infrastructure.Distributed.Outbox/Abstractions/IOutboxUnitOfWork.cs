using System.Threading.Tasks;

namespace ComX.Infrastructure.Distributed.Outbox
{
    public interface IOutboxUnitOfWork
    {
        public void BeginTransaction();
        public void CommitTransaction();
        public void RollbackTransaction();
        public bool TransactionExists();
        public Task SaveChangesAsync(System.Threading.CancellationToken cancellationToken = default);
    }
}
