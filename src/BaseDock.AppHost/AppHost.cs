var builder = DistributedApplication.CreateBuilder(args);

// Add infrastructure services
var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin()
    .AddDatabase("basedockdb");

var redis = builder.AddRedis("redis")
    .WithRedisCommander();

// Add the API project with service dependencies
var api = builder.AddProject<Projects.BaseDock_Api>("api")
    .WithReference(postgres)
    .WithReference(redis);

var frontend = builder.AddViteApp("frontend", "../frontend")
    .WithPnpm()
    .WithReference(api)
    .WithExternalHttpEndpoints();

builder.Build().Run();