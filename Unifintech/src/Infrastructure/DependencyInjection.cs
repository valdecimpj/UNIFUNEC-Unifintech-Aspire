using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Polly;
using Unifintech.Application.Common.Interfaces;
using Unifintech.Infrastructure.Cache;
using Unifintech.Infrastructure.Data;
using Unifintech.Infrastructure.Data.Interceptors;
using Unifintech.Infrastructure.Identity;
using Unifintech.Infrastructure.Integrations;
using Unifintech.Infrastructure.Sub;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString(Services.Database);
        Guard.Against.Null(
            connectionString,
            message: $"Connection string '{Services.Database}' not found."
        );

        builder.Services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        builder.Services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        builder.Services.AddDbContext<ApplicationDbContext>(
            (sp, options) =>
            {
                options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
                options.UseNpgsql(connectionString);
                options.ConfigureWarnings(warnings =>
                    warnings.Ignore(RelationalEventId.PendingModelChangesWarning)
                );
            }
        );

        builder.EnrichNpgsqlDbContext<ApplicationDbContext>();

        builder.Services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>()
        );

        builder.Services.AddScoped<ApplicationDbContextInitialiser>();

        builder
            .Services.AddAuthentication()
            .AddBearerToken(IdentityConstants.BearerScheme, ConfigureWebsocketAuthentication());

        builder.Services.AddAuthorizationBuilder();

        builder
            .Services.AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddApiEndpoints();

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddTransient<IIdentityService, IdentityService>();
        builder.AddRedis();
        builder.AddHttpIntegrations();
        builder.Services.AddSingleton<KafkaTopicInitializerService>();
    }

    private static void AddHttpIntegrations(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<ICustomerCreditService, CustomerCreditService>();

        builder
            .Services.AddHttpClient<ICustomerCreditService, CustomerCreditService>(client =>
            {
                client.BaseAddress = new Uri(
                    builder.Configuration["services:credit-bureau:http:0"]
                        ?? throw new Exception(
                            "Customer Credit Service base address not found in configuration."
                        )
                );
            })
            .AddStandardResilienceHandler(options =>
            {
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(1);
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(10);

                options.Retry.MaxRetryAttempts = 3;
                options.Retry.BackoffType = DelayBackoffType.Constant;
                options.Retry.Delay = TimeSpan.FromMilliseconds(10);

                options.CircuitBreaker.MinimumThroughput = 2; 
                options.CircuitBreaker.FailureRatio = 1.0;
                options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(10);
                options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30);

                options.CircuitBreaker.OnOpened = args =>
                {
                    Console.WriteLine($"[CIRCUIT BREAKER ALERT]: Circuit transitioned to OPEN for {args.BreakDuration.TotalSeconds} seconds.");
                    return ValueTask.CompletedTask;
                };

                options.CircuitBreaker.OnClosed = args =>
                {
                    Console.WriteLine("[CIRCUIT BREAKER ALERT]: Circuit is back to CLOSED.");
                    return ValueTask.CompletedTask;
                };

                options.CircuitBreaker.OnHalfOpened = args =>
                {
                    Console.WriteLine("[CIRCUIT BREAKER ALERT]: Circuit is HALF-OPEN. Testing next request.");
                    return ValueTask.CompletedTask;
                };
            });
    }

    private static void AddRedis(this IHostApplicationBuilder builder)
    {
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration =
                builder.Configuration.GetConnectionString("redis")
                ?? throw new Exception("REDIS_CONNECTION_STRING not found in configuration.");
        });

        builder.Services.AddScoped<ICacheService, RedisCacheService>();
    }

    private static Action<BearerTokenOptions> ConfigureWebsocketAuthentication() =>
        options =>
        {
            options.Events = new BearerTokenEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;

                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/ws"))
                    {
                        context.Token = accessToken!;
                    }

                    return Task.CompletedTask;
                },
            };
        };
}
