using System.Globalization;
using System.Net;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.RAA.Vacancy.AI.Api.Core;
using SFA.DAS.RAA.Vacancy.AI.Api.Data.Entities;
using SFA.DAS.RAA.Vacancy.AI.Api.Domain;
using SFA.DAS.RAA.Vacancy.AI.Api.Models;

namespace SFA.DAS.RAA.Vacancy.AI.Api.IntegrationTests.Controllers.AiVacancyReviewControllerTests;

public class WhenPatchingAiVacanyReview: BaseFixture
{
    [Test, MoqAutoData]
    public async Task Then_The_Ai_Vacancy_Review_Is_Not_Found(Guid id)
    {
        // arrange
        var items = Fixture.CreateMany<AiVacancyReviewEntity>(10).ToList();
        
        Server.DataContext
            .Setup(x => x.AiVacancyReviewEntities)
            .ReturnsDbSet(items);

        // act
        var patchDocument = new JsonPatchDocument<PatchableAiVacancyReviewDto>();
        patchDocument.Operations.Add(new Operation<PatchableAiVacancyReviewDto>("replace", "Status", "Pending", "Passed"));
        var response = await Client.PatchAsync($"{RouteNames.AiVacancyReview}/{id}", patchDocument);
    
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    public static object[] UnpatchablePropertyCases =
    [
        new object[] { "/VacancyReviewId", Guid.NewGuid().ToString() },
        new object[] { "/VacancyId", Guid.NewGuid().ToString() },
        new object[] { "/CreatedDate", DateTime.UtcNow.ToString(CultureInfo.InvariantCulture) },
        new object[] { "/UpdatedDate", DateTime.UtcNow.ToString(CultureInfo.InvariantCulture) }
    ];

    [TestCaseSource(nameof(UnpatchablePropertyCases))]
    public async Task Then_You_Cannot_Patch_Specified_Fields(string path, string value)
    {
        // arrange
        var items = Fixture.CreateMany<AiVacancyReviewEntity>(10).ToList();
        Server.DataContext
            .Setup(x => x.AiVacancyReviewEntities)
            .ReturnsDbSet(items);

        var patchDocument = new JsonPatchDocument();
        patchDocument.Add(path, value);
        
        // act
        var response = await Client.PatchAsync($"{RouteNames.AiVacancyReview}/{items[0].VacancyReviewId}", patchDocument);
        var errors = await response.Content.ReadAsAsync<ValidationProblemDetails>();

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errors.Should().NotBeNull();
        errors.Errors.Should().HaveCount(1);
        errors.Errors.Should().ContainKey(path);
    }
    
    [Test]
    public async Task Then_The_AiVacancyReview_Is_Patched()
    {
        var items = Fixture.CreateMany<AiVacancyReviewEntity>(10).ToList();
        
        var targetItem = items[4];
        targetItem.Status = AiReviewStatus.Pending;
        targetItem.ManualReviewRequired = false;
        targetItem.UpdatedDate = null;
        targetItem.Output = null;
        
        var expectedEntity = new AiVacancyReviewEntity
        {
            Output = "foo",
            Status = AiReviewStatus.Passed,
            ManualReviewRequired = true,
        };
        
        Server.DataContext
            .SetupSequence(x => x.AiVacancyReviewEntities)
            .ReturnsDbSet(items);

        var patchDocument = new JsonPatchDocument<PatchableAiVacancyReviewDto>();
        patchDocument.Add(x => x.Output, expectedEntity.Output);
        patchDocument.Add(x => x.Status, expectedEntity.Status);
        patchDocument.Add(x => x.ManualReviewRequired, expectedEntity.ManualReviewRequired);
        
        // act
        var response = await Client.PatchAsync($"{RouteNames.AiVacancyReview}/{targetItem.VacancyReviewId}", patchDocument);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        targetItem.Output.Should().Be(expectedEntity.Output);
        targetItem.Status.Should().Be(expectedEntity.Status);
        targetItem.ManualReviewRequired.Should().Be(expectedEntity.ManualReviewRequired);
        targetItem.UpdatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        Server.DataContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}