using Fraud.Data.Entities;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

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
            ApplySnakeCaseNames(modelBuilder);
            base.OnModelCreating(modelBuilder);
        }

        // snake_case tables and columns (Build spec: Data model). Runs after all configurations.
        private static void ApplySnakeCaseNames(ModelBuilder modelBuilder)
        {
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                var table = entity.GetTableName();
                if (table is null || entity.IsMappedToJson()) continue;

                entity.SetTableName(ToSnakeCase(table));
                var store = StoreObjectIdentifier.Table(entity.GetTableName()!, entity.GetSchema());

                foreach (var property in entity.GetProperties())
                {
                    // xmin (the Version row-version token) is a Postgres system column and can't be renamed
                    if (property.ClrType == typeof(uint) && property.IsConcurrencyToken) continue;

                    var column = property.GetColumnName(store);
                    if (column is not null)
                        property.SetColumnName(ToSnakeCase(column), store);
                }
            }
        }

        private static string ToSnakeCase(string name) =>
            Regex.Replace(name, "([a-z0-9])([A-Z])", "$1_$2").ToLowerInvariant();
    }
}
