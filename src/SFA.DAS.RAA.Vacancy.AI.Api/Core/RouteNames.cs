namespace SFA.DAS.RAA.Vacancy.AI.Api.Core;

internal struct RouteElements
{
    public const string AiVacancyReview = "ai-vacancy-reviews";
    public const string Api = "api";
}

internal struct RouteNames
{
    public const string AiVacancyReview = $"{RouteElements.Api}/{RouteElements.AiVacancyReview}";
}