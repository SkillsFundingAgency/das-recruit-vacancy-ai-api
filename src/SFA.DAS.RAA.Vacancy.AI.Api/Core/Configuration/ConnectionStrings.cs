using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.RAA.Vacancy.AI.Api.Core.Configuration;

[ExcludeFromCodeCoverage]
public class ConnectionStrings
{
    public required string SqlConnectionString { get; set; }
}