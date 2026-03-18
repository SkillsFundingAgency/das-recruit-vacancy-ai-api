using System.Diagnostics.CodeAnalysis;
using SFA.DAS.RAA.Vacancy.AI.Api.Core.NServiceBus;

namespace SFA.DAS.RAA.Vacancy.AI.Api;

[ExcludeFromCodeCoverage]
public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .UseServiceProviderFactory(new NServiceBusServiceProviderFactory())
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
    }
}