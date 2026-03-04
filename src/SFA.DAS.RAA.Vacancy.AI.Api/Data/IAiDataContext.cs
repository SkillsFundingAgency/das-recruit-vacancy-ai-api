using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SFA.DAS.RAA.Vacancy.AI.Api.Data.Entities;

namespace SFA.DAS.RAA.Vacancy.AI.Api.Data;

public interface IAiDataContext
{
    DbSet<AiVacancyReviewEntity> AiVacancyReviewEntities { get; }
    DatabaseFacade Database { get; }
    Task Ping(CancellationToken cancellationToken);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    void SetValues<TEntity>(TEntity to, TEntity from) where TEntity : class;
}