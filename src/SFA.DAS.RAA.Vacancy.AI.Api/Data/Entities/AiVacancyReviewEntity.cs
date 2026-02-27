using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.RAA.Vacancy.AI.Api.Domain;

namespace SFA.DAS.RAA.Vacancy.AI.Api.Data.Entities;

[Table("AiVacancyReview"), PrimaryKey("VacancyId")]
public class AiVacancyReviewEntity
{
    public Guid VacancyId { get; set; }
    public Guid VacancyReviewId { get; set; }
    [Column(TypeName = "nvarchar(12)")]
    public ReviewStatus Status { get; set; }
    public string? Output { get; set; }
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}