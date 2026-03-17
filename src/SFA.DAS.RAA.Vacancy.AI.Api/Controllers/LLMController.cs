using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.RAA.Vacancy.AI.Api.Core;
using SFA.DAS.RAA.Vacancy.AI.Api.Data;
using SFA.DAS.RAA.Vacancy.AI.Api.Domain;
using SFA.DAS.RAA.Vacancy.AI.Api.LLM.Models;
using SFA.DAS.RAA.Vacancy.AI.Api.LLM.Services;
using SFA.DAS.RAA.Vacancy.AI.Api.Services;

namespace SFA.DAS.RAA.Vacancy.AI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LlmController : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    
    [HttpPost(Name = "RunLLM")]
    [ProducesResponseType<AICheckReturnResultObject>(StatusCodes.Status200OK)]
    public async Task<IResult> RunLLM(
        [FromServices] ILLMExec llm,
        [FromBody] InputObject inputvacancy)
    {
        var llmoutput= await llm.ExecLLM(inputvacancy);
        return TypedResults.Ok(llmoutput);
    }
    
    [HttpPost, Route("vacancyReview/{vacancyReviewId:guid}/review")]
    [ProducesResponseType<AICheckReturnResultObject>(StatusCodes.Status200OK)]
    public async Task<IResult> PerformReview(
        [FromServices] IRecruitAiService aiService,
        [FromServices] IAiReviewResultChecker aiReviewResultChecker,
        [FromServices] IAiDataContext dataContext,
        [FromServices] IEventsService eventsService,
        [FromRoute] Guid vacancyReviewId,
        [FromBody, Required] InputObject? data,
        CancellationToken cancellationToken)
    {
        var aiVacancyReview = await dataContext.AiVacancyReviewEntities.FirstOrDefaultAsync(x => x.VacancyReviewId == vacancyReviewId, cancellationToken);
        if (aiVacancyReview is null)
        {
            return TypedResults.NotFound();
        }

        if (aiVacancyReview.Status is AiReviewStatus.Pending)
        {
            // perform the review
            var review = await aiService.ReviewVacancyAsync(data!, cancellationToken);
            var flagForReview = aiReviewResultChecker.FlagForReview(review, out var reviewStatus);
            
            // update the entity
            aiVacancyReview.Output = JsonSerializer.Serialize(review, JsonOptions);
            aiVacancyReview.ManualReviewRequired = flagForReview;
            aiVacancyReview.Status = reviewStatus;
            aiVacancyReview.UpdatedDate = DateTime.Now;
            await dataContext.SaveChangesAsync(cancellationToken);
        }

        await eventsService.PublishAiVacancyReviewCompletedEventAsync(aiVacancyReview);
        return TypedResults.Ok();
    }
}