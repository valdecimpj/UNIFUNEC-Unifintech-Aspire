using Unifintech.Shared;

var builder = DistributedApplication.CreateBuilder(args);

var databaseServer = builder.AddPostgres(Services.DatabaseServer).AddDatabase(Services.Database);

var kafka = builder.AddKafka("kafka").WithKafkaUI();

var redis = builder.AddRedis("redis");

var prometheus = builder
    .AddContainer("prometheus", "prom/prometheus")
    .WithBindMount("../prometheus", "/etc/prometheus", isReadOnly: true)
    .WithHttpEndpoint(port: 9090, targetPort: 9090, name: "http")
    .WithArgs("--config.file=/etc/prometheus/prometheus.yml", "--web.enable-otlp-receiver");

var web = builder
    .AddProject<Projects.Web>(Services.WebApi)
    .WithReference(databaseServer)
    .WithReference(kafka)
    .WithReference(redis)
    .WithReference(prometheus.GetEndpoint("http"))
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
