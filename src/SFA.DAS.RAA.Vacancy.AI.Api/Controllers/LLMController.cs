using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.RAA.Vacancy.AI.Api.Data;
using SFA.DAS.RAA.Vacancy.AI.Api.Domain;
using SFA.DAS.RAA.Vacancy.AI.Api.Models;
using SFA.DAS.RAA.Vacancy.AI.Api.Services;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices.Marshalling;
using System.Text.Json;

namespace SFA.DAS.RAA.Vacancy.AI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LlmController : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    
    [HttpPost, Route("vacancyReview/{vacancyReviewId:guid}/review")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<AILLMResultObject>(StatusCodes.Status200OK)]
    public async Task<IResult> PerformReview(
        [FromServices] IRecruitAiService aiService,
        [FromServices] IAiDataContext dataContext,
        [FromServices] IEventsService eventsService,
        [FromRoute] Guid vacancyReviewId,
        [FromBody, Required] PostPerformReviewDto? data,
        CancellationToken cancellationToken)
    {
        var aiVacancyReview = await dataContext.AiVacancyReviewEntities.FirstOrDefaultAsync(x => x.VacancyReviewId == vacancyReviewId, cancellationToken);
        if (aiVacancyReview is null)
        {
            return TypedResults.NotFound();
        }

        if (aiVacancyReview.Status is not AiReviewStatus.Pending)
        {
            // ignore anything that isn't in the pending state
            return TypedResults.Ok();
        }
        
        // perform the review
        var review = await aiService.ReviewVacancyAsync(data!, cancellationToken);

        // update the entity
        aiVacancyReview.Output = JsonSerializer.Serialize(review, JsonOptions);
        aiVacancyReview.ManualReviewRequired = review.ManualReviewRequired;
        aiVacancyReview.Status = review.Status;
        Console.WriteLine("Matt's Temp debug");
        Console.WriteLine(review.ToString());
        Console.WriteLine("*************************");
        Console.WriteLine(aiVacancyReview.Output.ToString());
        Console.WriteLine(aiVacancyReview.Output.GetType());
        aiVacancyReview.ManualReviewRequired = flagForReview;
        aiVacancyReview.Status = reviewStatus;
        aiVacancyReview.UpdatedDate = DateTime.Now;
        aiVacancyReview.Score = review.Errors?.Sum(x => x.Score) ?? 0;
        await dataContext.SaveChangesAsync(cancellationToken);
            
        await eventsService.PublishAiVacancyReviewCompletedEventAsync(aiVacancyReview);
        return TypedResults.Ok();

        AILLMResultObject output = new AILLMResultObject();

        output.VacancyId = data.VacancyId.ToString();
        output.VacancyReviewId = vacancyReviewId.ToString();
        output.LLMObject = aiVacancyReview.Output;
        output.ManualReviewRequired = flagForReview.ToString();
        output.Status = reviewStatus.ToString();
        output.UpdatedDateTime = aiVacancyReview.UpdatedDate.ToString();








        return TypedResults.Ok(output);
    }
}