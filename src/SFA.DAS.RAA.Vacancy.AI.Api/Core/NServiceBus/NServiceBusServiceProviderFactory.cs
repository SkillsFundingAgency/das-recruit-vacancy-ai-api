using NServiceBus.ObjectBuilder.MSDependencyInjection;

namespace SFA.DAS.RAA.Vacancy.AI.Api.Core.NServiceBus;

public class NServiceBusServiceProviderFactory : IServiceProviderFactory<UpdateableServiceProvider>
{
    public UpdateableServiceProvider CreateBuilder(IServiceCollection services)
    {
        return new UpdateableServiceProvider(services);
    }

    public IServiceProvider CreateServiceProvider(UpdateableServiceProvider containerBuilder)
    {
        return containerBuilder;
    }
}