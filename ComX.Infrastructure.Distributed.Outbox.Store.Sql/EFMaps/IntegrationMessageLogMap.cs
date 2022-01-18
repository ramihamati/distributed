using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComX.Infrastructure.Distributed.Outbox
{
    public  class IntegrationMessageLogMap : IEntityTypeConfiguration<IntegrationMessageLog>
    {
        public void Configure(EntityTypeBuilder<IntegrationMessageLog> builder)
        {
            builder.ToTable("IntegrationMessageLogs");

            //Primary Key
            builder.HasKey(t => t.Id);

            builder.Property(x => x.Id)
               .IsRequired();

            builder.Property(x => x.MessageBody)
               .IsRequired();
            
            builder.Property(x => x.Status)
                .IsRequired();

            builder.Property(x => x.MessageTypeName)
               .IsRequired();

            builder.Property(x => x.CreatedAt)
               .IsRequired()
               .HasColumnType("datetime2");

            builder.Property(x => x.LastAttemptDate)
               .HasColumnType("datetime2");

            builder.Property(x => x.LockUntil)
               .HasColumnType("datetime2");

            builder.Property(x => x.RetryCount)
              .IsRequired();

            builder.Property(x => x.LastError);

            builder.Property(x=> x.Timestamp)
                .IsConcurrencyToken();
        }
    }
}
