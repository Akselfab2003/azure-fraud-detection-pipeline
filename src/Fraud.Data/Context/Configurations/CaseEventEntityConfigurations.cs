using Fraud.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fraud.Data.Context.Configurations
{
    public class CaseEventEntityConfigurations : IEntityTypeConfiguration<CaseEventEntity>
    {
        public void Configure(EntityTypeBuilder<CaseEventEntity> builder)
        {
            builder.ToTable("CaseEvents");

            builder.HasKey(e => e.Id);

            // The Case -> Events relationship is configured on CaseEntity

            builder.Property(e => e.Type)
                   .HasConversion<string>()
                   .IsRequired();

            builder.Property(e => e.Actor).IsRequired();

            builder.Property(e => e.DataJson).HasColumnType("jsonb");

            builder.Property(e => e.At).IsRequired();

            builder.HasIndex(e => new { e.CaseId, e.At });
        }
    }
}
