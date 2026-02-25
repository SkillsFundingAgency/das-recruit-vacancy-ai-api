namespace SFA.DAS.RAA.Vacancy.AI.Api.Services;

public abstract class AiReviewResult
{
    public int Version => 1; // DO NOT REMOVE
    public abstract double GetScore();
}