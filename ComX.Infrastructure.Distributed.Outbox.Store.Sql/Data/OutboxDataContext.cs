using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComX.Infrastructure.Distributed.Outbox
{
    public class OutboxDataContext : DbContext
    {
        public OutboxDataContext(DbContextOptions<OutboxDataContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            new IntegrationMessageLogMap().Configure(
                    modelBuilder.Entity<IntegrationMessageLog>()
                );

            base.OnModelCreating(modelBuilder);
        }
    }

}
