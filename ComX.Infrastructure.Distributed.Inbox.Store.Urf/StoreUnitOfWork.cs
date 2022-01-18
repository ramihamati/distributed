using Microsoft.EntityFrameworkCore;
using URF.Core.EF;

namespace ComX.Infrastructure.Distributed.Inbox.Store.Urf
{
    public class StoreUnitOfWork : UnitOfWork, IStoreUnitOfWork
    {
        #region [ Fields ]
        private readonly DbContext _context;
        #endregion


        #region [ Constructor ]
        public StoreUnitOfWork(
            DbContext dbContext) : base(dbContext)
        {
            _context = dbContext;
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
        #endregion
    }
}