using URF.Core.Abstractions;

namespace ComX.Infrastructure.Distributed.Inbox.Store.Urf
{
    public interface IStoreUnitOfWork : IUnitOfWork
    {
        public void BeginTransaction();
        public void CommitTransaction();
        public void RollbackTransaction();
        public bool TransactionExists();
    }
}