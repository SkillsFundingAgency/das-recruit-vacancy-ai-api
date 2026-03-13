using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.RAA.Vacancy.AI.Api.Core;
using SFA.DAS.RAA.Vacancy.AI.Api.Data;
using SFA.DAS.RAA.Vacancy.AI.Api.Data.Entities;
using SFA.DAS.RAA.Vacancy.AI.Api.LLM.Models;
using SFA.DAS.RAA.Vacancy.AI.Api.LLM.Services;
using SFA.DAS.RAA.Vacancy.AI.Api.Services;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices.Marshalling;
using System.Text.Json;

namespace SFA.DAS.RAA.Vacancy.AI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LlmController : ControllerBase
{
    private readonly static JsonSerializerOptions JsonOptions = new()
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
    [ProducesResponseType<AILLMResultObject>(StatusCodes.Status200OK)]
    public async Task<IResult> PerformReview(
        [FromServices] IRecruitAiService aiService,
        [FromServices] IAiReviewResultChecker aiReviewResultChecker,
        [FromServices] IAiDataContext dataContext,
        [FromRoute] Guid vacancyReviewId,
        [FromBody, Required] InputObject? data,
        CancellationToken cancellationToken)
    {
        var review = await aiService.ReviewVacancyAsync(data!, cancellationToken);
        var flagForReview = aiReviewResultChecker.FlagForReview(review, out var reviewStatus);
        var aiVacancyReview = await dataContext.AiVacancyReviewEntities.FirstOrDefaultAsync(x => x.VacancyReviewId == vacancyReviewId, cancellationToken);
        if (aiVacancyReview is null)
        {   // we should never hit this, but just in case
            aiVacancyReview = new AiVacancyReviewEntity()
            {
                VacancyId = Guid.Parse(data!.VacancyId!),
                VacancyReviewId = vacancyReviewId,
            };
            
            // save now to avoid update date before created date
            await dataContext.SaveChangesAsync(cancellationToken);
        }


        aiVacancyReview.Output = JsonSerializer.Serialize(review, JsonOptions);
        Console.WriteLine("Matt's Temp debug");
        Console.WriteLine(review.ToString());
        Console.WriteLine("*************************");
        Console.WriteLine(aiVacancyReview.Output.ToString());
        Console.WriteLine(aiVacancyReview.Output.GetType());
        aiVacancyReview.ManualReviewRequired = flagForReview;
        aiVacancyReview.Status = reviewStatus;
        aiVacancyReview.UpdatedDate = DateTime.Now;
        await dataContext.SaveChangesAsync(cancellationToken);

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