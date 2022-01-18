using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace ComX.Infrastructure.Distributed.Outbox.Tests
{
    public class SampleOutboxRepository : IOutboxRepository
    {
        public Task DeleteAsync(IntegrationMessageLog entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsAsync(Guid entityId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IntegrationMessageLog> FindAsync(Guid entityId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<List<IntegrationMessageLog>> FindAsync(FinderMessageLog finder, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task InsertAsync(IntegrationMessageLog entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(IntegrationMessageLog entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
