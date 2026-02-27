using System.Net;
using SFA.DAS.RAA.Vacancy.AI.Api.Core;
using SFA.DAS.RAA.Vacancy.AI.Api.Data.Entities;
using SFA.DAS.RAA.Vacancy.AI.Api.Models;

namespace SFA.DAS.RAA.Vacancy.AI.Api.IntegrationTests.Controllers.AiVacancyReviewControllerTests;

public class WhenGettingAiVacancyReview: BaseFixture
{
    [Test]
    public async Task Then_The_Ai_Vacancy_Review_Is_Returned()
    {
        // arrange
        var items = Fixture.CreateMany<AiVacancyReviewEntity>(10).ToList();
        var expected = items[1];
        
        Server.DataContext
            .Setup(x => x.AiVacancyReviewEntities)
            .ReturnsDbSet(items);

        // act
        var response = await Client.GetAsync($"{RouteNames.AiVacancyReview}/{expected.VacancyReviewId}");
        var content = await response.Content.ReadAsAsync<DataResponse<AiVacancyReviewDto>>();
    
        // assert
        response.EnsureSuccessStatusCode();
        content.Should().NotBeNull();
        content.Data.Should().BeEquivalentTo(expected);
    }
    
    [Test, MoqAutoData]
    public async Task Then_The_Ai_Vacancy_Review_Is_Not_Found(Guid id)
    {
        // arrange
        var items = Fixture.CreateMany<AiVacancyReviewEntity>(10).ToList();
        
        Server.DataContext
            .Setup(x => x.AiVacancyReviewEntities)
            .ReturnsDbSet(items);

        // act
        var response = await Client.GetAsync($"{RouteNames.AiVacancyReview}/{id}");
    
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}