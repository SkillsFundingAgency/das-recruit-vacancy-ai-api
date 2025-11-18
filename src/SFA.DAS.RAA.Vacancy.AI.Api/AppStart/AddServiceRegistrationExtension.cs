using FluentValidation;
using Microsoft.Extensions.Options;
using SFA.DAS.Api.Common.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.RAA.Vacancy.AI.Api.AppStart;

[ExcludeFromCodeCoverage]
public static class AddServiceRegistrationExtension
{
    public static void AddApplicationDependencies(this IServiceCollection services)
    {
        // validators
        services.AddValidatorsFromAssembly(typeof(Program).Assembly, includeInternalTypes: true);        
    }

    public static void ConfigureHealthChecks(this IServiceCollection services)
    {
        // health checks
        services
            .AddHealthChecks()
            .AddCheck<DefaultHealthCheck>("default");            
    }

    public static void AddConfigurationOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions();
        services.Configure<AzureActiveDirectoryConfiguration>(configuration.GetSection("AzureAd"));
        services.AddSingleton(cfg => cfg.GetService<IOptions<AzureActiveDirectoryConfiguration>>()!.Value);
    }
}