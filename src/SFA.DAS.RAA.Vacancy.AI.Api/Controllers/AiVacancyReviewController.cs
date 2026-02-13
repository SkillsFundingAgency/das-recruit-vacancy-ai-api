using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.RAA.Vacancy.AI.Api.Core;
using SFA.DAS.RAA.Vacancy.AI.Api.Data;
using SFA.DAS.RAA.Vacancy.AI.Api.Models;
using SFA.DAS.RAA.Vacancy.AI.Api.Models.Mappers;

namespace SFA.DAS.RAA.Vacancy.AI.Api.Controllers;

[ApiController]
[Route(RouteNames.AiVacancyReview)]
public class AiVacancyReviewController: ControllerBase
{
    [HttpGet, Route("{vacancyReviewId:guid}")]
    [ProducesResponseType(typeof(DataResponse<AiVacancyReviewDto?>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> GetOneAsync(
        [FromServices] IAiDataContext dataContext,
        [FromRoute] Guid vacancyReviewId,
        CancellationToken cancellationToken)
    {
        var entity = await dataContext.AiVacancyReviewEntities.AsNoTracking().FirstOrDefaultAsync(v => v.VacancyReviewId == vacancyReviewId, cancellationToken: cancellationToken);
        return entity is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(new DataResponse<AiVacancyReviewDto>(entity.ToGetResponse()));
    }
    
    [HttpPut, Route("{vacancyReviewId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IResult> PutOneAsync(
        [FromServices] IAiDataContext dataContext,
        [FromRoute] Guid vacancyReviewId,
        [FromBody] PutAiVacancyReviewDto dto,
        CancellationToken cancellationToken)
    {
        var dtoEntity = dto.ToEntity(vacancyReviewId);
        var entity = await dataContext.AiVacancyReviewEntities.FirstOrDefaultAsync(x => x.VacancyReviewId == vacancyReviewId, cancellationToken: cancellationToken);
        if (entity is not null)
        {
            dataContext.SetValues(entity, dtoEntity);
            entity.UpdatedDate = DateTime.UtcNow;
            await dataContext.SaveChangesAsync(cancellationToken);
            return TypedResults.Ok();
        }

        await dataContext.AiVacancyReviewEntities.AddAsync(dtoEntity, cancellationToken);
        await dataContext.SaveChangesAsync(cancellationToken);
        return TypedResults.Created($"{RouteNames.AiVacancyReview}/{dtoEntity.VacancyReviewId}");
    }
    
    [HttpPatch, Route("{vacancyReviewId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> PatchOneAsync(
        [FromServices] IAiDataContext dataContext,
        [FromRoute] Guid vacancyReviewId,
        [FromBody] JsonPatchDocument<PatchableAiVacancyReviewDto> patchRequest,
        CancellationToken cancellationToken)
    {
        var entity = await dataContext.AiVacancyReviewEntities.FirstOrDefaultAsync(x => x.VacancyReviewId == vacancyReviewId, cancellationToken);
        if (entity is null)
        {
            return TypedResults.NotFound();
        }

        var patchableDto = entity.ToPatchDto();
        try
        {
            patchRequest.ApplyTo(patchableDto);
        }
        catch (JsonPatchException ex)
        {
            return TypedResults.ValidationProblem(ex.ToProblemsDictionary());
        }
        
        dataContext.SetValues(entity, patchableDto.ToEntity());
        entity.UpdatedDate = DateTime.UtcNow;
        await dataContext.SaveChangesAsync(cancellationToken);
        return TypedResults.Ok();
    }
    
    [HttpGet, Route("by/vacancy-id/{vacancyId:guid}")]
    [ProducesResponseType(typeof(DataResponse<IEnumerable<AiVacancyReviewDto>>), StatusCodes.Status200OK)]
    public async Task<IResult> GetManyByVacancyId(
        [FromServices] IAiDataContext dataContext,
        [FromRoute] Guid vacancyId,
        CancellationToken cancellationToken)
    {
        var entities = await dataContext
            .AiVacancyReviewEntities
            .AsNoTracking()
            .Where(x => x.VacancyId == vacancyId)
            .OrderBy(x => x.CreatedDate)
            .ToListAsync(cancellationToken);
        
        var items = entities.Select(entity => entity.ToGetResponse());
        return TypedResults.Ok(new DataResponse<IEnumerable<AiVacancyReviewDto>>(items));
    }
}