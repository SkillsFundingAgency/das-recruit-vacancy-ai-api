using SFA.DAS.RAA.Vacancy.AI.Api.Core;
using SFA.DAS.RAA.Vacancy.AI.Api.Data.Entities;
using SFA.DAS.RAA.Vacancy.AI.Api.Models;

namespace SFA.DAS.RAA.Vacancy.AI.Api.IntegrationTests.Controllers.AiVacancyReviewControllerTests;

public class WhenGettingAiVacancyReviewByVacancyId: BaseFixture
{
    [Test, MoqAutoData]
    public async Task Then_The_Ai_Vacancy_Reviews_Are_Returned(Guid vacancyId)
    {
        // arrange
        var items = Fixture.CreateMany<AiVacancyReviewEntity>(10).ToList();
        foreach (var item in items)
        {
            item.VacancyId = vacancyId;
        }
        
        Server.DataContext
            .Setup(x => x.AiVacancyReviewEntities)
            .ReturnsDbSet(items);

        // act
        var response = await Client.GetAsync($"{RouteNames.AiVacancyReview}/by/vacancy-id/{vacancyId}");
        var content = await response.Content.ReadAsAsync<DataResponse<List<AiVacancyReviewDto>>>();
    
        // assert
        response.EnsureSuccessStatusCode();
        content.Should().NotBeNull();
        content.Data.Should().BeEquivalentTo(items);
    }
    
    [Test, MoqAutoData]
    public async Task Then_An_Empty_Array_Is_Returned_If_No_Matching_Records_Found(Guid vacancyId)
    {
        // arrange
        var items = Fixture.CreateMany<AiVacancyReviewEntity>(10).ToList();
        
        Server.DataContext
            .Setup(x => x.AiVacancyReviewEntities)
            .ReturnsDbSet(items);

        // act
        var response = await Client.GetAsync($"{RouteNames.AiVacancyReview}/by/vacancy-id/{vacancyId}");
        var content = await response.Content.ReadAsAsync<DataResponse<List<AiVacancyReviewDto>>>();
    
        // assert
        response.EnsureSuccessStatusCode();
        content.Should().NotBeNull();
        content.Data.Should().BeEmpty();
    }
}