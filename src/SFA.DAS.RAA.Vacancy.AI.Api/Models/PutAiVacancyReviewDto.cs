using System.ComponentModel.DataAnnotations;
using SFA.DAS.RAA.Vacancy.AI.Api.Domain;

namespace SFA.DAS.RAA.Vacancy.AI.Api.Models;

public class PutAiVacancyReviewDto
{
    public bool ManualReviewRequired { get; set; }
    public string? Output { get; set; }
    [Required]
    public AiReviewStatus? Status { get; set; }
    [Required]
    public Guid? VacancyId { get; set; }
}