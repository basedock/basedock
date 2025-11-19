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

// Placeholder for TanStack Start frontend
// When ready, uncomment and configure the frontend app:
// var frontend = builder.AddNpmApp("frontend", "../../../frontend")
//     .WithReference(api)
//     .WithHttpEndpoint(env: "PORT")
//     .WithExternalHttpEndpoints()
//     .PublishAsDockerFile();

builder.Build().Run();