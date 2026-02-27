using SFA.DAS.RAA.Vacancy.AI.Api.Data.Entities;

namespace SFA.DAS.RAA.Vacancy.AI.Api.Models.Mappers;

internal static class AiVacancyReviewExtensions
{
    public static AiVacancyReviewDto ToGetResponse(this AiVacancyReviewEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return new AiVacancyReviewDto
        {
            VacancyReviewId = entity.VacancyReviewId,
            VacancyId = entity.VacancyId,
            Status = entity.Status,
            Output = entity.Output,
            CreatedDate = entity.CreatedDate,
            UpdatedDate = entity.UpdatedDate,
        };
    }
    
    public static PatchableAiVacancyReviewDto ToPatchDto(this AiVacancyReviewEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return new PatchableAiVacancyReviewDto
        {
            Status = entity.Status,
            Output = entity.Output,
        };
    }
    
    public static AiVacancyReviewEntity ToEntity(this PatchableAiVacancyReviewDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return new AiVacancyReviewEntity
        {
            Status = dto.Status,
            Output = dto.Output,
        };
    }

    public static AiVacancyReviewEntity ToEntity(this PutAiVacancyReviewDto dto, Guid vacancyReviewId)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return new AiVacancyReviewEntity
        {
            VacancyReviewId = vacancyReviewId,
            VacancyId = dto.VacancyId!.Value,
            Status = dto.Status!.Value,
            Output = dto.Output,
        };
    }
}