using NServiceBus;
using SFA.DAS.RAA.Vacancy.AI.Api.Core.Events;
using SFA.DAS.RAA.Vacancy.AI.Api.Data.Entities;
using SFA.DAS.RAA.Vacancy.AI.Api.Services;

namespace SFA.DAS.RAA.Vacancy.AI.Api.UnitTests.Services;

public class WhenPublishingEvents
{
    [Test, MoqAutoData]
    public async Task Then_The_Ai_Vacancy_Review_Completed_Event_Is_Published(
        AiVacancyReviewEntity entity,
        [Frozen] Mock<IMessageSession> messageSession,
        [Greedy] EventsService sut)
    {
        // arrange
        AiVacancyReviewCompletedEvent? capturedEvent = null;
        messageSession
            .Setup(x => x.Publish(It.IsAny<object>(), It.IsAny<PublishOptions>()))
            .Callback<object, PublishOptions>((x, _) => capturedEvent = x as AiVacancyReviewCompletedEvent)
            .Returns(Task.CompletedTask);
            
        // act
        await sut.PublishAiVacancyReviewCompletedEventAsync(entity);

        // assert
        capturedEvent.Should().NotBeNull();
        capturedEvent.VacancyId.Should().Be(entity.VacancyId);
        capturedEvent.VacancyReviewId.Should().Be(entity.VacancyReviewId);
        capturedEvent.ManualReviewRequired.Should().Be(entity.ManualReviewRequired);
        capturedEvent.ReviewStatus.Should().Be(entity.Status);
    }
}