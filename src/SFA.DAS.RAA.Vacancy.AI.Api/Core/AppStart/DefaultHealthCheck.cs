using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SFA.DAS.RAA.Vacancy.AI.Api.Data;

namespace SFA.DAS.RAA.Vacancy.AI.Api.Core.AppStart;

[ExcludeFromCodeCoverage]
public class DefaultHealthCheck(IAiDataContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (dbContext.Database.ProviderName?.EndsWith("InMemory", StringComparison.OrdinalIgnoreCase) ?? false)
        {
            return HealthCheckResult.Healthy();
        }
        
        try
        {
            await dbContext.Ping(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch
        {
            return HealthCheckResult.Unhealthy();
        }
    }
}