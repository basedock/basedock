using Projects;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<PostgresServerResource> postgres = builder.AddPostgres("postgres");
IResourceBuilder<PostgresDatabaseResource> database = postgres.AddDatabase("BasedockDb");

builder.AddProject<WebApi>("api")
    .WithReference(database)
    .WaitFor(database);

builder.Build().Run();