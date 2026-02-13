using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.RAA.Vacancy.AI.Api.Core;
using SFA.DAS.RAA.Vacancy.AI.Api.Data.Entities;
using SFA.DAS.RAA.Vacancy.AI.Api.Models;
using SFA.DAS.RAA.Vacancy.AI.Api.Models.Mappers;

namespace SFA.DAS.RAA.Vacancy.AI.Api.IntegrationTests.Controllers.AiVacancyReviewControllerTests;

public class WhenPuttingAiVacancyReview: BaseFixture
{
    [Test]
    public async Task Then_Without_Required_Fields_Bad_Request_Is_Returned()
    {
        // arrange
        Server.DataContext
            .Setup(x => x.AiVacancyReviewEntities)
            .ReturnsDbSet([]);
        
        // act
        var response = await Client.PutAsJsonAsync($"{RouteNames.AiVacancyReview}/{Guid.NewGuid()}", new {});
        var errors = await response.Content.ReadAsAsync<ValidationProblemDetails>();

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errors.Should().NotBeNull();
        errors.Errors.Should().HaveCount(2);
        errors.Errors.Should().ContainKeys(
            nameof(PutAiVacancyReviewDto.VacancyId),
            nameof(PutAiVacancyReviewDto.Status)
        );
    }
    
    [Test, MoqAutoData]
    public async Task Then_The_Review_Is_Created(Guid id, PutAiVacancyReviewDto dto)
    {
        // arrange
        var expectedEntity = dto.ToEntity(id);
        var dbSet = Fixture.CreateMany<AiVacancyReviewEntity>(10).ToList().BuildDbSetMock();
        
        Server.DataContext
            .Setup(x => x.AiVacancyReviewEntities)
            .Returns(dbSet.Object);
    
        // act
        var response = await Client.PutAsJsonAsync($"{RouteNames.AiVacancyReview}/{id}", dto);
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        dbSet.Verify(x => x.AddAsync(ItIs.EquivalentTo(expectedEntity), It.IsAny<CancellationToken>()), Times.Once);
        Server.DataContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Test, MoqAutoData]
    public async Task Then_The_Review_Is_Updated(Guid id, PutAiVacancyReviewDto dto)
    {
        // arrange
        var expectedEntity = dto.ToEntity(id);
        var items = Fixture.CreateMany<AiVacancyReviewEntity>(10).ToList();
        var targetItem = items[4];
        targetItem.VacancyReviewId = id;
        
        Server.DataContext
            .Setup(x => x.AiVacancyReviewEntities)
            .ReturnsDbSet(items);
    
        // act
        var response = await Client.PutAsJsonAsync($"{RouteNames.AiVacancyReview}/{id}", dto);
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        Server.DataContext.Verify(x => x.SetValues(targetItem, ItIs.EquivalentTo(expectedEntity)), Times.Once());
        Server.DataContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}