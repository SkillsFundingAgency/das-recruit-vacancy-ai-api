namespace SFA.DAS.RAA.Vacancy.AI.Api.Domain;

public class ReviewError(ReviewCategory category, string description, double score)
{
    public ReviewCategory Category { get; } = category;
    public string Description { get; } = description;
    public double Score { get; } = score;
}