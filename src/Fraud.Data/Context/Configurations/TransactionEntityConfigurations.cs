using Fraud.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fraud.Data.Context.Configurations
{
    public class TransactionEntityConfigurations : IEntityTypeConfiguration<TransactionEntity>
    {
        public void Configure(EntityTypeBuilder<TransactionEntity> builder)
        {
            builder.ToTable("Transactions");

            builder.HasKey(t => t.Id);


            builder.Property(t => t.CustomerId)
                   .IsRequired();

            builder.Property(t => t.MerchantId)
                   .IsRequired();

            builder.Property(t => t.Amount)
                   .HasColumnType("numeric(18,2)")
                   .IsRequired();

            builder.Property(t => t.Currency)
                   .HasColumnType("char(3)")
                   .IsRequired();

            builder.Property(t => t.Country)
                   .HasColumnType("char(2)")
                   .IsRequired();

            builder.Property(t => t.MerchantCategory)
                   .IsRequired();

            builder.Property(t => t.OccurredAt)
                   .IsRequired();

            builder.Property(t => t.ScoredAt)
                   .IsRequired();

            builder.Property(t => t.RiskScore)
                   .IsRequired();

            builder.Property(t => t.Decision)
                   .HasConversion<string>()
                   .IsRequired();

            builder.Property(t => t.Channel)
                   .HasConversion<string>()
                   .IsRequired();

            builder.HasIndex(t => new { t.CustomerId, t.OccurredAt });

        }
    }
}
