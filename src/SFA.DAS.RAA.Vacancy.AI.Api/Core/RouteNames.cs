namespace SFA.DAS.RAA.Vacancy.AI.Api.Core;

internal struct RouteElements
{
    public const string AiVacancyReview = "ai-vacancy-review";
    public const string Api = "api";
}

internal struct RouteNames
{
    public const string AiVacancyReview = $"{RouteElements.Api}/{RouteElements.AiVacancyReview}";
}