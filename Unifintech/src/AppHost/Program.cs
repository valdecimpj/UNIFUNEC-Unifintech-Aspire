using Unifintech.Shared;

var builder = DistributedApplication.CreateBuilder(args);

var databaseServer = builder.AddPostgres(Services.DatabaseServer).AddDatabase(Services.Database);

var kafka = builder.AddKafka("kafka").WithKafkaUI();

var redis = builder.AddRedis("redis");

var web = builder
    .AddProject<Projects.Web>(Services.WebApi)
    .WithReference(databaseServer)
    .WithReference(kafka)
    .WithReference(redis)
    .WaitFor(databaseServer)
    .WithExternalHttpEndpoints()
    .WithAspNetCoreEnvironment()
    .WithUrlForEndpoint(
        "http",
        url =>
        {
            url.DisplayText = "Scalar API Reference";
            url.Url = "/scalar";
        }
    );

builder.Build().Run();
