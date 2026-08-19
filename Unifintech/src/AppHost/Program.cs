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

var creditBureau = builder
    .AddDockerfile("credit-bureau", "../../../credit_bureau")
    .WithBuildArg("USER_ID", Environment.GetEnvironmentVariable("UID") ?? "1000")
    .WithBuildArg("GROUP_ID", Environment.GetEnvironmentVariable("GID") ?? "1000")
    .WithHttpEndpoint(port: 8080, targetPort: 8080, name: "http")
    .WithEnvironment("APP_ENV", "local")
    .WithEnvironment("APP_DEBUG", "true")
    .WithBindMount("../../../credit_bureau", "/var/www/html");

var web = builder
    .AddProject<Projects.Web>(Services.WebApi)
    .WithReference(databaseServer)
    .WithReference(kafka)
    .WithReference(redis)
    .WithReference(prometheus.GetEndpoint("http"))
    .WithReference(creditBureau.GetEndpoint("http"))
    .WaitFor(databaseServer)
    .WaitFor(kafka)
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
