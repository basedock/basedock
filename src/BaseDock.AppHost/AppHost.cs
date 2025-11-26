var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume(isReadOnly: false)
    .WithPgWeb();

var postgresdb = postgres.AddDatabase("basedock");

var redis = builder.AddRedis("redis")
    .WithDataVolume();

var api = builder.AddProject<Projects.BaseDock_Api>("api")
    .WithReference(postgresdb)
    .WithReference(redis);

builder.AddViteApp("web", "../BaseDock.Web")
    .WithPnpm()
    .WithReference(api)
    .WithExternalHttpEndpoints();

builder.Build().Run();
