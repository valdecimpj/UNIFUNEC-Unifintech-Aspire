using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;
using Unifintech.Application.Common.Interfaces;
using Unifintech.Infrastructure.Cache;
using Unifintech.Infrastructure.Data;
using Unifintech.Infrastructure.Data.Interceptors;
using Unifintech.Infrastructure.Identity;
using Unifintech.Infrastructure.Integrations;

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
            .AddStandardResilienceHandler();
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
