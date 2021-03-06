using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComX.Infrastructure.Distributed.Outbox
{
    public class IntegrationMessageLogMap: IntegrationMessageLogMap<IntegrationMessageLog>
    {
        public IntegrationMessageLogMap(string tableName) : base(tableName)
        {
        }

    }

    public  class IntegrationMessageLogMap<TMessageLog> : IEntityTypeConfiguration<TMessageLog>
        where TMessageLog : class, IIntegrationMessageLog
    {
        private readonly string _tableName;

        public IntegrationMessageLogMap(string tableName)
        {
            this._tableName = tableName;
        }

        public void Configure(EntityTypeBuilder<TMessageLog> builder)
        {
            builder.ToTable(_tableName);

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
