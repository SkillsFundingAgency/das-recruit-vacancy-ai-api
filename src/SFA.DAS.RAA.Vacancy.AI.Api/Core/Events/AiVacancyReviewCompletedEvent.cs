using SFA.DAS.RAA.Vacancy.AI.Api.Domain;

namespace SFA.DAS.RAA.Vacancy.AI.Api.Core.Events;

public sealed record AiVacancyReviewCompletedEvent(Guid VacancyId, Guid VacancyReviewId, AiReviewStatus ReviewStatus, bool ManualReviewRequired);