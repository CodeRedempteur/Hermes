var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Hermes_CoreServiceAPI>("hermes-corserviceapi");
builder.AddProject<Projects.Hermes_Website>("hermes-userwebsite");

builder.Build().Run();
