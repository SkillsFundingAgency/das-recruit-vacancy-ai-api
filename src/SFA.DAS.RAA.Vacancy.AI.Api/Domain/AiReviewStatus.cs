using System.Text.Json.Serialization;

namespace SFA.DAS.RAA.Vacancy.AI.Api.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AiReviewStatus
{
    Pending,
    Passed,
    Failed,
}