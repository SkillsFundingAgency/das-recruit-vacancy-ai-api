namespace SFA.DAS.RAA.Vacancy.AI.Api.Services;

public interface IAiReviewResult
{
    int Version { get; }
    double GetScore();
}