using Fraud.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fraud.Data.Context.Configurations
{
    public class GroundTruthEntityConfigurations : IEntityTypeConfiguration<GroundTruthEntity>
    {
        public void Configure(EntityTypeBuilder<GroundTruthEntity> builder)
        {
            builder.ToTable("GroundTruth");

            builder.HasKey(g => g.TransactionId);

            // Deliberately no FK to Transactions: the Simulator writes ground truth
            // independently of (and possibly before) the Scoring worker storing the transaction.
            builder.Property(g => g.TransactionId).ValueGeneratedNever();

            builder.Property(g => g.IsFraud).IsRequired();
            builder.Property(g => g.Pattern).IsRequired();

            // Rows written without a value (e.g. raw inserts) get the database clock.
            builder.Property(g => g.CreatedAt)
                   .HasDefaultValueSql("now()")
                   .IsRequired();

            builder.HasIndex(g => g.Pattern);
            builder.HasIndex(g => g.CreatedAt);
        }
    }
}
