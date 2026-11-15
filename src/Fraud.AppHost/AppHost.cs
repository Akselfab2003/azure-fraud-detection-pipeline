
var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Fraud_Api>("fraud-api");

builder.AddProject<Projects.Fraud_Scoring>("fraud-scoring");

builder.AddProject<Projects.Fraud_Simulator>("fraud-simulator");

builder.Build().Run();
