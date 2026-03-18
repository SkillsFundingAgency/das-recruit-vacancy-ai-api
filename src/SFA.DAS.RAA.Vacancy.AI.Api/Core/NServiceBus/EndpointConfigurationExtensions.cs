using System.Diagnostics.CodeAnalysis;
using System.Net;
using Azure.Identity;
using NServiceBus;
using NServiceBus.Container;
using NServiceBus.ObjectBuilder.MSDependencyInjection;

namespace SFA.DAS.RAA.Vacancy.AI.Api.Core.NServiceBus;

[ExcludeFromCodeCoverage]
public static class EndpointConfigurationExtensions
{
    public static EndpointConfiguration UseLicense(this EndpointConfiguration endpointConfiguration, string? license)
    {
        if (!string.IsNullOrEmpty(license))
        {
            endpointConfiguration.License(WebUtility.HtmlDecode(license));
        }

        return endpointConfiguration;
    }
    
    public static EndpointConfiguration UseConnectionString(this EndpointConfiguration endpointConfiguration, string? connectionString)
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
    
    public static EndpointConfiguration UseErrorQueue(this EndpointConfiguration config, string errorQueue)
    {
        config.SendFailedMessagesTo(errorQueue);
        return config;
    }
    
    public static EndpointConfiguration UseInstallers(this EndpointConfiguration config)
    {
        config.EnableInstallers();
        return config;
    }

    private static Func<Type, bool> CommandsConvention => t => (t.Namespace?.EndsWith("Commands") ?? false) || t.GetInterface("ICommand") is not null;
    private static Func<Type, bool> EventsConvention => t => (t.Namespace?.EndsWith("Events") ?? false) || t.GetInterface("IEvent") is not null;
    
    public static EndpointConfiguration UseMessageConventions(this EndpointConfiguration config)
    {
        var conventionsBuilder = config.Conventions();
        conventionsBuilder.DefiningCommandsAs(CommandsConvention);
        conventionsBuilder.DefiningEventsAs(EventsConvention);
        return config;
    }
    
    public static EndpointConfiguration UseServicesBuilder(this EndpointConfiguration config, UpdateableServiceProvider serviceProvider)
    {
        config.UseContainer<ServicesBuilder>((Action<ContainerCustomizations>) (c => c.ServiceProviderFactory((Func<IServiceCollection, UpdateableServiceProvider>) (_ => serviceProvider))));
        return config;
    }
    
    public static EndpointConfiguration UseNewtonsoftJsonSerializer(this EndpointConfiguration config)
    {
        config.UseSerialization<NewtonsoftJsonSerializer>();
        return config;
    }
    
    private static EndpointConfiguration UseAzureServiceBusTransport(this EndpointConfiguration config, string connectionString, Action<RoutingSettings>? routing = null)
    {
        var transportExtensions = config.UseTransport<AzureServiceBusTransport>();
        transportExtensions.CustomTokenCredential(new DefaultAzureCredential());
        transportExtensions.ConnectionString(connectionString.Replace("Endpoint=sb://", string.Empty).TrimEnd('/'));
        transportExtensions.Transactions(TransportTransactionMode.ReceiveOnly);
        transportExtensions.SubscriptionRuleNamingConvention(RuleNameShortener.Shorten);
        routing?.Invoke(transportExtensions.Routing());
        return config;
    }
}