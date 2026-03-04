using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SFA.DAS.RAA.Vacancy.AI.Api.Core.Configuration;
using SFA.DAS.RAA.Vacancy.AI.Api.Data.Entities;

namespace SFA.DAS.RAA.Vacancy.AI.Api.Data;

[ExcludeFromCodeCoverage]
internal class AiDataContext: DbContext, IAiDataContext
{
    private readonly ConnectionStrings? _configuration;
    
    public DbSet<AiVacancyReviewEntity> AiVacancyReviewEntities { get; set; }

    public AiDataContext()
    { }

    public AiDataContext(DbContextOptions<AiDataContext> options) : base(options)
    { }
    
    public AiDataContext(IOptions<ConnectionStrings> config, DbContextOptions<AiDataContext> options) : base(options)
    {
        _configuration = config.Value;
    }
    
    public async Task Ping(CancellationToken cancellationToken)
    {
        await Database
            .ExecuteSqlRawAsync("SELECT 1;", cancellationToken)
            .ConfigureAwait(false);
    }

    public void SetValues<TEntity>(TEntity to, TEntity from) where TEntity : class
    {
        Entry(to).CurrentValues.SetValues(from);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connection = new SqlConnection { ConnectionString = _configuration!.SqlConnectionString };
        optionsBuilder.UseSqlServer(connection, options => options.EnableRetryOnFailure(5, TimeSpan.FromSeconds(20), null));
        optionsBuilder.UseLazyLoadingProxies();
        
        // Note: useful to keep here
        // optionsBuilder.LogTo(message => Debug.WriteLine(message));
        // optionsBuilder.EnableDetailedErrors();
    }
}