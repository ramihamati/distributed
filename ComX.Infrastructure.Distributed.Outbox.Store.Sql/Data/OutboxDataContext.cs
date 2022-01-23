using Microsoft.EntityFrameworkCore;

namespace ComX.Infrastructure.Distributed.Outbox;

public class OutboxDataContext : DbContext
{
    public OutboxDataContext(DbContextOptions<OutboxDataContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        new IntegrationMessageLogMap("IntegrationMessageLogs").Configure(
                modelBuilder.Entity<IntegrationMessageLog>()
            );

        base.OnModelCreating(modelBuilder);
    }
}
