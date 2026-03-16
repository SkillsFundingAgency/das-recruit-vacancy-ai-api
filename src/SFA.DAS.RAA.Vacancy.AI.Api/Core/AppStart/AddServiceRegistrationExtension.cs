using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SFA.DAS.Api.Common.Configuration;
using SFA.DAS.RAA.Vacancy.AI.Api.Core.Clients;
using SFA.DAS.RAA.Vacancy.AI.Api.Core.Configuration;
using SFA.DAS.RAA.Vacancy.AI.Api.Data;
using SFA.DAS.RAA.Vacancy.AI.Api.LLM.Services;
using SFA.DAS.RAA.Vacancy.AI.Api.Services;

namespace SFA.DAS.RAA.Vacancy.AI.Api.Core.AppStart;

[ExcludeFromCodeCoverage]
public static class AddServiceRegistrationExtension
{
    public static void AddApplicationDependencies(this IServiceCollection services)
    {
        // validators
        services.AddScoped<ILLMExec, LLMExec>();
        services.AddScoped<IVacancyQA, VacancyQA>();
        services.AddScoped<IRandomNumberGenerator, RandomNumberGenerator>();
        services.AddScoped<IAiReviewResultChecker, AiReviewResultChecker>();
        services.AddScoped<IAzureAiClient, AzureAiClient>();
        services.AddScoped<IRecruitAiService, RecruitAiService>();
        services.AddScoped<IAzureAIClientSpellcheckVerifier, AzureAIClientSpellcheckVerifier>();
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
        services.Configure<VacancyAiConfiguration>(configuration.GetSection(nameof(VacancyAiConfiguration)));
        services.AddSingleton(cfg => cfg.GetService<IOptions<VacancyAiConfiguration>>()!.Value);
    }
    
    public static void AddDatabaseRegistration(
        this IServiceCollection services,
        ConnectionStrings config,
        string? environmentName)
    {
        services.AddHttpContextAccessor();

        if (string.Equals(environmentName, "DEV", StringComparison.CurrentCultureIgnoreCase))
        {
            services.AddDbContext<AiDataContext>(options =>
                options.UseInMemoryDatabase("SFA.DAS.RAA.Vacancy.AI.Api"), ServiceLifetime.Transient);
        }
        else
        {        
            services.AddDbContext<AiDataContext>(options =>
                options.UseSqlServer(config.SqlConnectionString), ServiceLifetime.Transient);
        }

        services.AddScoped<IAiDataContext, AiDataContext>(provider =>
            provider.GetRequiredService<AiDataContext>());
        services.AddScoped(provider =>
            new Lazy<AiDataContext>(provider.GetRequiredService<AiDataContext>));
    }
}