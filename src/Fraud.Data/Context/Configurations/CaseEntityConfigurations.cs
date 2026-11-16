using Fraud.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fraud.Data.Context.Configurations
{
    public class CaseEntityConfigurations : IEntityTypeConfiguration<CaseEntity>
    {
        public void Configure(EntityTypeBuilder<CaseEntity> builder)
        {
            builder.ToTable("Cases");

            builder.HasKey(c => c.Id);

            // 1:1 with Transaction, enforced by a unique FK (one case per transaction)
            builder.HasOne(c => c.Transaction)
                   .WithOne()
                   .HasForeignKey<CaseEntity>(c => c.TransactionId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(c => c.TransactionId).IsUnique();

            // 1:N with CaseEvents; events die with their case
            builder.HasMany(c => c.Events)
                   .WithOne()
                   .HasForeignKey(e => e.CaseId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(c => c.RiskScore).IsRequired();

            builder.Property(c => c.Decision)
                   .HasConversion<string>()
                   .IsRequired();

            builder.Property(c => c.Status)
                   .HasConversion<string>()
                   .IsRequired();

            builder.Property(c => c.Label)
                   .HasConversion<string>();

            // List<CaseReason> stored as a single jsonb column (EF JSON mapping, no dynamic JSON opt-in needed)
            builder.OwnsMany(c => c.Reasons, reasons => reasons.ToJson("reasons"));

            builder.Property(c => c.RuleSetVersion).IsRequired();

            // Optimistic concurrency via Postgres xmin
            builder.Property(c => c.Version)
                   .IsRowVersion();

            builder.HasIndex(c => c.Status);
            builder.HasIndex(c => new { c.Status, c.CreatedAt });
        }
    }
}
