using Microsoft.EntityFrameworkCore;
using URF.Core.Abstractions;

namespace ComX.Infrastructure.Distributed.Outbox.Tests
{
    public class CustomUnitOfWork : URF.Core.EF.UnitOfWork, IUnitOfWork
    {
        private readonly DbContext _context;

        public IRepository<IntegrationMessageLog> IntegrationMessageLogRepository { get; set; }

        /// <summary>
        /// Note: URF does not accept <see cref="CustomDataContext"/> instead of 
        /// <see cref="DbContext"/> here. 
        /// When using <see cref="CustomDataContext"/>, even if depency injection works and saving works,
        /// when querying it will return null
        /// </summary>
        public CustomUnitOfWork(
            CustomDataContext dbContext,
            IRepository<IntegrationMessageLog> integrationMessageLogRepository) : base(dbContext)
        {
            _context = dbContext;
            IntegrationMessageLogRepository = integrationMessageLogRepository;
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
