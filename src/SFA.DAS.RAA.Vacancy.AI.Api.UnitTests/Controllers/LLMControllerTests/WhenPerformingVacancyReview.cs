using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using SFA.DAS.RAA.Vacancy.AI.Api.Controllers;
using SFA.DAS.RAA.Vacancy.AI.Api.Core;
using SFA.DAS.RAA.Vacancy.AI.Api.Data;
using SFA.DAS.RAA.Vacancy.AI.Api.Data.Entities;
using SFA.DAS.RAA.Vacancy.AI.Api.Domain;
using SFA.DAS.RAA.Vacancy.AI.Api.Models;
using SFA.DAS.RAA.Vacancy.AI.Api.Services;
using SFA.DAS.RAA.Vacancy.AI.Api.Testing;

namespace SFA.DAS.RAA.Vacancy.AI.Api.UnitTests.Controllers.LLMControllerTests;

public class WhenPerformingVacancyReview
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    
    [Test, MoqAutoData]
    public async Task Then_The_Ai_Vacancy_Review_Is_Saved(
        PostPerformReviewDto? data,
        AiVacancyReviewEntity entity,
        AiReviewResultV1 aiReviewResult,
        CancellationToken cancellationToken,
        [Frozen] Mock<IRecruitAiService> aiService,
        [Frozen] Mock<IAiReviewResultChecker> aiReviewResultChecker,
        [Frozen] Mock<IAiDataContext> dataContext,
        [Frozen] Mock<IEventsService> eventsService, 
        [Greedy] LlmController sut)
    {
        // arrange
        entity.Status = AiReviewStatus.Pending;
        
        aiService
            .Setup(x => x.ReviewVacancyAsync(data!, cancellationToken))
            .ReturnsAsync(aiReviewResult);

        var status = AiReviewStatus.Passed;
        aiReviewResultChecker
            .Setup(x => x.FlagForReview(aiReviewResult, out status))
            .Returns(false);

        dataContext
            .Setup(x => x.AiVacancyReviewEntities)
            .ReturnsDbSet([entity]);
        
        // act
        var result = await sut.PerformReview(
            aiService.Object,
            aiReviewResultChecker.Object,
            dataContext.Object,
            eventsService.Object,
            entity.VacancyReviewId,
            data,
            cancellationToken);

        // assert
        result.Should().BeOfType<Ok>();
        entity.Status.Should().Be(status);
        entity.ManualReviewRequired.Should().BeFalse();
        entity.UpdatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        entity.Output.Should().Be(JsonSerializer.Serialize(aiReviewResult, JsonOptions));
        dataContext.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
    }
    
    [Test, MoqAutoData]
    public async Task Then_The_Ai_Vacancy_Review_Completed_Event_Is_Published(
        PostPerformReviewDto? data,
        AiVacancyReviewEntity entity,
        AiReviewResultV1 aiReviewResult,
        CancellationToken cancellationToken,
        [Frozen] Mock<IRecruitAiService> aiService,
        [Frozen] Mock<IAiReviewResultChecker> aiReviewResultChecker,
        [Frozen] Mock<IAiDataContext> dataContext,
        [Frozen] Mock<IEventsService> eventsService, 
        [Greedy] LlmController sut)
    {
        // arrange
        aiService
            .Setup(x => x.ReviewVacancyAsync(data!, cancellationToken))
            .ReturnsAsync(aiReviewResult);

        dataContext
            .Setup(x => x.AiVacancyReviewEntities)
            .ReturnsDbSet([entity]);
        
        // act
        await sut.PerformReview(
            aiService.Object,
            aiReviewResultChecker.Object,
            dataContext.Object,
            eventsService.Object,
            entity.VacancyReviewId,
            data,
            cancellationToken);

        // assert
        eventsService.Verify(x => x.PublishAiVacancyReviewCompletedEventAsync(entity), Times.Once);
    }
    
    [Test, MoqAutoData]
    public async Task Then_If_The_Review_Does_Not_Exist_Not_Found_Is_Returned(
        PostPerformReviewDto? data,
        CancellationToken cancellationToken,
        [Frozen] Mock<IRecruitAiService> aiService,
        [Frozen] Mock<IAiReviewResultChecker> aiReviewResultChecker,
        [Frozen] Mock<IAiDataContext> dataContext,
        [Frozen] Mock<IEventsService> eventsService, 
        [Greedy] LlmController sut)
    {
        // arrange
        dataContext
            .Setup(x => x.AiVacancyReviewEntities)
            .ReturnsDbSet([]);
        
        // act
        var result = await sut.PerformReview(
            aiService.Object,
            aiReviewResultChecker.Object,
            dataContext.Object,
            eventsService.Object,
            Guid.NewGuid(),
            data,
            cancellationToken);

        // assert
        result.Should().BeOfType<NotFound>();
    }
}