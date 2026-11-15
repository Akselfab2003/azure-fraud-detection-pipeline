using Aspire.Hosting.Azure;

var builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<AzureServiceBusResource> serviceBus = builder.AddAzureServiceBus("ServiceBus")
                                                              .RunAsEmulator();

IResourceBuilder<AzureServiceBusQueueResource> transactionsQueue = serviceBus.AddServiceBusQueue("transactions-queue").WithProperties(q =>
{
    q.MaxDeliveryCount = 5;
    q.LockDuration = TimeSpan.FromSeconds(30);
});

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
