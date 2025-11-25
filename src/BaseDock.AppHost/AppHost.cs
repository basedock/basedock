var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithPgAdmin();

var redis = builder.AddRedis("redis")
    .WithDataVolume();

var api = builder.AddProject<Projects.BaseDock_Api>("api")
    .WithReference(postgres)
    .WithReference(redis);

builder.AddNpmApp("web", "../BaseDock.Web")
    .WithReference(api)
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints();

builder.Build().Run();
