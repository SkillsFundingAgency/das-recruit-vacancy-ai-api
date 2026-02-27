using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SFA.DAS.RAA.Vacancy.AI.Api.Data;

namespace SFA.DAS.RAA.Vacancy.AI.Api.IntegrationTests;

public class TestServer : WebApplicationFactory<Program>
{
    public Mock<IAiDataContext> DataContext { get; } = new ();

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder
            .ConfigureHostConfiguration(configBuilder => configBuilder.AddJsonFile("appsettings.Test.json"))
            .ConfigureAppConfiguration(configBuilder => configBuilder.SetBasePath(Directory.GetCurrentDirectory()))
            .ConfigureServices(services =>
            {
                services.AddTransient<IAiDataContext>(x => DataContext.Object);
            });
        
        return base.CreateHost(builder);
    }
}