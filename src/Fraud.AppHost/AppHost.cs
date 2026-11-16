using Aspire.Hosting.Azure;

var builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<AzureServiceBusResource> serviceBus = builder.AddAzureServiceBus("ServiceBus")
                                                              .RunAsEmulator();

IResourceBuilder<AzureServiceBusQueueResource> transactionsQueue = serviceBus.AddServiceBusQueue("transactions-queue");

var postgres = builder.AddPostgres("Postgres")
                      .WithImage("pgvector/pgvector:0.8.7-pg18-trixie")
                      .WithVolume("fraud-postgres-data-pg18", "/var/lib/postgresql") // pg18+ keeps data in a versioned subdir
                      .WithLifetime(ContainerLifetime.Persistent)
                      .WithPgWeb()
                      .WithOtlpExporter();

var FraudDB = postgres.AddDatabase("FraudDB");

builder.AddProject<Projects.Fraud_Api>("fraud-api")
       .WithOtlpExporter();

builder.AddProject<Projects.Fraud_Scoring>("fraud-scoring")
       .WaitFor(serviceBus)
       .WaitFor(postgres)
       .WaitFor(FraudDB)
       .WithReference(serviceBus)
       .WithReference(FraudDB)
       .WithOtlpExporter();


builder.AddProject<Projects.Fraud_Simulator>("fraud-simulator")
       .WaitFor(serviceBus)
       .WaitFor(FraudDB)
       .WithReference(serviceBus)
       .WithReference(FraudDB)
       .WithOtlpExporter();

builder.Build().Run();
