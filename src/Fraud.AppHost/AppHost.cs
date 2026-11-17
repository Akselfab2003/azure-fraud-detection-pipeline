using Aspire.Hosting.Azure;

var builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<AzureServiceBusResource> serviceBus = builder.AddAzureServiceBus("ServiceBus")
                                                              .RunAsEmulator();

IResourceBuilder<AzureServiceBusQueueResource> transactionsQueue = serviceBus.AddServiceBusQueue("transactions-queue");

IResourceBuilder<PostgresServerResource> postgres = builder.AddPostgres("Postgres")
                                                           .WithImage("pgvector/pgvector:0.8.7-pg18-trixie")
                                                           .WithVolume("fraud-postgres-data-pg18", "/var/lib/postgresql") // pg18+ keeps data in a versioned subdir
                                                           .WithLifetime(ContainerLifetime.Persistent)
                                                           .WithPgWeb()
                                                           .WithOtlpExporter();
IResourceBuilder<RedisResource> redis = builder.AddRedis("Redis")
                                               .WithRepl()
                                               .WithRedisInsight()
                                               .WithOtlpExporter();

IResourceBuilder<PostgresDatabaseResource> FraudDB = postgres.AddDatabase("FraudDB");

builder.AddProject<Projects.Fraud_Api>("fraud-api")
       .WithOtlpExporter();

builder.AddProject<Projects.Fraud_Scoring>("fraud-scoring")
       .WaitFor(serviceBus)
       .WaitFor(postgres)
       .WaitFor(FraudDB)
       .WaitFor(redis)
       .WithReference(serviceBus)
       .WithReference(FraudDB)
       .WithReference(redis)
       .WithOtlpExporter();


builder.AddProject<Projects.Fraud_Simulator>("fraud-simulator")
       .WaitFor(serviceBus)
       .WaitFor(FraudDB)
       .WithReference(serviceBus)
       .WithReference(FraudDB)
       .WithOtlpExporter();

builder.Build().Run();
