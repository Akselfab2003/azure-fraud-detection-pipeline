using Aspire.Hosting.Azure;

var builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<AzureServiceBusResource> serviceBus = builder.AddAzureServiceBus("ServiceBus")
                                                              .RunAsEmulator();

IResourceBuilder<AzureServiceBusQueueResource> transactionsQueue = serviceBus.AddServiceBusQueue("transactions-queue");

builder.AddProject<Projects.Fraud_Api>("fraud-api").WithOtlpExporter();

builder.AddProject<Projects.Fraud_Scoring>("fraud-scoring")
       .WaitFor(serviceBus)
       .WithReference(serviceBus)
       .WithOtlpExporter();


builder.AddProject<Projects.Fraud_Simulator>("fraud-simulator")
       .WaitFor(serviceBus)
       .WithReference(serviceBus)
       .WithOtlpExporter();

builder.Build().Run();
