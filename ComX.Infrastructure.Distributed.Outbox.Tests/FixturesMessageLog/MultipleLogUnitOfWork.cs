using Microsoft.EntityFrameworkCore;
using URF.Core.Abstractions;

namespace ComX.Infrastructure.Distributed.Outbox.Tests
{
    public class MultipleLogUnitOfWork : URF.Core.EF.UnitOfWork, IUnitOfWork
    {
        private readonly MultipleLogDataContext _context;

        /// <summary>
        /// Note: URF does not accept <see cref="CustomDataContext"/> instead of 
        /// <see cref="DbContext"/> here. 
        /// When using <see cref="CustomDataContext"/>, even if depency injection works and saving works,
        /// when querying it will return null
        /// </summary>
        public MultipleLogUnitOfWork(MultipleLogDataContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }

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
    }
}
