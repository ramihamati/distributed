using Microsoft.EntityFrameworkCore;

namespace ComX.Infrastructure.Distributed.Outbox.Tests
{
    public class CustomDataContext : DbContext
    {
        public CustomDataContext(DbContextOptions<CustomDataContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            new IntegrationMessageLogMap("IntegrationMessageLogs")
                .Configure(modelBuilder.Entity<IntegrationMessageLog>());
            
            base.OnModelCreating(modelBuilder);
        }
    }
}
