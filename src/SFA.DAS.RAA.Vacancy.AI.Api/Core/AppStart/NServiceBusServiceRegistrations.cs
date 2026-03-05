using System.Diagnostics.CodeAnalysis;
using System.Net;
using NServiceBus;
using NServiceBus.ObjectBuilder.MSDependencyInjection;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.AzureServiceBus;
using SFA.DAS.NServiceBus.Configuration.MicrosoftDependencyInjection;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;
using SFA.DAS.NServiceBus.Hosting;
using Endpoint = NServiceBus.Endpoint;

namespace SFA.DAS.RAA.Vacancy.AI.Api.Core.AppStart;

[ExcludeFromCodeCoverage]
public static class NServiceBusServiceRegistrations
{
    private const string EndpointName = "SFA.DAS.Recruit.Vacancies";
    
    public static void StartNServiceBus(this UpdateableServiceProvider services, IConfiguration configuration)
    {
        var endpointConfiguration = new EndpointConfiguration(EndpointName)
            .UseErrorQueue($"{EndpointName}-errors")
            .UseInstallers()
            .UseMessageConventions()
            .UseServicesBuilder(services)
            .UseNewtonsoftJsonSerializer()
            .UseLicense(configuration["NServiceBusLicense"])
            .UseConnectionString(configuration["ServiceBusConnectionString"]);
        
        var endpoint = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();
        services
            .AddSingleton(endpoint)
            .AddSingleton<IMessageSession>(p => p.GetService<IEndpointInstance>()!)
            .AddHostedService<NServiceBusHostedService>();
    }

    private static EndpointConfiguration UseLicense(this EndpointConfiguration endpointConfiguration, string? license)
    {
        if (!string.IsNullOrEmpty(license))
        {
            endpointConfiguration.License(WebUtility.HtmlDecode(license));
        }

        return endpointConfiguration;
    }
    
    private static EndpointConfiguration UseConnectionString(this EndpointConfiguration endpointConfiguration, string? connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            endpointConfiguration.UseTransport<LearningTransport>();
        }
        else
        {
            endpointConfiguration.UseAzureServiceBusTransport(connectionString);
        }

        return endpointConfiguration;
    }
}