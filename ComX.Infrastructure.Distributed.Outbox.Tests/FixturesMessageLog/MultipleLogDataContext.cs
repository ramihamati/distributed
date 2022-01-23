using Microsoft.EntityFrameworkCore;

namespace ComX.Infrastructure.Distributed.Outbox.Tests
{
    public class MultipleLogDataContext : DbContext
    {
        public MultipleLogDataContext(DbContextOptions<MultipleLogDataContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            new IntegrationMessageLogMap( "MessageLogs")
                .Configure(modelBuilder.Entity<IntegrationMessageLog>());
            
            new IntegrationMessageLogMap<CustomMessageLog>("MessageLogsEventOneAndTwo")
                .Configure(modelBuilder.Entity<CustomMessageLog>());

            base.OnModelCreating(modelBuilder);
        }
    }
}
