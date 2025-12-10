using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using SFA.DAS.Api.Common.Infrastructure;

namespace SFA.DAS.RAA.Vacancy.AI.Api.AppStart;

[ExcludeFromCodeCoverage]
public static class HealthCheckStartup
{
    public static void UseHealthChecks(this IApplicationBuilder app)
    {
        app.UseHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = HealthCheckResponseWriter.WriteJsonResponse
        });
            
        app.UseHealthChecks("/ping", new HealthCheckOptions
        {
            Predicate = (_) => false,
            ResponseWriter = (context, report) => 
            {
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync("");
            }
        });
    }
}