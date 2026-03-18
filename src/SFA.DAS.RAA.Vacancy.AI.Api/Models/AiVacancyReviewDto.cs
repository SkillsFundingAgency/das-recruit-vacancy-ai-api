using SFA.DAS.RAA.Vacancy.AI.Api.Domain;

namespace SFA.DAS.RAA.Vacancy.AI.Api.Models;

public class AiVacancyReviewDto
{
    public Guid VacancyReviewId { get; set; }
    public Guid VacancyId { get; set; }
    public string? Output { get; set; }
    public bool ManualReviewRequired { get; set; }
    public AiReviewStatus Status { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public double? Score { get; set; }
}