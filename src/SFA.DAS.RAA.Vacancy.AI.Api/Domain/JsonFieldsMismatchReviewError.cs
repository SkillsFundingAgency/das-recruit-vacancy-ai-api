namespace SFA.DAS.RAA.Vacancy.AI.Api.Domain;

public record JsonFieldsMismatchReviewError(
    ReviewCategory Category,
    string Description,
    double Score,
    List<string> AdditionalFields,
    List<string> MissingFields) : ReviewError(Category, Description, Score);