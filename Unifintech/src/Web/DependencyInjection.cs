using Azure.Identity;
using Microsoft.AspNetCore.Mvc;
using Unifintech.Application.Common.Interfaces;
using Unifintech.Domain.Events;
using Unifintech.Infrastructure.Data;
using Unifintech.Infrastructure.Extensions;
using Unifintech.Infrastructure.Pub;
using Unifintech.Infrastructure.Sub;
using Unifintech.Web.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddWebServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddScoped<IUser, CurrentUser>();

        builder.Services.AddHttpContextAccessor();

        builder.Services.AddExceptionHandler<ProblemDetailsExceptionHandler>();

        builder.Services.AddTransient<IEventPublisherService, KafkaEventPublisherService>();

        builder.AddKafkaConsumerWorkers();

        // Customise default API behaviour
        builder.Services.Configure<ApiBehaviorOptions>(options =>
            options.SuppressModelStateInvalidFilter = true
        );

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddOpenApi(options =>
        {
            options.AddOperationTransformer<ApiExceptionOperationTransformer>();
            options.AddOperationTransformer<IdentityApiOperationTransformer>();
            options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
        });

        builder.Services.AddCors();
    }

    private static void AddKafkaConsumerWorkers(this IHostApplicationBuilder builder)
    {
        builder.Services.AddKafkaConsumerWorker<LoanCreatedEvent>();
    }

    private static IServiceCollection AddKafkaConsumerWorker<TEvent>(
        this IServiceCollection serviceCollection
    )
        where TEvent : INotification
    {
        serviceCollection.AddHostedService(sp => new KafkaEventConsumerWorker<TEvent>(
            sp.CreateScope(),
            typeof(TEvent).Name.ToKebabCase()
        ));

        return serviceCollection;
    }

    public static void AddKeyVaultIfConfigured(this IHostApplicationBuilder builder)
    {
        var keyVaultUri = builder.Configuration["AZURE_KEY_VAULT_ENDPOINT"];
        if (!string.IsNullOrWhiteSpace(keyVaultUri))
        {
            builder.Configuration.AddAzureKeyVault(
                new Uri(keyVaultUri),
                new DefaultAzureCredential()
            );
        }
    }
}
