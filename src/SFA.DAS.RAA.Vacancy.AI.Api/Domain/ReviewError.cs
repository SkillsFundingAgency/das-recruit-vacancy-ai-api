using System.Text.Json.Serialization;

namespace SFA.DAS.RAA.Vacancy.AI.Api.Domain;

[JsonDerivedType(typeof(ReviewError), typeDiscriminator: "base")]
[JsonDerivedType(typeof(JsonFieldsMismatchReviewError), typeDiscriminator: "json-fields-mismatch")]
public record ReviewError(ReviewCategory Category, string Description, double Score);