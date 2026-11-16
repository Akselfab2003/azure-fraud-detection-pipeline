using Aspire.Hosting.Azure;

var builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<AzureServiceBusResource> serviceBus = builder.AddAzureServiceBus("ServiceBus")
                                                              .RunAsEmulator();

IResourceBuilder<AzureServiceBusQueueResource> transactionsQueue = serviceBus.AddServiceBusQueue("transactions-queue");

var postgres = builder.AddPostgres("Postgres")
                      .WithImage("pgvector/pgvector:latest")
                      .WithDataVolume("fraud-postgres-data")
                      .WithLifetime(ContainerLifetime.Persistent)
                      .WithOtlpExporter();

var FraudDB = postgres.AddDatabase("FraudDB");

builder.AddProject<Projects.Fraud_Api>("fraud-api")
       .WithOtlpExporter();

builder.AddProject<Projects.Fraud_Scoring>("fraud-scoring")
       .WaitFor(serviceBus)
       .WithReference(serviceBus)
       .WithOtlpExporter();


builder.AddProject<Projects.Fraud_Simulator>("fraud-simulator")
       .WaitFor(serviceBus)
       .WithReference(serviceBus)
       .WithOtlpExporter();

builder.Build().Run();
