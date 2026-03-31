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
        VacancyAiReviewResponse vacancyAiReviewResult,
        CancellationToken cancellationToken,
        [Frozen] Mock<IRecruitAiService> aiService,
        [Frozen] Mock<IAiDataContext> dataContext,
        [Frozen] Mock<IEventsService> eventsService, 
        [Greedy] LlmController sut)
    {
        // arrange
        entity.Status = AiReviewStatus.Pending;
        vacancyAiReviewResult.ManualReviewRequired = false;
        vacancyAiReviewResult.Status = AiReviewStatus.Passed;
        
        aiService
            .Setup(x => x.ReviewVacancyAsync(data!, cancellationToken))
            .ReturnsAsync(vacancyAiReviewResult);
    
        dataContext
            .Setup(x => x.AiVacancyReviewEntities)
            .ReturnsDbSet([entity]);
        
        // act
        var result = await sut.PerformReview(
            aiService.Object,
            dataContext.Object,
            eventsService.Object,
            entity.VacancyReviewId,
            data,
            cancellationToken);
    
        // assert
        result.Should().BeOfType<Ok>();
        entity.Status.Should().Be(AiReviewStatus.Passed);
        entity.ManualReviewRequired.Should().BeFalse();
        entity.UpdatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        entity.Output.Should().Be(JsonSerializer.Serialize(vacancyAiReviewResult, JsonOptions));
        entity.Score.Should().Be(vacancyAiReviewResult.Errors!.Sum(x => x.Score));
        dataContext.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Test]
    [MoqInlineAutoData(AiReviewStatus.Pending, 1)]
    [MoqInlineAutoData(AiReviewStatus.Passed, 0)]
    [MoqInlineAutoData(AiReviewStatus.Failed, 0)]
    public async Task Then_The_AiVacancyReviewCompletedEvent_Is_Only_Published_If_Necessary(
        AiReviewStatus status,
        int times,
        PostPerformReviewDto? data,
        AiVacancyReviewEntity entity,
        VacancyAiReviewResponse vacancyAiReviewResult,
        CancellationToken cancellationToken,
        [Frozen] Mock<IRecruitAiService> aiService,
        [Frozen] Mock<IAiDataContext> dataContext,
        [Frozen] Mock<IEventsService> eventsService, 
        [Greedy] LlmController sut)
    {
        // arrange
        entity.Status = status;
        
        aiService
            .Setup(x => x.ReviewVacancyAsync(data!, cancellationToken))
            .ReturnsAsync(vacancyAiReviewResult);
    
        dataContext
            .Setup(x => x.AiVacancyReviewEntities)
            .ReturnsDbSet([entity]);
        
        // act
        await sut.PerformReview(
            aiService.Object,
            dataContext.Object,
            eventsService.Object,
            entity.VacancyReviewId,
            data,
            cancellationToken);
    
        // assert
        eventsService.Verify(x => x.PublishAiVacancyReviewCompletedEventAsync(entity), Times.Exactly(times));
    }
    
    [Test, MoqAutoData]
    public async Task Then_If_The_Review_Is_Skipped_The_Entity_Is_Updated_And_The_Event_Published_Correctly(
        PostPerformReviewDto? data,
        CancellationToken cancellationToken,
        [Frozen] Mock<IRecruitAiService> aiService,
        [Frozen] Mock<IAiReviewResultChecker> aiReviewResultChecker,
        [Frozen] Mock<IAiDataContext> dataContext,
        [Frozen] Mock<IEventsService> eventsService, 
        [Greedy] LlmController sut)
    {
        // arrange
        var entity = new AiVacancyReviewEntity()
        {
            VacancyId =  Guid.NewGuid(),
            VacancyReviewId = Guid.NewGuid(),
            Status = AiReviewStatus.Skipped,
            ManualReviewRequired = false,
            UpdatedDate = null,
            CreatedDate = DateTime.UtcNow.AddDays(-1),
            Output = null,
            Score = null,
        };
        
        dataContext
            .Setup(x => x.AiVacancyReviewEntities)
            .ReturnsDbSet([entity]);
        
        // act
        var result = await sut.PerformReview(
            aiService.Object,
            dataContext.Object,
            eventsService.Object,
            entity.VacancyReviewId,
            data,
            cancellationToken);
        
        // assert
        result.Should().BeOfType<Ok>();
        entity.Status.Should().Be(AiReviewStatus.Skipped);
        entity.ManualReviewRequired.Should().BeTrue();
        entity.UpdatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        entity.Output.Should().BeNull();
        entity.Score.Should().BeNull();
        dataContext.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
        eventsService.Verify(x => x.PublishAiVacancyReviewCompletedEventAsync(entity), Times.Once());
    }
    
    [Test, MoqAutoData]
    public async Task Then_If_The_Review_Does_Not_Exist_Not_Found_Is_Returned(
        PostPerformReviewDto? data,
        CancellationToken cancellationToken,
        [Frozen] Mock<IRecruitAiService> aiService,
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
            dataContext.Object,
            eventsService.Object,
            Guid.NewGuid(),
            data,
            cancellationToken);
    
        // assert
        result.Should().BeOfType<NotFound>();
    }
}