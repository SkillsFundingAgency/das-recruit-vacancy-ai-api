using FluentValidation;
using Microsoft.Extensions.Options;
using SFA.DAS.Api.Common.Configuration;
using System.Diagnostics.CodeAnalysis;
using SFA.DAS.RAA.Vacancy.AI.Api.Configuration;
using SFA.DAS.RAA.Vacancy.AI.Api.LLM.Services;

namespace SFA.DAS.RAA.Vacancy.AI.Api.AppStart;

[ExcludeFromCodeCoverage]
public static class AddServiceRegistrationExtension
{
    public static void AddApplicationDependencies(this IServiceCollection services)
    {
        // validators
        services.AddValidatorsFromAssembly(typeof(Program).Assembly, includeInternalTypes: true);
        services.AddScoped<ILLMExec, LLMExec>();
        services.AddScoped<IVacancyQA, VacancyQA>();
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
        services.Configure<VacancyAiConfiguration>(config =>
        {
            var mainConfig = configuration.GetSection(nameof(VacancyAiConfiguration)).Get<VacancyAiConfiguration>();

            config.LlmKey = mainConfig!.LlmKey;
            config.LlmEndpointShort = mainConfig.LlmEndpointShort;
            config.DiscriminationPrompt = configuration.GetSection("DiscriminationPrompt").Get<Prompt>()!;
            config.MissingContentPrompt = configuration.GetSection("MissingContentPrompt").Get<Prompt>()!;
            config.SpellingCheckPrompt = configuration.GetSection("SpellingCheckPrompt").Get<Prompt>()!;
        });
        services.AddSingleton(cfg => cfg.GetService<IOptions<VacancyAiConfiguration>>()!.Value);
    }
}