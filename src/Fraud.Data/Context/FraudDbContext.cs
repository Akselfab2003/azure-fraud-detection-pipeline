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


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("vector");
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(FraudDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
