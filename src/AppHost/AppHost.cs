using Projects;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<WebApi>("api");

builder.Build().Run();