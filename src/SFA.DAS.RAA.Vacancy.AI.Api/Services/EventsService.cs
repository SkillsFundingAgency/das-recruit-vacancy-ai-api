using NServiceBus;
using SFA.DAS.RAA.Vacancy.AI.Api.Core.Events;
using SFA.DAS.RAA.Vacancy.AI.Api.Data.Entities;

namespace SFA.DAS.RAA.Vacancy.AI.Api.Services;

public interface IEventsService
{
    Task PublishAiVacancyReviewCompletedEventAsync(AiVacancyReviewEntity entity);
}

public class EventsService(IMessageSession messageSession) : IEventsService
{
    public async Task PublishAiVacancyReviewCompletedEventAsync(AiVacancyReviewEntity entity)
    {
        await messageSession.Publish(new AiVacancyReviewCompletedEvent(entity.VacancyId, entity.VacancyReviewId, entity.Status, entity.ManualReviewRequired));
    }
}