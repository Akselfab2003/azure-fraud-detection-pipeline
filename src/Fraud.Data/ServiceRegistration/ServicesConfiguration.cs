using Fraud.Data.Context;
using Fraud.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Fraud.Data.ServiceRegistration
{
    public static class ServicesConfiguration
    {
        // Aspire integration: gives pooling, health check, OpenTelemetry tracing/metrics and retries.
        // "FraudDB" must match the database resource name in the AppHost.
        public static IHostApplicationBuilder ConfigureDatabaseServices(this IHostApplicationBuilder builder)
        {
            builder.AddNpgsqlDbContext<FraudDbContext>("FraudDB",
                configureDbContextOptions: options => options.UseNpgsql(npgsql => npgsql.UseVector()));

            builder.Services.AddScoped<ITransactionRepository, TransactionRepository>()
                            .AddScoped<ICaseRepository, CaseRepository>()
                            .AddScoped<IGroundTruthRepository, GroundTruthRepository>();

            return builder;
        }

        // Development only. EF takes a database lock while migrating, so
        // two services calling this at startup is safe.
        public static async Task MigrateDatabaseAsync(this IHost host, CancellationToken ct = default)
        {
            if (!host.Services.GetRequiredService<IHostEnvironment>().IsDevelopment())
                return;

            await using var scope = host.Services.CreateAsyncScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Fraud.Data.Migrations");
            var db = scope.ServiceProvider.GetRequiredService<FraudDbContext>();

            await db.Database.MigrateAsync(ct);
            logger.LogInformation("Database migration completed successfully.");
        }
    }
}
