using Scalar.AspNetCore;
using Unifintech.Infrastructure.Data;
using Unifintech.Infrastructure.Sub;
using Unifintech.Web.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddServiceDefaults();

builder.AddKeyVaultIfConfigured();
builder.AddApplicationServices();
builder.AddInfrastructureServices();
builder.AddWebServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    await app.InitialiseDatabaseAsync();
    var kafkaTopicInitializer = app.Services.GetRequiredService<KafkaTopicInitializerService>();

    var topics = new List<string>
    {
        "employee-fired-event",
        "loan-created-event",
    };

    foreach (var topic in topics)
    {
        await kafkaTopicInitializer.EnsureTopicExistsAsync(topic);
    }
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors(static builder => builder.AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin());

app.UseFileServer();

app.MapOpenApi();
app.MapScalarApiReference();

app.UseExceptionHandler(options => { });

app.Map("/", () => Results.Redirect("/scalar"));
app.MapHub<EventHub>("/ws").RequireAuthorization();

app.MapDefaultEndpoints();
app.MapEndpoints(typeof(Program).Assembly);

app.Run();
