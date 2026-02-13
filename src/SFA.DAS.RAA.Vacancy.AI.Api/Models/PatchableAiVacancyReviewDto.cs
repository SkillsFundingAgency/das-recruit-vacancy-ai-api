using SFA.DAS.RAA.Vacancy.AI.Api.Domain;

namespace SFA.DAS.RAA.Vacancy.AI.Api.Models;

public class PatchableAiVacancyReviewDto
{
    public string? Output { get; set; }
    public ReviewStatus Status { get; set; }
}