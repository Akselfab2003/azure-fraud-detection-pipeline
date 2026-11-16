using Fraud.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fraud.Data.Context
{
    public class FraudDbContext : DbContext
    {
        public FraudDbContext(DbContextOptions<FraudDbContext> options) : base(options)
        {
        }

        public DbSet<TransactionEntity> Transactions => Set<TransactionEntity>();
        public DbSet<CaseEntity> Cases => Set<CaseEntity>();
        public DbSet<CaseEventEntity> CaseEvents => Set<CaseEventEntity>();
        public DbSet<GroundTruthEntity> GroundTruth => Set<GroundTruthEntity>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(con => con.UseVector());
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("vector");
            modelBuilder.ApplyConfiguration(new Configurations.TransactionEntityConfigurations());
            base.OnModelCreating(modelBuilder);
        }
    }
}
