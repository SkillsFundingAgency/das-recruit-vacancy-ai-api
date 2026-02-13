using System.ComponentModel.DataAnnotations;
using SFA.DAS.RAA.Vacancy.AI.Api.Domain;

namespace SFA.DAS.RAA.Vacancy.AI.Api.Models;

public class PutAiVacancyReviewDto
{
    [Required]
    public Guid VacancyId { get; set; }
    public string? Output { get; set; }
    [Required]
    public ReviewStatus Status { get; set; }
}